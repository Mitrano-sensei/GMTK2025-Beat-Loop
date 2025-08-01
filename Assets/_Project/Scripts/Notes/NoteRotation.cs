using System;
using PrimeTween;
using UnityEngine;

public class NoteRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Transform origin;

    [SerializeField] private Transform note1;
    [SerializeField] private Transform note2;
    [SerializeField] private Transform note3;

    [SerializeField] private Transform placeHolder;
    
    [SerializeField] private TweenSettings<Vector3> rotationSettings;
    [SerializeField] private TweenSettings<Vector3> scaleSettings;

    private void Start()
    {
        Tween.LocalRotation(note1, rotationSettings);
        Tween.LocalRotation(note2, rotationSettings);
        Tween.LocalRotation(note3, rotationSettings);

        Tween.Scale(placeHolder, scaleSettings);
    }

    private void Update()
    {
        note1.RotateAround(origin.position, Vector3.forward, rotationSpeed * Time.deltaTime);
        note2.RotateAround(origin.position, Vector3.forward, rotationSpeed * Time.deltaTime);
        note3.RotateAround(origin.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}