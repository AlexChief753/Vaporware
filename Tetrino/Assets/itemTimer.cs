using UnityEngine;
using System.Collections;
using System;

public class itemTimer : MonoBehaviour
{
    public itemEffects itemeffects;

    public void Start()
    {
      //StartCoroutine(timer(15f));
    }

    //I used Action effect so I can pass a function through the coroutine
    public IEnumerator timer(float duration, Action effect)
    {
        Debug.Log(duration);
        //wait duration amount of seconds before doing something
        yield return new WaitForSeconds(duration);
        Debug.Log("done");
        //After the time is up, do the effect
        effect.Invoke();
    }

}
