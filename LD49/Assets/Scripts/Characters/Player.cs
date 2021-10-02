using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    // Player jumps when joyful
    public float happyJumpTimer = 1f;
    private float mJumpCooldown = 0f;

    // Update is called once per frame
    void Update()
    {
        if (mJumpCooldown > 0f)
        {
            mJumpCooldown -= Time.deltaTime;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal") , Input.GetAxis("Vertical"));
        if (mEmotion != Emotion.HAPPY)
        {
            DoDirectionalMovement(inputVector);
        }
        else if (mEmotion == Emotion.HAPPY)
        {
            // totally different movement if happy and jumping
            if (CanJump())
            {
                Vector3 jumpForce = Vector3.up * jumpAccel;
                mRigidbody.velocity = Vector3.zero;
                mRigidbody.AddForce(jumpForce);
                mJumpCooldown = happyJumpTimer;

                DoDirectionalMovement(inputVector);
            }
        }
    }

    private bool ShouldJump()
    {
        return mEmotion == Emotion.HAPPY || Input.GetButton("Jump"); // TODO - remove the input button
    }

    private bool CanJump()
    {
        return !mInAir && mRigidbody.velocity.y <= 0f && mJumpCooldown <= 0f;
    }

    public override void SetEmotion(Emotion e)
    {
        base.SetEmotion(e);
        if (mEmotion == Emotion.HAPPY)
        {
            mJumpCooldown = happyJumpTimer;
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
}
