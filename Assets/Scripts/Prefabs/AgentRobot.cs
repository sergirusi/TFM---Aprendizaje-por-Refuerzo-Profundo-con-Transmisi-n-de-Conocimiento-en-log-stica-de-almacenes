using UnityEngine;

public class AgentRobot
{
    private GameObject agent;
    private string PREFAB_AGENT_PATH = "Prefabs/Agent";

    private Vector3[] freePositionsMap;
    public AgentRobot(GameObject environment)
    {
        agent = environment.transform.Find("Agent").gameObject;
    }

    public void CreateAgent(Vector3 position)
    {
        agent = Object.Instantiate(Resources.Load(PREFAB_AGENT_PATH), position, Quaternion.identity) as GameObject;
    }

    public GameObject GetAgent() => agent;

    public void DestroyAgent() => Object.Destroy(agent);

    public Rigidbody GetAgentRigidbody() => agent.GetComponent<Rigidbody>();
    public Bounds GetAgentBounds() => agent.GetComponent<BoxCollider>().bounds;

    public Vector3 GetAgentPosition() => agent.transform.position;

    public void SetAgentPosition(Vector3 position) => agent.transform.position = position;
    public void SetAgentInBounds() {;
        Vector3 position = freePositionsMap[Random.Range(0, freePositionsMap.Length)];
        SetAgentPosition(position);
    }

    public void SetFreePositionsMap(Vector3[] freePositions)
    {
        freePositionsMap = freePositions;
    }
    
}