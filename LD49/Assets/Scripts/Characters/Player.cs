using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public float visionRadius = 3f;
    // Player jumps when joyful
    public float happyJumpTimer = 1f;
    public float skipHeightMultiplier = 0.5f;
    private float mJumpCooldown = 0f;
    private bool mAlternateFoot = false;

    private GameObject mMoodEnvironmentEffects;
    private Vector2 mCurrentJumpDirection;

    public override void Update()
    {
        base.Update();

        if (mJumpCooldown > 0f)
        {
            mJumpCooldown -= Time.deltaTime;
        }

        mAnimator.SetBool("AlternateFoot", mAlternateFoot);
    }

    private void CheckEmotionTriggers()
    {
        Cat nearbyCat = ObjectInRangeOrNull<Cat>(visionRadius);
        Food nearbyFood = ObjectInRangeOrNull<Food>(visionRadius);
        // Angry cat - be afraid!
        if (nearbyCat != null && nearbyCat.GetEmotion() == Emotion.ANGRY)
        {
            SetEmotion(Emotion.AFRAID);
        }
        // Ignore food if already joyful. If food is consumed later, maybe change this to allow smitten for more food.
        else if (nearbyFood != null && GetEmotion() != Emotion.JOYFUL)
        {
            // The food is the child of the cat while the cat is carrying it
            bool catHasFood = nearbyFood.transform.parent != null;
            if (catHasFood)
            {
                SetEmotion(Emotion.ANGRY);
            }
            else
            {
                SetEmotion(Emotion.SMITTEN);
            }
        }
        // Joy is triggered by touching an EmotionTrigger on the food.
    }

    public override void FixedUpdate()
    {
        bool prevInAir = mInAir;
        base.FixedUpdate();
        CheckEmotionTriggers();
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal") , Input.GetAxis("Vertical"));
        if (mEmotion != Emotion.JOYFUL)
        {
            DoDirectionalMovement(inputVector);
        }
        else if (mEmotion == Emotion.JOYFUL)
        {
            if (prevInAir != mInAir && !mInAir)
            {
                mJumpCooldown = happyJumpTimer;
            }

            // totally different movement if happy and jumping
            if (CanJump())
            {
                if (!mAlternateFoot)
                {
                    // skip
                    Vector3 jumpForce = Vector3.up * jumpAccel * skipHeightMultiplier;
                    mRigidbody.velocity = Vector3.zero;
                    mRigidbody.AddForce(jumpForce);
                    
                    DoDirectionalMovement(inputVector, true);
                    mCurrentJumpDirection = inputVector;
                }
                else
                {
                    // jump
                    Vector3 jumpForce = Vector3.up * jumpAccel;
                    mRigidbody.velocity = Vector3.zero;
                    mRigidbody.AddForce(jumpForce);
                    
                    DoDirectionalMovement(inputVector, true);
                    mCurrentJumpDirection = inputVector;
                }
                
                mAlternateFoot = !mAlternateFoot;
            }
            else if (mInAir)
            {
                // make it so that we can jump onto a block that's right beside us
                DoDirectionalMovement(mCurrentJumpDirection, true);
            }
        }
    }

    protected override bool CanChangeDirection()
    {
        return !mInAir || mEmotion == Emotion.JOYFUL; // special case: can accelerate during a joyful jump
    }

    private bool ShouldJump()
    {
        return mEmotion == Emotion.JOYFUL;
    }

    private bool CanJump()
    {
        return !mInAir && mRigidbody.velocity.y <= 0f && mJumpCooldown <= 0f;
    }

    public override void SetEmotion(Emotion e)
    {
        base.SetEmotion(e);
        if (mEmotion == Emotion.JOYFUL)
        {
            mJumpCooldown = happyJumpTimer;
            mAlternateFoot = false;
        }

        // environmental vfx
        if (mMoodEnvironmentEffects != null)
        {
            mMoodEnvironmentEffects.SetActive(false);
        }

        string moodTag = mEmotion.GetMoodTag();
        if (moodTag != null)
        {
            GameObject rootObj = GameObject.FindWithTag(moodTag);
            if (rootObj != null)
            {
                mMoodEnvironmentEffects = rootObj.transform.GetChild(0).gameObject;
                mMoodEnvironmentEffects.SetActive(true);
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        BreakIfBreakableAndAngry(collision.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        BreakIfBreakableAndAngry(other.gameObject);
    }

    private void BreakIfBreakableAndAngry(GameObject other)
    {
        if (mEmotion == Emotion.ANGRY)
        {
            Breakable breakable = other.GetComponent<Breakable>();
            if (breakable != null)
            {
                breakable.Break();
            }
        }
    }

    public void StompGround()
    {
        if (mEmotion != Emotion.ANGRY) return;

        Debug.Log("STOMP");

        Camera camera = Camera.main;
        CameraShake shake = camera.gameObject.GetComponent<CameraShake>();
        StartCoroutine(shake.Shake(0.15f, 0.025f));
    }
}
