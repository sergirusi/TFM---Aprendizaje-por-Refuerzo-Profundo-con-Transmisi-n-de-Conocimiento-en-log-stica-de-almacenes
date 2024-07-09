using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ProTrainingDistribution : MonoBehaviour, IAgentObserver
{
    private GameObject pedestals;
    private List<GameObject> pedestalsList = new List<GameObject>();
    private GameObject shelfs;
    private List<GameObject> shelfsList = new List<GameObject>();
    private static string PREFABS_PATH = "Prefabs/";
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

    private AgentRobot agent;
    private Goal goal = new Goal();
    private List<GameObject> vehicles = new List<GameObject>();
    private List<GameObject> workers = new List<GameObject>();
    private float distanceToGoal = 0;
    private bool pedestalsActive = false;
    private bool shelfsActive = false;
    private int maxVehicles = 0;
    private int minVehicles = 0;
    private int maxWorkers = 0;
    private int minWorkers = 0;
    private DRLAgent_v2 agentScript;

    private NavigationLesson navigationLesson = new NavigationLesson();
    private float currentLesson = 1.1f;
    private Bounds bounds;

    // Start is called before the first frame update
    void Start()
    {
        agent = new AgentRobot(transform.gameObject);
        agent.CreateAgent(Vector3.zero);
        goal.CreateGoal(Vector3.zero);
        bounds = GetEnvBounds();
        pedestals = transform.Find("Pedestals").gameObject;
        shelfs = transform.Find("Shelfs").gameObject;

        // Checking Lesson value to get the correct values for the current Lesson
        CheckLessonAndSetInitialState();

        // Setting the agent in a random position
        Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
        agent.SetAgentPosition(position);

        // Registering the observer in the agent
        agentScript = GameObject.FindObjectOfType<DRLAgent_v2>();
        agentScript.RegisterObserver(this);

        // Setting the goal in a random position
        SetGoal();

        // Creating random vehicles and workers
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
        DestroyAll(vehicles);
        DestroyAll(workers);

        
        Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
        agent.SetAgentPosition(position);

        SetGoal();
        CreateVehiclesFromPrefabs();
        CreateWorkersFromPrefabs();
    }

    private void CheckLessonAndSetInitialState()
    {
        if (currentLesson < 2)
        {
            Dictionary<string, object> lessonValues = navigationLesson.GetLessonInitValues(currentLesson);
            distanceToGoal = (float)lessonValues["distance"];
            pedestalsActive = (bool)lessonValues["pedestals"];
            pedestals.SetActive(pedestalsActive);
            if (pedestalsActive && pedestals != null && pedestalsList.Count == 0)
            {
                pedestalsList = Utils.GetChildrenAsList(pedestals);
            }

            shelfsActive = (bool)lessonValues["shelfs"];
            shelfs.SetActive(shelfsActive);
            if (shelfsActive && shelfs != null && shelfsList.Count == 0)
            {
                shelfsList = Utils.GetChildrenAsList(shelfs);
            }

            maxWorkers = (int)lessonValues["maxWorkers"];
            minWorkers = maxWorkers;
        }
    }

    private void CreateVehiclesFromPrefabs()
    {
        // Instantiate random vehicles in random positions different from the agent
        for (int i = 0; i < UnityEngine.Random.Range(minVehicles, maxVehicles); i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, VEHICLES_PREFABS.Length);
            GameObject prefab = Resources.Load<GameObject>(PREFABS_PATH + VEHICLES_PREFABS[prefabIndex]);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + VEHICLES_PREFABS[prefabIndex]);
                continue;
            }
            Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            while (!AcceptablePosition(position, "Vehicle"))
            {
                position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            }
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            vehicles.Add(obj);
        }
    }

    private void CreateWorkersFromPrefabs()
    {
        // Instantiate random workers in random positions different from the agent
        
        for (int i = 0; i < UnityEngine.Random.Range(minWorkers, maxWorkers); i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, WORKER_PREFABS.Length);
            GameObject prefab = Resources.Load<GameObject>(PREFABS_PATH + WORKER_PREFABS[prefabIndex]);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + WORKER_PREFABS[prefabIndex]);
                continue;
            }
            Vector3 position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            while (!AcceptablePosition(position, "Worker"))
            {
                position = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 0, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
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
            Vector3 maxDistance = GetGoalMaxDistance();
            // goal.MoveGoalFromAgent(agent, 0.05f, bounds);
            
            // Inform the agent about the new goal position in order to update the observation
            agentScript.onGoalPositionChanged(goal.GetGoalPosition());
        }
    }

    private Vector3 GetGoalMaxDistance()
    {
        
        float maxX = bounds.max.x * distanceToGoal;
        float maxZ = bounds.max.z * distanceToGoal;
        return new Vector3(maxX, 0, maxZ);
    }

    private bool AcceptablePosition(Vector3 position, string tag)
    {
        // Concatening all lists: objects, vehicles, workers in order to check if the position is acceptable
        List<GameObject> all = new List<GameObject>();
        all.Add(agent.GetAgent());
        all.AddRange(vehicles);
        all.AddRange(workers);
        all.AddRange(pedestalsList);
        all.AddRange(shelfsList);

        return all.Exists(obj => Vector3.Distance(position, obj.transform.position) > 5f);
    }


    private Bounds GetEnvBounds()
    {
        GameObject bounding = transform.Find("Boundings").gameObject;
        Renderer boundingRenderer = bounding.GetComponent<Renderer>();
        Bounds bounds = boundingRenderer.bounds;
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
