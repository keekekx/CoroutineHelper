using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public class CoroutineHelper_Example2_CoroutineGroup : MonoBehaviour
    {
        CoroutineGroup mCoroutineGroup;


        void OnEnable()
        {
            mCoroutineGroup = CoroutineHelper.Factory.CreateCoroutineGroup();

            for (int i = 0; i < 20; i++)
                mCoroutineGroup.StartCoroutine(Foo());
        }

        void OnDisable()
        {
            mCoroutineGroup.StopAllCoroutines();
        }

        void OnGUI()
        {
            GUILayout.Box("CoroutineGroup: " + mCoroutineGroup.CoroutineCount);
        }

        IEnumerator Foo()
        {
            while (true)
            {
                Debug.Log("Foo");

                yield return null;
            }
        }
    }
}
