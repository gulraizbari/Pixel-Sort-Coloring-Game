using System;
using System.Collections;
using System.Collections.Generic;
/*using Save_System;*/
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> Clips;

    public AudioSource Source;

    public static AudioManager instance;

    

    public void SFXState()
    {
        /*Source.mute = SaveData.Instance.sfx != 0;*/
        Source.mute = false;
    }
    void Awake()
    {
        instance = this;
        /*SFXState();*/
    }

    public void AddSound()
    {
        PlaySound(0,1);
        if (HapticManager.Instance)
        {
            HapticManager.Instance.TriggerHapticFeedback(0.5f, 1f, 0.017f);
        }
    }
    public void ThrowSound()
    {
        PlaySound(1,1);
        if (HapticManager.Instance)
        {
            HapticManager.Instance.TriggerHapticFeedback(0.3f, 1f, 0.017f);
        }
    }
    
    public void PlaceBrick()
    {   
        if (HapticManager.Instance)
        {
            HapticManager.Instance.TriggerHapticFeedback(0.3f, 1f, 0.017f);
        }
        PlaySound(2,1);
    }
    
    public void ButtonClick()
    {   
        if (HapticManager.Instance)
        {
            HapticManager.Instance.TriggerHapticFeedback(0.3f, 1f, 0.017f);
        }
        PlaySound(4,1);
    } 
   
    public void CoinsAdd()
    {
        if (HapticManager.Instance)
        {
            HapticManager.Instance.TriggerHapticFeedback(0.3f, 1f, 0.017f);
        }
        PlaySound(5,1);
    } 
    
    public void CrossPanelSound()
    {
        PlaySound(6,0.4f);
        if (HapticManager.Instance)
        {
            HapticManager.Instance.TriggerHapticFeedback(0.3f, 1f, 0.017f);
        }
    }
    
    public void PopSound()
    {
        PlaySound(7,1);
    }
    
    public void PlaySound(int index, float volunme)
    {
        Source.PlayOneShot(Clips[index],volunme);
    }
    public void ConfettiSfx()
    {
        PlaySound(3,1);
    }

    public void EnableSoundEffects() => Source.mute = false;

    public void DisableSoundEffects() => Source.mute = true;

    public void EnableBackgroundMusic() => Source.mute = false; // bg source

    public void DisableBackgroundMusic() => Source.mute = true;
}