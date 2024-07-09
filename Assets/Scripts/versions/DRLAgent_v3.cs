using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;
// This class is responsible for controlling the agent's behavior
// Using ML-Agents, the agent will learn to navigate the environment
// and reach the target object
public class DRLAgent_v3 : Agent
{
    public GameObject environment;
    private AgentRobot agentRobot;
    private Goal goal;
    private ShelfsHandler shelfs;
    private PedestalsHandler pedestals;
    private PillarsHandler pillars;
    private Transform boundings;
    private Bounds envBounds;
    // private Transform cage;
    // private bool cageActive;
    private Bounds buildingBounds;
    private Rigidbody agentRB;

    /**
     * Training parameters 
     */
    public float speed = 10.0f;
    private NavigationLesson navigationLesson = new NavigationLesson();
    private float currentLesson = 0f;
    private float distanceFromGoal = 0.5f;
    private Vector3[] freePositions;
    private bool timePenalty = false;
    private Dictionary<string, object> trainingStatus = new Dictionary<string, object>{
        {"Lesson", 1.1f},
        {"Episodes", 0.0f},
        {"Episodes in Lesson", 0.0f},
        {"Total Reward", 0.0f},
        {"Mean Reward", 0.0f},
        {"Successes", 0.0f},
        {"Failures", 0.0f},
        {"Success Rate", "0%"}
    };

    private Dictionary<string, object> observationsStatus = new Dictionary<string, object>{
        {"Agent Pos.:", Vector3.zero},
        {"Goal Pos.:", Vector3.zero},
        {"Distance to Goal:", 0.0f},
        {"Direction to Goal:", Vector3.zero},
        {"Raycast Hits Distances:", "[]"},
        {"Agent Orientation:", Vector3.zero},
        {"Current Reward:", 0.0f}
    };

    List<float> raycastHitsDistances = new List<float>();

    private float lastObservedDistance = 0.0f;

    /**
     * ML-Agents methods
     */
    public override void Initialize()
    {
        agentRobot = new AgentRobot(environment);
        agentRB = GetComponent<Rigidbody>();
        goal = new Goal();
        shelfs = new ShelfsHandler(environment);
        pedestals = new PedestalsHandler(environment);
        pillars = new PillarsHandler(environment);
        boundings = environment.transform.Find("Boundings");
        // cage = boundings.Find("Cage");
        MaxStep = 50000;
    }

    public override void OnEpisodeBegin()
    {
        float lesson = Academy.Instance.EnvironmentParameters.GetWithDefault("lesson", 1.1f);
        CheckLessonAndSetInitialState(lesson);
        // Setting the cage properties only when the lesson changes
        if (currentLesson != lesson || currentLesson == 0)
        {
            currentLesson = lesson;
            // cage.gameObject.SetActive(cageActive);
            // cage.localScale = new Vector3(distanceFromGoal*2, 10, distanceFromGoal);
            boundings.localScale *= distanceFromGoal;
            boundings.localScale = new Vector3(boundings.localScale.x, 1, boundings.localScale.z);
            envBounds = boundings.GetComponent<BoxCollider>().bounds;
        }

        // Setting the agent in a random position
        freePositions = GetFreePositionsMap();
        agentRobot.SetFreePositionsMap(freePositions);
        agentRobot.SetAgentInBounds();
        // Setting the cage in the agent position
        // cage.position = transform.position;
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            // Setting the goal in a random position
            // Updating the free positions map after setting the agent position
            Bounds bounds = envBounds;
            // if (cageActive)
            // {
            //     bounds = cage.GetComponent<BoxCollider>().bounds;
            // }
            goal.SetFreePositionsMap(freePositions);
            if (goal.GetGoal() != null)
            {
                goal.MoveGoalFromAgent(transform, distanceFromGoal, bounds);
            }
            else
            {
                goal.CreateGoal(Vector3.zero).MoveGoalFromAgent(transform, distanceFromGoal, bounds);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            UpdateObservationsStatus();
            sensor.AddObservation(transform.position);
            sensor.AddObservation(goal.GetGoalPosition());
            Vector3 toTarget = goal.GetGoalPosition() - transform.position;
            sensor.AddObservation(toTarget.magnitude);
            // sensor.AddObservation(toTarget.normalized);

            raycastHitsDistances.Clear();
            float rayDistance = 10.0f;
            RaycastHit hit;
            float[] rayAngles = { -75, -60, -45, -30, -15, 0, 15, 30, 45, 60, 75 };
            foreach (float angle in rayAngles)
            {
                Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
                if (Physics.Raycast(transform.position, dir, out hit, rayDistance))
                {
                    sensor.AddObservation(hit.distance / rayDistance);
                    raycastHitsDistances.Add(hit.distance / rayDistance);
                }
                else
                {
                    sensor.AddObservation(1.0f);
                    raycastHitsDistances.Add(1.0f);
                }
            }
            // sensor.AddObservation(agentRB.velocity);
            // sensor.AddObservation(transform.forward);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // If the agent is upside down, the episode ends
        if (Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            AddReward(-1.0f);
            UpdateTrainingStatus();
            EndEpisode();
        }
        if (timePenalty)
        {
            AddReward(-1.0f / MaxStep);
        }
        float moveForward = actions.ContinuousActions[0];
        float moveRotate = actions.ContinuousActions[1];
        agentRB.MovePosition(transform.position + transform.forward * moveForward * speed * Time.fixedDeltaTime);
        transform.Rotate(transform.up, moveRotate * 90 * Time.fixedDeltaTime);

        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            float distanceToTarget = (float)Math.Round(Vector3.Distance(transform.position, goal.GetGoalPosition()), 2);
            if (distanceToTarget < goal.GetGoalBounds().extents.magnitude / 2)
            {
                AddReward(1.0f);
                UpdateTrainingStatus();
                EndEpisode();
            }
            else if (distanceToTarget > lastObservedDistance)
            {
                AddReward(-0.001f);
            }
            else if (distanceToTarget < lastObservedDistance)
            {
                AddReward(0.001f);
            }
            lastObservedDistance = distanceToTarget;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (navigationLesson.IsNavigationLesson(currentLesson))
        {
            float reward = navigationLesson.GetLessonReward(currentLesson, other.tag);
            if (reward != 0)
            {
                AddReward(reward);
                UpdateTrainingStatus();
                EndEpisode();
            }
        }
    }

    /**
     * Agent's methods
     */
    public Dictionary<string, object> GetAgentStatus()
    {
        return trainingStatus;
    }

    private void UpdateTrainingStatus()
    {
        if (currentLesson != (float)trainingStatus["Lesson"])
        {
            trainingStatus["Lesson"] = currentLesson;
            trainingStatus["Episodes in Lesson"] = 0;
        }
        else
        {
            trainingStatus["Episodes in Lesson"] = (float)trainingStatus["Episodes in Lesson"] + 1;
        }
        trainingStatus["Episodes"] = (float)trainingStatus["Episodes"] + 1;
        trainingStatus["Total Reward"] = (float)trainingStatus["Total Reward"] + GetCumulativeReward();
        trainingStatus["Mean Reward"] = (float)trainingStatus["Total Reward"] / (float)trainingStatus["Episodes"];

        if (GetCumulativeReward() > 0)
        {
            trainingStatus["Successes"] = (float)trainingStatus["Successes"] + 1;
        }
        else
        {
            trainingStatus["Failures"] = (float)trainingStatus["Failures"] + 1;
        }
        trainingStatus["Success Rate"] = (((float)trainingStatus["Successes"] / (float)trainingStatus["Episodes"]) * 100).ToString() + "%";
    }
    public Dictionary<string, object> GetObservationsStatus()
    {
        return observationsStatus;
    }

    private void UpdateObservationsStatus()
    {
        observationsStatus["Agent Pos.:"] = transform.position;
        observationsStatus["Goal Pos.:"] = goal.GetGoalPosition();
        Vector3 toTarget = goal.GetGoalPosition() - transform.position;
        observationsStatus["Distance to Goal:"] = toTarget.magnitude;
        observationsStatus["Direction to Goal:"] = toTarget.normalized;
        string raycastHitsDistancesStr = "["
            + string.Join(", ", raycastHitsDistances.ConvertAll(i => i.ToString()).ToArray())
            + "]";
        observationsStatus["Raycast Hits Distances:"] = raycastHitsDistancesStr;
        observationsStatus["Agent Orientation:"] = transform.forward;
        observationsStatus["Current Reward:"] = GetCumulativeReward();
    }

    // Method to map free positions in the environment
    private Vector3[] GetFreePositionsMap()
    {
        // transform bounds of the environment to Vector3 array positions
        List<Vector3> freePositions = new List<Vector3>();
        Bounds bounds = envBounds;
        for (float x = bounds.min.x; x < bounds.max.x; x += 0.5f)
        {
            for (float z = bounds.min.z; z < bounds.max.z; z += 0.5f)
            {
                Vector3 position = new Vector3(x, 0, z);
                if (Utils.CheckIfPositionIsFree(position, agentRobot.GetAgentBounds()))
                {
                    freePositions.Add(position);
                }
            }
        }
        return freePositions.ToArray();
    }

    private void CheckLessonAndSetInitialState(float lesson)
    {
        // Checking Lesson value to get the correct values for the current Lesson
        if (lesson < 2)
        {
            Dictionary<string, object> lessonValues = navigationLesson.GetLessonInitValues(lesson);
            // cageActive = (bool)lessonValues["cageActive"];
            distanceFromGoal = (float)lessonValues["distance"];
            pedestals.SetPedestalsActive((bool)lessonValues["pedestals"]);
            shelfs.SetShelfsActive((bool)lessonValues["shelfs"]);
            timePenalty = (bool)lessonValues["timePenalty"];
        }
    }

}

