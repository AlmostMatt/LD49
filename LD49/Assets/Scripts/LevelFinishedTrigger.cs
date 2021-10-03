using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinishedTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Level finished!");
            LevelManager.LevelFinished();
        }
    }

}
