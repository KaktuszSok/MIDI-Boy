using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
//using NAudio.Midi;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class VisualisationManager : MonoBehaviour {

    [Header("References")]
    public static VisualisationManager instance;
    public GameObject testPrefab;
    public AudioSource music;
    public string songName = "";
    public MidiFile midi;

    [Header("Parameters")]
    public double startOffset = 0f; //At which point in the song should we start from?
    public float autoplayDelay = 0.5f; //How long should we wait before starting playback?
    public double visualsOffset = -0.05f; //How much later should the visualisation be compared to the audio?

    [Header("Keybinds")]
    public KeyCode KeyPlayPause = KeyCode.Space;
    public KeyCode KeyStop = KeyCode.Backspace;
    public KeyCode KeyModifier = KeyCode.LeftShift;
    public int logTrack = -1;

    //----Runtime----\\
    public bool started = false;
    public float time { get { return (float)preciseTime; } }
    public double preciseTime { get; private set; }
    double prevTime = 0;
    public bool playing { get { return music.isPlaying; } }
    //MIDI stuff
    int timeDivision = 96;
    public ITimedObject[][] MIDIEvents; //Array of tracks, each track being an array of Notes and/or timed events.
    public TempoMap MIDITempoMap;

    public delegate void MIDINoteDownHandler(int track, Note note);
    public event MIDINoteDownHandler OnMIDINoteDown;

    public int[] lastEventCache; //Index of last event triggered on each track.
    public int[] trackLengthsCache; //Length of every track's events list.

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(SetUp());
    }

    IEnumerator SetUp()
    {
        //MIDI Setup
        midi = MidiFile.Read(Application.streamingAssetsPath + "/Songs/" + songName + ".mid");
        timeDivision = midi.TimeDivision.ToInt16();
        MIDITempoMap = midi.GetTempoMap();

        //Put midi events into lists by track.
        TrackChunk[] tracks = midi.GetTrackChunks().ToArray();
        MIDIEvents = new ITimedObject[tracks.Count()][];
        trackLengthsCache = new int[MIDIEvents.Length];
        for(int i = 0; i < tracks.Count(); i++)
        {
            MIDIEvents[i] = tracks[i].GetTimedEventsAndNotes().ToArray();
            trackLengthsCache[i] = MIDIEvents[i].Count();

            /*//Validate chronological integrity of track. (Seems to always be intact)
            long highestTime = 0;
            for(int j = 0; j < MIDIEvents[i].Length; j++)
            {
                if(MIDIEvents[i][j].Time >= highestTime)
                {
                    highestTime = MIDIEvents[i][j].Time;
                }
                else
                {
                    Debug.LogWarning("Track #" + i + " is out of chronological order! (element " + j + " violates check)");
                    break;
                }
            }
            */
        }

        //Clear last event cache
        lastEventCache = new int[MIDIEvents.Length];
        for(int i = 0; i < lastEventCache.Length; i++)
        {
            lastEventCache[i] = -1;
        }

        //Music Setup
        yield return StartCoroutine(LoadSong());
        yield return new WaitForSeconds(autoplayDelay);
        started = true;
        GoToTime(startOffset);
        Play();

        //Events
        OnMIDINoteDown += OnNoteDown;
    }

    IEnumerator LoadSong()
    {
        UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file://" + Application.streamingAssetsPath + "/Songs/" + songName + ".ogg", AudioType.OGGVORBIS);

        yield return req.SendWebRequest();
        if(req.isNetworkError)
        {
            Debug.Log(req.error);
        }
        else
        {
            music.clip = DownloadHandlerAudioClip.GetContent(req);
            samplesPerSecond = music.clip.frequency;
            secondsPerSample = 1f / samplesPerSecond;
        }

    }

    private void Update()
    {
        preciseTime = GetAudioTimeInSeconds(music.timeSamples) - visualsOffset;
        //if (preciseTime < prevTime) { GoToTime(preciseTime); }

        if (playing)
        {
            if (StepMIDI(prevTime, preciseTime)) prevTime = preciseTime;
        }

        //Controls
        if (Input.GetKeyDown(KeyPlayPause) && started)
        {
            TogglePause();
        }
        if (Input.GetKeyDown(KeyStop) && started)
        {
            Stop();
            GoToTime(startOffset);
        }
        if (Input.mouseScrollDelta.y != 0 && started)
        {
            GoToTime(preciseTime + Mathf.Sign(Input.mouseScrollDelta.y)*(Input.GetKey(KeyModifier) ? 4:1)/(MIDITempoMap.Tempo.AtTime(GetTimeInTicks(preciseTime)).BeatsPerMinute/60f));
        }
    }

    /// <returns>Was the step successful? (false if e.g. startTime == endTIme)</returns>
    public bool StepMIDI(double startTime, double endTime) //Fire all events between prevTime and time.
    {
        //Convert times to absolute:
        startTime = GetTimeInTicks(startTime);
        endTime = GetTimeInTicks(endTime);

        if (startTime == endTime) return false; //Ignore if start and end time are equal.

        Debug.Log("Stepping MIDI from " + startTime + " to " + endTime + "...");
        for (int t = 0; t < MIDIEvents.Length; t++) //for every track...
        {
            for (int i = lastEventCache[t] + 1; i < trackLengthsCache[t]; i++) //for every event...
            {
                ITimedObject e = MIDIEvents[t][i];
                if (e.Time < startTime) //Event has already played. Skip to next one.
                {
                    if (t == logTrack) Debug.Log("Read expired event on track " + t + " between times " + startTime + " and " + endTime + ". Event time is " + e.Time);
                    lastEventCache[t] = i;
                    continue;
                }
                if (e.Time >= endTime) //Event is too far in the future. Abort loop.
                {
                    if (t == logTrack) Debug.Log("Read future event on track " + t + " between times " + startTime + " and " + endTime + ". Event time is " + e.Time);
                    //Don't set last event cache - we want to revisit this event in the future.
                    break;
                }
                //Else, event is within the timestep.
                if(e is TimedEvent && ((TimedEvent)e).Event is NoteOnEvent) //NoteOn without Note Off - convert it to a short note because Reaper fucked shit up.
                {
                    NoteOnEvent noteOn = ((TimedEvent)e).Event as NoteOnEvent;
                    Note newNote = new Note(noteOn.NoteNumber, midi.TimeDivision.ToInt16() / 4, e.Time);
                    newNote.Velocity = noteOn.Velocity;
                    e = newNote;
                }
                if (e is Note) //If it is a Note...
                {
                    if (t == logTrack) Debug.Log("Read new note on track " + t + " between times " + startTime + " and " + endTime + ". Event time is " + e.Time);
                    OnMIDINoteDown(t, e as Note); //Fire OnMIDINoteDown event.
                    lastEventCache[t] = i;
                }
            }
        }
        return true;
    }

    void OnNoteDown(int track, Note note)
    {
        //Debug Function. Uncomment line below to disable.
        return;

        GameObject testInstance = Instantiate(testPrefab);
        testInstance.transform.SetParent(Camera.main.transform); //Parent to camera (so it spawns relative to cam)
        testInstance.transform.localPosition = Vector3.Lerp(Vector3.left * 15f, Vector3.right * 15f, note.NoteNumber / 127f)*2f; //Position based on pitch
        testInstance.transform.localPosition += Vector3.up*3f + Vector3.forward*10f; //Position shifted by constant
        testInstance.transform.localScale = Vector3.one * note.Velocity / 127f; //Scale based on velocity
        //testInstance.GetComponent<Rigidbody>().velocity = testInstance.transform.parent.forward * 10f; //Physics velocity constant
        testInstance.name = "Note on track " + track; //Name based on track
        testInstance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((float)track/trackLengthsCache.Length, 1, 1)); //Colour based on track
        testInstance.transform.Rotate(testInstance.transform.parent.up * track * -90f / trackLengthsCache.Length); //Rotation based on track
        testInstance.transform.SetParent(transform); //Re-parent to visualisation.
    }

    public void TogglePause()
    {
        if(playing)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }
    public void Play()
    {
        music.Play();
    }
    public void Pause()
    {
        music.Pause();
    }
    public void Stop()
    {
        music.Stop();
    }

    public void GoToTime(double t)
    {
        t = t + visualsOffset;
        if (t < 0) t = 0;
        if (t > music.clip.length) t = music.clip.length;
        music.time = (float)t;
        prevTime = t;
        preciseTime = t - visualsOffset;
        Debug.Log(t);
        //Clear last event cache
        lastEventCache = new int[MIDIEvents.Length];
        for (int i = 0; i < lastEventCache.Length; i++)
        {
            lastEventCache[i] = -1;
        }
    }

    /// <summary>
    /// Converts MIDI's absolute time to seconds.
    /// </summary>
    public double GetTimeInSeconds(long ticks)
    {
        return TimeConverter.ConvertTo<MetricTimeSpan>(ticks, MIDITempoMap).TotalMicroseconds/1000000f;
    }

    /// <summary>
    /// Converts seconds to MIDI's absolute time.
    /// </summary>
    public long GetTimeInTicks(double seconds)
    {
        return TimeConverter.ConvertFrom(new MetricTimeSpan((int)(seconds * 1000000)), MIDITempoMap);
    }

    double secondsPerSample;
    /// <summary>
    /// Converts AudioClip's sample time to seconds.
    /// </summary>
    public double GetAudioTimeInSeconds(long samples)
    {
        return samples * secondsPerSample;
    }

    int samplesPerSecond;
    /// <summary>
    /// Converts seconds to AudioClip's sample time.
    /// </summary>
    public int GetAudioTimeInSamples(double seconds)
    {
        return (int)(seconds * samplesPerSecond);
    }
}
