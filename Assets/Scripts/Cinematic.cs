using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cinematic : System.Object
{

    public AudioSource audioSource;
    public Animator animator;
    public string trigger;

    public void StartAnimation()
    {
        if (animator != null)
            animator.SetTrigger(trigger);
    }
}
