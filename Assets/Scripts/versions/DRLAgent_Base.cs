using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class DRLAgent_Base : Agent
{
    public float moveSpeed = 5f;
    [SerializeField] private Transform target;

    [SerializeField] private GameObject boundings;
    private Vector3 originalBoundsScale;

    private Rigidbody rb;

    private float currentLesson = 0f;
    private NavigationLesson navigationLesson;
    private Dictionary<string, object> lessonValues;
    private Dictionary<string, object> trainingStatus = new Dictionary<string, object> {
        { "Lesson", 0f },
        { "Episodes", 0f },
        { "Episodes in Lesson", 0f },
        { "Total Reward", 0f },
        { "Mean Reward", 0f },
        { "Successes", 0f },
        { "Failures", 0f },
        { "Success Rate", "0%" }
    };
    private Dictionary<string, object> observationsStatus = new Dictionary<string, object> {
        { "Agent Pos.:", Vector3.zero },
        { "Goal Pos.:", Vector3.zero },
        { "Distance to Goal:", 0f }
    };


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        navigationLesson = new NavigationLesson();
        originalBoundsScale = boundings.transform.localScale;
    }

    public override void OnEpisodeBegin()
    {
        InitCurriculumValues();
        // Agent initial position
        Vector3 initialPosition = new Vector3(Random.Range(GetResizedValue(-40), GetResizedValue(40)), 0, Random.Range(GetResizedValue(-40), GetResizedValue(40)));
        transform.localPosition = initialPosition;
        // Target initial position
        Vector3 targetPosition = new Vector3(Random.Range(GetResizedValue(-45), GetResizedValue(45)), 0, Random.Range(GetResizedValue(-45), GetResizedValue(45)));
        target.localPosition = targetPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        UpdateObservationsStatus();
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            AddReward(-1.0f);
            EndEpisode();
        }
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * moveRotate * 300f * Time.fixedDeltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Goal")
        {
            SetReward(2f);
            UpdateTrainingStatus();
            EndEpisode();

        }
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Restart")
        {
            SetReward(-1f);
            UpdateTrainingStatus();
            EndEpisode();
        }
    }

    private void InitCurriculumValues()
    {
        float lesson = Academy.Instance.EnvironmentParameters.GetWithDefault("lesson", 1.1f);
        if (navigationLesson.IsNavigationLesson(lesson))
        {
            lessonValues = navigationLesson.GetLessonInitValues(lesson);
            if (currentLesson != lesson)
            {
                currentLesson = lesson;
                float distance = (float)lessonValues["distance"];
                boundings.transform.localScale = new Vector3(originalBoundsScale.x * distance, 1f, originalBoundsScale.z * distance);
            }
        }
    }

    private float GetResizedValue(float value)
    {
        float distance = (float)lessonValues["distance"];
        return distance * value;
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
        observationsStatus["Goal Pos.:"] = target.localPosition;
        Vector3 toTarget = target.localPosition - transform.localPosition;
        observationsStatus["Distance to Goal:"] = toTarget.magnitude;
    }

}
