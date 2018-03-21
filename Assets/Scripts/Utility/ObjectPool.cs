using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Object
{
    private T prefab;
    private List<T> objects = new List<T>();
    private List<bool> available = new List<bool>();
    private Transform parent = null;
    // Initialization (Constructor)
    public ObjectPool(T prefab, int startAmount = 20, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < startAmount; i++)
        {
            T newObject;
            if (parent != null)
                newObject = Object.Instantiate(prefab, parent);
            else
                newObject = Object.Instantiate(prefab);
            objects.Add(newObject);
            available.Add(true);
        }
    }
    public T GetObject()
    {
        for (int i = 0; i < available.Count; i++)
            if (available[i])
            {
                available[i] = false;
                return objects[i];
            }
        T newObject;
        if (parent != null)
            newObject = Object.Instantiate(prefab, parent);
        else
            newObject = Object.Instantiate(prefab);
        objects.Add(newObject);
        available.Add(false);
        return newObject;
    }
    public void ReturnObject(T returnedObject)
    {
        for (int i = 0; i < objects.Count; i++)
            if (objects[i] == returnedObject)
            {
                available[i] = true;
                return;
            }
        Object.Destroy(returnedObject);
        Debug.LogError("Error: Trying to return an object that's not from this pool " +
            $"(of type {typeof(T)}), object destroyed");
    }
}
