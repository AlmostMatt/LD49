using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cat : Character
{
    public float visionRadius = 3f; // Other object collider radius is added to this
    public float fleeRadius = 4f; // Does not include other object collider radius.
    public Transform foodAttachPoint;

    private Food mCurrentlyHeldFood = null;
    private bool mRecentlyDroppedFood = false;
    private NavMeshPath mPath = null;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        NavMeshPath desiredPath = HandleEmotionTriggersAndGetPath();
        if (mEmotion != Emotion.JOYFUL && desiredPath != null)
        {
            FollowPath(desiredPath);
        }
    }

    private NavMeshPath HandleEmotionTriggersAndGetPath()
    {
        // Emotion triggers and movement
        // Only notice nearby food if not already holding food.
        Food nearbyFood = null;
        if (mCurrentlyHeldFood == null && !mRecentlyDroppedFood)
        {
            nearbyFood = ObjectInRangeOrNull<Food>(visionRadius);
        }
        Player nearbyPlayer = ObjectInRangeOrNull<Player>(visionRadius);
        // If recently dropped food, run away!
        if (mRecentlyDroppedFood)
        {
            if (nearbyPlayer != null)
            {
                // Run away!
                SetEmotion(Emotion.AFRAID);
                return FindPathFleeingPoint(nearbyPlayer.transform.position, fleeRadius);
            }
            else
            {
                // After running away, feel free to do anything
                mRecentlyDroppedFood = false;
                SetEmotion(Emotion.NEUTRAL);
            }
        }
        // Angry player - run away! (keep running if food dropped recently)
        if (nearbyPlayer != null && nearbyPlayer.GetEmotion() == Emotion.ANGRY)
        {
            // Run away!
            SetEmotion(Emotion.AFRAID);
            return FindPathFleeingPoint(nearbyPlayer.transform.position, fleeRadius);
        }
        else if (nearbyFood != null)
        {
            // Go get the food!
            SetEmotion(Emotion.SMITTEN);
            return FindPathToPoint(nearbyFood.transform.position);
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
            return FindPathToPoint(nearbyPlayer.transform.position);
        }
        else
        {
            // Cat resets to neutral when nothing is happening.
            SetEmotion(Emotion.NEUTRAL);
        }
        // Default to being lazy
        return null;
    }

    private NavMeshPath FindPathToPoint(Vector3 targetPos)
    {
        // Raycast downward from above the target to find the ground for that position.
        targetPos.y += 10f;
        bool rayHit = Physics.Raycast(targetPos, Vector3.down, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Default"));
        if (rayHit)
        {
            targetPos = hitInfo.point;
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path);
            if (path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1)
            {
                return path;
            }
            else
            {
                Debug.LogWarning(path.status);
            }
        }
        else
        {
            Debug.LogWarning("No nearby ground for Cat movement target.");
        }
        return null;
    }

    private NavMeshPath FindPathFleeingPoint(Vector3 threatPos, float desiredRadius)
    {
        // Try a bunch points that are the desired radius away from the threat.
        NavMeshPath bestPath = null;
        float bestPathLength = 0f;
        int numPoints = 16;
        for (int i=0; i<numPoints; i++)
        {
            float cosx = Mathf.Cos((2 * Mathf.PI * i) / numPoints);
            float cosy = Mathf.Sin((2 * Mathf.PI * i) / numPoints);
            Vector3 desiredPos = threatPos + (desiredRadius * new Vector3(cosx, 0f, cosy));
            NavMeshPath possiblePath = FindPathToPoint(desiredPos);
            if (possiblePath != null)
            {
                float pathLen = GetLengthOfPath(possiblePath);
                if (bestPath == null || pathLen < bestPathLength)
                {
                    bestPath = possiblePath;
                    bestPathLength = pathLen;
                }
            }
        }
        return bestPath;
    }

    private void FollowPath(NavMeshPath path)
    {
        mPath = path;
        Vector3 directionVector = (path.corners[1] - transform.position);
        Vector2 moveVector = new Vector2(directionVector.x, directionVector.z);
        // Stop if close enough (not sure if this is needed or relevant)
        if (moveVector.sqrMagnitude < 0.01f)
        {
            moveVector = Vector2.zero;
        } else
        {
            moveVector = moveVector.normalized;
        }
        DoDirectionalMovement(moveVector);
    }

    private float GetLengthOfPath(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 prevPoint = path.corners[i - 1];
            Vector3 nextPoint = path.corners[i];
            length += (nextPoint - prevPoint).magnitude;
        }
        return length;
    }

    private void PickupFood(Food food)
    {
        if (mCurrentlyHeldFood != null || mRecentlyDroppedFood)
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
            mRecentlyDroppedFood = true;
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
        /*
         * Draw potential paths for fleeing cat
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan};
        Player nearbyPlayer = ObjectInRangeOrNull<Player>(visionRadius);
        if (nearbyPlayer != null)
        {
            float desiredRadius = visionRadius * 1.5f;
            int numPoints = 16;
            for (int i = 0; i < numPoints; i++)
            {
                Gizmos.color = colors[i % colors.Length];
                float cosx = Mathf.Cos((2 * Mathf.PI * i) / numPoints);
                float cosy = Mathf.Sin((2 * Mathf.PI * i) / numPoints);
                Vector3 desiredPos = nearbyPlayer.transform.position + (desiredRadius * new Vector3(cosx, 0f, cosy));
                NavMeshPath possiblePath = FindPathToPoint(desiredPos);
                if (possiblePath != null)
                {
                    DrawPath(possiblePath);
                    Gizmos.DrawSphere(desiredPos, 0.1f);
                }
            }
        }
        */
        Gizmos.color = Color.white;
        DrawPath(mPath);
    }

    private void DrawPath(NavMeshPath path)
    {
        if (path == null || path == null || path.corners.Length < 2)
        {
            return;
        }
        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 prevPoint = path.corners[i - 1];
            Vector3 nextPoint = path.corners[i];
            Gizmos.DrawLine(prevPoint, nextPoint);
            Gizmos.DrawSphere(path.corners[i], 0.05f);
        }
    }

    protected override float GetMaxSpeed(Vector2 inDirection)
    {
        // TODO - maybe make cat speed vary based on emotion.
        return 3f;
    }
}
