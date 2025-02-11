﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=9A9yj8KnM8c
public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }

    public IEnumerator ZoomOut(float duration, float zoomSpeed)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            GetComponent<Camera>().orthographicSize += zoomSpeed * Time.deltaTime;

            elapsed += Time.deltaTime;

            yield return null;
        }
    }

}