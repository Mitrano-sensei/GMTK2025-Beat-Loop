using PrimeTween;
using UnityEngine;

public class TestJiggle : MonoBehaviour
{
    private void Start()
    {
        Tween.ScaleY(transform, duration: 1, endValue: 0.6f, ease:Ease.InOutSine, cycles:-1);
    }
}
