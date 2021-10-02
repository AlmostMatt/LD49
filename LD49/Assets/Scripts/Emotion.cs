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
            case Emotion.JOYFUL:
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
            case Emotion.ANGRY:
                return "AngryEnvFx";
            default:
                return null;
        }
    }
}
