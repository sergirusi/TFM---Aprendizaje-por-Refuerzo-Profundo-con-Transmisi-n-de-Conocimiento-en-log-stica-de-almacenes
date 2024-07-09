// This class will be used to control the environment elements and transformations to apply when the agent is learning the Navigation Lesson and changes from one level to another
using System;
using System.Collections.Generic;
using UnityEngine;

public class DropLesson
{
    // Dictionary with the reward values for each element in the environment
    private Dictionary<string, float> pickUpRewards = new Dictionary<string, float>
    {
        { "Pedestal", -1f },
        { "Shelf", -1f },
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
        if (lessonValue >= 3  && lessonValue < 4)
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
            case 3.1f:
                lessonValues.Add("dropzone", -1);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 5000);
                break;
            case 3.2f:
                lessonValues.Add("dropzone", 0);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 5000);
                break;
            case 3.3f:
                lessonValues.Add("dropzone", 1);
                lessonValues.Add("pedestals", false);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 5000);
                break;
            case 3.4f:
                lessonValues.Add("dropzone", 2);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", false);
                lessonValues.Add("MaxSteps", 5000);
                break;
            case 3.5f:
                lessonValues.Add("dropzone", 3);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 3.6f:
                lessonValues.Add("dropzone", 4);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 3.7f:
                lessonValues.Add("dropzone", 5);
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 3.8f:
                lessonValues.Add("dropzone", UnityEngine.Random.Range(0, 5));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
            case 3.9f:
                lessonValues.Add("dropzone", UnityEngine.Random.Range(0, 10));
                lessonValues.Add("pedestals", true);
                lessonValues.Add("shelfs", true);
                lessonValues.Add("MaxSteps", 10000);
                break;
                
            
        }
        return lessonValues;

    }

    public bool IsDropLesson(float lessonValue)
    {
        return lessonValue >= 3.1f && lessonValue < 4;
    }

}