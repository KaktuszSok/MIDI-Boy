using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueTransform : MonoBehaviour
{
    public Transform GluedTo;
    public bool position = true;
    public bool rotation = true;
    public bool scale = true;

    private void LateUpdate()
    {
        if(position) transform.position = GluedTo.position;
        if(rotation) transform.rotation = GluedTo.rotation;
        if(scale) transform.localScale = GluedTo.localScale;
    }
}
