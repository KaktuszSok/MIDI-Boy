using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEventRelay : MonoBehaviour
{
    public delegate void RenderImageHandler(RenderTexture source, RenderTexture destination);
    public event RenderImageHandler RenderImageEvent;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderImageEvent(source, destination);
    }
}
