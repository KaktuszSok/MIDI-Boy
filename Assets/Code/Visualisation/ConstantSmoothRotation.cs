using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantSmoothRotation : MonoBehaviour
{
    VisualisationManager visualisation;

    public int seed;
    //Keyframe Distance is how long, as a fraction of the song, does it take to get from one keyframe to the next.
    public float minKeyframeDistance = 0.05f;
    public float maxKeyframeDistance = 0.15f;
    public float rotationLimitXY = 180f; //Rotation on any X and Y axes can be at most between - this and + this.
    public float rotationSpeedZMult = 1f; //Multiplier to how fast, on average, Z rotation changes.
    public Vector3 forwardRotation = Vector3.one*-1234; //Rotation around which everything revolves. -1234 on all to set to starting rotation.

    [Header("Rotation Curves")]
    public AnimationCurve XThroughoutSong;
    public AnimationCurve YThroughoutSong;
    public AnimationCurve ZThroughoutSong;

    void Start()
    {
        Random.InitState(seed);
        visualisation = VisualisationManager.instance;

        //Generate curves
        PopulateCurveRandomly(XThroughoutSong);
        PopulateCurveRandomly(YThroughoutSong);
        PopulateCurveRandomly(ZThroughoutSong, rotationSpeedZMult);

        if(forwardRotation == Vector3.one*-1234)
        {
            forwardRotation = transform.eulerAngles;
        }
    }

    void Update()
    {
        if (visualisation.started)
        {
            float songProgress = visualisation.time / visualisation.music.clip.length;
            transform.localEulerAngles = new Vector3(XThroughoutSong.Evaluate(songProgress)*(rotationLimitXY / 180f), YThroughoutSong.Evaluate(songProgress)*(rotationLimitXY / 180f), 0);
            transform.rotation *= Quaternion.Euler(forwardRotation);
            transform.Rotate(Vector3.forward, ZThroughoutSong.Evaluate(songProgress), Space.Self);
        }
    }

    AnimationCurve PopulateCurveRandomly(AnimationCurve curve, float speedMultiplier = 1f)
    {
        float genTime = 0f; //Keep track of how far we are in the curve, in range 0 to 1.
        while (genTime < 1)
        {
            float curveValue = Random.Range(-180f, 180f);
            Keyframe kf = new Keyframe(genTime, curveValue); //Create keyframe at current time and with random value.
            curve.AddKey(kf); //Add to curve.

            //Advance time by random amount, to be used for next keyframe.
            float keyframeDistance = Random.Range(minKeyframeDistance, maxKeyframeDistance)/speedMultiplier;
            genTime += keyframeDistance;
            if (genTime >= 1f) //Put down last keyframe.
            {
                //Simulate having a frame after the ending
                curveValue = Random.Range(-180f, 180f);
                kf = new Keyframe(genTime, curveValue);
                curve.AddKey(kf);
                int simIndex = curve.keys.Length - 1; //Keep track of this keyframe's index as it is but a simulated helper i.e. should be removed.

                //Create new keyframe at time 1 with value of what the curve would be at with the above simulated keyframe. This is to smooth out the ending, as tangent of all keyframes is zero, so ending on a keyframe is smoother than cutting off.
                curveValue = curve.Evaluate(1);
                kf = new Keyframe(1f, curveValue);
                curve.RemoveKey(simIndex); //Remove simulated keyframe (byebyeee... :().
                curve.AddKey(kf); //Add new ending keyframe.
                break;
            }
        }
        return curve;
    }
}
