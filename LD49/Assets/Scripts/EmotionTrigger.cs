using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionTrigger : MonoBehaviour
{
    public Emotion emotion;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(name + " trigger with " + other.name);
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.SetEmotion(emotion);
        }
    }
}
