using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlotEffectsManager : MonoBehaviour
{
    public AudioSource audioSource;
    public Image image;
    
    void Start()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        image = GetComponentInChildren<Image>();
        audioSource.playOnAwake = false;
    }
    
    public void PlaySound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
