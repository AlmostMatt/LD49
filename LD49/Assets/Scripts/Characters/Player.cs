﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    private float foodVisionRadius = 3f;
    private float angryCatVisionRadius = 1.5f;
    // Player jumps when joyful
    public float happyJumpTimer = 1f; // This is effectively the jump cooldown
    private const float JUMP_QUEUED_INPUT_TIME = 0.25f; // How long before input is forgotten
    private const float JUMP_MIDAIR_TURN_GRACE_TIME = 0.15f; // How long after jumping to still allow input
    public float skipHeightMultiplier = 0.5f;
    private float mJumpCooldown = 0f;
    private bool mAlternateFoot = false;

    private GameObject mMoodEnvironmentEffects;
    private Vector2 mCurrentJumpDirection;
    private Vector3? mAttraction;
    private Vector3? mThreat;

    // Timers to allow queued and delayed inputs when jumping.
    private float mLastHInput = 0f;
    private float mLastVInput = 0f;
    private float mTimeSinceHInput = 0f;
    private float mTimeSinceVInput = 0f;
    private float mTimeSinceOnGround = 0f;
    
    public override void Update()
    {
        base.Update();

        if (mJumpCooldown > 0f)
        {
            mJumpCooldown -= Time.deltaTime;
        }

        mAnimator.SetBool("AlternateFoot", mAlternateFoot);

        if (mMoodEnvironmentEffects == null)
        {
            SetEmotion(mEmotion, true); // doesn't work in Start for some reason (wrong finds the wrong gameObject by tag?)
        }
    }

    private void CheckEmotionTriggers()
    {
        if (EmotionChangeOnCooldown())
        {
            return;
        }

        // Reset attractions and threats.
        mAttraction = null;
        mThreat = null;
        Cat nearbyCat = ObjectInRangeOrNull<Cat>(angryCatVisionRadius);
        Food nearbyFood = ObjectInRangeOrNull<Food>(foodVisionRadius);
        // Angry cat - be afraid!
        if (nearbyCat != null && nearbyCat.GetEmotion() == Emotion.ANGRY)
        {
            SetEmotion(Emotion.AFRAID);
            mThreat = nearbyCat.transform.position;
        }
        // Ignore food if already joyful. Maybe change this to allow smitten or angry for more food.
        else if (nearbyFood != null /*&& GetEmotion() != Emotion.JOYFUL*/)
        {
            // The food is the child of the cat while the cat is carrying it
            bool catHasFood = nearbyFood.transform.parent != null;
            if (catHasFood)
            {
                if (GetEmotion() != Emotion.ANGRY)
                {
                    SetEmotion(Emotion.ANGRY);
                }
                
                mAttraction = nearbyFood.transform.parent.position;
            }
            else
            {
                if (GetEmotion() != Emotion.SMITTEN)
                {
                    SetEmotion(Emotion.SMITTEN);
                }

                mAttraction = nearbyFood.transform.position;
            }
        }
        // Joy is triggered by touching an EmotionTrigger on the food.
    }

    // Returns x and z components of distance as an XY vector
    private Vector2 GetDistance2D(Vector3? other)
    {
        if (other.HasValue)
        {
            Vector3 difference = other.Value - transform.position;
            return new Vector2(difference.x, difference.z);
        }
        else
        {
            return Vector2.zero;
        }
    }

    // Return position of nearest food (or null if no food)
    public Vector3? GetSmittenTargetForFocus()
    {
        if (GetEmotion() == Emotion.SMITTEN)
        {
            Food nearbyFood = ObjectInRangeOrNull<Food>(100f);
            if (nearbyFood != null)
            {
                return nearbyFood.transform.position;
            }
        }
        return null;
    }

    // Returns a value between 0f and 1f based on proximity to attractor
    public float GetAttractionStrength(float minForceIfSmitten = 0.25f)
    {
        if (!mAttraction.HasValue)
        {
            return 0f;
        }
        Vector2 delta = GetDistance2D(mAttraction);
        float deltaDistance = delta.magnitude;
        if (GetEmotion() == Emotion.SMITTEN)
        {
            // At a distance of 0.5, pull player away with full force (1)
            // At a distance of just below visionRadius, pull with a low force (0.25)
            float minDist = 0.5f;
            float maxDist = foodVisionRadius;
            if (deltaDistance > maxDist) { return 0f;  }
            float minForce = minForceIfSmitten;
            return minForce + (1-minForce)*(Mathf.Max(minDist, maxDist - deltaDistance) - minDist) / (maxDist - minDist);
        } else if (GetEmotion() == Emotion.ANGRY)
        {
            // TODO (attraction to cat?)
            return 0f;
        }
        return 0f;
    }

    // Returns a value between 0f and 1f based on proximity to threat
    private float GetThreatStrength()
    {
        if (!mThreat.HasValue)
        {
            return 0f;
        }
        // The only possible threat (for a player) is a cat.
        // TODO (fear of cat)

        // At a distance of 0, push player away equal to their accel
        // At a distance of visionRadius, do not push at all (0)
        Vector2 delta = GetDistance2D(mThreat);
        float threatDistance = delta.magnitude;
        float angryCatRepulsionRadius = 3f;
        float repulsionFromDistance = Mathf.Max(0f, angryCatRepulsionRadius - threatDistance) / angryCatRepulsionRadius;
        // TODO (linear or quadratic interpolation based on proximity and parameters)
        return repulsionFromDistance;
    }

    public override void FixedUpdate()
    {
        bool prevInAir = mInAir;
        base.FixedUpdate();
        CheckEmotionTriggers();
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // Remember buffered inputs for queued jump inputs
        if (inputVector.x != 0f)
        {
            mTimeSinceHInput = 0f;
            mLastHInput = inputVector.x;
        }
        if (inputVector.y != 0f)
        {
            mTimeSinceVInput = 0f;
            mLastVInput = inputVector.y;
        }
        if (mEmotion != Emotion.JOYFUL)
        {
            DoDirectionalMovement(inputVector);
        }
        else if (mEmotion == Emotion.JOYFUL)
        {
            // Construct a buffered input vector based on recent user input
            Vector2 bufferedInputVector = Vector2.zero;
            if (mTimeSinceHInput < JUMP_QUEUED_INPUT_TIME)
            {
                bufferedInputVector.x = mLastHInput;
            }
            if (mTimeSinceVInput < JUMP_QUEUED_INPUT_TIME)
            {
                bufferedInputVector.y = mLastVInput;
            }
            //Debug.Log(bufferedInputVector);
            // Use the bufferred input vector if there is no current input, but there was input recently.

            if (prevInAir != mInAir && !mInAir)
            {
                mJumpCooldown = happyJumpTimer;
                mRigidbody.velocity = Vector3.zero;
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
                }
                else
                {
                    // jump
                    Vector3 jumpForce = Vector3.up * jumpAccel;
                    mRigidbody.velocity = Vector3.zero;
                    mRigidbody.AddForce(jumpForce);
                }

                mCurrentJumpDirection = inputVector;
                if (inputVector.magnitude == 0)
                {
                    mCurrentJumpDirection = bufferedInputVector;
                }
                DoDirectionalMovement(mCurrentJumpDirection, true);
                mAlternateFoot = !mAlternateFoot;
            }
            else if (mInAir)
            {
                // Allow change of direction for a small amount of time
                if (inputVector.magnitude > 0f && mTimeSinceOnGround < JUMP_MIDAIR_TURN_GRACE_TIME)
                {
                    mCurrentJumpDirection = inputVector;
                }
                // make it so that we can jump onto a block that's right beside us
                DoDirectionalMovement(mCurrentJumpDirection, true);
            }
        }
        // Push and pull from attraction and threats
        if (mThreat.HasValue)
        {
            float threatStrength = GetThreatStrength();
            Vector3 awayFromThreat = transform.position - mThreat.Value;
            awayFromThreat.y = 0;
            // Afraid of cat
            float repulsionAmount = 1f * threatStrength;
            mRigidbody.AddForce(repulsionAmount * GetAccel() * awayFromThreat.normalized);
        }
        if (mAttraction.HasValue)
        {
            float attractionStrength = GetAttractionStrength();
            Vector3 towardAttraction = mAttraction.Value - transform.position;
            towardAttraction.y = 0;
            // Smitten with bun
            if (GetEmotion() == Emotion.SMITTEN)
            {
                float attrAmount = 0.95f * attractionStrength;
                mRigidbody.AddForce(attrAmount * GetAccel() * towardAttraction.normalized);
            }
            // Mad at cat with bun
            if (GetEmotion() == Emotion.ANGRY)
            {
                // For now this has no effect on speed.
            }
        }
        // Don't exceed max speed (relevant for the push force from nearby angry cat)
        // Also relevant for change-in-state to immediately slow down
        Vector2 v = new Vector2(mRigidbody.velocity.x, mRigidbody.velocity.z);
        float maxSpeed = GetMaxSpeed(v);
        if (v.magnitude > maxSpeed)
        {
            float oldSpeed = v.magnitude;
            v = maxSpeed * v.normalized;
            float newSpeed = v.magnitude;
            mRigidbody.velocity = new Vector3(v.x, mRigidbody.velocity.y, v.y);
            // Debug.Log("Capping speed. Was " + oldSpeed + ", now " + newSpeed);
        }
        // Update timers for buffered and delayed inputs
        mTimeSinceHInput += Time.fixedDeltaTime;
        mTimeSinceVInput += Time.fixedDeltaTime;
        if (mInAir)
        {
            mTimeSinceOnGround += Time.fixedDeltaTime;
        } else
        {
            mTimeSinceOnGround = 0f;
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

    protected override float GetMaxSpeed(Vector2 inDirection)
    {
        // Per-emotion speed comes from the base method
        float baseSpeed = base.GetMaxSpeed(inDirection);
        /**
        // Speed modifiers based on nearby cats or buns are added here.
        if (mAttraction.HasValue)
        {
            Vector2 attractionDirection = GetDistance2D(mAttraction);
            float towardsAttractionCosTheta = Vector2.Dot(inDirection.normalized, attractionDirection.normalized);
            float attractionStrength = GetAttractionStrength();
            if (GetEmotion() == Emotion.SMITTEN)
            {
                // Slower speed-limit when moving away from the bun?
            } else if (GetEmotion() == Emotion.ANGRY)
            {
                // Speed boost towards cat that stole bun?
            }
        }
        if (mThreat.HasValue)
        {
            Vector2 threatDirection = GetDistance2D(mThreat);
            float towardsThreatCosTheta = Vector2.Dot(inDirection.normalized, threatDirection.normalized);
            float threatStrength = GetThreatStrength();
            //Vector2 inDirTowardThreat = Vector3.Project(inDirection, threatDirection);
            //Vector2 inDirPerpindicularToThreat = Vector3.ProjectOnPlane(inDirection, threatDirection);
            
            // Slow down when running towards dangerous cat.
            // Max speed of 0f towards the cat at a 
        **/

        if (mEmotion == Emotion.JOYFUL && mAlternateFoot)
        {
            // skipping. slow down a bit so that we cover approx. 1m
            // (skipping code says alternate foot is jump, so somehow this must get called *after* the alternate foot changes?! don't understand that though)
            baseSpeed *= 0.8f;
        }

        return baseSpeed;
    }

    // Note that this method can trigger repeatedly, even if this is already the current emotion.
    public override void SetEmotion(Emotion e, bool initial = false)
    {
        Emotion prevEmotion = mEmotion;

        base.SetEmotion(e);
        if (mEmotion == Emotion.JOYFUL)
        {
            mJumpCooldown = happyJumpTimer;
            mAlternateFoot = false;
        }

        if (!initial)
        {
            // trigger scripted level events, if any (but not if this is the emotion we started the level with)
            ScriptedLevelEvents.Trigger(mEmotion);
        }

        if (e != prevEmotion || initial)
        {
            // Play audio effects for change of emotion
            if (e == Emotion.JOYFUL)
            {
                SFXPlayer.PlayAudioClip("joy_voice");
                MusicPlayer.FadeOut();
                MusicPlayer.StartPlaying("JoyTheme");
            }
            else if (e == Emotion.AFRAID)
            {
                SFXPlayer.PlayAudioClip("fear_voice");
                MusicPlayer.FadeOut();
                MusicPlayer.StartPlaying("FearTheme");
            }
            else if (e == Emotion.ANGRY)
            {
                SFXPlayer.PlayAudioClip("angry_voice");
                MusicPlayer.FadeOut();
                MusicPlayer.StartPlaying("AngryTheme");
            }
            else if (e == Emotion.SMITTEN)
            {                    
                SFXPlayer.PlayAudioClip("smitten_voice");
                MusicPlayer.FadeOut();
                MusicPlayer.StartPlaying("SmittenTheme");
            }
        }

        // environmental vfx
        GameObject newMoodEnvEffects = null;
        string moodTag = mEmotion.GetMoodTag();
        if (moodTag != null)
        {
            GameObject rootObj = GameObject.FindWithTag(moodTag);
            if (rootObj != null)
            {
                // Turn on new mood effects
                newMoodEnvEffects = rootObj.transform.GetChild(0).gameObject;
                newMoodEnvEffects.SetActive(true);
            }
        }
        // Turn off old mood effects and update the current mood effects variable
        if (mMoodEnvironmentEffects != null && mMoodEnvironmentEffects != newMoodEnvEffects)
        {
            mMoodEnvironmentEffects.SetActive(false);
        }
        mMoodEnvironmentEffects = newMoodEnvEffects;
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

        Camera camera = Camera.main;
        SFXPlayer.PlayAudioClip("stomp");
        CameraShake shake = camera.gameObject.GetComponent<CameraShake>();
        StartCoroutine(shake.Shake(0.15f, 0.025f));
    }
}
