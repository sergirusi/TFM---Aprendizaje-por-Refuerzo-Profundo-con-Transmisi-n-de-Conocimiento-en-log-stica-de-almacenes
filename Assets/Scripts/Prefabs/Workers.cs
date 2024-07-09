using UnityEngine;

public class Workers
{
    private GameObject worker;

    private string PREFAB_WORKER_PATH = "Prefabs/Worker";

    public void SetWorker(Vector3 position) {
        worker = Object.Instantiate(Resources.Load(PREFAB_WORKER_PATH), position, Quaternion.identity) as GameObject;
    }

    public GameObject GetWorker() => worker;

    public void DestroyWorker() => Object.Destroy(worker);

    public Bounds GetWorkerBounds() => worker.GetComponent<BoxCollider>().bounds;

    public Vector3 GetWorkerPosition() => worker.transform.position;

    public void SetWorkerPosition(Vector3 position) => worker.transform.position = position;

}