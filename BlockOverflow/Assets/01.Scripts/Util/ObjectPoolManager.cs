    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolObjData
{
    public PoolObject prefab;
    public int count = 50;
}

//싱글톤 오브젝트 풀링 매니저
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{

    [Header("Pool Object List")]
    [SerializeField] private PoolObjData[] poolObjData;

    [System.Serializable]
    class PoolQueue//각각의 풀을 저장할 큐에 대한 정보
    {
        public PoolQueue()
        {
            queue = new Queue<PoolObject>();
            count = 0;
        }

        public Queue<PoolObject> queue;

        public Transform parent;

        public int count;
    }

    private Dictionary<int, PoolQueue> _poolDictionary;//여기다 풀링할 오브젝트들 저장해둠 

    protected override void Awake()
    {
        _poolDictionary = new Dictionary<int, PoolQueue>();
        base.Awake();
    }

    private void Start()
    {
        foreach (PoolObjData objData in poolObjData)//각각의 풀들 생성
        {
            CreatePool(objData.prefab.gameObject, objData.count);
        }
    }
    
    public void CreatePool(GameObject prefab, int count)//프리펩 오브젝트의 풀 만들어주는 함수
    {

        string name = prefab.name + " pool";

        int id = prefab.GetInstanceID();
        if (!_poolDictionary.ContainsKey(id))//새로운 풀 일 경우
        {
            _poolDictionary.Add(id, new PoolQueue());//새로운 풀 생성

            _poolDictionary[id].count = count;//총 개수 저장

            //Set pool object Parent
            GameObject poolParent = new GameObject(name);
            poolParent.transform.parent = transform;

            _poolDictionary[id].parent = poolParent.transform;//풀 관리하는 부모 트렌스폼 저장

        }
        else//이미 있는 풀 일 경우 기존에 있는 풀에 추가하는 오브젝트들 넣어줌
        {
            _poolDictionary[id].count += count;
        }

        for (int i = 0; i < count; i++)//오브젝트들 풀에  추가
        {
            GameObject obj = Instantiate(prefab);
            PoolObject poolObject = obj.GetComponent<PoolObject>();

            poolObject.PrefabID = id;
            //poolObject.parentTransform = poolParent.transform;

            poolObject.transform.SetParent(_poolDictionary[id].parent.transform);

            obj.SetActive(false);

            _poolDictionary[id].queue.Enqueue(poolObject);

        }
    }

    #region Get
    public GameObject Get(GameObject prefab)
    {
        int id = prefab.GetInstanceID();

        if (!_poolDictionary.ContainsKey(id))
        {
            Debug.LogError(prefab.name + " Pool not found");
            return null;
        }
        
        
        // 수정 버전
        
        PoolQueue poolQueue = _poolDictionary[id];
        PoolObject poolObject = null;

        //큐에서 "살아 있는" 오브젝트를 찾을 때까지 꺼냄
        while (poolQueue.queue.Count > 0)
        {
            poolObject = poolQueue.queue.Dequeue();

            // Unity 특유의 pseudo-null까지 포함해서 체크
            if (poolObject != null && poolObject.gameObject != null)
            {
                break; // 살아있는 놈 찾음
            }

            // null / 이미 Destroy된 놈이면 그냥 버리고 다음으로 넘어감
            poolObject = null;
        }

        // 큐에 쓸만한 놈이 하나도 없었으면 새로 생성
        if (poolObject == null || poolObject.gameObject == null)
        {
            GameObject obj = Instantiate(prefab);

            poolObject = obj.GetComponent<PoolObject>();
            poolObject.PrefabID = id;
        }

        // 여기까지 오면 무조건 유효한 오브젝트
        poolObject.Get();

        return poolObject.gameObject;

        // PoolObject poolObject; 수정 이전 버전
        // if (_poolDictionary[id].queue.Count != 0)
        // {
        //     poolObject = _poolDictionary[id].queue.Dequeue();
        //
        // }
        // else
        // {
        //     GameObject obj = Instantiate(prefab);
        //
        //     poolObject = obj.GetComponent<PoolObject>();
        //     poolObject.PrefabID = id;
        // }
        //
        // poolObject.Get();
        //
        // return poolObject.gameObject;

    }
    
    public GameObject Get(GameObject prefab, Vector3 pos)
    {
        GameObject poolObj = Get(prefab);

        poolObj.transform.position = pos;

        return poolObj;
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject poolObj = Get(prefab, pos);
        
        poolObj.transform.rotation = rot;

        return poolObj;
    }
    #endregion Get

    #region Release
    public bool Release(PoolObject poolObject)
    {
        if (poolObject == null)
        {
            Debug.LogError("Null object cannot be returned to pool!!");
            return false;
        }

        if (!_poolDictionary.ContainsKey(poolObject.PrefabID))
        {
            Debug.LogError(poolObject.name + " Pool not found!!");
            return false;
        }
        
        _poolDictionary[poolObject.PrefabID].queue.Enqueue(poolObject);

        return true;
    }

    public void Release(GameObject poolObject)
    {
        PoolObject obj = poolObject.GetComponent<PoolObject>();

        obj.Release();
    }
    #endregion Release
}
