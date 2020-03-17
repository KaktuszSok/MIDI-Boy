using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_Ripple : VisualEventListener
{
    public RippleEffect ripple;

    public Vector2 pos = new Vector2(0.5f, 0.5f);
    public Color tint = Color.grey;
    public float refracMin = 0f;
    public float refracMax = 0.383f;
    public float reflecMin = 0.7f;
    public float reflecMax = 1f;
    public float speedMin = 1f;
    public float speedMax = 1.81f;

    private void Awake()
    {
        ripple = GetComponent<RippleEffect>();
        ripple.spawnPos = pos;
        ripple.reflectionColor = tint;
    }

    public override void OnNoteDown(int track, Note note)
    {
        float velNormalised = note.Velocity / 127f;
        ripple.refractionStrength = Mathf.Lerp(refracMin, refracMax, velNormalised);
        ripple.reflectionStrength = Mathf.Lerp(reflecMin, reflecMax, velNormalised);
        ripple.waveSpeed = Mathf.Lerp(speedMin, speedMax, velNormalised);
        ripple.Emit(pos);
    }
}
