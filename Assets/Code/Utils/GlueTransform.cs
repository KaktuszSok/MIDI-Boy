using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueTransform : MonoBehaviour
{
    public Transform GluedTo;
    public bool scale = true;

    private void LateUpdate()
    {
        transform.position = GluedTo.position;
        transform.rotation = GluedTo.rotation;
        if(scale) transform.localScale = GluedTo.localScale;
    }
}
