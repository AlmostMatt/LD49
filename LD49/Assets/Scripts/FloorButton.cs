using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorButton : MonoBehaviour
{
    public List<GameObject> objectsToTrigger = new List<GameObject>();

    private Transform mButtonObject;
    private Vector3 mOriginalButtonPos;
    private int mNumPressing = 0;

    private void Start()
    {
        mButtonObject = transform.GetChild(0);
        mOriginalButtonPos = mButtonObject.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mNumPressing == 0)
        {
            foreach(GameObject go in objectsToTrigger)
            {
                go.BroadcastMessage("OnTriggerButtonPressed");
            }

            mButtonObject.localPosition = new Vector3(0, 0, 0);
        }
        
        ++mNumPressing;
    }

    private void OnTriggerExit(Collider other)
    {
        --mNumPressing;

        if (mNumPressing == 0)
        {
            foreach(GameObject go in objectsToTrigger)
            {
                go.BroadcastMessage("OnTriggerButtonReleased");
            }

            mButtonObject.localPosition = mOriginalButtonPos;
        }
    }
}
