using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_SpawnBasic : VisualEventListener
{
    public GameObject PrefabToSpawn;

    public Color colourOverride = Color.clear; //Clear = don't override
    public Vector3 scaleAtMinVel = Vector3.one;
    public Vector3 scaleAtMaxVel = Vector3.one;
    public Vector3 posAtMinPitch = Vector3.zero;
    public Vector3 posAtMaxPitch = Vector3.zero;
    public bool setParent = true;

    public override void OnNoteDown(int track, Note note)
    {
        Transform instance = Instantiate(PrefabToSpawn, setParent ? transform.parent : null).transform;
        instance.position = transform.position;
        instance.rotation = transform.rotation;
        instance.localScale = transform.localScale;

        //Colour
        if(colourOverride != Color.clear)
        {
            instance.GetComponent<Renderer>().material.SetColor("_Color", colourOverride);
        }

        //Pitch (Pos)
        instance.position += Vector3.Lerp(posAtMinPitch, posAtMaxPitch, note.NoteNumber / 127f);

        //Velocity (Scale)
        Vector3 scaleFactors = Vector3.Lerp(scaleAtMinVel, scaleAtMaxVel, note.Velocity / 127f);
        Vector3 instanceScale = instance.localScale;
        instanceScale.x *= scaleFactors.x;
        instanceScale.y *= scaleFactors.y;
        instanceScale.z *= scaleFactors.z;
        instance.localScale = instanceScale;

        VisualEffectInstance effect = instance.GetComponent<VisualEffectInstance>();
        if (effect != null)
        {
            effect.Initialise();
            effect.Visualise(track, note);
        }
    }
}
