using Hont.CoroutineHelperInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public sealed class CoroutineHelper_Factory
    {
        /// <summary>
        /// 创建一个协程组，可以自由开关组内协程。
        /// </summary>
        /// <param name="capacity">协程组内置List初始缓存大小</param>
        public CoroutineGroup CreateCoroutineGroup(int capacity = 10)
        {
            var type = typeof(CoroutineGroup);

            var constructorInfoArray = type.GetConstructors(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public);

            return (CoroutineGroup)constructorInfoArray[0].Invoke(new object[] { capacity });
        }

        /// <summary>
        /// 创建一个协程池。
        /// </summary>
        /// <param name="poolSize">池大小</param>
        /// <param name="waitQueueCapacity">等待队列的初始缓存大小</param>
        public CoroutinePool CreateCoroutinePool(int poolSize, int waitQueueCapacity = 3)
        {
            var type = typeof(CoroutinePool);

            var constructorInfoArray = type.GetConstructors(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public);

            return (CoroutinePool)constructorInfoArray[0].Invoke(new object[] { poolSize, waitQueueCapacity });
        }
    }
}
