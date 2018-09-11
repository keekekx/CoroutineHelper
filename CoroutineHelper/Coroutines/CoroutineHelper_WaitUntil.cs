using System;
using System.Collections;
using System.Collections.Generic;

namespace Hont
{
    public sealed class CoroutineHelper_WaitUntil : IEnumerator
    {
        Func<bool> mWaitUntilFunc;
        object IEnumerator.Current { get { return null; } }


        public CoroutineHelper_WaitUntil()
        {
        }

        public CoroutineHelper_WaitUntil Reset(Func<bool> waitUntilFunc)
        {
            mWaitUntilFunc = waitUntilFunc;

            return this;
        }

        bool IEnumerator.MoveNext()
        {
            return !mWaitUntilFunc();
        }

        void IEnumerator.Reset()
        {
        }
    }
}

