using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public bool active;
    public CinematicInfo cinematic;
    private float elapsedTime = 0;

    public void StartAnimation()
    {
        elapsedTime = 0.0f;
        cinematic.StartAnimation();
    }

    public void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    public bool AnimationEnded()
    {
        return (elapsedTime > cinematic.duration);
    }
}
