using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionEffect : MonoBehaviour
{
    [Serializable]
    public struct EmotionMaterial
    {
        public Emotion emotion;
        public Material material;
    }

    public EmotionMaterial[] EmotionMaterialMap;

    public void Start()
    {
        // TODO - call this from player or cat.
        SetEmotion(Emotion.NEUTRAL);
    }

    public void SetEmotion(Emotion e)
    {
        foreach (EmotionMaterial em in EmotionMaterialMap)
        {
            if (em.emotion == e)
            {
                Material m = em.material;
                GetComponent<MeshRenderer>().enabled = (m != null);
                GetComponent<MeshRenderer>().materials = new Material[] { m };
            }
        }
    }
}
