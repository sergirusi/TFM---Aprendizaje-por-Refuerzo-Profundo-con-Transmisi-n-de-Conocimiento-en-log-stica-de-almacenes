// This class will be used to control the environment elements and transformations to apply when the agent is learning the Navigation Lesson and changes from one level to another
using System;
using System.Collections.Generic;

public class NavigationLesson_v1
{

    // Dictionary with the reward values for each element in the environment
    private Dictionary<string, float> rewards = new Dictionary<string, float>
    {
        { "Object", -1 },
        { "Worker", -2 },
        { "Wall", -1 },
        { "Goal", 2 },
        { "Time", -0.01f }
    };

    /**
     * Function that returns the reward value for a specific tag
     * 
     * @param tag The tag of the object to get the reward value
     * @return The reward value for the object
     */
    public float GetLessonReward(string tag)
    {
        return rewards.ContainsKey(tag) ? rewards[tag] : 0;
    }

    /**
     * Function that depending on the current lesson value, will return:
     * - The maximum number of objects that can be placed in the environment and its collision penalty
     * - The minimum number of workers that can be placed in the environment and its collision penalty
     * - The scenary dimensions to resize the environment
     * - The time penalty flag and its penalty value
     * 
     * Lesson levels:
     * 1.1 - 0 object, 0 worker, 10x10, no time penalty
     * 1.2 - 0 object, 0 worker, 20x20, no time penalty
     * 1.3 - 1 object, 0 worker, 20x20, no time penalty
     * 1.4 - 1 object, 1 worker, 20x20, no time penalty
     * 1.5 - 1 objects, 1 worker, Random size, no time penalty
     * 1.6 - 3 objects, 1 worker, Random size, no time penalty
     * 1.7 - 3 objects, 3 workers, Random size, no time penalty
     * 1.8 - Random num. objects, Random num workers, Random size, no time penalty
     * 1.9 - Random num. objects, Random num workers, Random size, time penalty
     * 
     * @param lessonValue The current lesson value. It can be from 1.1 to 1.9.
     * @return A dictionary with the values for the current lesson
     */
    public Dictionary<string, object> GetLessonInitValues(float lessonValue)
    {
        Dictionary<string, object> lessonValues = new Dictionary<string, object>();
        switch (lessonValue)
        {
            case 1.1f:
                lessonValues.Add("maxObjects", 0);
                lessonValues.Add("minObjects", 0);
                lessonValues.Add("maxWorkers", 0);
                lessonValues.Add("minWorkers", 0);
                lessonValues.Add("scenarySize", new float[] { 1, 1 });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.2f:
                lessonValues.Add("maxObjects", 0);
                lessonValues.Add("minObjects", 0);
                lessonValues.Add("maxWorkers", 0);
                lessonValues.Add("minWorkers", 0);
                lessonValues.Add("scenarySize", new float[] {UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(1, 2) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.3f:
                lessonValues.Add("maxObjects", 1);
                lessonValues.Add("minObjects", 1);
                lessonValues.Add("maxWorkers", 0);
                lessonValues.Add("minWorkers", 0);
                lessonValues.Add("scenarySize", new float[] {UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(1, 2) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.4f:
                lessonValues.Add("maxObjects", 1);
                lessonValues.Add("minObjects", 1);
                lessonValues.Add("maxWorkers", 1);
                lessonValues.Add("minWorkers", 1);
                lessonValues.Add("scenarySize", new float[] {UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(1, 3) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.5f:
                lessonValues.Add("maxObjects", 1);
                lessonValues.Add("minObjects", 1);
                lessonValues.Add("maxWorkers", 1);
                lessonValues.Add("minWorkers", 1);
                lessonValues.Add("scenarySize", new float[] { UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.6f:
                lessonValues.Add("maxObjects", 3);
                lessonValues.Add("minObjects", 3);
                lessonValues.Add("maxWorkers", 1);
                lessonValues.Add("minWorkers", 1);
                lessonValues.Add("scenarySize", new float[] { UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.7f:
                lessonValues.Add("maxObjects", 3);
                lessonValues.Add("minObjects", 3);
                lessonValues.Add("maxWorkers", 3);
                lessonValues.Add("minWorkers", 3);
                lessonValues.Add("scenarySize", new float[] { UnityEngine.Random.Range(2, 4), UnityEngine.Random.Range(2, 4) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.8f:
                lessonValues.Add("maxObjects", UnityEngine.Random.Range(3, 1));
                lessonValues.Add("minObjects", 3);
                lessonValues.Add("maxWorkers", UnityEngine.Random.Range(5, 1));
                lessonValues.Add("minWorkers", 1);
                lessonValues.Add("scenarySize", new float[] { UnityEngine.Random.Range(2, 4), UnityEngine.Random.Range(2, 4) });
                lessonValues.Add("timePenalty", false);
                break;
            case 1.9f:
                lessonValues.Add("maxObjects", UnityEngine.Random.Range(3, 1));
                lessonValues.Add("minObjects", 3);
                lessonValues.Add("maxWorkers", UnityEngine.Random.Range(5, 1));
                lessonValues.Add("minWorkers", 1);
                lessonValues.Add("scenarySize", new float[] { UnityEngine.Random.Range(2, 4), UnityEngine.Random.Range(2, 4) });
                lessonValues.Add("timePenalty", true);
                break;
            default:
                lessonValues.Add("maxObjects", 0);
                lessonValues.Add("minObjects", 0);
                lessonValues.Add("maxWorkers", 0);
                lessonValues.Add("minWorkers", 0);
                lessonValues.Add("scenarySize", new float[] { 1, 1 });
                lessonValues.Add("timePenalty", false);
                break;
        }
        return lessonValues;

    }
}