using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmittenFocus : MonoBehaviour
{
    void Update()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            Player p = playerObj.GetComponent<Player>();
            Vector3? smittenTarget = p.GetSmittenTargetForFocus();
            if (smittenTarget.HasValue)
            {
                Vector3 camOffset = GetOffsetRelativeToCameraForPoint(smittenTarget.Value);
                transform.localPosition = camOffset;
                Color col = GetComponent<SpriteRenderer>().color;
                col.a = 0.1f; // Mathf.Min(0.2f, 1.2f * p.GetAttractionStrength(0f));
                GetComponent<SpriteRenderer>().color = col;
            }
            else
            {
                Color col = GetComponent<SpriteRenderer>().color;
                col.a = 0f;
                GetComponent<SpriteRenderer>().color = col;
            }
        }
    }

    // returns screen x/y for an object from -0.5 to 0.5
    private Vector2 GetViewportPointForPoint(Vector3 point)
    {
        Camera cam = GetComponentInParent<Camera>();
        Vector3 screenPos = cam.WorldToViewportPoint(point);
        // Bottom-left is 0,0 and Top-right is 1,1
        // Z is distance from camera
        return (Vector2)screenPos - new Vector2(0.5f,0.5f);
    }

    // returns how a child of the camera should be locally offset to line up with an object in world space
    private Vector3 GetOffsetRelativeToCameraForPoint(Vector3 point)
    {
        Camera cam = GetComponentInParent<Camera>();
        Vector2 viewPortPos = GetViewportPointForPoint(point);
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        return new Vector3(viewPortPos.x * width, viewPortPos.y * height, 0f);
    }
}
