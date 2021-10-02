using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : Character
{
    public float visionRadius = 3f;
    public Transform foodAttachPoint;

    private Food mCurrentlyHeldFood = null;
    private float mRecentlyDroppedFoodCooldown = 0f;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Emotion triggers and movement
        // Default to being lazy
        Vector3 directionVector = Vector3.zero;
        // Only notice nearby food if not already holding food.
        Food nearbyFood = null;
        if (mCurrentlyHeldFood == null && mRecentlyDroppedFoodCooldown <= 0f)
        {
            nearbyFood = ObjectInRangeOrNull<Food>(visionRadius);
        }
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
        }
        // TODO - edge case? if cat has bun, player will become angry before cat becomes angry.
        else if (nearbyPlayer != null && mCurrentlyHeldFood == null) {
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
        mRecentlyDroppedFoodCooldown -= Time.fixedDeltaTime;
    }

    private void PickupFood(Food food)
    {
        if (mCurrentlyHeldFood != null || mRecentlyDroppedFoodCooldown > 0f)
        {
            return;
        }
        Debug.Log("Cat picked up food!");
        mCurrentlyHeldFood = food;
        // set parent, set local pos to 0, keep local scale.
        food.transform.SetParent(foodAttachPoint, false);
        food.transform.localPosition = Vector3.zero;
        // Disallow the player from collecting this food
        food.GetComponent<EmotionTrigger>().enabled = false;
    }
    private void DropFood()
    {
        if (mCurrentlyHeldFood != null)
        {
            Debug.Log("Cat dropped food because of angry player!");
            // unset parent. keep world pos, keep local scale
            Vector3 pos = mCurrentlyHeldFood.transform.position;
            mCurrentlyHeldFood.transform.SetParent(null, false);
            mCurrentlyHeldFood.transform.position = pos;
            // Reenable player collection of the food
            mCurrentlyHeldFood.GetComponent<EmotionTrigger>().enabled = true;
            mCurrentlyHeldFood = null;
            // don't allow cat to pickup again for 0.5s
            mRecentlyDroppedFoodCooldown = 0.5f;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        HandleCollisionOrTrigger(collision.gameObject);
    }

    public void OnCollisionStay(Collision collision)
    {
        HandleCollisionOrTrigger(collision.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        HandleCollisionOrTrigger(other.gameObject);
    }

    public void OnTriggerStay(Collider other)
    {
        HandleCollisionOrTrigger(other.gameObject);
    }

    private void HandleCollisionOrTrigger(GameObject other)
    {
        // If touch food, pickup food
        Food food = other.GetComponent<Food>();
        if (food != null)
        {
            PickupFood(food);
        }
        // If touch angry player, drop food
        Player player = other.GetComponent<Player>();
        if (player != null && player.GetEmotion() == Emotion.ANGRY)
        {
            DropFood();
        }
    }
}
