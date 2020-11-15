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

        /// <summary>
        /// 协程助手实例是否有效。
        /// </summary>
        public static bool InstanceValid { get { return (bool)CoroutineRunner.Instance; } }


        static CoroutineHelper()
        {
            mPool_WaitForSecondsRealtime = new CoroutineHelper_SimplePool<CoroutineHelper_WaitForSecondsRealtime>(128, () => new CoroutineHelper_WaitForSecondsRealtime());
            mPool_WaitForSeconds = new CoroutineHelper_SimplePool<CoroutineHelper_WaitForSeconds>(128, () => new CoroutineHelper_WaitForSeconds());
            mPool_WaitUntil = new CoroutineHelper_SimplePool<CoroutineHelper_WaitUntil>(128, () => new CoroutineHelper_WaitUntil());
            mPool_WaitWhile = new CoroutineHelper_SimplePool<CoroutineHelper_WaitWhile>(128, () => new CoroutineHelper_WaitWhile());
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
        /// 等待一定秒数(协程池处理)
        /// </summary>
        /// <param name="waitSecondTime">等待的秒数</param>
        public static IEnumerator WaitForSeconds(float waitSecondTime)
        {
            CoroutineHelper_WaitForSeconds waitForSeconds = mPool_WaitForSeconds.Spawn();
            yield return waitForSeconds.Reset(waitSecondTime);
            mPool_WaitForSeconds.Despawn(waitForSeconds);
        }

        /// <summary>
        /// 等待一定的真实秒数(协程池处理)
        /// </summary>
        /// <param name="waitSecondTime">等待的秒数</param>
        public static IEnumerator WaitForSecondsRealtime(float waitSecondTime)
        {
            CoroutineHelper_WaitForSecondsRealtime waitForSecondsRealtime = mPool_WaitForSecondsRealtime.Spawn();
            yield return waitForSecondsRealtime.Reset(waitSecondTime);
            mPool_WaitForSecondsRealtime.Despawn(waitForSecondsRealtime);
        }

        /// <summary>
        /// 等待指定条件直到为true(协程池处理)
        /// </summary>
        /// <param name="waitUntilFunc">等待的指定条件</param>
        public static IEnumerator WaitUntil(Func<bool> waitUntilFunc)
        {
            CoroutineHelper_WaitUntil waitUntil = mPool_WaitUntil.Spawn();
            yield return waitUntil.Reset(waitUntilFunc);
            mPool_WaitUntil.Despawn(waitUntil);
        }

        /// <summary>
        /// 等待指定条件直到为false(协程池处理)
        /// </summary>
        /// <param name="waitWhileFunc">等待的指定条件</param>
        public static IEnumerator WaitWhile(Func<bool> waitWhileFunc)
        {
            CoroutineHelper_WaitWhile waitWhile = mPool_WaitWhile.Spawn();
            yield return waitWhile.Reset(waitWhileFunc);
            mPool_WaitWhile.Despawn(waitWhile);
        }

        /// <summary>
        /// 延迟一帧调用目标内容，并返回协程对象(通过对象自身协程开启）。
        /// </summary>
        public static Coroutine DelayNextFrameInvoke(MonoBehaviour container, Action action)
        {
            return container.StartCoroutine(DelayFrameFunc(action));
        }

        /// <summary>
        /// 延迟一帧调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine DelayNextFrameInvoke(Action action)
        {
            return StartCoroutine(DelayFrameFunc(action));
        }

        /// <summary>
        /// 延迟一段秒数后调用目标内容，并返回协程对象(通过对象自身协程开启）。
        /// </summary>
        public static Coroutine DelayInvoke(MonoBehaviour container, Action action, float seconds, bool ignoreTimeScale = false)
        {
            return container.StartCoroutine(DelaySecondsFunc(action, seconds, ignoreTimeScale));
        }

        /// <summary>
        /// 延迟一段秒数后调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine DelayInvoke(Action action, float seconds, bool ignoreTimeScale = false)
        {
            return StartCoroutine(DelaySecondsFunc(action, seconds, ignoreTimeScale));
        }

        /// <summary>
        /// 等待直到条件满足调用目标内容，并返回协程对象(通过对象自身协程开启）。
        /// </summary>
        public static Coroutine WaitUntil(MonoBehaviour container, Func<bool> condition, Action action)
        {
            return container.StartCoroutine(WaitUntilFunc(condition, action));
        }

        /// <summary>
        /// 等待直到条件满足调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine WaitUntil(Func<bool> condition, Action action)
        {
            return StartCoroutine(WaitUntilFunc(condition, action));
        }

        /// <summary>
        /// 等待直到条件返回False调用目标内容，并返回协程对象(通过对象自身协程开启）。
        /// </summary>
        public static Coroutine WaitWhile(MonoBehaviour container, Func<bool> condition, Action action)
        {
            return container.StartCoroutine(WaitWhileFunc(condition, action));
        }

        /// <summary>
        /// 等待直到条件返回False调用目标内容，并返回协程对象。
        /// </summary>
        public static Coroutine WaitWhile(Func<bool> condition, Action action)
        {
            return StartCoroutine(WaitWhileFunc(condition, action));
        }

        /// <summary>
        /// 增加协程执行完成后操作
        /// </summary>
        public static IEnumerator Callback(IEnumerator x, Action onCallback)
        {
            yield return x;
            onCallback();
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
