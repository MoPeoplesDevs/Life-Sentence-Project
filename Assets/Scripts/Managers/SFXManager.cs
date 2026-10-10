using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SFXManager : MonoBehvaiour
{
    public static SFXManager Instance {get; private set;}

    [SerializeField] private AudioSource sfxObject;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void PlaySFXAt(AudioClip clip, Transform pos, float volume)
    {
        AudioSource audio = Instantiate(sfxObject, pos.position, Quaternion.identity);
        audio.clip = clip;
        audio.volume = volume;

        float clipLength = audio.clip.length;
        Destroy(audio.gameObject, clipLength + 0.1f);
    }
}