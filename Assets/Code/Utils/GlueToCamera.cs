using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueToCamera : GlueTransform
{
    void Start()
    {
        if(!GluedTo) GluedTo = Camera.main.transform;
    }
}
