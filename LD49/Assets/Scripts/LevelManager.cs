using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private enum SceneLoadState
    {
        SLS_LOADED,
        SLS_NEXT_SCENE_PENDING,
        SLS_LOADING,
        SLS_UNLOADING,
        SLS_ERROR
    };

    public bool hubMode = false;
    public string hubLevel;
    public List<string> scenes = new List<string>();
    private int mCurrentLevelIdx = -1;
    private float mNextLevelTimer = 0;
    private string mPendingLevel = null;
    private string mCurrentLevel = null;

    private SceneLoadState mSceneLoadState = SceneLoadState.SLS_LOADED;
    private AsyncOperation mCurrentLoadOp;

    private static LevelManager sLevelManagerSingleton;

    private Camera mOpeningLevelCamera;

    void Awake()
    {
        if (hubMode)
        {
            GoToLevelInternal(hubLevel, 0.01f);
        }
        else
        {
            mOpeningLevelCamera = Camera.main;
            LevelFinishedInternal(); // load the next one (which is the first one)
        }
    }

    public static LevelManager GetSingleton()
    {
        if (sLevelManagerSingleton == null)
        {
            GameObject levelManager = GameObject.FindGameObjectWithTag("LevelManager");
            if (levelManager != null)
            {
                sLevelManagerSingleton = levelManager.GetComponent<LevelManager>();
            }
        } else
        {
            Debug.LogWarning("Unable to find level manager");
        }
        return sLevelManagerSingleton;
    }

    public static void LevelFinished()
    {
        if (GetSingleton() != null) // could still be null if running directly in a level (instead of from the main scene)
        {
            GetSingleton().LevelFinishedInternal();
        }
    }

    private void LevelFinishedInternal()
    {
        if (hubMode)
        {
            GoToLevelInternal(hubLevel, 2f);
        }
        else
        {
            if (++mCurrentLevelIdx >= scenes.Count)
            {
                Debug.Log("No more scenes to load!");
                mSceneLoadState = SceneLoadState.SLS_LOADED;
                return;
            }

            string nextLevel = scenes[mCurrentLevelIdx];
            GoToLevelInternal(nextLevel, 2);
        }
    }

    public static void RestartCurrentLevel()
    {
        if (GetSingleton() != null)
        {
            LevelManager.GoToLevel(GetSingleton().mCurrentLevel);
        }
    }

    public static void GoToLevel(string name)
    {
        if (GetSingleton() != null)
        {
            sLevelManagerSingleton.GoToLevelInternal(name, 0.2f);
        }
    }

    private void GoToLevelInternal(string name, float delay)
    {
        HUDControl.FadeToBlack();
        mPendingLevel = name;
        mSceneLoadState = SceneLoadState.SLS_NEXT_SCENE_PENDING;
        mNextLevelTimer = delay;
    }

    private void UnloadCurrentLevel()
    {
        if (mCurrentLevel == null)
        {
            LoadPendingLevel();
            return;
        }

        mSceneLoadState = SceneLoadState.SLS_UNLOADING;
        mCurrentLoadOp = SceneManager.UnloadSceneAsync(mCurrentLevel);
    }

    private void LoadPendingLevel()
    {
        if (mPendingLevel == null)
        {
            return;
        }

        if (mOpeningLevelCamera != null)
        {
            // reenable original camera while no other scenes are loaded
            mOpeningLevelCamera.gameObject.SetActive(true);
        }

        mSceneLoadState = SceneLoadState.SLS_LOADING;
        mCurrentLoadOp = SceneManager.LoadSceneAsync(mPendingLevel, LoadSceneMode.Additive);
        if (mCurrentLoadOp == null)
        {
            mSceneLoadState = SceneLoadState.SLS_ERROR;
            Debug.Log("Could not load scene " + mPendingLevel + "! Maybe it's not in the build?");
        }
    }

    private void LevelDoneLoading()
    {        
        mCurrentLevel = mPendingLevel;
        mPendingLevel = null;
        mSceneLoadState = SceneLoadState.SLS_LOADED;

        if (mOpeningLevelCamera != null)
        {
            // disable main scene camera when the new scene is here
            mOpeningLevelCamera.gameObject.SetActive(false);
        }

        HUDControl.FadeFromBlack();
        Debug.Log("Scene loaded");
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (mSceneLoadState == SceneLoadState.SLS_NEXT_SCENE_PENDING)
        {
            mNextLevelTimer -= Time.deltaTime;
            if (mNextLevelTimer <= 0)
            {
                UnloadCurrentLevel();
            }
        }
        else if (mSceneLoadState == SceneLoadState.SLS_UNLOADING)
        {
            if (mCurrentLoadOp.isDone)
            {
                LoadPendingLevel();
            }
        }
        else if (mSceneLoadState == SceneLoadState.SLS_LOADING)
        {
            if (mCurrentLoadOp.isDone)
            {
                LevelDoneLoading();
            }
        }
    }
}
