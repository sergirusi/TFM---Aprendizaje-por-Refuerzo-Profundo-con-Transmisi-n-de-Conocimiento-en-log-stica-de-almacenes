using System;
using UnityEngine;

public class Goal
{
    private GameObject goal;
    private string PREFAB_GOAL_PATH = "Prefabs/Goal";

    private Vector3[] freePositionsMap;

    public Goal CreateGoal(Vector3 position)
    {
        goal = UnityEngine.Object.Instantiate(Resources.Load(PREFAB_GOAL_PATH), position, Quaternion.identity) as GameObject;
        return this;
    }

    public GameObject GetGoal() => goal;

    public void DestroyGoal() => UnityEngine.Object.Destroy(goal);

    public Bounds GetGoalBounds() => goal.GetComponent<BoxCollider>().bounds;

    public Vector3 GetGoalPosition() => goal.transform.position;

    public void SetGoalPosition(Vector3 position) => goal.transform.position = position;

    public void MoveGoalFromAgent(Transform agentTransform, float distanceFromGoal, Bounds maxBounds)
    {
        Vector3 agentPosition = agentTransform.position;
        Bounds agentBounds = agentTransform.GetComponent<BoxCollider>().bounds;
        Vector3[] filteredPositions = Array.FindAll(freePositionsMap, position => maxBounds.Contains(position));
        Vector3 newGoalPosition = Vector3.zero;
        if(filteredPositions.Length == 0)
        {
            Debug.LogError("No free positions found for the goal");
            newGoalPosition = freePositionsMap[UnityEngine.Random.Range(0, freePositionsMap.Length)];
        }
        newGoalPosition = filteredPositions[UnityEngine.Random.Range(0, filteredPositions.Length)];
        SetGoalPosition(newGoalPosition);
    }

    public bool CheckIfGoalIsInsideBounds(Bounds bounds)
    {
        Bounds goalBounds = GetGoalBounds();
        Vector3 goalPosition = GetGoalPosition();
        return bounds.Contains(goalPosition) && bounds.Contains(goalPosition + goalBounds.size);
    }

    public void SetFreePositionsMap(Vector3[] freePositions)
    {
        freePositionsMap = freePositions;
    }
}