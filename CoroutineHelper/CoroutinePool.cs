using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public sealed class CoroutinePool
    {
        public delegate Coroutine CoroutineCreator(IEnumerator enumerator);

        public delegate void CoroutineTeminator(Coroutine coroutine);

        public enum ExecuteResultEnum { ImmediateExecute, QueueWait }

        public struct CoroutineExecuteResult
        {
            public long CoroutineID { get; set; }
            public ExecuteResultEnum ExecuteResult { get; set; }
        }

        struct CoroutineQueueItem
        {
            public long CoroutineID { get; set; }
            public IEnumerator Enumerator { get; set; }
        }

        struct CoroutineInfo
        {
            public Coroutine Coroutine { get; set; }
            public Task Task { get; set; }
        }

        class Task
        {
            long mCurrentTaskID;
            IEnumerator mCurrentTask;
            bool mHasAbortRequire;
            public Action OnTaskFinished;

            public bool IsIdle { get { return mCurrentTask == null; } }
            public long? CurrentCoroutineID { get { return IsIdle ? null : (long?)mCurrentTaskID; } }


            public void Init()
            {
                mHasAbortRequire = false;
                mCurrentTask = null;
            }

            public void SetTask(IEnumerator task, long taskID)
            {
                mCurrentTask = task;
                mCurrentTaskID = taskID;
            }

            public IEnumerator Tick()
            {
                while (true)
                {
                    if (mCurrentTask != null)
                    {
                        while (mCurrentTask != null && mCurrentTask.MoveNext())
                        {
                            if (mHasAbortRequire)
                            {
                                mHasAbortRequire = false;
                                break;
                            }

                            yield return mCurrentTask.Current;
                        }

                        mCurrentTask = null;
                        if (OnTaskFinished != null) OnTaskFinished();

                    }//Process task.

                    yield return null;
                }
            }

            public void Abort()
            {
                mHasAbortRequire = true;
            }
        }

        CoroutineTeminator mCoroutineTeminator;
        CoroutineCreator mCoroutineCreator;
        CoroutineInfo[] mCoroutinesArray;
        List<CoroutineQueueItem> mWaitQueue;
        long mCoroutineIDAccumulator;
        bool mIsInitialized;

        /// <summary>
        /// 是否已初始化。
        /// </summary>
        public bool IsInitialized { get { return mIsInitialized; } }

        /// <summary>
        /// 池的大小。
        /// </summary>
        public int PoolSize { get { return mCoroutinesArray.Length; } }

        /// <summary>
        /// 当前等待队列的任务数量。
        /// </summary>
        public int CoroutineWaitQueueCount { get { return mWaitQueue.Count; } }

        /// <summary>
        /// 当前是否有运行中的任务。
        /// </summary>
        public bool HasCoroutineRuning
        {
            get
            {
                for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
                    if (!mCoroutinesArray[i].Task.IsIdle) return true;

                return false;
            }
        }

        /// <summary>
        /// 当前是否有空闲协程。
        /// </summary>
        public bool HasCoroutineIdle
        {
            get
            {
                for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
                    if (mCoroutinesArray[i].Task.IsIdle) return true;

                return false;
            }
        }


        private CoroutinePool(int poolSize, int waitQueueCapacity = 3)
        {
            mCoroutinesArray = new CoroutineInfo[poolSize];
            mWaitQueue = new List<CoroutineQueueItem>(waitQueueCapacity);
            mCoroutineIDAccumulator = long.MinValue;
        }

        /// <summary>
        /// 初始化协程。
        /// </summary>
        /// <param name="customCoroutineCreator">自定义协程创建器，若为空则调用全局协程</param>
        /// <param name="customCoroutineTeminator">自定义协程终止器，若为空则调用全局协程</param>
        public void Initialization(CoroutineCreator customCoroutineCreator = null, CoroutineTeminator customCoroutineTeminator = null)
        {
            mCoroutineCreator = customCoroutineCreator;
            mCoroutineTeminator = customCoroutineTeminator;

            if (mCoroutineCreator == null)
                mCoroutineCreator = (enumerator) => CoroutineHelper.StartCoroutine(enumerator);

            if (mCoroutineTeminator == null)
                mCoroutineTeminator = (enumerator) => CoroutineHelper.StopCoroutine(enumerator);

            for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
            {
                var coroutineInfo = new CoroutineInfo();
                coroutineInfo.Task = new Task();
                coroutineInfo.Task.Init();
                coroutineInfo.Task.OnTaskFinished = DetectTaskWaitCoroutine;

                var coroutine = mCoroutineCreator(coroutineInfo.Task.Tick());

                coroutineInfo.Coroutine = coroutine;

                mCoroutinesArray[i] = coroutineInfo;
            }

            mIsInitialized = true;
        }

        /// <summary>
        /// 尝试开始一个新的协程，如果当前协程池已满则立即创建协程执行。
        /// </summary>
        public void TryStartCoroutineInPool(IEnumerator enumerator)
        {
            if (HasCoroutineIdle)
            {
                StartCoroutine(enumerator);
            }
            else
            {
                mCoroutineCreator(enumerator);
            }
        }

        /// <summary>
        /// 开始一个新的协程。
        /// </summary>
        /// <returns>返回协程ID以及执行状态</returns>
        public CoroutineExecuteResult StartCoroutine(IEnumerator enumerator)
        {
            var result = new CoroutineExecuteResult();

            var targetTask = default(Task);
            for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
            {
                var coroutineInfo = mCoroutinesArray[i];
                var task = coroutineInfo.Task;

                if (task.IsIdle)
                {
                    targetTask = task;
                    break;
                }
            }

            if (targetTask == null)
            {
                mWaitQueue.Add(new CoroutineQueueItem() { CoroutineID = mCoroutineIDAccumulator, Enumerator = enumerator });

                result = new CoroutineExecuteResult() { CoroutineID = mCoroutineIDAccumulator, ExecuteResult = ExecuteResultEnum.QueueWait };
            }
            else
            {
                targetTask.SetTask(enumerator, mCoroutineIDAccumulator);
                result = new CoroutineExecuteResult() { CoroutineID = mCoroutineIDAccumulator, ExecuteResult = ExecuteResultEnum.ImmediateExecute };
            }

            if (mCoroutineIDAccumulator + 1 == long.MaxValue)
                mCoroutineIDAccumulator = long.MinValue;

            mCoroutineIDAccumulator++;

            return result;
        }

        /// <summary>
        /// 依据ID停止协程，且包含等待队列(在当前yield完毕后)。
        /// </summary>
        public bool StopCoroutine(long coroutineID)
        {
            var result = false;

            if (mWaitQueue.Count > 0)
            {
                for (int i = 0, iMax = mWaitQueue.Count; i < iMax; i++)
                {
                    var item = mWaitQueue[i];

                    if (item.CoroutineID == coroutineID)
                    {
                        result = true;
                        mWaitQueue.RemoveAt(i);
                        break;
                    }
                }
            }

            if (!result)
            {
                var targetTask = default(Task);
                for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
                {
                    var coroutineInfo = mCoroutinesArray[i];

                    if (coroutineInfo.Task.CurrentCoroutineID != null
                        && coroutineInfo.Task.CurrentCoroutineID.Value == coroutineID)
                    {
                        targetTask = coroutineInfo.Task;
                        break;
                    }
                }

                if (targetTask != null)
                {
                    targetTask.Abort();
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 依据ID立即停止协程，且包含等待队列。
        /// </summary>
        public void ImmediateStopCoroutine(long coroutineID)
        {
            if (mWaitQueue.Count > 0)
            {
                for (int i = 0, iMax = mWaitQueue.Count; i < iMax; i++)
                {
                    var item = mWaitQueue[i];

                    if (item.CoroutineID == coroutineID)
                    {
                        mWaitQueue.RemoveAt(i);
                        return;
                    }
                }
            }

            var targetCoroutineInfo = default(CoroutineInfo);
            for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
            {
                var coroutineInfo = mCoroutinesArray[i];

                if (coroutineInfo.Task.CurrentCoroutineID != null
                && coroutineInfo.Task.CurrentCoroutineID.Value == coroutineID)
                {
                    targetCoroutineInfo = coroutineInfo;
                    break;
                }
            }

            if (targetCoroutineInfo.Coroutine != null)
            {
                mCoroutineTeminator(targetCoroutineInfo.Coroutine);

                targetCoroutineInfo.Task.Init();
                targetCoroutineInfo.Task.OnTaskFinished = DetectTaskWaitCoroutine;
                var coroutine = mCoroutineCreator(targetCoroutineInfo.Task.Tick());
                targetCoroutineInfo.Coroutine = coroutine;
            }
        }

        /// <summary>
        /// 立即停止所有协程，且包含等待队列。
        /// </summary>
        public void ImmediateStopAllCoroutines()
        {
            for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
            {
                var coroutineInfo = mCoroutinesArray[i];

                mCoroutineTeminator(coroutineInfo.Coroutine);

                coroutineInfo.Task.Init();
                coroutineInfo.Task.OnTaskFinished = DetectTaskWaitCoroutine;
                var coroutine = mCoroutineCreator(coroutineInfo.Task.Tick());
                coroutineInfo.Coroutine = coroutine;
            }

            mWaitQueue.Clear();
        }

        /// <summary>
        /// 获得当前运行中的协程数。
        /// </summary>
        public int GetRuningCoroutineCount()
        {
            int result = 0;

            for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
            {
                var taskInfo = mCoroutinesArray[i];

                if (!taskInfo.Task.IsIdle)
                    result++;
            }

            return result;
        }

        /// <summary>
        /// 获得当前空闲的协程数。
        /// </summary>
        public int GetIdleCoroutineCount()
        {
            return mCoroutinesArray.Length - GetRuningCoroutineCount();
        }

        void DetectTaskWaitCoroutine()
        {
            if (mWaitQueue.Count > 0)
            {
                var targetTask = default(Task);
                for (int i = 0, iMax = mCoroutinesArray.Length; i < iMax; i++)
                {
                    var taskInfo = mCoroutinesArray[i];

                    if (taskInfo.Task.IsIdle)
                    {
                        targetTask = taskInfo.Task;
                        break;
                    }
                }

                if (targetTask != null)
                {
                    var item = mWaitQueue[0];
                    mWaitQueue.RemoveAt(0);

                    targetTask.SetTask(item.Enumerator, mCoroutineIDAccumulator);
                }
            }
        }
    }
}
