using UnityEngine;

public class ShelfsHandler
{
    private GameObject shelfs;

    public ShelfsHandler(GameObject environment) {
        shelfs = environment.transform.Find("Shelfs").gameObject;
    }

    public void SetShelfsActive(bool active) => shelfs.SetActive(active);

    public bool AreShelfsActive() => shelfs.activeSelf;

    public GameObject[] getAllShelfs() {
        return Utils.GetChildrenAsList(shelfs).ToArray();
    }
}