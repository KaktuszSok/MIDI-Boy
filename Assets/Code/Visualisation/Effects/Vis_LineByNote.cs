using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Vis_LineByNote : VisualEffectInstance
{
    public LineRenderer line;

    [Header("Timing")]
    public float durationMult = 1f;

    [Header("Shape")]
    public float lengthOverride = -1;
    float baseWidth; //Take from line component
    public float widthMult = 1f;
    public AnimationCurve widthEnvelope = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 1f), new Keyframe(0.65f, 1f), new Keyframe(1f, 0f));

    [Header("Colour")]
    public float baseAlpha = 1f;
    public AnimationCurve alphaEnvelope = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
    public Color PrimaryColour = Color.white;
    public float colourMix = 1f; // 0 = all primary, 1 = all secondary (i.e. note colour)
    public float transposeHue;

    [Header("Position")]
    public AnimationCurve positionEnvelope = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
    public float positionEnvMult = 1f;
    public Vector3 positionAxis = Vector3.forward;
    public Space positionAxisSpace = Space.Self;
    public bool positionEnvRelativeToDurationMult = false;
    public bool positionEnvRelativeToLength = false;
    [HideInInspector] public Vector3 startPos;

    bool started;
    float startTime;
    float endTime;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        baseWidth = line.widthMultiplier;
        line.enabled = false;
    }

    void Update()
    {
        if(started)
        {
            float progress = (visualisation.time - startTime) / (endTime - startTime);
            if(progress >= 1f || progress < -0.25f) //Note is finished or we skipped back in time
            {
                started = false; //Disable processing
                Destroy(gameObject);
            }

            //Animate line
            progress = Mathf.Clamp01(progress);
            line.widthMultiplier = widthEnvelope.Evaluate(progress)*baseWidth*widthMult;
            Color finalColour = line.startColor;
            finalColour.a = alphaEnvelope.Evaluate(progress)*baseAlpha;
            line.startColor = line.endColor = finalColour;
            //Animate position
            if (positionEnvelope.keys.Length > 1) {
                transform.localPosition = startPos;
                Vector3 posOffset = positionAxis * positionEnvelope.Evaluate(progress);
                posOffset *= positionEnvMult;
                if (positionEnvRelativeToDurationMult) posOffset *= durationMult;
                if (positionEnvRelativeToLength) posOffset *= transform.localScale.z;
                transform.Translate(posOffset, positionAxisSpace);
            }
        }
    }

    public override void Visualise(int track, ITimedObject timedMidiEvent)
    {
        if(timedMidiEvent is Note)
        {
            if (lengthOverride != -1) transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, lengthOverride);

            Note note = timedMidiEvent as Note;
            Color noteColour = ColourUtils.ColourFromNote(note, transposeHue);
            noteColour = Color.Lerp(PrimaryColour, noteColour, colourMix);
            baseAlpha *= note.Velocity / 127f;
            noteColour.a = alphaEnvelope.Evaluate(0f) * baseAlpha;
            line.startColor = line.endColor = noteColour;

            startTime = visualisation.time;
            endTime = startTime + (note.LengthAs<MetricTimeSpan>(visualisation.MIDITempoMap).TotalMicroseconds / 1000000f)*durationMult;

            startPos = transform.localPosition;

            line.widthMultiplier = 0f; //This will be updated to correct value next frame.
            line.enabled = true;
            started = true;
        }
    }
}
