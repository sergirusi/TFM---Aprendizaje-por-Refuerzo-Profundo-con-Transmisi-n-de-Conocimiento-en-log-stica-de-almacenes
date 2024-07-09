using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingHUD : MonoBehaviour
{
    public GameObject agent;
    private TextMeshProUGUI text;
    private DRLAgent agentScript;
    // private DRLInferenceAgent agentScript;


    // Start is called before the first frame update
    void Start()
    {
        // agentScript = agent.GetComponent<DRLAgent>();
        agentScript = agent.GetComponent<DRLAgent>();
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<string, object> agentStatus = agentScript.GetAgentStatus();
        text.text = "";
        foreach (KeyValuePair<string, object> entry in agentStatus)
        {
            text.text += entry.Key + ": " + entry.Value + "\n";
        }
    }
}
