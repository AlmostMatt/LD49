using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGate : MonoBehaviour
{    
    public float timeToOpen = 1f;
    public float timeToStayOpen = 1f;
    public float timeToClose = 1f;

    public float maxHeight = 2.2f;

    private Transform mGate;
    private float mStayOpenTimer = 0f;
    private float mOpenAmount = 0f;
    private float mOpenSpeed = 0f;
    private BoxCollider mGateCollider;
    private int mGateCollisionMask;

    void Start()
    {
        mGate = transform.Find("Gate");
        mGateCollider = mGate.GetComponent<BoxCollider>();
        mGateCollisionMask = LayerMask.GetMask("Player", "Cat", "Bun");
    }

    void FixedUpdate()
    {
        if (mStayOpenTimer <= 0f)
        {
            if (mOpenSpeed != 0f)
            {
                float nextOpenAmount = mOpenAmount + mOpenSpeed * Time.fixedDeltaTime;
                bool done = false;
                if (nextOpenAmount <= 0f)
                {
                    nextOpenAmount = 0f;
                    done = true;
                }
                else if (nextOpenAmount >= 1f)
                {
                    nextOpenAmount = 1f;
                    done = true;
                }

                float y = Mathf.Lerp(0f, maxHeight, nextOpenAmount);
                float deltaY = y - mGate.transform.localPosition.y;

                if (!Physics.BoxCast(mGate.transform.position, Vector3.Scale(mGate.transform.localScale, mGateCollider.size) * 0.5f, Vector3.up * Mathf.Sign(deltaY), mGate.transform.rotation, Mathf.Abs(deltaY), mGateCollisionMask))
                {
                    mOpenAmount = nextOpenAmount;
                    mGate.transform.localPosition = new Vector3(0, y, 0);
                    if (done)
                    {
                        mOpenSpeed = 0f;
                    }
                }
            }
        }
        else
        {
            mStayOpenTimer -= Time.fixedDeltaTime;
        }
    }

    void OnTriggerButtonPressed()
    {
        if (timeToOpen <= 0f)
        {
            mGate.transform.localPosition = new Vector3(0, maxHeight, 0);
        }
        else
        {
            mOpenSpeed = 1 / timeToClose;
        }
    }

    void OnTriggerButtonReleased()
    {
        if (timeToClose <= 0f)
        {
            mGate.transform.localPosition = new Vector3(0, 0, 0);
        }
        else
        {
            if (mOpenAmount >= 1f)
            {
                mStayOpenTimer = timeToStayOpen;
            }

            mOpenSpeed = -1 / timeToClose;
        }
    }
}
