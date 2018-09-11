using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.CoroutineHelperInternal
{
    internal sealed class CoroutineRunner : MonoBehaviour
    {
        static CoroutineRunner mInstance;
        public static CoroutineRunner Instance
        {
            get
            {
                if (Application.isPlaying && mInstance == null)
                {
                    mInstance = new GameObject("[CoroutineRunner]").AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(mInstance.gameObject);
                }

                return mInstance;
            }
        }
    }
}
