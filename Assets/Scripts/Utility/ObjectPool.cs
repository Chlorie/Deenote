using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Object
{
    private T _prefab;
    private List<T> _objects = new List<T>();
    private List<bool> _available = new List<bool>();
    private Transform _parent = null;
    // Initialization (Constructor)
    public ObjectPool(T prefab, int startAmount = 20, Transform parent = null)
    {
        this._prefab = prefab;
        this._parent = parent;
        for (int i = 0; i < startAmount; i++)
        {
            T newObject;
            if (parent != null)
                newObject = Object.Instantiate(prefab, parent);
            else
                newObject = Object.Instantiate(prefab);
            _objects.Add(newObject);
            _available.Add(true);
        }
    }
    public T GetObject()
    {
        for (int i = 0; i < _available.Count; i++)
            if (_available[i])
            {
                _available[i] = false;
                return _objects[i];
            }
        T newObject;
        if (_parent != null)
            newObject = Object.Instantiate(_prefab, _parent);
        else
            newObject = Object.Instantiate(_prefab);
        _objects.Add(newObject);
        _available.Add(false);
        return newObject;
    }
    public void ReturnObject(T returnedObject)
    {
        for (int i = 0; i < _objects.Count; i++)
            if (_objects[i] == returnedObject)
            {
                _available[i] = true;
                return;
            }
        Object.Destroy(returnedObject);
        Debug.LogError("Error: Trying to return an object that's not from this pool " +
            $"(of type {typeof(T)}), object destroyed");
    }
}
