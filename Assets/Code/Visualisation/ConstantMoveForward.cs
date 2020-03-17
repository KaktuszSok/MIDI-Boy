using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMoveForward : MonoBehaviour
{
    public float velocity = 5f;

    void Update()
    {
        transform.Translate(Vector3.forward * velocity * Time.deltaTime, Space.Self);
    }
}
