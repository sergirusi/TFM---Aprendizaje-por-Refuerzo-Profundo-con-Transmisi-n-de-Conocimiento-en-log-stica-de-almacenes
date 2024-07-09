using UnityEngine;

public class PedestalsHandler
{
    private GameObject pedestals;

    public PedestalsHandler(GameObject environment) {
        pedestals = environment.transform.Find("Pedestals").gameObject;
    }

    public void SetPedestalsActive(bool active) => pedestals.SetActive(active);

    public bool ArePedestalsActive() => pedestals.activeSelf;

    public GameObject[] getAllPedestals() {
        return Utils.GetChildrenAsList(pedestals).ToArray();
    }
}