using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMoveForward : ModifiableComponent
{
    VisualisationManager visualisation;

    public float baseVelocity = 5f;
    public float maxAcceleration = 64f; //Set to 0 for instant accel

    public float velocityMod = 1f;
    public float targetVelocity;
    public float currentVelocity;
    float pastVelocity;
    float lastVelChangeTime;
    float accelTime;

    private void Start()
    {
        visualisation = VisualisationManager.instance;

        pastVelocity = currentVelocity;
        lastVelChangeTime = visualisation.time;
        accelTime = Mathf.Abs(targetVelocity - pastVelocity) / maxAcceleration;
    }

    void Update()
    {
        if (visualisation.playing)
        {
            if (accelTime != 0 && maxAcceleration != 0)
            {
                float accelProgress = (visualisation.time - lastVelChangeTime) / accelTime; //Time elapsed divided by total acceleration time (deltaV/accel).
                currentVelocity = Mathf.SmoothStep(pastVelocity, targetVelocity, accelProgress);
            }
            else
            {
                currentVelocity = targetVelocity;
            }
            transform.Translate(Vector3.forward * currentVelocity * Time.deltaTime, Space.Self);
        }
    }

    public override void Modify(string field, object value)
    {
        switch(field)
        {
            case "Velocity":
                velocityMod = (float)value;
                pastVelocity = currentVelocity; //Remember past velocity for interpolation purposes.
                targetVelocity = baseVelocity*velocityMod;
                lastVelChangeTime = visualisation.time;
                accelTime = Mathf.Abs(targetVelocity - pastVelocity) / maxAcceleration;
                break;
            default:
                Debug.LogWarning("Tried to modify invalid field '" + field + "' on " + transform.name + ".", this);
                break;
        }
    }
}
