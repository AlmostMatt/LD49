using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : Character
{
    public float visionRadius = 3f;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Emotion triggers and movement
        // Default to being lazy
        Vector3 directionVector = Vector3.zero;
        Food nearbyFood = ObjectInRangeOrNull<Food>(visionRadius);
        Player nearbyPlayer = ObjectInRangeOrNull<Player>(visionRadius);
        if (nearbyFood != null)
        {
            // Go get the food!
            SetEmotion(Emotion.SMITTEN);
            directionVector = nearbyFood.transform.position - transform.position;
        }
        // Food is more important than player (for now)
        if (nearbyFood == null && nearbyPlayer != null)
        {
            if (nearbyPlayer.GetEmotion() == Emotion.ANGRY)
            {
                // Run away!
                SetEmotion(Emotion.AFRAID);
                directionVector = transform.position - nearbyPlayer.transform.position;
            } else
            {
                // Chase!
                SetEmotion(Emotion.ANGRY);
                directionVector = nearbyPlayer.transform.position - transform.position;
            }
        }
        if (nearbyFood == null && nearbyPlayer == null)
        {
            SetEmotion(Emotion.NEUTRAL);
        }
        if (mEmotion != Emotion.JOYFUL)
        {
            Vector2 inputVector = new Vector2(directionVector.x, directionVector.z);
            DoDirectionalMovement(inputVector.normalized);
        }
    }
}
