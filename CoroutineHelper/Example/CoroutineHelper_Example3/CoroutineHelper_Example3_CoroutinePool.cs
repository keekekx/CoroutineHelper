using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont
{
    public class CoroutineHelper_Example3_CoroutinePool : MonoBehaviour
    {
        CoroutinePool mCoroutinePool;
        Stack<long> mCoroutineIDStack;


        void OnEnable()
        {
            mCoroutinePool = CoroutineHelper.Factory.CreateCoroutinePool(10);
            mCoroutinePool.Initialization();

            mCoroutineIDStack = new Stack<long>();
        }

        void OnGUI()
        {
            GUILayout.Box("mCoroutinePool.HasTaskRuning: " + mCoroutinePool.HasCoroutineRuning);
            GUILayout.Box("Task Wait Queue Count: " + mCoroutinePool.CoroutineWaitQueueCount);
            GUILayout.Box("Runing Task Count: " + mCoroutinePool.GetRuningCoroutineCount());
            GUILayout.Box("Idle Task Count: " + mCoroutinePool.GetIdleCoroutineCount());
            GUILayout.Box("HasCoroutineIdle: " + mCoroutinePool.HasCoroutineIdle);
            GUILayout.Box("HasCoroutineRuning: " + mCoroutinePool.HasCoroutineRuning);

            if (GUILayout.Button("Execute New Virtual Light Task"))
            {
                var r = mCoroutinePool.StartCoroutine(VirtualLightTask());
                mCoroutineIDStack.Push(r.CoroutineID);
            }

            if (GUILayout.Button("Execute New Virtual Heavy Task"))
            {
                var r = mCoroutinePool.StartCoroutine(VirtualHeavyTask());
                mCoroutineIDStack.Push(r.CoroutineID);
            }

            if (GUILayout.Button("Execute New Virtual Heavy Task(Can`t interruption)"))
            {
                var r = mCoroutinePool.StartCoroutine(VirtualHeavyTask2());
                mCoroutineIDStack.Push(r.CoroutineID);
            }

            if (GUILayout.Button("Test Stop Task"))
            {
                if (mCoroutineIDStack.Count > 0)
                {
                    var id = mCoroutineIDStack.Pop();
                    mCoroutinePool.StopCoroutine(id);
                }
            }

            if (GUILayout.Button("Test Immediate Stop Task"))
            {
                if (mCoroutineIDStack.Count > 0)
                {
                    var id = mCoroutineIDStack.Pop();
                    mCoroutinePool.ImmediateStopCoroutine(id);
                }
            }
        }

        IEnumerator VirtualLightTask()
        {
            var waitForSeconds = CoroutineHelper.Pool_WaitForSeconds.Spawn();
            yield return waitForSeconds.Reset(1f);
            CoroutineHelper.Pool_WaitForSeconds.Despawn(waitForSeconds);
        }

        IEnumerator VirtualHeavyTask()
        {
            for (int i = 0; i < 200; i++)
            {
                var waitForSeconds = CoroutineHelper.Pool_WaitForSeconds.Spawn();
                yield return waitForSeconds.Reset(0.1f);
                CoroutineHelper.Pool_WaitForSeconds.Despawn(waitForSeconds);
            }
        }

        IEnumerator VirtualHeavyTask2()
        {
            var waitForSeconds = CoroutineHelper.Pool_WaitForSeconds.Spawn();
            yield return waitForSeconds.Reset(20f);
            CoroutineHelper.Pool_WaitForSeconds.Despawn(waitForSeconds);
        }
    }
}
