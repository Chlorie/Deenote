using System.Collections.Generic;
using UnityEngine;

public class ObjectPool_Legacy<T> where T : Object
{
    public delegate void ObjectCall(T item);
    private List<T> _objects = new List<T>();
    private List<bool> _available = new List<bool>();
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly ObjectCall _initCall;
    private readonly ObjectCall _getCall;
    private readonly ObjectCall _returnCall;

    private T InstantiateNew()
    {
        T newObject = _parent != null ? Object.Instantiate(_prefab, _parent) : Object.Instantiate(_prefab);
        _initCall?.Invoke(newObject);
        _objects.Add(newObject);
        return newObject;
    }

    public ObjectPool_Legacy(T prefab, Transform parent = null, int startAmount = 20,
        ObjectCall initCall = null, ObjectCall getCall = null, ObjectCall returnCall = null)
    {
        _prefab = prefab;
        _parent = parent;
        _initCall = initCall;
        _getCall = getCall;
        _returnCall = returnCall;
        for (int i = 0; i < startAmount; i++)
        {
            InstantiateNew();
            _available.Add(true);
        }
    }

    public T GetObject()
    {
        for (int i = 0; i < _available.Count; i++)
            if (_available[i])
            {
                _available[i] = false;
                _getCall?.Invoke(_objects[i]);
                return _objects[i];
            }
        T newObject = InstantiateNew();
        _available.Add(false);
        _getCall?.Invoke(newObject);
        return newObject;
    }

    public void ReturnObject(T returnedObject)
    {
        for (int i = 0; i < _objects.Count; i++)
            if (_objects[i] == returnedObject)
            {
                _available[i] = true;
                _returnCall?.Invoke(returnedObject);
                return;
            }
        Object.Destroy(returnedObject);
        Debug.LogError("Error: Trying to return an object that's not from this pool " +
            $"(of type {typeof(T)}), object destroyed");
    }
}
