using UnityEngine;

public class PillarsHandler
{
    private GameObject pillars;

    public PillarsHandler(GameObject environment) {
        pillars = environment.transform.Find("Pillars").gameObject;
    }

    public void SetPillarsActive(bool active) => pillars.SetActive(active);

    public bool ArePillarsActive() => pillars.activeSelf;

    public GameObject[] getAllPillars() {
        return Utils.GetChildrenAsList(pillars).ToArray();
    }
}