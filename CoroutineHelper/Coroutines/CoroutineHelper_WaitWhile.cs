using System;
using System.Collections;
using System.Collections.Generic;

namespace Hont
{
    public sealed class CoroutineHelper_WaitWhile : IEnumerator
    {
        Func<bool> mWaitWhileFunc;
        object IEnumerator.Current { get { return null; } }


        public CoroutineHelper_WaitWhile()
        {
        }

        public CoroutineHelper_WaitWhile Reset(Func<bool> waitWhileFunc)
        {
            mWaitWhileFunc = waitWhileFunc;

            return this;
        }

        bool IEnumerator.MoveNext()
        {
            return mWaitWhileFunc();
        }

        void IEnumerator.Reset()
        {
        }
    }
}

