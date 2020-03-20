using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

//Inserts a "stop" (keyframes w/ tangent zero) in a ConstantSmoothRotation's curves throughout the note being played.
//It removes any keyframes during the note, and instead adds ones at the start and end of the note which reflect the value the curve had at the midpoint of the note.
//This way, long notes can be used to mark "smooth" areas where delta rotation is limited.
//The lower the note pitch (i.e. max delta rotation per second) and the longer the length, the more unnatural rotation may become if a stop is inserted during a steep portion of the curve.
//Use sparingly and with much care.
public class Effect_RotationStops : VisualEventListener
{
    public ConstantSmoothRotation targetRotator;
    public bool affect_x, affect_y, affect_z;

    protected override void Start()
    {
        base.Start();
        PreReadMIDI();
    }


    protected override void OnNoteInMIDI(int track, Note note)
    {
        if (affect_x) ApplyStopNoteToCurve(targetRotator.XThroughoutSong, note);
        if (affect_y) ApplyStopNoteToCurve(targetRotator.YThroughoutSong, note);
        if (affect_z) ApplyStopNoteToCurve(targetRotator.ZThroughoutSong, note);
    }

    public void ApplyStopNoteToCurve(AnimationCurve curve, Note note)
    {
        double secondsTimeToCurveTimeFactor = 1f / visualisation.music.clip.length;

        //Calculate start, middle and end times relative to curve:
        float noteStart = (float)((note.TimeAs<MetricTimeSpan>(visualisation.MIDITempoMap).TotalMicroseconds / 1000000f)*secondsTimeToCurveTimeFactor);
        float noteEnd = (float)((note.EndTimeAs<MetricTimeSpan>(visualisation.MIDITempoMap).TotalMicroseconds / 1000000f)*secondsTimeToCurveTimeFactor);
        float noteCentre = (noteStart + noteEnd) / 2f;

        //Sample curve at start, middle and end of note:
        float startValue = curve.Evaluate(noteStart);
        float endValue = curve.Evaluate(noteEnd);
        float midValue = curve.Evaluate(noteCentre);

        //Clamp start and end values relative to mid value, using maxDeltaRotationPerSec (read from note pitch) to calculate their max deviation.
        float maxDeltaRotationPerSec = note.NoteNumber;
        float halfLength = (float)((noteCentre-noteStart)/secondsTimeToCurveTimeFactor); //Get half the length of the note, in seconds.
        float maxDeviation = halfLength * maxDeltaRotationPerSec;
        startValue = Mathf.Clamp(startValue, midValue - maxDeviation, midValue + maxDeviation);
        endValue = Mathf.Clamp(endValue, midValue - maxDeviation, midValue + maxDeviation);
        
        //Cleanse curve of any keyframes during the note:
        for(int i = 0; i < curve.keys.Length; i++)
        {
            if(curve.keys[i].time >= noteStart && curve.keys[i].time <= noteEnd)
            {
                curve.RemoveKey(i);
            }
        }

        //Add keys at start and end of note
        curve.AddKey(new Keyframe(noteStart, startValue));
        curve.AddKey(new Keyframe(noteEnd, endValue));
    }
}
