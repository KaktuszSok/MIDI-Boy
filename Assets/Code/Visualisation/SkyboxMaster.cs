using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxMaster : MonoBehaviour
{
    public static SkyboxMaster instance;
    public Material skybox;
    public Color baseTint = new Color(0.5f, 0.5f, 0.5f);
    public float tintPower = 0.5f;
    public float tintChangeSpeed = 2f;
    public float baseExposure = 1f;
    public float baseRotation = 1f;
    public Light lightSource;
    public Color baseLightColour = Color.clear;
    public float lightTintPower = 0.5f;
    public float baseLightIntensity = -1;
    public float lightExposureReactiveness = 0.5f; //How much does light intensity react to exposure?

    //per-frame variables:
    public List<Color> modColours { get; private set; } = new List<Color>();
    public float modExp;
    public float modRot;

    Vector4 targetModHSV = new Vector4();
    Vector4 currentModHSV = new Vector4();

    private void Awake()
    {
        instance = this;
        skybox = RenderSettings.skybox;
        if (!lightSource) lightSource = FindObjectOfType<Light>();

        if(baseLightColour == Color.clear)
        {
            baseLightColour = lightSource.color;
        }
        if (baseLightIntensity == -1) baseLightIntensity = lightSource.intensity;
    }

    public void SetSkyboxMat(Material newSkybox, bool inheritBaseSettings = false)
    {
        skybox = RenderSettings.skybox = newSkybox;
        if(inheritBaseSettings)
        {
            baseTint = newSkybox.GetColor("_Tint");
            baseExposure = newSkybox.GetFloat("_Exposure");
            baseRotation = newSkybox.GetFloat("_Rotation");
        }
    }

    private void LateUpdate()
    {
        //Smooth tint change
        targetModHSV = ComputeColourMix(modColours.ToArray()); //Get target RGBA first
        Color.RGBToHSV(targetModHSV, out targetModHSV.x, out targetModHSV.y, out targetModHSV.z); //Convert to HSVA
        if (float.IsNaN(targetModHSV.w)) targetModHSV.w = 0; //Get rid of possible NaNs in alpha value
        Vector4 finalModColour = currentModHSV = MoveTowardsHSVA(currentModHSV, targetModHSV, tintChangeSpeed*Time.deltaTime); //Interpolate towards target colour
        //Convert back to RGBA
        float alpha = finalModColour.w;
        finalModColour = Color.HSVToRGB(finalModColour.x, finalModColour.y, finalModColour.z);
        finalModColour.w = alpha;

        skybox.SetColor("_Tint", SetRGBABrightness(ColourToVec4(baseTint) + finalModColour*tintPower, GetRGBABrightness(baseTint))); //Keep tinted brightness at base brightness
        float totalExposure = baseExposure + modExp;
        if(totalExposure < 0f)
        {
            //totalExposure -= 15f; //Cool effect
            totalExposure = 0f;
        }
        skybox.SetFloat("_Exposure", totalExposure);
        skybox.SetFloat("_Rotation", baseRotation + modRot);

        lightSource.color = SetRGBABrightness(ColourToVec4(baseLightColour) + finalModColour*lightTintPower, GetRGBABrightness(baseLightColour)); //Keep tinted brightness at base brightness
        lightSource.intensity = baseLightIntensity * (baseExposure + modExp*lightExposureReactiveness);

        modColours.Clear();
        modExp = modRot = 0f;
    }
    
    public void AddModColour(Color colour)
    {
        modColours.Add(colour);
    }

    Vector4 ComputeColourMix(params Color[] colours)
    {
        Vector4 mixedColour = new Vector4();
        for(int i = 0; i < colours.Length; i++)
        {
            mixedColour.x += colours[i].r;
            mixedColour.y += colours[i].g;
            mixedColour.z += colours[i].b;
            mixedColour.w += colours[i].a;
        }
        float highestRGBValue = Mathf.Max(mixedColour.x, mixedColour.y, mixedColour.z);
        if (highestRGBValue > 1) mixedColour = new Vector4(mixedColour.x / highestRGBValue, mixedColour.y / highestRGBValue, mixedColour.z / highestRGBValue, mixedColour.w); //Scale back RGB so highest is 1.
        mixedColour.w /= colours.Length; //Make alpha average.
        return mixedColour;
    }

    Vector4 ColourToVec4(Color colour)
    {
        return new Vector4(colour.r, colour.g, colour.b, colour.a);
    }

    Vector4 MoveTowardsHSVA(Vector4 from, Vector4 to, float maxStep)
    {
        float hueAngle = Mathf.MoveTowardsAngle(from.x * 360f, to.x * 360f, maxStep * 360f);
        while(hueAngle < 0)
        {
            hueAngle += 360f;
        }
        return new Vector4(
            hueAngle / 360f,
            Mathf.MoveTowards(from.y, to.y, maxStep),
            Mathf.MoveTowards(from.z, to.z, maxStep),
            Mathf.MoveTowards(from.w, to.w, maxStep)
            );

    }

    Vector4 SetRGBABrightness(Vector4 RGBA, float brightness)
    {
        Vector4 hsva = RGBA;
        Color.RGBToHSV(RGBA, out hsva.x, out hsva.y, out hsva.z);
        hsva.z = brightness;
        RGBA = Color.HSVToRGB(hsva.x, hsva.y, hsva.z);
        RGBA.w = hsva.w;
        return RGBA;
    }

    float GetRGBABrightness(Vector4 RGBA)
    {
        return Mathf.Max(RGBA.x, RGBA.y, RGBA.z);
    }
}
