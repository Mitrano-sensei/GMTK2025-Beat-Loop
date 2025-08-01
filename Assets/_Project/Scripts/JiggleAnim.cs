using PrimeTween;
using UnityEngine;

public class JiggleAnim : MonoBehaviour
{
    [SerializeField] private TweenSettings<Vector3> jiggleSettings;
    
    void Start()
    {
        Tween.LocalRotation(transform, jiggleSettings);
    }
}
