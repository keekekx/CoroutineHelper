using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    /// <summary>
    /// 通过协程组控制组内的协程，从而避免全局性的调用。
    /// </summary>
    public sealed class CoroutineGroup
    {
        List<Coroutine> mCoroutineList;

        /// <summary>
        /// 当前协程组里运行的协程数量。
        /// </summary>
        public long CoroutineCount { get { return mCoroutineList.Count; } }


        private CoroutineGroup(int capacity)
        {
            mCoroutineList = new List<Coroutine>(capacity);
        }

        /// <summary>
        /// 开启一个新的协程，并返回协程对象。
        /// </summary>
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            var coroutine = CoroutineHelper.StartCoroutine(routine);
            mCoroutineList.Add(coroutine);

            return coroutine;
        }

        /// <summary>
        /// 停止一个创建出的协程。
        /// </summary>
        public void StopCoroutine(Coroutine routine)
        {
            mCoroutineList.Remove(routine);
            CoroutineHelper.StopCoroutine(routine);
        }

        /// <summary>
        /// 停止组内所有协程。
        /// </summary>
        public void StopAllCoroutines()
        {
            for (int i = 0, iMax = mCoroutineList.Count; i < iMax; i++)
            {
                var coroutine = mCoroutineList[i];
                CoroutineHelper.StopCoroutine(coroutine);
            }

            mCoroutineList.Clear();
        }

        /// <summary>
        /// 延迟一帧调用目标内容，并返回协程对象。
        /// </summary>
        public Coroutine DelayNextFrameInvoke(Action action)
        {
            var coroutine = CoroutineHelper.DelayNextFrameInvoke(action);
            mCoroutineList.Add(coroutine);

            return coroutine;
        }

        /// <summary>
        /// 延迟一段秒数后调用目标内容，并返回协程对象。
        /// </summary>
        public Coroutine DelayInvoke(Action action, float seconds, bool ignoreTimeScale = false)
        {
            var coroutine = CoroutineHelper.DelayInvoke(action, seconds, ignoreTimeScale);

            mCoroutineList.Add(coroutine);

            return coroutine;
        }

        /// <summary>
        /// 等待直到条件满足调用目标内容，并返回协程对象。
        /// </summary>
        public Coroutine WaitUntil(Func<bool> condition, Action action)
        {
            var coroutine = CoroutineHelper.WaitUntil(condition, action);
            mCoroutineList.Add(coroutine);

            return coroutine;
        }

        /// <summary>
        /// 等待直到条件返回False调用目标内容，并返回协程对象。
        /// </summary>
        public Coroutine WaitWhile(Func<bool> condition, Action action)
        {
            var coroutine = CoroutineHelper.WaitWhile(condition, action);
            mCoroutineList.Add(coroutine);

            return coroutine;
        }
    }
}
