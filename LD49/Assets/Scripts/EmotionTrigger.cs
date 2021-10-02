using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionTrigger : MonoBehaviour
{
    public Emotion emotion;
    public bool oneTimeUse = false;

    public void Start()
    {
        // The presence of a start (or update) method is necessary so that the component can be disabled.
        // Since enable/disable normally only affects start/update methods.
        // but I call isActiveAndEnabled below
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) {
            return;
        }

        // Debug.Log(name + " trigger with " + other.name);
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.SetEmotion(emotion);

            if (oneTimeUse)
            {
                Destroy(gameObject);
            }
        }
    }
}
