using UnityEngine;
using UnityEngine.Pool;

public class PoolBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
    private IObjectPool<T> _pool;

    public void SetPool(IObjectPool<T> pool) {
        _pool = pool;
    }

    public void Release() {
        _pool?.Release(this as T);
    }
}