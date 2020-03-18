using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModifiableComponent : MonoBehaviour, IModifiable
{
    public virtual void Modify(string field, object value)
    {
        return;
    }
}

public interface IModifiable
{
    void Modify(string field, object value);
}
