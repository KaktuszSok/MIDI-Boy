using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public abstract class VisualEffectInstance : MonoBehaviour
{
    protected VisualisationManager visualisation;

    public virtual void Initialise()
    {
        visualisation = VisualisationManager.instance;
    }

    public abstract void Visualise(int track, ITimedObject timedMidiEvent);
}
