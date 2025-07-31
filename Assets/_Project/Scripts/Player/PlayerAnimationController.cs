using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Transform modelTransform;

    [Header("Idle")]
    [SerializeField] private TweenSettings<Vector3> idleJiggleTweenSettings;
    [SerializeField] private TweenSettings<Vector3> idleBreatheTweenSettings;
    
    private Sequence _idleTween;

    private void Start()
    {
        SetupTweens();
    }

    private void OnValidate()
    {
        // Kill the sequences
        _idleTween.Stop();
        SetupTweens();
    }


    private void SetupTweens()
    {
        SetupIdleTween();
    }
    
    private void SetupIdleTween()
    {
        if (_idleTween.isAlive)
            return;
        
        // If we are in the editor, we don't want to play the idle animation
        if (!Application.isPlaying)
            return;
        
        _idleTween = Sequence.Create(cycles: -1, CycleMode.Yoyo).Group(Tween.LocalRotation(modelTransform, idleJiggleTweenSettings)).Group(Tween.Scale(modelTransform, idleBreatheTweenSettings));
    }

    public void StartIdleAnimation()
    {
        if (!_idleTween.isAlive)
            SetupIdleTween();

        if (!_idleTween.isPaused)
            return;
        
        _idleTween.Complete();
        _idleTween.isPaused = false;
    }
    
    public void StopIdleAnimation()
    {
        if (!_idleTween.isAlive)
            return;

        if (_idleTween.isPaused)
            return;
        
        _idleTween.Complete();  
        _idleTween.Stop();
        _idleTween.isPaused = true;
    }
}