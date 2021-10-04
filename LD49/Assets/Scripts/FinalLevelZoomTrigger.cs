using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalLevelZoomTrigger : MonoBehaviour
{
    private bool mFired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (mFired) return;

        Debug.Log("final!");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("zoom!");
            Camera camera = Camera.main;
            CameraShake shake = camera.gameObject.GetComponent<CameraShake>();
            StartCoroutine(shake.ZoomOut(2.5f, 1f));
            mFired = true;

            GameObject fx = GameObject.FindGameObjectWithTag("JoyfulEnvFx");
            Transform toggle = fx.transform.GetChild(0);
            toggle.gameObject.SetActive(true);
            Transform child = toggle.Find("ParticleSystem");
            child.localScale = new Vector3(1.5f, 1.5f, 1);
            /*
            ParticleSystem ps = child.GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.scale = Vector3.Scale(new Vector3(2.5f, 2.5f, 1f), shape.scale);
            */
        }
    }
}
