using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadAnimator : MonoBehaviour
{
    public int numSprites = 1;

    [HideInInspector]
    public int spriteIdx = 0;

    private int mPrevNumSprites;
    private int mPrevSpriteIdx;
    
    private Mesh mMesh;
    
    Vector2[] uvs = new Vector2[4];

    void Start()
    {
        mMesh = GetComponent<MeshFilter>().mesh;

        float u = 1f / numSprites;
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(u, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(u, 1);
        mMesh.SetUVs(0, uvs);
        mPrevSpriteIdx = spriteIdx;
        mPrevNumSprites = numSprites;
    }

    void LateUpdate() 
    {
        // if the material changes, unity seems to take one frame before it actually uses the new one
        // but it immediately uses any changes to the uvs.
        // so this is in late update so that the uv changes don't take effect until next frame, which is when the new material should be in use as well.
        if ((spriteIdx != mPrevSpriteIdx) || (numSprites != mPrevNumSprites))
        {
            spriteIdx = Mathf.Min(spriteIdx, numSprites - 1);
            uvs[0].x = uvs[2].x = (float)spriteIdx / numSprites;
            uvs[1].x = uvs[3].x = uvs[0].x + (1f / numSprites);
            mMesh.SetUVs(0, uvs);
            mPrevSpriteIdx = spriteIdx;
            mPrevNumSprites = numSprites;
        }
    }
}
