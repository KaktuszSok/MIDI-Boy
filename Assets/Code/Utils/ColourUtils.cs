using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public static class ColourUtils
{
    public static Color ColourFromNote(Note note, float transpose)
    {
        return Color.HSVToRGB(HueFromNote(note, transpose), 1, 1);
    }

    public static float HueFromNote(Note note, float transpose)
    {
        return TransposeHue((float)note.NoteName / 12, transpose);
    }

    public static float TransposeHue(float hue, float transpose)
    {
        return Mathf.Repeat(hue + transpose, 1f);
    }
}
