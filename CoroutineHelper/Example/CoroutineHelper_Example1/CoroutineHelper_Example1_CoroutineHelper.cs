using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont;

public class CoroutineHelper_Example1_CoroutineHelper : MonoBehaviour
{
    Coroutine mCoroutine;


    void OnEnable()
    {
        mCoroutine = CoroutineHelper.StartCoroutine(A());
    }

    void OnDisable()
    {
        CoroutineHelper.StopCoroutine(mCoroutine);
    }

    IEnumerator A()
    {
        var waitForSecond = CoroutineHelper.Pool_WaitForSeconds.Spawn();
        waitForSecond.Reset(0.1f);

        while (true)
        {
            yield return waitForSecond;
            B();
        }
    }

    void B()
    {
    }
}
