﻿using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class Effect_TintByNote : VisualEventListener
{
    SkyboxMaster skybox;

    public float transposeHue = 0;
    public float intensityMult = 1f;
    public float attackTime = 0.1f;

    public Color overrideColour = Color.clear;

    List<Color> frameColourBuffer = new List<Color>();
    public Color[] currColours = new Color[0];
    public Color[] pastColours = new Color[0];
    float lastColoursSetTime = 0f;

    protected override void Start()
    {
        base.Start();
        skybox = SkyboxMaster.instance;
    }

    private void Update()
    {
        //If new colours have been input this frame, overwrite current colours with the new ones.
        if(frameColourBuffer.Count != 0)
        {
            pastColours = currColours; //Remember past colours for cross-fading
            currColours = frameColourBuffer.ToArray();
            lastColoursSetTime = visualisation.time;
            frameColourBuffer.Clear(); //Clear colour input buffer for this frame.
        }

        float currStrength = Mathf.SmoothStep(0f, 1f, (visualisation.time - lastColoursSetTime) / attackTime); //Fade between old and new colours based on attackTime
        for (int i = 0; i < currColours.Length; i++) //Fade new colour in
        {
            skybox.AddModColour(currColours[i]*currStrength);
        }
        for(int i = 0; i < pastColours.Length; i++) //Fade old colour out
        {
            skybox.AddModColour(pastColours[i]*(1-currStrength));
        }
    }

    public override void OnNoteDown(int track, Note note)
    {
        //Buffer colour to skybox based on the note pitch.
        if (overrideColour == Color.clear)
        {
            Color noteColour = ColourUtils.ColourFromNote(note, transposeHue) * intensityMult;
            frameColourBuffer.Add(noteColour);
        }
        else
        {
            frameColourBuffer.Add(overrideColour*intensityMult);
        }
    }
}