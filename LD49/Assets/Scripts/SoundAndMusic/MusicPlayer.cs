using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public List<string> clipNames;
    public List<AudioClip> clips;
    public List<float> volumes;
    public Sprite mutedImage;
    public Sprite unmutedImage;
    public GameObject muteButton;

    private static MusicPlayer sSingleton;

    private bool mPlaying = false;
    private bool mMuted = false;
    private AudioSource mAudioSource;

    private Dictionary<string, AudioClip> mClipsDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, float> mVolumesDict = new Dictionary<string, float>();

    public float fadeOutTime = 0.5f;
    private float mOriginalVolume;
    private bool mFadingOut = false;
    private float mFadeOutSpeed;

    // Start is called before the first frame update
    void Start()
    {
        sSingleton = this;
        mAudioSource = GetComponent<AudioSource>();
        mOriginalVolume = mAudioSource.volume;
        mFadeOutSpeed = 1 / fadeOutTime;

        for(int i = 0; i < clipNames.Count; ++i)
        {
            mClipsDict.Add(clipNames[i], clips[i]);
            mVolumesDict.Add(clipNames[i], volumes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        muteButton.GetComponent<Image>().sprite = mMuted ? mutedImage : unmutedImage;

        if(mFadingOut)
        {
            mAudioSource.volume -= Time.deltaTime * mFadeOutSpeed;
            if(mAudioSource.volume <= 0f)
            {
                mFadingOut = false;
                mAudioSource.Stop();
            }
        }
    }

    private static MusicPlayer GetSingleton()
    {
        if (sSingleton == null || !sSingleton.gameObject.activeInHierarchy)
        {
            sSingleton = GameObject.FindObjectsOfType<MusicPlayer>()[0];
        }
        return sSingleton;
    }

    public static void FadeOut()
    {
        GetSingleton()._FadeOut();
    }
    private void _FadeOut()
    {
        mFadingOut = true;
    }

    public static void StartPlaying(string music)
    {
        GetSingleton()._Start(music);
    }

    private void _Start(string music)
    {
        AudioClip clip = mClipsDict[music];
        float volume = mVolumesDict[music];
        mAudioSource.clip = clip;
        mFadingOut = false;
        mPlaying = true;
        mAudioSource.volume = mOriginalVolume * volume;
        if(!mMuted)
        {
            mAudioSource.Play();
        }
    }

    public void ToggleMute()
    {
        if(mMuted)
        {
            mMuted = false;
            if(mPlaying)
            {
                mAudioSource.Play();
            }
        }
        else
        {
            mMuted = true;
            mAudioSource.Stop();
        }
    }
}
