using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum FacingDirection
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    protected Vector3[] FACING_VECTORS =
    {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left
    };

    public float normalMoveMaxSpeed = 5f;
    public float normalMoveAccel = 50f;
    public float jumpAccel = 10f;

    protected Emotion mEmotion = Emotion.NEUTRAL;
    protected Rigidbody mRigidbody;
    protected bool mInAir = false;

    private float mCapsuleCenterToFeet;
    private CapsuleCollider mCapsule;

    private FacingDirection mFacingDirection = FacingDirection.EAST;
    private Animator mAnimator;
    private QuadAnimator mQuadAnimator;

    // Start is called before the first frame update
    void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();
        mCapsule = GetComponent<CapsuleCollider>();
        mAnimator = GetComponent<Animator>();
        mQuadAnimator = GetComponentInChildren<QuadAnimator>();
    }

    public virtual void Update()
    {
        // TODO - add animator to cat
        if (mAnimator != null) {
            mAnimator.SetInteger("Emotion", (int)mEmotion);
            mAnimator.SetBool("InAir", mInAir);
        }
    }

    public virtual void FixedUpdate()
    {
        // in air check
        mInAir = true;
        RaycastHit hitInfo;
        float capsuleHalfHeight = (mCapsule.height * 0.5f); // height includes radius?
        if (Physics.SphereCast(transform.position, mCapsule.radius, Vector3.down, out hitInfo, capsuleHalfHeight + 0.01f))
        {
            mInAir = false;
        }

        // TODO - add mAnimator and mQuadAnimator to cat so that it can rotate like player
        if (mAnimator != null)
        {
            mAnimator.SetFloat("Speed", mRigidbody.velocity.magnitude);
        }
        if (mQuadAnimator != null)
        {
            mQuadAnimator.flipX = mRigidbody.velocity.x < 0;
        }
    }

    protected void DoDirectionalMovement(Vector2 inputVector, bool instantAcceleration=false)
    {
        float horz = inputVector.x;
        float vert = inputVector.y;

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

        if (CanChangeDirection())
        {
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

            Vector3 xzVelocity = mRigidbody.velocity;
            xzVelocity.y = 0;
            Vector3 deltaV = desiredVelocity - xzVelocity;
            if (deltaV.magnitude > 0)
            {
                float accelNeeded = deltaV.magnitude / Time.fixedDeltaTime;
                float actualAccel = instantAcceleration ? accelNeeded : Mathf.Min(accelNeeded, GetAccel());
                Vector3 movementForce = deltaV.normalized * actualAccel;
                mRigidbody.AddForce(movementForce);
            }
        }
    }

    private bool CanChangeDirection()
    {
        return !mInAir;
    }

    private float GetMaxSpeed()
    {
        return mEmotion.GetMaxSpeed();
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

    public virtual void SetEmotion(Emotion e)
    {
        mEmotion = e;
        foreach (EmotionEffect effect in GetComponentsInChildren<EmotionEffect>())
        {
            effect.SetEmotion(e);
        }
    }

    public Emotion GetEmotion()
    {
        return mEmotion;
    }

    // This can be used to find nearby cats, players, or meatbuns
    public T ObjectInRangeOrNull<T>(float radius) where T : MonoBehaviour
    {
        T closestT = null;
        foreach (Collider other in Physics.OverlapSphere(
            transform.position, radius, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        {
            T otherT = other.GetComponent<T>();
            if (otherT != null)
            {
                if (closestT == null ||
                    (otherT.transform.position - transform.position).sqrMagnitude
                    < (closestT.transform.position - transform.position).sqrMagnitude)
                {
                    closestT = otherT;
                }
            }
        }
        return closestT;
    }
}
