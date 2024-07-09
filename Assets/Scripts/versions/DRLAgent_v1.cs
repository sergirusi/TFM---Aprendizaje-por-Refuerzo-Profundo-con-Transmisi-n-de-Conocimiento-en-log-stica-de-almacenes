using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
// This class is responsible for controlling the agent's behavior
// Using ML-Agents, the agent will learn to navigate the environment
// and reach the target object
public class DRLAgent_v1 : Agent
{
    public float moveSpeed = 2f;
    public GameObject environment;
    private Rigidbody rb;
    private bool missionComplete = false;
    private EnvironmentParameters environmentParameters;
    private float lesson = 1.0f;
    private Dictionary<string, (float reward, bool endEpisode)> lesson1Rewards = new Dictionary<string, (float, bool)> {
        { "Wall", (-1f, true) },
        { "Pedestal", (2f, true) },
        { "Restart", (0f, true) }
    };

    private Dictionary<string, (float reward, bool endEpisode)> lesson2Rewards = new Dictionary<string, (float, bool)> {
        { "Wall", (-1f, true) },
        { "Pedestal", (2f, true) },
        { "Pedestal_Leg", (-0.5f, true) },
        { "Restart", (0f, true) }
    };

    private Dictionary<string, (float reward, bool endEpisode)> lesson3Rewards = new Dictionary<string, (float, bool)> {
        { "Wall", (-1f, true) },
        { "Pedestal", (0.5f, false) },
        { "Pedestal_Leg", (-0.5f, true) },
        { "Pallet", (2f, true) },
        { "Restart", (0f, true) }
    };

    private Dictionary<string, (float reward, bool endEpisode)> lesson4Rewards = new Dictionary<string, (float, bool)> {
        { "Wall", (-1f, true) },
        { "Pedestal", (0.5f, false) },
        { "Pedestal_Leg", (-0.5f, true) },
        { "Pallet", (2f, true) },
        { "Restart", (0f, true) }
    };

    private Transform pedestals;
    private List<Vector3> possiblePositions = new List<Vector3>();
    private List<Vector3> pedestalsVisited = new List<Vector3>();


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        environmentParameters = Academy.Instance.EnvironmentParameters;
    }

    public override void OnEpisodeBegin()
    {
        pedestalsVisited.Clear();
        lesson = environmentParameters.GetWithDefault("lesson_num", 1.0f);
        randomizeEnvElements();
    }

    private void randomizeEnvElements()
    {
        var floor = environment.transform.Find("Scenario").Find("Floor");
        var floorDimensions = floor.GetComponent<Renderer>().bounds.size;
        for (float i = -floorDimensions.x / 2 + 1; i < floorDimensions.x / 2 - 1; i += 2)
        {
            for (float j = -floorDimensions.z / 2 + 1; j < floorDimensions.z / 2 - 1; j += 2)
            {
                possiblePositions.Add(new Vector3(i, 0f, j));
            }
        }
        pedestals = environment.transform.Find("Pedestals");
        for (int i = 0; i < pedestals.childCount; i++)
        {
            var pedestal = pedestals.GetChild(i);
            int posPedestal;
            do
            {
                posPedestal = Random.Range(0, possiblePositions.Count);
            } while (!IsPositionValid(possiblePositions[posPedestal], pedestal.gameObject));
            pedestal.localPosition = possiblePositions[posPedestal];
            if (lesson > 3.0f)
            {
                pedestal.localRotation = Quaternion.Euler(0f, Random.Range(0, 60) * 6, 0f);
            }
            possiblePositions.RemoveAt(posPedestal);
        }

        int posAgent;
        do
        {
            posAgent = Random.Range(0, possiblePositions.Count);
        } while (!IsPositionValid(possiblePositions[posAgent], transform.gameObject));
        transform.localPosition = possiblePositions[posAgent];
        transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 60) * 6, 0f);
        possiblePositions.RemoveAt(posAgent);

    }

    private bool IsPositionValid(Vector3 position, GameObject obj)
    {
        // Check if the position is within the bounds of the agent or another pedestal
        for (int i = 0; i < pedestals.childCount; i++)
        {
            var pedestal = pedestals.GetChild(i);
            if (Vector3.Distance(position, pedestal.localPosition) < 1.5f)
            {
                return false;
            }
        }
        return true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (transform.rotation.z > 100)
        {
            EndEpisode();
        }
        if (!missionComplete)
        {
            float moveRotate = actions.ContinuousActions[0];
            float moveForward = actions.ContinuousActions[1];
            rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.fixedDeltaTime);
            transform.Rotate(transform.up, moveRotate * 90 * Time.fixedDeltaTime);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lesson == 1.0f)
        {
            if (lesson1Rewards.ContainsKey(other.gameObject.tag))
            {
                Log("Lesson 1 - " + other.gameObject.tag + "- Reward:", lesson1Rewards[other.gameObject.tag]);
                SetReward(lesson1Rewards[other.gameObject.tag].reward);
                if (lesson1Rewards[other.gameObject.tag].endEpisode)
                {
                    EndEpisode();
                }
            }
        }
        else if (lesson == 2.0f)
        {
            if (lesson2Rewards.ContainsKey(other.gameObject.tag))
            {
                Log("Lesson 2 - " + other.gameObject.tag + "- Reward:", lesson2Rewards[other.gameObject.tag]);
                SetReward(lesson2Rewards[other.gameObject.tag].reward);
                if (lesson2Rewards[other.gameObject.tag].endEpisode)
                {
                    EndEpisode();
                }
            }
        }
        else if (lesson == 3.0f)
        {
            if (lesson3Rewards.ContainsKey(other.gameObject.tag))
            {
                if(other.gameObject.tag == "Pedestal")
                {
                    if (isPedestalVisited(other.gameObject.transform.localPosition))
                    {
                        SetReward(-0.5f);
                        EndEpisode();
                    }
                    pedestalsVisited.Add(other.gameObject.transform.localPosition);
                }
                Log("Lesson 3 - " + other.gameObject.tag + "- Reward:", lesson3Rewards[other.gameObject.tag]);
                SetReward(lesson3Rewards[other.gameObject.tag].reward);
                if (lesson3Rewards[other.gameObject.tag].endEpisode)
                {
                    EndEpisode();
                }
            }
        }
        else if (lesson == 4.0f)
        {
            if (lesson4Rewards.ContainsKey(other.gameObject.tag))
            {
                if(other.gameObject.tag == "Pedestal")
                {
                    if (isPedestalVisited(other.gameObject.transform.localPosition))
                    {
                        SetReward(-0.5f);
                        EndEpisode();
                    }
                    pedestalsVisited.Add(other.gameObject.transform.localPosition);
                }
                Log("Lesson 4 - " + other.gameObject.tag + "- Reward:", lesson4Rewards[other.gameObject.tag]);
                SetReward(lesson4Rewards[other.gameObject.tag].reward);
                if (lesson4Rewards[other.gameObject.tag].endEpisode)
                {
                    EndEpisode();
                }
            }
        }
    }

    private bool isPedestalVisited(Vector3 position)
    {
        foreach (Vector3 visited in pedestalsVisited)
        {
            if (Vector3.Distance(position, visited) < 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    private void Log(string message, object obj)
    {
        var output = JsonUtility.ToJson(obj, true);
        Debug.Log(message + ' ' + output);
    }
}

