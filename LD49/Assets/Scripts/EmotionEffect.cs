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

    private float mInitialRotation;
    private Vector3 mInitialScale;

    public void Start()
    {
        // TODO - call this from player or cat.
        SetEmotion(Emotion.NEUTRAL);
        mInitialRotation = transform.localEulerAngles.z;
        mInitialScale = transform.localScale;
    }

    public void Update()
    {
        // Rotate over time
        float wavinessAmount = 5f; // in degrees
        float wavinessSpeed = 5f;
        transform.localEulerAngles = new Vector3(
            0f, 0f, mInitialRotation + (wavinessAmount * Mathf.Sin(wavinessSpeed * Time.time)));

        // Pulse in size over time
        wavinessAmount = 0.15f; // as a multiple of the base scale
        wavinessSpeed = 5f;
        transform.localScale = (1f + (wavinessAmount * Mathf.Cos(wavinessSpeed * Time.time))) * mInitialScale;
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
