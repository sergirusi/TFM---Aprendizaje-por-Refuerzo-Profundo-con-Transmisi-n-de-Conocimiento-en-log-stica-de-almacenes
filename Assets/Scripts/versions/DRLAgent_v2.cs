

using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
// This class is responsible for controlling the agent's behavior
// Using ML-Agents, the agent will learn to navigate the environment
// and reach the target object
public class DRLAgent_v2 : Agent
{
    public float moveSpeed = 2f;
    private Rigidbody rb;
    private EnvironmentParameters environmentParameters;
    private List<IAgentObserver> observers = new List<IAgentObserver>();
    private NavigationLesson navigationLesson = new NavigationLesson();
    private bool timePenalty = false;
    private float timePenaltyValue = 0;

    private float currentLesson = 1.1f;

    // The TrainingDistribution Class will access to the agent and inform every time the goal position changes.
    private Vector3 goalPosition;
    private float currentDistance = -1;
    private float totalReward = 0;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        environmentParameters = Academy.Instance.EnvironmentParameters;
    }

    public override void OnEpisodeBegin()
    {
        totalReward = 0;
        currentLesson = environmentParameters.GetWithDefault("lesson_num", 1.1f);
        foreach (var observer in observers)
        {
            observer.OnLessonChanged(currentLesson);
        }
        if (currentLesson < 2)
        {
            // If the current lesson is less than 2, it means that the agent is in the navigation lesson
            // Getting the time penalty flag value for the current lesson
            Dictionary<string, object> lessonValues = navigationLesson.GetLessonInitValues(currentLesson);
            timePenalty = (bool)lessonValues["timePenalty"];
        }
    }

    public void RegisterObserver(IAgentObserver observer)
    {
        observers.Add(observer);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goalPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (transform.rotation.x > 10 || transform.rotation.z > 10 || transform.rotation.x < -10 || transform.rotation.z < -10)
        {
            EndEpisodeProcess();
        }
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up, moveRotate * 90 * Time.fixedDeltaTime);
        if (currentDistance < Vector3.Distance(transform.localPosition, goalPosition))
        {
            currentDistance = Vector3.Distance(transform.localPosition, goalPosition);
            AddReward(-0.001f);
            totalReward -= 0.001f;
        }
        else
        {
            currentDistance = Vector3.Distance(transform.localPosition, goalPosition);
            AddReward(0.001f);
            totalReward += 0.001f;
        }
        if (timePenalty)
        {
            AddReward(-0.001f);
            totalReward -= 0.001f;
            if (timePenaltyValue < -4)
            {
                EndEpisodeProcess();
                Debug.Log("Time penalty reached the limit! Ending episode.");
            }
            else
            {
                timePenaltyValue -= 0.001f;
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    public void onGoalPositionChanged(Vector3 goalPosition)
    {
        this.goalPosition = goalPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentLesson < 2)
        {
            // If the current lesson is less than 2, it means that the agent is in the navigation lesson
            float reward = navigationLesson.GetLessonReward(currentLesson, other.tag);
            AddReward(reward);
            totalReward += reward;
            // Tautological flag. Must be changed when changing the lesson behavior in order 
            // to have EndEpisode conditions when collisioning with elements.
            bool endEpisode = true;
            if (endEpisode && reward != 0)
            {
                EndEpisodeProcess();
            }
        }
    }

    private void EndEpisodeProcess()
    {
        timePenaltyValue = 0;
        EndEpisode();
        foreach (var observer in observers)
        {
            observer.OnEndEpisode(totalReward);
        }
    }
}

