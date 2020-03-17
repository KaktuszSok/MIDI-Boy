using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public abstract class VisualEventListener : MonoBehaviour
{
    public int[] listenedTracks = new int[1]; //For ease of use in editor. To reapply, disable and re-enable this script.
    public HashSet<int> listenedTracksHashset = new HashSet<int>();

    protected virtual void Start()
    {
        VisualisationManager.instance.OnMIDINoteDown += OnAnyNoteDown;
    }

    protected virtual void OnEnable()
    {
        listenedTracksHashset = new HashSet<int>(listenedTracks);
    }

    private void OnAnyNoteDown(int track, Note note)
    {
        if (!enabled) return;
        if (listenedTracksHashset.Contains(track)) OnNoteDown(track, note);
    }

    public abstract void OnNoteDown(int track, Note note);
}
