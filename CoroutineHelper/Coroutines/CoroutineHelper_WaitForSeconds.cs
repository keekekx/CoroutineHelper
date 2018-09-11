using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public sealed class CoroutineHelper_WaitForSeconds : IEnumerator
    {
        float mBeginTime;
        float mWaitSecondsTime;
        object IEnumerator.Current { get { return null; } }


        public CoroutineHelper_WaitForSeconds()
        {
        }

        public CoroutineHelper_WaitForSeconds Reset(float waitSecondTime)
        {
            mBeginTime = Time.time;
            mWaitSecondsTime = waitSecondTime;

            return this;
        }

        bool IEnumerator.MoveNext()
        {
            return (Time.time - mBeginTime) <= mWaitSecondsTime;
        }

        void IEnumerator.Reset()
        {
        }
    }
}

