using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public sealed class CoroutineHelper_WaitForSecondsRealtime : IEnumerator
    {
        float mBeginTime;
        float mWaitSecondsTime;
        object IEnumerator.Current { get { return null; } }


        public CoroutineHelper_WaitForSecondsRealtime()
        {
        }

        public CoroutineHelper_WaitForSecondsRealtime Reset(float waitSecondTime)
        {
            mBeginTime = Time.unscaledTime;
            mWaitSecondsTime = waitSecondTime;

            return this;
        }

        bool IEnumerator.MoveNext()
        {
            return (Time.unscaledTime - mBeginTime) <= mWaitSecondsTime;
        }

        void IEnumerator.Reset()
        {
        }
    }
}

