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

    public Transform horizontalFlipGroup; // any stuff that should be mirrored when moving left

    protected Emotion mEmotion = Emotion.NEUTRAL;
    protected Rigidbody mRigidbody;
    protected bool mInAir = false;

    private float mCapsuleCenterToFeet;
    private CapsuleCollider mCapsule;

    private FacingDirection mFacingDirection = FacingDirection.EAST;
    protected Animator mAnimator;
    private QuadAnimator mQuadAnimator;
    private bool mFlipX = false;

    private string[] OBJECT_IN_RANGE_LAYERS = { "Cat", "Player", "Collectible" };

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
            mAnimator.SetInteger("FacingDirection", (int)mFacingDirection);
        }
    }

    public virtual void FixedUpdate()
    {
        // in air check
        mInAir = true;
        float capsuleHalfHeight = (mCapsule.height * 0.5f); // height is the entire capsule
        RaycastHit[] onGroundHits = Physics.SphereCastAll(transform.position, mCapsule.radius, Vector3.down, capsuleHalfHeight - mCapsule.radius + 0.01f, LayerMask.GetMask("Default"));
        foreach(RaycastHit hitInfo in onGroundHits)
        {
            if (hitInfo.collider.gameObject == gameObject) continue;

            if (Vector3.Dot(hitInfo.normal, Vector3.up) > 0.3f) // don't count as on ground unless the collision is flat enough
            {
                mInAir = false;
                break;
            }
        }

        // TODO - add mAnimator and mQuadAnimator to cat so that it can rotate like player
        if (mAnimator != null)
        {
            mAnimator.SetFloat("Speed", mRigidbody.velocity.magnitude);
            mAnimator.SetBool("Falling", mRigidbody.velocity.y < 0f);
        }

        if (mRigidbody.velocity.x != 0)
        {
            bool flipX = mRigidbody.velocity.x < 0;
            if (mFlipX != flipX)
            {
                if (mQuadAnimator != null)
                {
                    mQuadAnimator.flipX = flipX;
                }

                mFlipX = flipX;

                if (horizontalFlipGroup != null)
                {
                    horizontalFlipGroup.localScale = new Vector3(flipX ? -1 : 1, 1, 1);
                }
            }
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

    protected virtual bool CanChangeDirection()
    {
        return !mInAir;
    }

    // Cat and Player can override this
    protected virtual float GetMaxSpeed()
    {
        return mEmotion.GetMaxSpeed();
    }

    private float GetAccel()
    {
        return normalMoveAccel * mEmotion.GetAccelModifier();
    }

    private void ChangeDirection(FacingDirection newDir)
    {
        mFacingDirection = newDir;
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
            transform.position, radius, LayerMask.GetMask(OBJECT_IN_RANGE_LAYERS), QueryTriggerInteraction.Collide))
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
