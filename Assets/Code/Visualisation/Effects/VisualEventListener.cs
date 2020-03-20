using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public abstract class VisualEventListener : ModifiableComponent
{
    protected VisualisationManager visualisation;

    public int[] listenedTracks = new int[1]; //For ease of use in editor. To reapply, disable and re-enable this script.
    public HashSet<int> listenedTracksHashset = new HashSet<int>();

    protected virtual void Start()
    {
        visualisation = VisualisationManager.instance;
        visualisation.OnMIDINoteDown += OnAnyNoteDown;
    }

    protected virtual void OnEnable()
    {
        listenedTracksHashset = new HashSet<int>(listenedTracks);
    }

    /// <summary>
    /// Pre-read the MIDI file and call OnNoteInMIDI function for every note of listened tracks.
    /// </summary>
    protected void PreReadMIDI()
    {
        foreach (int track in listenedTracks)
        {
            foreach (Note note in visualisation.GetAllNotesOnTrack(track))
            {
                OnNoteInMIDI(track, note);
            }
        }
    }

    private void OnAnyNoteDown(int track, Note note)
    {
        if (!enabled) return;
        if (listenedTracksHashset.Contains(track)) OnNoteDown(track, note);
    }

    public virtual void OnNoteDown(int track, Note note)
    {
        return;
    }

    /// <summary>
    /// For this function to be called, you must call PreReadMIDI() e.g. by overriding Start(). Make sure to do base.Start() in the override or everything will break.
    /// </summary>
    protected virtual void OnNoteInMIDI(int track, Note note)
    {
        return;
    }
}
