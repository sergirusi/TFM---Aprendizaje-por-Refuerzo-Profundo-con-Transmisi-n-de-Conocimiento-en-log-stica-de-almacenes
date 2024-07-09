// This class will be used to control the environment elements and transformations to apply when the agent is learning the Navigation Lesson and changes from one level to another
using System;
using System.Collections.Generic;
using UnityEngine;

public class NavigationLesson
{
    // Dictionary with the reward values for each element in the environment
    private Dictionary<string, float> navigationRewards = new Dictionary<string, float>
    {
        { "Pedestal", -1f },
        { "Shelf", -1f },
        { "Worker", -1f },
        { "Wall", -1f },
        { "Pillar", -1f },
        { "Goal", 1f }
    };

    /**
     * Function that returns the reward value for a specific tag
     * 
     * @param tag The tag of the object to get the reward value
     * @return The reward value for the object
     */
    public float GetLessonReward(float lessonValue, string tag)
    {
        if (lessonValue < 2)
        {
            return navigationRewards.ContainsKey(tag) ? navigationRewards[tag] : 0;
        } else {
            return 0;
        }
    }

    /**
     * Function that depending on the current lesson value, will return:
     * - The distance to the goal
     * - Activate Pedestals flag
     * - Activate Shelfs flag
     * - The maximum number of workers to instantiate
     * - The Time Penalty flag
     * 
     * Lesson levels:
     *      - 1.1: 5% of training bounds as distance, no pedestals, no shelfs, 0 workers, no time penalty
     *      - 1.2: 10% of training bounds as distance, no pedestals, no shelfs, 0 workers, no time penalty
     *      - 1.3: 20% of training bounds as distance, no pedestals, no shelfs, 0 workers, no time penalty
     *      - 1.4: Between 5% and 30% of training bounds as distance, pedestals enabled, no shelfs, 0 workers, no time penalty
     *      - 1.5: Between 5% and 50% of training bounds as distance, pedestals enabled, shelfs enabled, 0 workers, no time penalty
     *      - 1.6: Between 5% and 50% of training bounds as distance, pedestals enabled, shelfs enabled, 1 worker, no time penalty
     *      - 1.7: Between 5% and 80% of training bounds as distance, pedestals enabled, shelfs enabled, 3 workers, no time penalty
     *      - 1.8: Between 20% and 100% of training bounds as distance, pedestals enabled, shelfs enabled, 5 workers, no time penalty
     *      - 1.9: Between 20% and 100% of training bounds as distance, pedestals enabled, shelfs enabled, 10 workers, time penalty
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
                lessonValues.Add("distanceLvl", 1);
                lessonValues.Add("MaxSteps", 5000);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("maxWorkers", 0);
                break;
            case 1.2f:
                lessonValues.Add("distanceLvl", 2);
                lessonValues.Add("MaxSteps", 5000);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("maxWorkers", 0);
                break;
            case 1.3f:
                lessonValues.Add("distanceLvl", 3);
                lessonValues.Add("MaxSteps", 5000);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("maxWorkers", 0);
                break;
            case 1.4f:
                lessonValues.Add("distanceLvl", 2);
                lessonValues.Add("MaxSteps", 10000);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("maxWorkers", 0);
                break;
            case 1.5f:
                lessonValues.Add("distanceLvl", 2);
                lessonValues.Add("MaxSteps", 10000);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("maxWorkers", 0);
                break;
            case 1.6f:
                lessonValues.Add("distanceLvl", 2);
                lessonValues.Add("MaxSteps", 10000);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("maxWorkers", 1);
                break;
            case 1.7f:
                lessonValues.Add("distanceLvl", 3);
                lessonValues.Add("MaxSteps", 10000);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("maxWorkers", 3);
                break;
            case 1.8f:
                lessonValues.Add("distanceLvl", 3);
                lessonValues.Add("MaxSteps", 10000);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("maxWorkers", 5);
                break;
            case 1.9f:
                lessonValues.Add("distanceLvl", 3);
                lessonValues.Add("MaxSteps", 10000);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("maxWorkers", 10);
                break;
                
            
        }
        return lessonValues;

    }

    public bool IsNavigationLesson(float lessonValue)
    {
        return lessonValue >= 1.1f && lessonValue < 2;
    }

}