using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantSpinning : MonoBehaviour
{
    VisualisationManager visualisation;

    public float animationTime = 60f;
    public Vector3 startRot;
    public Vector3 endRot;
    public enum RepeatType
    {
        CLAMP,
        LOOP,
        PINGPONG
    }
    public RepeatType repeatType = RepeatType.LOOP;

    private void Start()
    {
        visualisation = VisualisationManager.instance;
    }

    private void Update()
    {
        transform.localEulerAngles = GetCurrRot(visualisation.time);
    }

    public Vector3 GetCurrRot(float time)
    {
        return Vector3.Lerp(startRot, endRot, GetAnimTime(time) / animationTime);
    }

    public float GetAnimTime(float time)
    {
        switch(repeatType)
        {
            case RepeatType.CLAMP:
                return Mathf.Clamp(time, 0, animationTime);
            case RepeatType.LOOP:
                return Mathf.Repeat(time, animationTime);
            case RepeatType.PINGPONG:
                return Mathf.PingPong(time, animationTime);
            default:
                return time;
        }
    }
}
