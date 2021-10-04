using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedWallFall : MonoBehaviour
{
    void OnTriggerLevelEvent()
    {
        GetComponent<Animator>().SetTrigger("Fall");
    }
}
