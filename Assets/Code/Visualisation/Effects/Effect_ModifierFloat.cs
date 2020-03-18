using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_ModifierFloat : VisualEventListener
{
    [Header("References")]
    public ModifiableComponent targetComponent;
    public IModifiable target; //Can't be accessed through editor, only through code. Therefore above line is necessary.
    public string targetField;

    [Header("Parameters")]
    public AnimationCurve inputOutput = new AnimationCurve(new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0));
    public enum ReactType
    {
        VELOCITY, //Note velocity. 0 = 0, 127 = 1.
        NOTE_FULL, //Note number. 0 = 0, 127 = 1. 
        NOTE_12 //Note within octave. 0 = 0, 11 = 1.
    }
    public ReactType ReactTo = ReactType.VELOCITY;

    private void Awake()
    {
        if (targetComponent)
        {
            target = targetComponent;
        }
    }

    public override void OnNoteDown(int track, Note note)
    {
        if (target == null) return;

        float inputNormalised = 0f;
        switch(ReactTo)
        {
            case ReactType.VELOCITY:
                inputNormalised = note.Velocity / 127f;
                break;
            case ReactType.NOTE_FULL:
                inputNormalised = note.NoteNumber / 127f;
                break;
            case ReactType.NOTE_12:
                inputNormalised = (float)note.NoteName / 11f;
                break;
            default:
                break;
        }

        target.Modify(targetField, inputOutput.Evaluate(inputNormalised));
    }
}
