using System.Collections.Generic;
using UnityEngine;

public class NoteIndicatorPool : MonoBehaviour
{
    public int pooledAmount = 25;
    public GameObject notePrefab;
    private List<NoteIndicatorController_Legacy> pooledObjects = new List<NoteIndicatorController_Legacy>();
    private List<bool> objectAvailable = new List<bool>();
    public NoteIndicatorController_Legacy GetObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
            if (objectAvailable[i])
            {
                objectAvailable[i] = false;
                return pooledObjects[i];
            }
        GameObject newObject = Instantiate(notePrefab);
        NoteIndicatorController_Legacy noteObject = newObject.GetComponent<NoteIndicatorController_Legacy>();
        pooledObjects.Add(noteObject);
        newObject.SetActive(false);
        objectAvailable.Add(false);
        return noteObject;
    }
    public void ReturnObject(NoteIndicatorController_Legacy returnObject)
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
            GameObject newObject = Instantiate(notePrefab);
            NoteIndicatorController_Legacy noteObject = newObject.GetComponent<NoteIndicatorController_Legacy>();
            pooledObjects.Add(noteObject);
            objectAvailable.Add(true);
            newObject.SetActive(false);
        }
    }
}
