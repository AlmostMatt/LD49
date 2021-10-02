using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotion
{
    NEUTRAL,
    HAPPY,
    ANGRY
}


public static class EmotionExtensions
{
    public static float GetMaxSpeed(this Emotion e)
    {
        switch(e)
        {
            case Emotion.HAPPY:
                return 2f;
            default:
                return 3f;
        }
    }

    public static float GetAccel(this Emotion e)
    {
        switch(e)
        {
            case Emotion.HAPPY:
                return 25f;
            default:
                return 50f;
        }
    }
}
