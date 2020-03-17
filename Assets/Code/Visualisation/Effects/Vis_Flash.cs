using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class Vis_Flash : VisualEffectInstance
{
    SkyboxMaster skybox;

    public float intensity = 1f;
    public AnimationCurve intensityByPitch = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(127f, 1f));
    public float durationMult = 1f;
    public AnimationCurve envelope = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.05f, 1f), new Keyframe(1f, 0f));

    public float progress = 0f;

    public override void Initialise()
    {
        base.Initialise();
        skybox = SkyboxMaster.instance;
    }

    public override void Visualise(int track, ITimedObject timedMidiEvent)
    {
        float duration = 1f;
        float intensityMult = 1f;
        if(timedMidiEvent is Note)
        {
            Note note = timedMidiEvent as Note;
            duration = note.LengthAs<MetricTimeSpan>(visualisation.MIDITempoMap).TotalMicroseconds / 1000000f;
            intensityMult = note.Velocity / 127f;
            intensityMult *= intensityByPitch.Evaluate(note.NoteNumber);
        }
        StartCoroutine(FlashCoroutine(duration, intensityMult));
    }

    WaitForEndOfFrame EOF = new WaitForEndOfFrame();
    public IEnumerator FlashCoroutine(float duration = 1f, float intensityMult = 1f)
    {
        duration *= durationMult;
        float endTime = visualisation.time + duration;

        while (visualisation.time < endTime)
        {
            progress = 1 - ((endTime - visualisation.time) / duration);
            skybox.modExp += envelope.Evaluate(progress) * intensity * intensityMult;
            yield return EOF;
        }
        Destroy(gameObject);
    }
}
