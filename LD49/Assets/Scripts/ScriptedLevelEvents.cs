using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedLevelEvents : MonoBehaviour
{
    private static ScriptedLevelEvents sSingleton;

    public List<GameObject> angryTriggers = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        sSingleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void _Trigger(Emotion e)
    {
        switch(e)
        {
            case Emotion.ANGRY:
                foreach(GameObject go in angryTriggers)
                {
                    go.BroadcastMessage("OnTriggerLevelEvent");
                }
                break;
            default:
                break;
        }
    }

    public static void Trigger(Emotion e)
    {
        if (sSingleton != null) sSingleton._Trigger(e);
    }
}
