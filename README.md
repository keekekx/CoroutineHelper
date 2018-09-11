# CoroutineHelper

[![Version](](https://codecov.io/gh/ninject/Ninject)

CoroutineHelper is a unity3d coroutine tool collections.
协程助手是一个Unity3D的协程工具集合。


-Common:
Non-MonoBehaviour class start coroutine:
```C#
CoroutineHelper.StartCoroutine(...)
```

And packaged some common extensions
```C#
CoroutineHelper.DelayInvoke(() => Debug.Log("foo!"), 2f);
CoroutineHelper.DelayNextFrameInvoke(() => Debug.Log("foo!"));
```

-CoroutineGroup:
or like the MonoBehaviour class, you can ‘StopAllCoroutine’ through CoroutineGroup:
```C#
void Test()
{
    var coroutineGroup = CoroutineHelper.Factory.CreateCoroutineGroup();
    coroutineGroup.StartCoroutine(Foo());
    coroutineGroup.StartCoroutine(Foo());
    coroutineGroup.StartCoroutine(Foo());
    coroutineGroup.StartCoroutine(Foo());

    coroutineGroup.StopAllCoroutines();
}

IEnumerator Foo() { yield break; }
```

-CoroutinePool:
You can also create a coroutine pool to avoid the GC of StartCoroutine,
but the coroutine pool is more troublesome to use.

```C#
var coroutinePool = CoroutineHelper.Factory.CreateCoroutinePool(10);
coroutinePool.TryStartCoroutineInPool(Foo());//If pool is full, execute it immediately with StartCoroutine.

if (coroutinePool.HasCoroutineIdle)
    coroutinePool.StartCoroutine(Foo());//Or like this.
```

-InternalPool:
and you can avoid GC with some internal pools.

```C#
IEnumerator Foo()
{
    yield return new WaitForSeconds(1f);//wait 1 sec, has GC.

    var waitForSeconds = CoroutineHelper.Pool_WaitForSeconds.Spawn();
    yield return waitForSeconds.Reset(1f);
    CoroutineHelper.Pool_WaitForSeconds.Despawn(waitForSeconds);//wait 1 sec with CoroutineHelper, no GC.
}
```
