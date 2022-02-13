using UnityEngine;
using System.Collections.Generic;

public sealed class ObjectPool : MonoBehaviour
{
	static ObjectPool _instance;

    // Lookup from prefab -> List of inactive objects, ready to be instanced
	public Dictionary<PooledMonoBehaviour, List<PooledMonoBehaviour>> objectLookup = new Dictionary<PooledMonoBehaviour, List<PooledMonoBehaviour>>();
    // Lookup from prefab -> List of active objects, to be recycled if needed
    public Dictionary<PooledMonoBehaviour, List<PooledMonoBehaviour>> activeLookup = new Dictionary<PooledMonoBehaviour, List<PooledMonoBehaviour>>();
    // Lookup from object -> prefab, to find key in object lookup
	public Dictionary<PooledMonoBehaviour, PooledMonoBehaviour> prefabLookup = new Dictionary<PooledMonoBehaviour, PooledMonoBehaviour>();

    void OnDestroy()
    {
        foreach (var pair in objectLookup)
        {
            pair.Value.ForEach((pmb) => Destroy(pmb.gameObject));
        }
        objectLookup.Clear();
        foreach (var pair in prefabLookup)
        {
            if (pair.Key)
            {
                Destroy(pair.Key.gameObject);
            }
        }
        prefabLookup.Clear();

        // No need to destroy the objects here, it's handled by clearing prefab lookup
        activeLookup.Clear();
        _instance = null;
	}

	public static void CreatePool<T>(T prefab) where T : PooledMonoBehaviour
    {
		if (instance == null) return;
        if (!instance.objectLookup.ContainsKey(prefab))
        {
            instance.objectLookup.Add(prefab, new List<PooledMonoBehaviour>());
            instance.activeLookup.Add(prefab, new List<PooledMonoBehaviour>());
        }
	}
	
	public static void RemovePool<T>(T prefab) where T : PooledMonoBehaviour
    {
		if (instance == null) return;

        instance.objectLookup[prefab].ForEach((pmb) => Destroy(pmb.gameObject));
		instance.objectLookup.Remove(prefab);

        instance.activeLookup[prefab].ForEach((pmb) => 
        {
            instance.prefabLookup.Remove(pmb);
            Destroy(pmb.gameObject);
        });
        instance.activeLookup.Remove(prefab);
    }

    public static void RecyclePool<T>(T prefab) where T : PooledMonoBehaviour
    {
        if (instance == null) return;
        if (!instance.activeLookup.ContainsKey(prefab))
        {
            Debug.LogWarning("Attempting to recycle pool for object " + prefab.name + ", but that doesn't contain a pool (or any active lookup elements)");
            return;
        }
        List<PooledMonoBehaviour> active = instance.activeLookup[prefab];
        for (int i = active.Count - 1; i >= 0; --i)
        {
            active[i].Recycle();
        }
    }
	
    public static bool HasPool<T>(T prefab) where T : PooledMonoBehaviour
    {
        if (instance == null) return false;
        return (instance.objectLookup.ContainsKey(prefab));
    }

	public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : PooledMonoBehaviour
	{
		if (instance == null) return null;

		if (instance.objectLookup.ContainsKey(prefab))
		{
			T obj = null;
			var list = instance.objectLookup[prefab];
			if (list.Count > 0)
			{
				while (obj == null && list.Count > 0)
				{
					obj = list[0] as T;
					list.RemoveAt(0);
				}
				if (obj != null)
				{
					obj.transform.localPosition = position;
					obj.transform.localRotation = rotation;
                    if (parent != null)
                    {
                        obj.transform.SetParent(parent, false);
                    }
                    obj.gameObject.SetActive(true);
                    obj.OnSpawn();
					instance.prefabLookup.Add(obj, prefab);
                    instance.activeLookup[prefab].Add(obj);
					return obj;
				}
			}
            obj = Instantiate(prefab, position, rotation);
            if (parent != null)
            {
                obj.transform.SetParent(parent, true);
            }
            obj.OnInstantiate();
            obj.OnSpawn();
			instance.prefabLookup.Add(obj, prefab);
            instance.activeLookup[prefab].Add(obj);
            return obj;
		}
		else
		{
			Debug.LogError ("Object pool not created for prefab " + prefab.name + "!", prefab.gameObject);
            T obj = (T)Object.Instantiate(prefab, position, rotation);
            obj.OnInstantiate();
            obj.OnSpawn();
            return obj;
		}
    }
    public static T Spawn<T>(T prefab, Transform parent) where T : PooledMonoBehaviour
    {
        return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
    }
    public static T Spawn<T>(T prefab, Vector3 position) where T : PooledMonoBehaviour
	{
		return Spawn(prefab, null, position, Quaternion.identity);
	}
    public static T Spawn<T>(T prefab) where T : PooledMonoBehaviour
	{
		return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}
    public static void Bake<T>(T prefab, int count, Transform parent) where T : PooledMonoBehaviour
    {
        if (instance == null) return;

        if (instance.objectLookup.ContainsKey(prefab))
        {
            T obj = null;
            var list = instance.objectLookup[prefab];
            for (int i = list.Count; i < count; ++i)
            {
                obj = Instantiate(prefab, parent);
                obj.OnInstantiate();
                obj.gameObject.SetActive(false);
                if (Debug.isDebugBuild)
                {
                    obj.gameObject.name = "Card " + i.ToString();
                }
                // Straight to object lookup: do not instance right away
                instance.objectLookup[prefab].Add(obj);
            }
        }
        else
        {
            Debug.LogError("Object pool not created for prefab " + prefab.name + "!", prefab.gameObject);
            Debug.Break();
        }
    }

    public static void Recycle<T>(T obj) where T : PooledMonoBehaviour
	{
		if ((instance == null) || (obj == null)) return;

        if (instance.prefabLookup.ContainsKey(obj))
        {
            PooledMonoBehaviour prefab = instance.prefabLookup[obj];
            instance.objectLookup[prefab].Add(obj);
            Debug.Assert(instance.activeLookup[prefab].Remove(obj));
            instance.prefabLookup.Remove(obj);

            obj.OnRecycle();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(instance.transform);
        }
        else
        {
            Debug.LogWarning("Recycling is destroying a GameObject; intentional?");
            Object.Destroy(obj.gameObject);
        }
	}

    public static int Count<T>(T prefab) where T : PooledMonoBehaviour
	{
		if (instance == null) return 0;
		if (instance.objectLookup.ContainsKey(prefab))
			return instance.objectLookup[prefab].Count;
		else
			return 0;
	}

	public static ObjectPool instance
	{
		get
		{
            if (_instance == null)
			{
                GameObject obj = GameObject.FindWithTag("RecycleBin");
                _instance = obj?.GetComponent<ObjectPool>();
            }

            //Debug.Log ("Returning ObjectPool instance : IsNull[" + (_instance == null).ToString () + "]");
			return _instance;
		}
	}
}

public static class ObjectPoolExtensions
{
    public static void CreatePool<T>(this T prefab) where T : PooledMonoBehaviour
	{
		ObjectPool.CreatePool(prefab);
	}
    public static void RemovePool<T>(this T prefab) where T : PooledMonoBehaviour
	{
		ObjectPool.RemovePool(prefab);
	}
    public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : PooledMonoBehaviour
	{
		return ObjectPool.Spawn(prefab, null, position, rotation);
	}
    public static T Spawn<T>(this T prefab, Vector3 position) where T : PooledMonoBehaviour
	{
		return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
	}
    public static T Spawn<T>(this T prefab) where T : PooledMonoBehaviour
    {
        return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
    }
    public static T Spawn<T>(this T prefab, Transform xform) where T : PooledMonoBehaviour
    {
        return ObjectPool.Spawn(prefab, xform);
    }

    public static void Recycle<T>(this T obj) where T : PooledMonoBehaviour
	{
		ObjectPool.Recycle(obj);
	}

    public static int Count<T>(T prefab) where T : PooledMonoBehaviour
	{
		return ObjectPool.Count(prefab);
	}
}
