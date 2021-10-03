using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDControl : MonoBehaviour
{
    private static HUDControl sSingleton;
    private Animator mAnimator;
    public Image splashScreen;

    public static void FadeToBlack()
    {
        if (sSingleton != null) sSingleton._FadeToBlack();
    }

    public static void FadeFromBlack()
    {
        if (sSingleton != null) sSingleton._FadeFromBlack();
    }

    private void Awake()
    {
        sSingleton = this;
        mAnimator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void _FadeToBlack()
    {
        mAnimator.SetTrigger("FadeToBlack");
    }

    private void _FadeFromBlack()
    {
        // Hide the splash screen the first time that we fade from black.
        splashScreen.enabled = false;
        mAnimator.SetTrigger("FadeFromBlack");
    }
}
