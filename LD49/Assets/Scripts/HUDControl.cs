using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDControl : MonoBehaviour
{
    private static HUDControl sSingleton;
    private Animator mAnimator;

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
        mAnimator.SetTrigger("FadeFromBlack");
    }
}
