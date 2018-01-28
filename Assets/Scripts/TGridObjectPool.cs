using System.Collections.Generic;
using UnityEngine;

public class TGridObjectPool : MonoBehaviour
{
    public int pooledAmount = 25;
    public Transform parent;
    private List<TGridController> pooledObjects = new List<TGridController>();
    private List<bool> objectAvailable = new List<bool>();
    public TGridController GetObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
            if (objectAvailable[i])
            {
                objectAvailable[i] = false;
                return pooledObjects[i];
            }
        Line grid = Utility.DrawLineInWorldSpace(Vector3.zero, Vector3.up, Color.white, 0.035f);
        grid.transform.SetParent(parent);
        grid.gameObject.AddComponent<TGridController>();
        TGridController lineObject = grid.gameObject.GetComponent<TGridController>();
        lineObject.grid = grid;
        pooledObjects.Add(lineObject);
        grid.SetActive(false);
        objectAvailable.Add(false);
        return lineObject;
    }
    public void ReturnObject(TGridController returnObject)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
            if (pooledObjects[i] == returnObject)
            {
                returnObject.gameObject.SetActive(false);
                objectAvailable[i] = true;
                return;
            }
        Debug.LogError("Try to return an object that's not from the pool. Destroying the object.");
        Destroy(returnObject.gameObject);
        return;
    }
    public void Initialize()
    {
        while (pooledObjects.Count < pooledAmount)
        {
            Line grid = Utility.DrawLineInWorldSpace(Vector3.zero, Vector3.up, Color.white, 0.035f);
            grid.transform.SetParent(parent);
            grid.gameObject.AddComponent<TGridController>();
            TGridController lineObject = grid.gameObject.GetComponent<TGridController>();
            lineObject.grid = grid;
            pooledObjects.Add(lineObject);
            grid.SetActive(false);
            objectAvailable.Add(true);
        }
    }
}
