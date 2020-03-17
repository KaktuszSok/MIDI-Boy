using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class Vis_Shockwave : VisualEffectInstance
{
    public float baseIntensity = 0.5f;
    public float baseSize = 7f;
    public float sizeAtMinVel = 7f;
    public float baseLifetime = 1f;
    public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f));

    public float progress;

    public override void Visualise(int track, ITimedObject timedMidiEvent)
    {
        if (timedMidiEvent is Note) {
            Note note = timedMidiEvent as Note;
            baseIntensity *= note.Velocity / 127f; //Higher vel = more intensity
            baseSize = Mathf.Lerp(sizeAtMinVel, baseSize, note.Velocity / 127f); //Higher vel = bigger size
            baseLifetime *= (1 - note.NoteNumber / 127f); //Lower pitch = longer lifetime (slower wave)
        }

        StartCoroutine(ShockwaveCoroutine());
    }

    WaitForEndOfFrame EOF = new WaitForEndOfFrame();
    IEnumerator ShockwaveCoroutine()
    {
        float endTime = visualisation.time + baseLifetime;
        gameObject.AddComponent<AutoDestroy>().DestroyAfterTime(baseLifetime*1.1f);
        Material mat = GetComponent<Renderer>().material;
        while(visualisation.time < endTime)
        {
            progress = 1-((endTime - visualisation.time) / baseLifetime);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * baseSize, progress);
            mat.SetFloat("_Intensity", intensityCurve.Evaluate(progress) * baseIntensity);
            yield return EOF;
        }
    }
}
