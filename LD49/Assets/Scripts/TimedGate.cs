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

    void Start()
    {
        mGate = transform.Find("Gate");
    }

    void FixedUpdate()
    {
        if (mStayOpenTimer <= 0f)
        {
            if (mOpenSpeed != 0f)
            {
                mOpenAmount += mOpenSpeed * Time.fixedDeltaTime;
            
                if (mOpenAmount <= 0f)
                {
                    mOpenAmount = 0f;
                    mOpenSpeed = 0f;
                }
                else if (mOpenAmount >= 1f)
                {
                    mOpenAmount = 1f;
                    mOpenSpeed = 0f;
                }

                float y = Mathf.Lerp(0f, maxHeight, mOpenAmount);
                mGate.transform.localPosition = new Vector3(0, y, 0);
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
