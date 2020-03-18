using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_LineTunnel : VisualEventListener
{
    public GameObject LinePrefab;
    public Transform LinesParent;
    public bool unparentAfterSetup = false;

    [Header("Tunnel Shape")]
    public float tunnelRadius = 1f;
    public float tunnelFwdOffset = 0f; //How far ahead to spawn the tunnel?
    public float tunnelFwdOffsetVariation = 0f; //Randomises how many units a note spawns in front of or behind the tunnel's offset.

    [Header("Tunnel Rotation")] //All rotations in degrees
    public float tunnelRotation = 0f; //Around forward axis
    public float bonusRotationPerSecond = 0f;
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
    public float lineDurationMult = 1f;
    public float lineLengthOverride = -1f;
    public float lineWidthMult = -1f;
    public float lineAlpha = -1f;
    public Color PrimaryColour = Color.white;
    public float colourMix = 1f; // 0 = all primary, 1 = all secondary (i.e. note colour)
    public float positionEnvelopeMult = 1f;

    public override void OnNoteDown(int track, Note note)
    {
        //Rotation
        float finalTunnelRotation = tunnelRotation + bonusRotationPerSecond * visualisation.time;
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
        notePos.z += tunnelFwdOffset;
        if (tunnelFwdOffsetVariation != 0)
        {
            notePos.z += UnityEngine.Random.Range(-tunnelFwdOffsetVariation, tunnelFwdOffsetVariation);
        }

        //Prefab
        Transform noteLine = Instantiate(LinePrefab, LinesParent).transform;
        noteLine.localPosition = notePos;

        //Line Component
        Vis_LineByNote noteLineComponent = noteLine.GetComponent<Vis_LineByNote>();

        noteLineComponent.durationMult = lineDurationMult;

        noteLineComponent.lengthOverride = lineLengthOverride;
        if (lineWidthMult != -1) noteLineComponent.widthMult = lineWidthMult;
        if (lineAlpha != -1) noteLineComponent.baseAlpha = lineAlpha;

        noteLineComponent.PrimaryColour = PrimaryColour;
        noteLineComponent.colourMix = colourMix;

        noteLineComponent.positionEnvMult = positionEnvelopeMult;

        if (lineShapeOverride.keys.Length > 1)
        {
            noteLineComponent.line.widthCurve = lineShapeOverride;
        }

        noteLineComponent.Initialise();
        noteLineComponent.Visualise(track, note);

        //Parenting
        if (unparentAfterSetup)
        {
            noteLine.SetParent(visualisation.transform); //"Unparent" meaning set parent to visualisation, which is static so effectively same thing anyway. This is just more organised to look at.
            noteLineComponent.startPos = noteLine.position;
        }
    }
}
