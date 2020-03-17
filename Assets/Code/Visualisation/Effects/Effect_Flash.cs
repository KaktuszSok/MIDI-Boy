using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_Flash : VisualEventListener
{
    public GameObject FlashPrefab;
    public float intensityMult = 1f;
    public AnimationCurve intensityByPitch = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(127f, 1f));
    public float durationMult = 1f;

    public override void OnNoteDown(int track, Note note)
    {
        Vis_Flash flashInstance = Instantiate(FlashPrefab, transform).GetComponent<Vis_Flash>();
        flashInstance.intensity *= intensityMult;
        flashInstance.intensityByPitch = intensityByPitch;
        flashInstance.durationMult *= durationMult; //multception!

        flashInstance.Initialise();
        flashInstance.Visualise(track, note);
    }
}
