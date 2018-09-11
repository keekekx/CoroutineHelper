using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public class CoroutineHelper_Example1_UnityCoroutine : MonoBehaviour
    {
        void OnEnable()
        {
            StartCoroutine(A());
        }

        IEnumerator A()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                B();
            }
        }

        void B()
        {
        }
    }
}
