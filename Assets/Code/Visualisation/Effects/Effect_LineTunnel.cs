using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_LineTunnel : VisualEventListener
{
    public GameObject LinePrefab;
    public Transform LinesParent;
    ConstantMoveForward parentMover;
    public bool unparentAfterSetup = false;
    public bool inheritVelocity = false; //Makes position envelope scale with camera speed

    [Header("Tunnel Shape")]
    public float tunnelRadius = 1f;
    public Vector3 tunnelOffset = Vector3.zero;
    public Vector3 tunnelOffsetVariation = Vector3.zero;

    [Header("Tunnel Rotation")] //All rotations in degrees
    public float tunnelRotation = 0f; //Around forward axis
    public float bonusRotationPerSecond = 0f;
    public float bonusRotationPerBeat = 0f;
    public float rotateDir = -1f; //Clockwise or Anticlockwise?
    public enum RotateType
    {
        DONT,
        NOTE,
        RANDOM,
    }
    public RotateType RotateBy = RotateType.NOTE;

    [Header("Line Parameters")]
    public AnimationCurve lineShapeOverride = new AnimationCurve();
    public float lineDurationOverride = -1f; //-1 = use prefab's default, 0 = use note duration (aka don't override)
    public float lineDurationMult = 1f;
    public float lineLengthOverride = -1f; //-1 or 0 = don't override
    public float lineWidthMult = -1f;
    public float lineAlpha = -1f;
    public Color PrimaryColour = Color.white;
    public float colourMix = 1f; // 0 = all primary, 1 = all secondary (i.e. note colour)
    public float positionEnvelopeMult = 1f;

    private void Awake()
    {
        parentMover = LinesParent.GetComponent<ConstantMoveForward>();
    }

    public override void OnNoteDown(int track, Note note)
    {
        //Rotation
        float noteTime = note.TimeAs<MetricTimeSpan>(visualisation.MIDITempoMap).TotalMicroseconds / 1000000f;
        float bonusRotation = bonusRotationPerSecond * noteTime;
        if(bonusRotationPerBeat != 0) bonusRotation += bonusRotationPerBeat * (float)visualisation.GetTimeInBeats(note.Time);

        float finalTunnelRotation = tunnelRotation + bonusRotation;
        float noteRotation = 0f;
        switch(RotateBy)
        {
            case RotateType.DONT:
                noteRotation = 0f;
                break;
            case RotateType.NOTE:
                noteRotation = ColourUtils.HueFromNote(note, 0) * 360f;
                break;
            case RotateType.RANDOM:
                noteRotation = UnityEngine.Random.Range(0f, 360f);
                break;
            default:
                break;
        }

        //Position
        Vector3 notePos = Vector3.right * tunnelRadius;
        notePos = Quaternion.Euler(0, 0, (noteRotation + finalTunnelRotation) * rotateDir) * notePos; //Rotate position around forward axis based on tunnel rotation and note rotation.
        notePos += tunnelOffset;
        if (tunnelOffsetVariation != Vector3.zero)
        {
            notePos.x += UnityEngine.Random.Range(-tunnelOffsetVariation.x, tunnelOffsetVariation.x);
            notePos.y += UnityEngine.Random.Range(-tunnelOffsetVariation.y, tunnelOffsetVariation.y);
            notePos.z += UnityEngine.Random.Range(-tunnelOffsetVariation.z, tunnelOffsetVariation.z);
        }

        //Prefab
        Transform noteLine = Instantiate(LinePrefab, LinesParent).transform;
        noteLine.localPosition = notePos;

        //Line Component
        Vis_LineByNote noteLineComponent = noteLine.GetComponent<Vis_LineByNote>();

        if(lineDurationOverride != -1) noteLineComponent.durationOverride = lineDurationOverride;
        if(lineDurationMult != -1) noteLineComponent.durationMult = lineDurationMult;

        noteLineComponent.lengthOverride = lineLengthOverride;
        if (lineWidthMult != -1) noteLineComponent.widthMult = lineWidthMult;
        if (lineAlpha != -1) noteLineComponent.baseAlpha = lineAlpha;

        noteLineComponent.PrimaryColour = PrimaryColour;
        noteLineComponent.colourMix = colourMix;

        if(positionEnvelopeMult != -1)
            noteLineComponent.positionEnvMult = positionEnvelopeMult;

        if (lineShapeOverride.keys.Length > 1)
        {
            noteLineComponent.line.widthCurve = lineShapeOverride;
        }

        noteLineComponent.Initialise();
        noteLineComponent.Visualise(track, note);

        //Add parent's velocity to position envelope.
        if (inheritVelocity)
        {

            for (int i = 0; i < noteLineComponent.positionEnvelope.keys.Length; i++)
            {
                Keyframe k = noteLineComponent.positionEnvelope.keys[i];
                k.value += parentMover.currentVelocity * k.time / noteLineComponent.GetTotalDuration();
                noteLineComponent.positionEnvelope.keys[i] = k;
            }
        }

        //Parenting
        if (unparentAfterSetup)
        {
            noteLine.SetParent(visualisation.transform); //"Unparent" meaning set parent to visualisation, which is static so effectively same thing anyway. This is just more organised to look at.
            noteLineComponent.startPos = noteLine.position;
        }
    }
}
