# CoroutineHelper

CoroutineHelper is a unity3d coroutine tool collections.
</br>协程助手是一个Unity3D的协程工具集合。


### Common:
Non-MonoBehaviour class start coroutine
</br>你可以给非MonoBehaviour类开启协程
```C#
CoroutineHelper.StartCoroutine(...)
```

And packaged some common extensions
</br>并且封装了一些常用扩展
```C#
CoroutineHelper.DelayInvoke(() => Debug.Log("foo!"), 2f);
CoroutineHelper.DelayNextFrameInvoke(() => Debug.Log("foo!"));
```

### CoroutineGroup:
or like the MonoBehaviour class, you can ‘StopAllCoroutine’ through CoroutineGroup
</br>或者像MonoBehaviour类一样，你可以用StopAllCoroutine来关闭当前Mono运行的所有协程通过协程组
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

### CoroutinePool:
You can also create a coroutine pool to avoid the GC of StartCoroutine,
but the coroutine pool is more troublesome to use.

</br>你还可以创建协程池来避免StartCoroutine的GC问题，但是这个用起来会复杂一些。

```C#
var coroutinePool = CoroutineHelper.Factory.CreateCoroutinePool(10);
coroutinePool.TryStartCoroutineInPool(Foo());//If pool is full, execute it immediately with StartCoroutine.

if (coroutinePool.HasCoroutineIdle)
    coroutinePool.StartCoroutine(Foo());//Or like this.
```

### InternalPool:
and you can avoid GC with some internal pools.

</br>另外还封装了一些yield函数的内部池，避免诸如new WaitForSeconds(...)带来的GC开销。

```C#
IEnumerator Foo()
{
    yield return new WaitForSeconds(1f);//wait 1 sec, has GC.

    var waitForSeconds = CoroutineHelper.Pool_WaitForSeconds.Spawn();
    yield return waitForSeconds.Reset(1f);
    CoroutineHelper.Pool_WaitForSeconds.Despawn(waitForSeconds);//wait 1 sec with CoroutineHelper, no GC.
}
```
