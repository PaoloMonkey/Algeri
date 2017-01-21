using System;
using UnityEngine;

[Serializable]
public class Cinematic : object
{
    // public camera movement
    public AudioSource audioSource;
    public Animator animator;
    public string trigger;
    public float duration;

    public void StartAnimation()
    {
        if (animator != null)
            animator.SetTrigger(trigger);
    }


}
