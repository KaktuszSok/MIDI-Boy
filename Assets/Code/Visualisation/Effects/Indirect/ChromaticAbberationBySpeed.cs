using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ChromaticAbberationBySpeed : MonoBehaviour
{

    public ConstantMoveForward speedSource;
    public PostProcessVolume postProcessing;
    public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public float maxExpectedVelocityFraction = 1f; //Fraction of baseSpeed at which our curve caps out.
    ChromaticAberration aberration;

    private void Start()
    {
        aberration = postProcessing.profile.GetSetting<ChromaticAberration>();
    }

    void LateUpdate()
    {
        aberration.intensity.value = intensityCurve.Evaluate(Mathf.Clamp01(speedSource.currentVelocity/(speedSource.baseVelocity*maxExpectedVelocityFraction)));
    }
}
