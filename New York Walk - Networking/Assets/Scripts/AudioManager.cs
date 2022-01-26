using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager _instance { get; private set; }
    [SerializeField] private AudioClip positionning;
    [SerializeField] private AudioClip inGameMusic;
    [SerializeField] private AudioClip lastCitizenMusic;
    [SerializeField] private AudioSource pickUp;
    private AudioSource audioSource;

    private void Awake()
    {
        _instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void SetPositioningMusicClip()
    {
        audioSource.clip = positionning;
        audioSource.Play();
    }

    public void SetInGameMusicClip()
    {
        audioSource.clip = inGameMusic;
        audioSource.Play();
    }

    public void SetlastCitizenMusicClip()
    {
        audioSource.clip = lastCitizenMusic;
        audioSource.Play();
    }

    public void SetPickupMusicClip()
    {
        pickUp.Play();
    }

  
}
