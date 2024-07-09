// This class will be used to control the environment elements and transformations to apply when the agent is learning the Navigation Lesson and changes from one level to another
using System;
using System.Collections.Generic;
using UnityEngine;

public class PickUpLesson
{
    // Dictionary with the reward values for each element in the environment
    private Dictionary<string, float> pickUpRewards = new Dictionary<string, float>
    {
        { "Pedestal", -1f },
        { "Shelf", -1f },
        { "Wall", -1f },
        { "Pillar", -1f },
        { "Goal", 1f },
    };

    /**
     * Function that returns the reward value for a specific tag
     * 
     * @param tag The tag of the object to get the reward value
     * @return The reward value for the object
     */
    public float GetLessonReward(float lessonValue, string tag)
    {
        if (lessonValue >= 2  && lessonValue < 3)
        {
            return pickUpRewards.ContainsKey(tag) ? pickUpRewards[tag] : 0;
        } else {
            return 0;
        }
    }

    public Dictionary<string, object> GetLessonInitValues(float lessonValue)
    {
        Dictionary<string, object> lessonValues = new Dictionary<string, object>();
        switch (lessonValue)
        {
            case 2.1f:
                lessonValues.Add("trolleyDistance", 5);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 5000);
                break;
            case 2.2f:
                lessonValues.Add("trolleyDistance", 10);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 5000);
                break;
            case 2.3f:
                lessonValues.Add("trolleyDistance", 15);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 2.4f:
                lessonValues.Add("trolleyDistance", 8);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 2.5f:
                lessonValues.Add("trolleyDistance", UnityEngine.Random.Range(2, 10));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 2.6f:
                lessonValues.Add("trolleyDistance", UnityEngine.Random.Range(4, 10));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 2.7f:
                lessonValues.Add("trolleyDistance", UnityEngine.Random.Range(6, 10));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 2.8f:
                lessonValues.Add("trolleyDistance", UnityEngine.Random.Range(0, 5));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 2.9f:
                lessonValues.Add("trolleyDistance", UnityEngine.Random.Range(8, 10));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
                
            
        }
        return lessonValues;

    }

    public bool IsPickUpLesson(float lessonValue)
    {
        return lessonValue >= 2.1f && lessonValue < 3;
    }

}