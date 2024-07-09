using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentFollowing : MonoBehaviour
{
    public GameObject agent;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = agent.transform.position - agent.transform.forward * 3 + agent.transform.up * 1;
        transform.rotation = agent.transform.rotation;

        // camera in upper view, following the agent
        // transform.position = agent.transform.position + new Vector3(0, 10, 0);
        // transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
