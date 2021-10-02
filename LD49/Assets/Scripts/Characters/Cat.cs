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
        // Angry player - run away!
        if (nearbyPlayer != null && nearbyPlayer.GetEmotion() == Emotion.ANGRY) {
            // Run away!
            SetEmotion(Emotion.AFRAID);
            directionVector = transform.position - nearbyPlayer.transform.position;
        }
        else if (nearbyFood != null)
        {
            // Go get the food!
            SetEmotion(Emotion.SMITTEN);
            directionVector = nearbyFood.transform.position - transform.position;
            // Stop if close enough
            if (directionVector.sqrMagnitude < 0.1f)
            {
                directionVector = Vector3.zero;
            }
        } else if (nearbyPlayer != null) {
            // Chase!
            SetEmotion(Emotion.ANGRY);
            directionVector = nearbyPlayer.transform.position - transform.position;
            // Stop if close enough
            if (directionVector.sqrMagnitude < 0.1f)
            {
                directionVector = Vector3.zero;
            }
        } else
        {
            // Cat resets to neutral when nothing is happening.
            SetEmotion(Emotion.NEUTRAL);
        }
        if (mEmotion != Emotion.JOYFUL)
        {
            Vector2 inputVector = new Vector2(directionVector.x, directionVector.z);
            DoDirectionalMovement(inputVector.normalized);
        }
    }
}
