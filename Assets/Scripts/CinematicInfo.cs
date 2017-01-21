using System;
using UnityEngine;

[Serializable]
public class CinematicInfo : object
{
    // public camera movement
    public GameManager.Actor actor;
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
