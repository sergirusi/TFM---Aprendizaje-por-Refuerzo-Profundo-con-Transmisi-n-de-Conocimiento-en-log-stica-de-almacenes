using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class DRLAgent : Agent
{
    public float moveSpeed = 5f;
    [SerializeField] private Transform goals;
    [SerializeField] private GameObject boundings;
    [SerializeField] private GameObject pedestals;
    [SerializeField] private GameObject shelfs;
    [SerializeField] private GameObject pillars;
    [SerializeField] private GameObject dropZones;
    [SerializeField] private GameObject trolley;
    private Rigidbody rb;
    private GameObject agentPackage;
    private Bounds agentBounds;
    private List<Transform> goalsList = new List<Transform>();
    private Transform currentGoal;
    private int currentGoalIndex = 0;
    private Bounds trolleyBounds;
    private Bounds boundingBounds;
    private float currentLesson = 0f;
    private NavigationLesson navigationLesson;
    private PickUpLesson pickUpLesson;
    private DropLesson dropLesson;
    private Dictionary<string, object> lessonValues;
    private Dictionary<string, object> trainingStatus = new Dictionary<string, object> {
        { "Lesson", 0f },
        { "Episodes", 0f },
        { "Episodes in Lesson", 0f },
        { "Total Reward", 0f },
        { "Mean Reward", 0f },
        { "Last Reward", 0f },
        { "Successes", 0f },
        { "Failures", 0f },
        { "Success Rate", "0%" }
    };
    private Dictionary<string, object> observationsStatus = new Dictionary<string, object> {
        { "Agent Pos.:", Vector3.zero },
        { "Current Reward:", 0f }
    };

    private float currentMove = 0.0f;
    private float currentRotate = 0.0f;
    private string standingWorkerPath = "Prefabs/Standing_Worker";
    private string tabletWorkerPath = "Prefabs/Tablet_Worker";
    private List<GameObject> workers = new List<GameObject>();
    private float currentDistance = 0f;
    // private Vector3[] OccupiedLocalPositions;
    private Vector3[] freePositions;
    private Vector3 minBounds = new Vector3(-0.15f, 0, 1);
    private Vector3 maxBounds = new Vector3(35, 0, 52);

    // Load and Drop Lesson variables
    private bool isAnyDropZone = false;
    private GameObject currentDropZone;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        navigationLesson = new NavigationLesson();
        pickUpLesson = new PickUpLesson();
        dropLesson = new DropLesson();
        agentBounds = transform.GetComponent<BoxCollider>().bounds;
        for(int i = 0; i < goals.childCount; i++)
        {
            goalsList.Add(goals.GetChild(i));
        }
        currentGoalIndex = 0;
        currentGoal = goalsList[0];
        boundingBounds = boundings.GetComponent<Renderer>().bounds;
        freePositions = GetFreeLocalPositions();
        agentPackage = transform.Find("Package").gameObject;
        trolleyBounds = trolley.transform.GetChild(0).GetComponent<Renderer>().bounds;
    }

    public override void OnEpisodeBegin()
    {
        InitCurriculumValues();
        DestroyWorkers();

        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            transform.localPosition = new Vector3(1, 2.23f, 5);
            transform.rotation = Quaternion.Euler(0, 90, 0);
            for (int i = 0; i < goalsList.Count; i++)
            {
                goalsList[i].gameObject.SetActive(false);
            }
            goalsList[0].gameObject.SetActive(true);
            currentGoalIndex = 0;
            currentGoal = goalsList[0];
            WorkersPositioning();
        }
        else if (pickUpLesson.IsPickUpLesson(currentLesson))
        {
            int agentPosIndex = UnityEngine.Random.Range(0, freePositions.Length);
            transform.localPosition = freePositions[agentPosIndex];
            transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            agentPackage.SetActive(false);
            trolley.transform.Find("Package").gameObject.SetActive(true);
            int trolleyDistance = (int)lessonValues["trolleyDistance"];
            if (!pedestals.activeSelf)
            {
                trolley.transform.localPosition = transform.localPosition + (transform.forward * trolleyDistance);
                if (trolley.transform.localPosition.x < boundingBounds.min.x || trolley.transform.localPosition.x > boundingBounds.max.x ||
                    trolley.transform.localPosition.z < boundingBounds.min.z || trolley.transform.localPosition.z > boundingBounds.max.z)
                {
                    trolley.transform.localPosition = freePositions[UnityEngine.Random.Range(0, freePositions.Length)];
                }
            }
            else
            {
                Vector3[] trolleyFreePos = Array.FindAll(freePositions, pos =>
                {
                    return
                        Vector3.Distance(transform.localPosition, pos) > trolleyDistance &&
                        Vector3.Distance(transform.localPosition, pos) <= (agentBounds.extents.magnitude + trolleyDistance) &&
                        !Physics.CheckBox(pos, trolleyBounds.extents);
                });
                if (trolleyFreePos.Length == 0)
                {
                    Debug.Log("No free positions for trolley found");
                    trolley.transform.localPosition = freePositions[UnityEngine.Random.Range(0, freePositions.Length)];
                }
                else
                {
                    trolley.transform.localPosition = trolleyFreePos[UnityEngine.Random.Range(0, trolleyFreePos.Length)];
                }
            }
        }
        else if (dropLesson.IsDropLesson(currentLesson))
        {
            int agentPosIndex = UnityEngine.Random.Range(0, freePositions.Length);
            transform.localPosition = freePositions[agentPosIndex];
            transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            agentPackage.SetActive(true);
            trolley.transform.Find("Package").gameObject.SetActive(false);
            trolley.SetActive(false);
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        UpdateObservationsStatus();
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            Vector3 dirToGoal = (currentGoal.localPosition - transform.localPosition).normalized;
            sensor.AddObservation(dirToGoal.x);
            sensor.AddObservation(dirToGoal.z);
            float goalPosX = currentGoal.localPosition.x / boundingBounds.extents.x;
            float goalPosZ = currentGoal.localPosition.z / boundingBounds.extents.z;
            sensor.AddObservation(goalPosX);
            sensor.AddObservation(goalPosZ);
            float agentPosX = transform.localPosition.x / boundingBounds.extents.x;
            float agentPosZ = transform.localPosition.z / boundingBounds.extents.z;
            sensor.AddObservation(agentPosX);
            sensor.AddObservation(agentPosZ);
        }
        else if (pickUpLesson.IsPickUpLesson(currentLesson))
        {
            Vector3 dirToTrolley = (trolley.transform.localPosition - transform.localPosition).normalized;
            sensor.AddObservation(dirToTrolley.x);
            sensor.AddObservation(dirToTrolley.z);
            float trolleyPosX = trolley.transform.localPosition.x / boundingBounds.extents.x;
            float trolleyPosZ = trolley.transform.localPosition.z / boundingBounds.extents.z;
            sensor.AddObservation(trolleyPosX);
            sensor.AddObservation(trolleyPosZ);
            float agentPosX = transform.localPosition.x / boundingBounds.extents.x;
            float agentPosZ = transform.localPosition.z / boundingBounds.extents.z;
            sensor.AddObservation(agentPosX);
            sensor.AddObservation(agentPosZ);
        }
        else if (dropLesson.IsDropLesson(currentLesson))
        {
            if (!isAnyDropZone)
            {
                Vector3 dirToDropZone = (currentDropZone.transform.localPosition - transform.localPosition).normalized;
                sensor.AddObservation(dirToDropZone.x);
                sensor.AddObservation(dirToDropZone.z);
                float dropZonePosX = currentDropZone.transform.localPosition.x / boundingBounds.extents.x;
                float dropZonePosZ = currentDropZone.transform.localPosition.z / boundingBounds.extents.z;
                sensor.AddObservation(dropZonePosX);
                sensor.AddObservation(dropZonePosZ);
            }
            else if (isAnyDropZone)
            {
                GameObject closestDropZone = null;
                float closestDistance = Mathf.Infinity;
                for (int i = 0; i < dropZones.transform.childCount; i++)
                {
                    float distance = Vector3.Distance(transform.localPosition, dropZones.transform.GetChild(i).transform.localPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDropZone = dropZones.transform.GetChild(i).gameObject;
                    }
                }
                Vector3 dirToClosestDropZone = (closestDropZone.transform.localPosition - transform.localPosition).normalized;
                sensor.AddObservation(dirToClosestDropZone.x);
                sensor.AddObservation(dirToClosestDropZone.z);
                float closestDropZonePosX = closestDropZone.transform.localPosition.x / boundingBounds.extents.x;
                float closestDropZonePosZ = closestDropZone.transform.localPosition.z / boundingBounds.extents.z;
                sensor.AddObservation(closestDropZonePosX);
                sensor.AddObservation(closestDropZonePosZ);
            }
            float agentPosX = transform.localPosition.x / boundingBounds.extents.x;
            float agentPosZ = transform.localPosition.z / boundingBounds.extents.z;
            sensor.AddObservation(agentPosX);
            sensor.AddObservation(agentPosZ);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (transform.localPosition.y < 2.2f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 2.5f, transform.localPosition.z);
        }
        if (Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            UpdateTrainingStatus();
            EndEpisode();
        }
        if (StepCount >= MaxStep - 1 && MaxStep > 0)
        {
            Debug.Log("Max Steps Reached: " + MaxStep + " - Ending Episode");
            AddReward(-1f);
            UpdateTrainingStatus();
            EndEpisode();
        }
        if (GetCumulativeReward() < -1.0f)
        {
            Debug.Log("Negative Reward: " + GetCumulativeReward() + " - Ending Episode");
            UpdateTrainingStatus();
            EndEpisode();
        }
        currentRotate = actions.ContinuousActions[0];
        currentMove = actions.ContinuousActions[1];
        AddActionRewards();
        rb.MovePosition(transform.position + transform.forward * currentMove * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * currentRotate * 45f * Time.fixedDeltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Restart")
        {
            AddReward(-1f);
            UpdateTrainingStatus();
            EndEpisode();
        }
        float reward = 0f;
        bool hasToEndEpisode = true;
        bool hasToGiveReward = true;
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            reward = navigationLesson.GetLessonReward(currentLesson, other.gameObject.tag);
            if(other.gameObject.tag == "Goal")
            {
                reward = navigationLesson.GetLessonReward(currentLesson, other.gameObject.tag) / (goalsList.Count*2);
                currentGoal.gameObject.SetActive(false);
                currentGoalIndex++;
                if (currentGoalIndex < goalsList.Count)
                {
                    currentGoal = goalsList[currentGoalIndex];
                    currentGoal.gameObject.SetActive(true);
                    hasToEndEpisode = false;
                }
                else
                {
                    hasToEndEpisode = true;
                }
            }
        }
        else if (pickUpLesson.IsPickUpLesson(currentLesson))
        {
            reward = pickUpLesson.GetLessonReward(currentLesson, other.gameObject.tag);
            if (agentPackage.activeSelf)
            {
                hasToGiveReward = false;
            }
            if (other.gameObject.tag == "Trolley")
            {
                hasToEndEpisode = false;
                agentPackage.SetActive(true);
                trolley.transform.Find("Package").gameObject.SetActive(false);
            }
        }
        else if (dropLesson.IsDropLesson(currentLesson))
        {
            reward = dropLesson.GetLessonReward(currentLesson, other.gameObject.tag);
            if (other.gameObject.tag == "DropZone")
            {
                hasToEndEpisode = true;
                agentPackage.SetActive(false);
            }
        }

        if (reward != 0)
        {
            if (hasToGiveReward) AddReward(reward);
            if (hasToEndEpisode)
            {
                UpdateTrainingStatus();
                EndEpisode();
            }
        }
    }

    private void InitCurriculumValues()
    {
        float lesson = Academy.Instance.EnvironmentParameters.GetWithDefault("lesson", 1.1f);
        if (navigationLesson.IsNavigationLesson(lesson))
        {
            currentGoal = goalsList[0];
            currentGoal.gameObject.SetActive(true);
            goalsList[0].gameObject.SetActive(true);
            lessonValues = navigationLesson.GetLessonInitValues(lesson);
            if (currentLesson != lesson)
            {
                currentLesson = lesson;
                pedestals.SetActive((bool)lessonValues["pedestals"]);
                shelfs.SetActive((bool)lessonValues["shelfs"]);
                freePositions = GetFreeLocalPositions();
            }
        }
        else if (pickUpLesson.IsPickUpLesson(lesson))
        {
            agentPackage.SetActive(false);
            trolley.SetActive(true);
            trolley.transform.Find("Package").gameObject.SetActive(true);
            lessonValues = pickUpLesson.GetLessonInitValues(lesson);
            if (currentLesson != lesson)
            {
                currentLesson = lesson;
                pedestals.SetActive((bool)lessonValues["pedestals"]);
                shelfs.SetActive((bool)lessonValues["shelfs"]);
            }
        }
        else if (dropLesson.IsDropLesson(lesson))
        {
            agentPackage.SetActive(true);
            trolley.transform.Find("Package").gameObject.SetActive(false);
            trolley.SetActive(false);
            lessonValues = dropLesson.GetLessonInitValues(lesson);
            isAnyDropZone = (int)lessonValues["dropzone"] == -1 ? true : false;
            if (currentLesson != lesson)
            {
                currentLesson = lesson;
                pedestals.SetActive((bool)lessonValues["pedestals"]);
                shelfs.SetActive((bool)lessonValues["shelfs"]);
            }
            if (isAnyDropZone)
            {
                for (int i = 0; i < dropZones.transform.childCount; i++)
                {
                    dropZones.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < dropZones.transform.childCount; i++)
                {
                    dropZones.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(false);
                }
                currentDropZone = dropZones.transform.GetChild((int)lessonValues["dropzone"]).gameObject;
                currentDropZone.transform.GetChild(0).gameObject.SetActive(true);

            }

        }
        if (lessonValues.ContainsKey("MaxSteps"))
        {
            MaxStep = (int)lessonValues["MaxSteps"];
        }
    }

    public Dictionary<string, object> GetAgentStatus()
    {
        return trainingStatus;
    }

    public Dictionary<string, object> GetObservationsStatus()
    {
        return observationsStatus;
    }

    private void UpdateTrainingStatus()
    {
        if (currentLesson != (float)trainingStatus["Lesson"])
        {
            trainingStatus["Lesson"] = currentLesson;
            trainingStatus["Episodes in Lesson"] = 0f;
        }
        else
        {
            trainingStatus["Episodes in Lesson"] = (float)trainingStatus["Episodes in Lesson"] + 1f;
        }
        trainingStatus["Episodes"] = (float)trainingStatus["Episodes"] + 1;
        trainingStatus["Total Reward"] = (float)trainingStatus["Total Reward"] + GetCumulativeReward();
        trainingStatus["Mean Reward"] = (float)trainingStatus["Total Reward"] / (float)trainingStatus["Episodes"];
        trainingStatus["Last Reward"] = GetCumulativeReward();

        if (GetCumulativeReward() > 0)
        {
            trainingStatus["Successes"] = (float)trainingStatus["Successes"] + 1;
        }
        else if (GetCumulativeReward() < 0)
        {
            trainingStatus["Failures"] = (float)trainingStatus["Failures"] + 1;
        }
        trainingStatus["Success Rate"] = (((float)trainingStatus["Successes"] / (float)trainingStatus["Episodes"]) * 100).ToString() + "%";
    }

    private void UpdateObservationsStatus()
    {
        observationsStatus["Agent Pos.:"] = transform.localPosition;
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            observationsStatus["Goal Pos.:"] = currentGoal.localPosition;
            Vector3 toGoal = currentGoal.localPosition - transform.localPosition;
            observationsStatus["Distance to Goal:"] = toGoal.magnitude;
        }
        else if (pickUpLesson.IsPickUpLesson(currentLesson))
        {
            observationsStatus["Trolley Pos.:"] = trolley.transform.localPosition;
            Vector3 toTrolley = trolley.transform.localPosition - transform.localPosition;
            observationsStatus["Distance to Trolley:"] = toTrolley.magnitude;
        }
        else if (dropLesson.IsDropLesson(currentLesson))
        {
            if (!isAnyDropZone)
            {
                observationsStatus["Drop Zone Pos.:"] = currentDropZone.transform.localPosition;
                Vector3 toDropZone = currentDropZone.transform.localPosition - transform.localPosition;
                observationsStatus["Distance to Drop Zone:"] = toDropZone.magnitude;
            }
            else
            {
                GameObject closestDropZone = null;
                float closestDistance = Mathf.Infinity;
                for (int i = 0; i < dropZones.transform.childCount; i++)
                {
                    float distance = Vector3.Distance(transform.localPosition, dropZones.transform.GetChild(i).transform.localPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDropZone = dropZones.transform.GetChild(i).gameObject;
                    }
                }
                observationsStatus["Drop Zone Pos.:"] = closestDropZone.transform.localPosition;
                Vector3 toClosestDropZone = closestDropZone.transform.localPosition - transform.localPosition;
                observationsStatus["Distance to Drop Zone:"] = toClosestDropZone.magnitude;
            }
        }
        observationsStatus["Current Reward:"] = GetCumulativeReward();
    }

    private Vector3[] GetFreeLocalPositions()
    {
        List<Vector3> freePositions = new List<Vector3>();
        foreach(Transform goal in goalsList)
        {
            freePositions.Add(goal.localPosition);
        }
        return freePositions.ToArray();
    }

    private void AddActionRewards()
    {
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            float distanceToTarget = Vector3.Distance(transform.localPosition, currentGoal.localPosition);
            distanceToTarget = (float)Math.Round(distanceToTarget, 2);
            if (distanceToTarget < currentDistance)
            {
                AddReward(0.001f);
            }
            else if (distanceToTarget > currentDistance)
            {
                AddReward(-0.001f);
            }
            currentDistance = distanceToTarget;
        }
        else if (pickUpLesson.IsPickUpLesson(currentLesson))
        {
            if (!agentPackage.activeSelf)
            {
                float distanceToTrolley = Vector3.Distance(transform.localPosition, trolley.transform.localPosition);
                distanceToTrolley = (float)Math.Round(distanceToTrolley, 2);
                if (distanceToTrolley < currentDistance)
                {
                    AddReward(0.001f);
                }
                else if (distanceToTrolley > currentDistance)
                {
                    AddReward(-0.001f);
                }
                currentDistance = distanceToTrolley;
            }
            else if (StepCount % 100 == 0)
            {
                Debug.Log("Package obtained! Ending Episode.");
                UpdateTrainingStatus();
                EndEpisode();
            }
        }
        else if (dropLesson.IsDropLesson(currentLesson))
        {
            if (!isAnyDropZone)
            {
                float distanceToDropZone = Vector3.Distance(transform.localPosition, currentDropZone.transform.localPosition);
                distanceToDropZone = (float)Math.Round(distanceToDropZone, 2);
                if (distanceToDropZone < currentDistance)
                {
                    AddReward(0.001f);
                }
                else if (distanceToDropZone > currentDistance)
                {
                    AddReward(-0.001f);
                }
                currentDistance = distanceToDropZone;
            }
            else if (isAnyDropZone)
            {
                GameObject closestDropZone = null;
                float closestDistance = Mathf.Infinity;
                for (int i = 0; i < dropZones.transform.childCount; i++)
                {
                    float distance = Vector3.Distance(transform.localPosition, dropZones.transform.GetChild(i).transform.localPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDropZone = dropZones.transform.GetChild(i).gameObject;
                    }
                }
                float distanceToClosestDropZone = Vector3.Distance(transform.localPosition, closestDropZone.transform.localPosition);
                distanceToClosestDropZone = (float)Math.Round(distanceToClosestDropZone, 2);
                if (distanceToClosestDropZone < currentDistance)
                {
                    AddReward(0.001f);
                }
                else if (distanceToClosestDropZone > currentDistance)
                {
                    AddReward(-0.001f);
                }
                currentDistance = distanceToClosestDropZone;
            }
        }
    }

    private void DestroyWorkers()
    {
        if (workers != null)
        {
            foreach (GameObject worker in workers)
            {
                Destroy(worker);
            }
            workers.Clear();
        }
    }

    private void WorkersPositioning()
    {
        int maxWorkers = (int)lessonValues["maxWorkers"];
        if (maxWorkers > 0)
        {
            for (int i = 0; i < maxWorkers; i++)
            {
                float workerType = i % 2; ;
                string workersPrefabPath = workerType == 1 ? tabletWorkerPath : standingWorkerPath;
                GameObject worker = Instantiate(Resources.Load(workersPrefabPath), new Vector3(0,0,0), Quaternion.identity) as GameObject;
                worker.transform.parent = transform.parent;
                Vector3 workerPos = new Vector3(UnityEngine.Random.Range(minBounds.x, maxBounds.x), 2.23f, UnityEngine.Random.Range(minBounds.z, maxBounds.z));
                worker.transform.localPosition = workerPos;
                workers.Add(worker);
            }
        }
    }

}
