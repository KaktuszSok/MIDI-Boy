using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{

    public bool activateOnStart = false;
    public float lifetime = 5f;

    // Start is called before the first frame update
    void Start()
    {
        DestroyAfterTime(lifetime);
    }

    public void DestroyAfterTime(float time)
    {
        StartCoroutine(AutoDestroyCoroutine(time));
    }

    IEnumerator AutoDestroyCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
