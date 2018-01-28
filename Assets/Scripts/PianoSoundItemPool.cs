using System.Collections.Generic;
using UnityEngine;

public class PianoSoundItemPool : MonoBehaviour
{
    public int pooledAmount = 25;
    public GameObject prefab;
    public RectTransform parent;
    private List<PianoSoundItem> pooledObjects = new List<PianoSoundItem>();
    private List<bool> objectAvailable = new List<bool>();
    public PianoSoundItem GetObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
            if (objectAvailable[i])
            {
                objectAvailable[i] = false;
                return pooledObjects[i];
            }
        GameObject newObject = Instantiate(prefab, parent);
        PianoSoundItem itemObject = newObject.GetComponent<PianoSoundItem>();
        pooledObjects.Add(itemObject);
        newObject.SetActive(false);
        objectAvailable.Add(false);
        return itemObject;
    }
    public void ReturnObject(PianoSoundItem returnObject)
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
            GameObject newObject = Instantiate(prefab, parent);
            PianoSoundItem itemObject = newObject.GetComponent<PianoSoundItem>();
            pooledObjects.Add(itemObject);
            objectAvailable.Add(true);
            newObject.SetActive(false);
        }
    }
}
