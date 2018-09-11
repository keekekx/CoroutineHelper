using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public class CoroutineHelper_Debuger : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.Box("Coroutine Count: " + CoroutineHelper.CoroutineCount);
            GUILayout.Box("Pool Total Size: " + CoroutineHelper.PoolTotalSize);
        }
    }
}
