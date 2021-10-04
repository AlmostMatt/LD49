using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotion
{
    NEUTRAL,
    JOYFUL,
    ANGRY,
    AFRAID,
    SMITTEN,
}


public static class EmotionExtensions
{
    public static float GetMaxSpeed(this Emotion e)
    {
        switch(e)
        {
            case Emotion.AFRAID:
                return 5f;
            case Emotion.JOYFUL:
                return 2f;
            case Emotion.ANGRY:
                return 2.5f;
            case Emotion.SMITTEN:
                return 2f;
            default:
                return 3f;
        }
    }

    public static float GetAccelModifier(this Emotion e)
    {
        switch(e)
        {
            default:
                return 1f;
        }
    }

    public static string GetMoodTag(this Emotion e)
    {
        switch(e)
        {
            case Emotion.JOYFUL:
                return "JoyfulEnvFx";
            case Emotion.ANGRY:
                return "AngryEnvFx";
            case Emotion.SMITTEN:
                return "SmittenEnvFx";
            default:
                return null;
        }
    }
}
