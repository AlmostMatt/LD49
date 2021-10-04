using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is totally not SpriteDictionary
public class SFXPlayer : MonoBehaviour
{
    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip AudioClip;
    }

    // These fields are functionally equivalent, but they logically group audio clips
    // If you add a group to this, add a reference to it in Setup
    public NamedAudioClip[] namedAudioClips1;
    public NamedAudioClip[] namedAudioClips2;
    public NamedAudioClip[] namedAudioClips3;

    private Dictionary<string, AudioClip> stringToAudioClipMap = new Dictionary<string, AudioClip>();
    private static SFXPlayer mSFXPlayerSingleton;

    void Awake()
    {
        if (stringToAudioClipMap.Count == 0)
        {
            Setup();
        }
    }

    private void Setup()
    {
        foreach (NamedAudioClip[] namedAudioClipArray in new NamedAudioClip[][] {
                    namedAudioClips1,
                    namedAudioClips2,
                    namedAudioClips3,
                })
        {
            foreach (NamedAudioClip namedAudioClip in namedAudioClipArray)
            {
                if (stringToAudioClipMap.ContainsKey(namedAudioClip.name))
                {
                    Debug.LogWarning("Multiple AudioClips with name: " + namedAudioClip.name);
                }
                stringToAudioClipMap.Add(namedAudioClip.name, namedAudioClip.AudioClip);
            }
        }
    }

    public static void PlayAudioClip(string clipName)
    {
        if (mSFXPlayerSingleton == null || !mSFXPlayerSingleton.gameObject.activeInHierarchy)
        {
            mSFXPlayerSingleton = GameObject.FindObjectsOfType<SFXPlayer>()[0];
        }
        mSFXPlayerSingleton.PlayAudioClipInternal(clipName);
    }

    // To be called by the static GetAudioClip
    public void PlayAudioClipInternal(string clipName)
    {
        if (clipName == "" || clipName == "None")
        {
            Debug.LogWarning("Playing an empty or None audio clip - " + clipName);
            return;
        }
        // Call Setup here in case awake hasnt yet happened
        if (stringToAudioClipMap.Count == 0)
        {
            Setup();
        }
        if (stringToAudioClipMap.ContainsKey(clipName))
        {
            AudioClip clip = stringToAudioClipMap[clipName];
            GetComponent<AudioSource>().PlayOneShot(clip);
        } else
        {
            Debug.LogWarning("WARNING(AudioClipManager): No AudioClip found for name: " + clipName);
        }
    }
}
