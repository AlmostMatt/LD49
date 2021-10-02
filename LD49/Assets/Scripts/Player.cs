using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float normalMoveMaxSpeed = 5f;
    public float normalMoveAccel = 50f;

    enum FacingDirection
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    private Vector3[] FACING_VECTORS =
    {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left
    };

    private FacingDirection mFacingDirection = FacingDirection.EAST;
    private Rigidbody mRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        FacingDirection desiredFacingDirection = mFacingDirection;

        if (horz != 0)
        {
            if (horz > 0)
            {
                desiredFacingDirection = FacingDirection.EAST;
            }
            else
            {
                desiredFacingDirection = FacingDirection.WEST;
            }
        }

        if (vert != 0)
        {
            if (vert > 0)
            {
                desiredFacingDirection = FacingDirection.NORTH;
            }
            else
            {
                desiredFacingDirection = FacingDirection.SOUTH;
            }
        }

        if (desiredFacingDirection != mFacingDirection)
        {
            ChangeDirection(desiredFacingDirection);
        }


        Vector3 desiredDir = new Vector3(horz, 0, vert);
        Vector3 desiredVelocity = Vector3.zero;
        if (desiredDir.magnitude > 0)
        {
            desiredVelocity = GetMaxSpeed() * desiredDir.normalized;
        }

        Vector3 deltaV = desiredVelocity - mRigidbody.velocity;
        if (deltaV.magnitude > 0)
        {
            float accelNeeded = deltaV.magnitude / Time.fixedDeltaTime;
            Vector3 movementForce = deltaV.normalized * Mathf.Min(accelNeeded, GetAccel());
            mRigidbody.AddForce(movementForce);
        }
    }

    private float GetMaxSpeed()
    {
        // todo: emotion check
        return normalMoveMaxSpeed;
    }

    private float GetAccel()
    {
        // todo: emotion check
        return normalMoveAccel;
    }

    private void ChangeDirection(FacingDirection newDir)
    {
        mFacingDirection = newDir;
        // transform.rotation = Quaternion.Euler(FACING_VECTORS[(int)newDir]); // not needed? if movement is not relative to facing direction
    }
}
