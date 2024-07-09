using System.Collections.Generic;
using UnityEngine;

public class TrainingDistribution : MonoBehaviour, IAgentObserver
{

    private static string PREFABS_PATH = "Prefabs/";
    private static string[] OBJECT_PREFABS =
    {
        "Pedestal",
        "PedestalWithPallet",
        "PedestalWithPalletA",
        "PedestalWithPalletA1",
        "PedestalWithPalletA2",
        "PedestalWithPalletB",
        "PedestalWithPalletB1",
        "PedestalWithPalletB2",
        "PedestalWithPalletC1",
        "PedestalWithPalletC2",
        "ShelfWithContents",
        "Trolley",
    };
    private static string[] VEHICLES_PREFABS =
    {
        "Reachlift",
        "Palletrobot",
        "Forklift",
    };
    private static string[] WORKER_PREFABS =
    {
        "Tablet_Worker",
        "Standing_Worker"
    };

    private static string AGENT_PREFAB = "Agent";
    private static string GOAL_PREFAB = "Goal";
    private GameObject agent;
    private GameObject goal;
    private List<GameObject> objects = new List<GameObject>();
    private List<GameObject> vehicles = new List<GameObject>();
    private List<GameObject> workers = new List<GameObject>();

    private int maxObjects = 0;
    private int minObjects = 0;
    private int maxVehicles = 0;
    private int minVehicles = 0;
    private int maxWorkers = 0;
    private int minWorkers = 0;
    private float[] scenarySize = { 1, 1 };

    private DRLAgent_v2 agentScript;

    private NavigationLesson navigationLesson = new NavigationLesson();
    private float currentLesson = 1.1f;

    // Start is called before the first frame update
    void Start()
    {
        // Checking Lesson value to get the correct values for the current Lesson
        CheckLessonAndSetInitialState();
        GameObject agentPrefab = Resources.Load<GameObject>(PREFABS_PATH + AGENT_PREFAB);
        if (agentPrefab == null)
        {
            Debug.LogError("Agent prefab not found");
            return;
        }
        Bounds bounds = getEnvBounds();
        Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
        agent = Instantiate(agentPrefab, position, Quaternion.identity);

        agentScript = GameObject.FindObjectOfType<DRLAgent_v2>();
        agentScript.RegisterObserver(this);

        SetGoal();
        CreateObjectsFromPrefabs();
        CreateVehiclesFromPrefabs();
        CreateWorkersFromPrefabs();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnLessonChanged(float lessonValue)
    {
        currentLesson = lessonValue;
    }

    public void OnEndEpisode(float reward)
    {
        // Checking Lesson value to get the correct values for the current Lesson
        CheckLessonAndSetInitialState();

        // Restart the environment by destroying all objects and creating new ones in random positions
        Bounds bounds = getEnvBounds();
        Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
        agent.transform.position = position;

        DestroyAll(objects);
        DestroyAll(vehicles);
        DestroyAll(workers);
        if (currentLesson < 2)
        {
            // If the lesson is less than 2, then the current lesson is NavigationLesson and it needs a goal
            SetGoal();
        }
        CreateObjectsFromPrefabs();
        CreateVehiclesFromPrefabs();
        CreateWorkersFromPrefabs();
    }

    private void CheckLessonAndSetInitialState()
    {
        if (currentLesson < 2)
        {
            Dictionary<string, object> lessonValues = navigationLesson.GetLessonInitValues(currentLesson);
            maxObjects = (int)lessonValues["maxObjects"];
            minObjects = (int)lessonValues["minObjects"];
            maxWorkers = (int)lessonValues["maxWorkers"];
            minWorkers = (int)lessonValues["minWorkers"];
            scenarySize = (float[])lessonValues["scenarySize"];

            transform.localScale = new Vector3(scenarySize[0], 1, scenarySize[1]);
        }
    }

    private void CreateObjectsFromPrefabs()
    {
        // Instantiate random prefabs in random positions different from the agent
        Bounds bounds = getEnvBounds();
        for (int i = 0; i < UnityEngine.Random.Range(minObjects, maxObjects); i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, OBJECT_PREFABS.Length);
            GameObject prefab = Resources.Load<GameObject>(PREFABS_PATH + OBJECT_PREFABS[prefabIndex]);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + OBJECT_PREFABS[prefabIndex]);
                continue;
            }
            Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            while (!AcceptablePosition(position, "Object"))
            {
                position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            }
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            objects.Add(obj);
        }
    }

    private void CreateVehiclesFromPrefabs()
    {
        // Instantiate random vehicles in random positions different from the agent
        Bounds bounds = getEnvBounds();
        for (int i = 0; i < UnityEngine.Random.Range(minVehicles, maxVehicles); i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, VEHICLES_PREFABS.Length);
            GameObject prefab = Resources.Load<GameObject>(PREFABS_PATH + VEHICLES_PREFABS[prefabIndex]);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + VEHICLES_PREFABS[prefabIndex]);
                continue;
            }
            Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            while (!AcceptablePosition(position, "Vehicle"))
            {
                position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            }
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            vehicles.Add(obj);
        }
    }

    private void CreateWorkersFromPrefabs()
    {
        // Instantiate random workers in random positions different from the agent
        Bounds bounds = getEnvBounds();
        for (int i = 0; i < UnityEngine.Random.Range(minWorkers, maxWorkers); i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, WORKER_PREFABS.Length);
            GameObject prefab = Resources.Load<GameObject>(PREFABS_PATH + WORKER_PREFABS[prefabIndex]);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + WORKER_PREFABS[prefabIndex]);
                continue;
            }
            Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            while (!AcceptablePosition(position, "Worker"))
            {
                position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            }
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            workers.Add(obj);
        }
    }

    private void SetGoal()
    {
        if (currentLesson < 2)
        {
            // If the lesson is less than 2, then the current lesson is NavigationLesson and it needs a goal
            Bounds bounds = getEnvBounds();
            Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            while (!AcceptablePosition(position, "Goal"))
            {
                position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            }
            if (goal == null)
            {
                goal = Instantiate(Resources.Load<GameObject>(PREFABS_PATH + GOAL_PREFAB), position, Quaternion.identity);
            }
            else
            {
                goal.transform.position = position;
            }
            // Inform the agent about the new goal position in order to update the observation
            agentScript.onGoalPositionChanged(position);
        }
    }

    private bool AcceptablePosition(Vector3 position, string tag)
    {
        if (tag == "Goal")
        {
            return Vector3.Distance(position, agent.transform.position) > 5f;
        }
        else
        {
            // Concatening all lists: objects, vehicles, workers in order to check if the position is acceptable
            List<GameObject> all = new List<GameObject>();
            all.AddRange(objects);
            all.AddRange(vehicles);
            all.AddRange(workers);

            return Vector3.Distance(position, agent.transform.position) > 5f ||
            (goal != null && Vector3.Distance(position, goal.transform.position) > 5f) ||
            all.Exists(obj => Vector3.Distance(position, obj.transform.position) > 5f);
        }

    }


    private Bounds getEnvBounds()
    {
        Bounds bounds = transform.GetChild(0).GetChild(0).GetComponent<Renderer>().bounds;
        bounds.min += new Vector3(0.5f, 0, 0.5f);
        bounds.max -= new Vector3(0.5f, 0, 0.5f);
        return bounds;
    }

    private void DestroyAll(List<GameObject> gameObjectList)
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();
    }
}
