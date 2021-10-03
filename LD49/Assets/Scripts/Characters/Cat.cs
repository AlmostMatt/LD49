using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cat : Character
{
    public float visionRadius = 3f;
    public Transform foodAttachPoint;

    private Food mCurrentlyHeldFood = null;
    private float mRecentlyDroppedFoodCooldown = 0f;
    private Vector3[] mPathCorners;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Emotion triggers and movement
        // Default to being lazy
        Vector3 targetPos = transform.position;
        Vector3 directionVector = Vector3.zero;
        // Only notice nearby food if not already holding food.
        Food nearbyFood = null;
        if (mCurrentlyHeldFood == null && mRecentlyDroppedFoodCooldown <= 0f)
        {
            nearbyFood = ObjectInRangeOrNull<Food>(visionRadius);
        }
        Player nearbyPlayer = ObjectInRangeOrNull<Player>(visionRadius);
        // Angry player - run away!
        if (nearbyPlayer != null && nearbyPlayer.GetEmotion() == Emotion.ANGRY)
        {
            // Run away!
            SetEmotion(Emotion.AFRAID);
            Vector3 awayDir = (transform.position - nearbyPlayer.transform.position);
            targetPos = transform.position + awayDir.normalized;
        }
        else if (nearbyFood != null)
        {
            // Go get the food!
            SetEmotion(Emotion.SMITTEN);
            targetPos = nearbyFood.transform.position;
        }
        // TODO - edge case? if cat has bun, player will become angry before cat becomes angry.
        else if (nearbyPlayer != null && mCurrentlyHeldFood == null)
        {
            // Chase!
            if (mEmotion != Emotion.ANGRY)
            {
                Debug.Log("Cat became angry because it saw a player");
            }
            SetEmotion(Emotion.ANGRY);
            targetPos = nearbyPlayer.transform.position;
        }
        else
        {
            // Cat resets to neutral when nothing is happening.
            SetEmotion(Emotion.NEUTRAL);
        }
        if (mEmotion != Emotion.JOYFUL)
        {
            // Vector2 inputVector = new Vector2(directionVector.x, directionVector.z);
            //DoDirectionalMovement(inputVector.normalized);
            // Vector3 targetPos = nearbyPlayer.transform.position;
            // raycast downward to find the ground.
            bool rayHit = Physics.Raycast(targetPos, Vector3.down, out RaycastHit hitInfo, 10f, LayerMask.GetMask("Default"));
            if (rayHit)
            {
                targetPos = hitInfo.point;
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    mPathCorners = path.corners;
                    // agent.SetDestination();
                    directionVector = (path.corners[1] - transform.position);
                    Vector2 v = new Vector2(directionVector.x, directionVector.z);
                    // Stop if close enough
                    //if (v.sqrMagnitude < 0.01f)
                    //{
                    //    v = Vector2.zero;
                    //} else
                    //{
                        v = v.normalized;
                    //}
                    DoDirectionalMovement(v);
                    // Debug.Log(v);
                    // Debug.Log(path.corners[1]);
                    Debug.Log(mInAir);
                }
                else
                {
                    Debug.LogWarning(path.status);
                }
            }
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

    private void OnDrawGizmos()
    {
        if (mPathCorners == null)
        {
            return;
        }
        for (int i = 1; i < mPathCorners.Length; i++)
        {
            Vector3 prevPoint = mPathCorners[i-1];
            Vector3 nextPoint = mPathCorners[i];
            Gizmos.DrawLine(prevPoint, nextPoint);
        }
        Gizmos.DrawSphere(mPathCorners[1], 0.05f);
    }
}
