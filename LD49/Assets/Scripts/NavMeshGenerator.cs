using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// This is a script so that I can be lazy about not baking navmesh in the editor
public class NavMeshGenerator : MonoBehaviour
{
    public bool generateAtRuntime = false;

    void Start()
    {
        if (generateAtRuntime)
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }
}
