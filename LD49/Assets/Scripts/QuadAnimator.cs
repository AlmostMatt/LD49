using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadAnimator : MonoBehaviour
{
    public int numSprites = 1;

    [HideInInspector]
    public int spriteIdx = 0;

    private int mPrevSpriteIdx;
    
    private Mesh mMesh;

    Vector2[] uvs = new Vector2[4];

    // Start is called before the first frame update
    void Start()
    {
        mMesh = GetComponent<MeshFilter>().mesh;

        float u = 1f / numSprites;
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(u, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(u, 1);

        mPrevSpriteIdx = spriteIdx;
    }

    // Update is called once per frame
    void Update()
    {
        if(spriteIdx != mPrevSpriteIdx)
        {
            uvs[0].x = uvs[2].x = (float)spriteIdx / numSprites;
            uvs[1].x = uvs[3].x = uvs[0].x + (1f / numSprites);
            mMesh.SetUVs(0, uvs);
            mPrevSpriteIdx = spriteIdx;
        }
    }
}
