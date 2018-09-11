using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.CoroutineHelperInternal;

namespace Hont
{
    public static partial class CoroutineHelper
    {
        //注:WaitForEndOfFrame通常只在特殊情况下使用，故不做封装处理。
        static readonly CoroutineHelper_SimplePool<CoroutineHelper_WaitForSecondsRealtime> mPool_WaitForSecondsRealtime;
        static readonly CoroutineHelper_SimplePool<CoroutineHelper_WaitForSeconds> mPool_WaitForSeconds;
        static readonly CoroutineHelper_SimplePool<CoroutineHelper_WaitUntil> mPool_WaitUntil;
        static readonly CoroutineHelper_SimplePool<CoroutineHelper_WaitWhile> mPool_WaitWhile;

        public static CoroutineHelper_SimplePool<CoroutineHelper_WaitForSecondsRealtime> Pool_WaitForSecondsRealtime { get { return mPool_WaitForSecondsRealtime; } }
        public static CoroutineHelper_SimplePool<CoroutineHelper_WaitForSeconds> Pool_WaitForSeconds { get { return mPool_WaitForSeconds; } }
        public static CoroutineHelper_SimplePool<CoroutineHelper_WaitUntil> Pool_WaitUntil { get { return mPool_WaitUntil; } }
        public static CoroutineHelper_SimplePool<CoroutineHelper_WaitWhile> Pool_WaitWhile { get { return mPool_WaitWhile; } }

        static CoroutineHelper_Factory mFactory;
        static long mCoroutineCount;

        /// <summary>
        /// 获得内部池的总数量，一般非调试时不需要使用。
        /// </summary>
        public static long PoolTotalSize
        {
            get
            {
                return mPool_WaitForSecondsRealtime.QueueCount
                    + mPool_WaitForSeconds.QueueCount
                    + mPool_WaitUntil.QueueCount
                    + mPool_WaitWhile.QueueCount;
            }
        }

        /// <summary>
        /// 当前被CoroutineHelper托管的所有协程数量。
        /// </summary>
        public static long CoroutineCount { get { return mCoroutineCount; } }

        /// <summary>
        /// 协程相关类工厂。
        /// </summary>
        public static CoroutineHelper_Factory Factory { get { return mFactory; } }


        static CoroutineHelper()
        {
            mPool_WaitForSecondsRealtime = new CoroutineHelper_SimplePool<CoroutineHelper_WaitForSecondsRealtime>(100, () => new CoroutineHelper_WaitForSecondsRealtime());
            mPool_WaitForSeconds = new CoroutineHelper_SimplePool<CoroutineHelper_WaitForSeconds>(100, () => new CoroutineHelper_WaitForSeconds());
            mPool_WaitUntil = new CoroutineHelper_SimplePool<CoroutineHelper_WaitUntil>(100, () => new CoroutineHelper_WaitUntil());
            mPool_WaitWhile = new CoroutineHelper_SimplePool<CoroutineHelper_WaitWhile>(100, () => new CoroutineHelper_WaitWhile());
            mFactory = new CoroutineHelper_Factory();
        }

        /// <summary>
        /// 获得全局协程的MonoBehaviour对象。
        /// </summary>
        public static MonoBehaviour GetCoroutineMonoBehaviour()
        {
            return CoroutineRunner.Instance;
        }

        /// <summary>
        /// 运行一个新的协程(包含协程统计)。
        /// </summary>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return CoroutineRunner.Instance.StartCoroutine(CoroutineStatistics(routine));
        }

        /// <summary>
        /// 停止一个协程。
        /// </summary>
        public static void StopCoroutine(Coroutine routine)
        {
            mCoroutineCount--;
            CoroutineRunner.Instance.StopCoroutine(routine);
        }

        /// <summary>
        /// 延迟一帧调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine DelayNextFrameInvoke(Action action)
        {
            return StartCoroutine(DelayFrameFunc(action));
        }

        /// <summary>
        /// 延迟一段秒数后调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine DelayInvoke(Action action, float seconds, bool ignoreTimeScale = false)
        {
            return StartCoroutine(DelaySecondsFunc(action, seconds, ignoreTimeScale));
        }

        /// <summary>
        /// 等待直到条件满足调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine WaitUntil(Func<bool> condition, Action action)
        {
            return StartCoroutine(WaitUntilFunc(condition, action));
        }

        /// <summary>
        /// 等待直到条件返回False调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine WaitWhile(Func<bool> condition, Action action)
        {
            return StartCoroutine(WaitWhileFunc(condition, action));
        }

        static IEnumerator DelayFrameFunc(Action action)
        {
            yield return null;

            if (action != null) action();
        }

        static IEnumerator DelaySecondsFunc(Action action, float seconds, bool ignoreTimeScale)
        {
            if (!ignoreTimeScale)
            {
                var waitForSeconds = mPool_WaitForSeconds.Spawn();
                waitForSeconds.Reset(seconds);
                yield return waitForSeconds;

                if (action != null) action();

                mPool_WaitForSeconds.Despawn(waitForSeconds);
            }
            else
            {
                var waitForSecondsRealtime = mPool_WaitForSecondsRealtime.Spawn();
                yield return waitForSecondsRealtime.Reset(seconds);

                if (action != null) action();

                mPool_WaitForSecondsRealtime.Despawn(waitForSecondsRealtime);
            }
        }

        static IEnumerator WaitUntilFunc(Func<bool> waitUntilFunc, Action action)
        {
            var waitUntil = mPool_WaitUntil.Spawn();
            yield return waitUntil.Reset(waitUntilFunc);

            if (action != null) action();

            mPool_WaitUntil.Despawn(waitUntil);
        }

        static IEnumerator WaitWhileFunc(Func<bool> waitWhileFunc, Action action)
        {
            var waitWhile = mPool_WaitWhile.Spawn();
            yield return waitWhile.Reset(waitWhileFunc);

            if (action != null) action();

            mPool_WaitWhile.Despawn(waitWhile);
        }

        static IEnumerator CoroutineStatistics(IEnumerator enumerator)
        {
            mCoroutineCount++;
            yield return enumerator;
            mCoroutineCount--;
        }
    }
}
