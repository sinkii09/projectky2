using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class ParticlePool : MonoBehaviour
{
    public static ParticlePool Singleton { get; private set; }

    [SerializeField]
    List<PoolConfigObject> PooledPrefabsList;

    HashSet<GameObject> m_Prefabs = new HashSet<GameObject>();

    Dictionary<GameObject, ObjectPool<GameObject>> m_PooledObjects = new Dictionary<GameObject, ObjectPool<GameObject>>();
    public void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }

    }
    private void Start()
    {
        foreach (var configObject in PooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
        }
    }
    private void OnDestroy()
    {
        foreach (var prefab in m_Prefabs)
        {
            m_PooledObjects[prefab].Clear();
        }
        m_PooledObjects.Clear();
        m_Prefabs.Clear();
    }
    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        GameObject CreateFunc()
        {
            return Instantiate(prefab);
        }
        void ActionOnGet(GameObject obj)
        {
            obj.SetActive(true);
        }
        void ActionOnRelease(GameObject obj)
        {
            obj.SetActive(false);
        }
        void ActionOnDestroy(GameObject obj)
        {
            Destroy(obj);
        }

        m_Prefabs.Add(prefab);

        m_PooledObjects[prefab] = new ObjectPool<GameObject>(CreateFunc,ActionOnGet,ActionOnRelease,ActionOnDestroy);

        var prewarmNetworkObjects = new List<GameObject>();
        for (var i = 0; i < prewarmCount; i++)
        {
            prewarmNetworkObjects.Add(m_PooledObjects[prefab].Get());
        }
        foreach (var networkObject in prewarmNetworkObjects)
        {
            m_PooledObjects[prefab].Release(networkObject);
        }
    }
    public GameObject GetObject(GameObject prefab,Vector3 position, Quaternion rotation)
    {
        var obj = m_PooledObjects[prefab].Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }
    public void ReturnObject(GameObject gameObject, GameObject prefab)
    {
        m_PooledObjects[prefab].Release(gameObject);
    }
}
