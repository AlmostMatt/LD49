using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButton : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }
    }

    // Triggered by R or by reset button click
    public void ResetLevel()
    {
        LevelManager.RestartCurrentLevel();
    }
}
