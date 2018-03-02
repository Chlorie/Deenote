using System.Collections.Generic;
using UnityEngine;

public class LinePool : MonoBehaviour
{
    private ProjectController projectController;
    public int pooledAmount = 25;
    public GameObject prefab;
    private List<Line> pooledObjects = new List<Line>();
    private List<bool> objectAvailable = new List<bool>();
    public Line GetObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
            if (objectAvailable[i])
            {
                objectAvailable[i] = false;
                return pooledObjects[i];
            }
        GameObject newObject = Instantiate(prefab, Utility.lineCanvas);
        Line line = newObject.GetComponent<Line>();
        projectController.resolutionChange.AddListener(line.ResolutionReset);
        pooledObjects.Add(line);
        newObject.SetActive(false);
        objectAvailable.Add(false);
        return line;
    }
    public void ReturnObject(Line returnObject)
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
        projectController = FindObjectOfType<ProjectController>();
        while (pooledObjects.Count < pooledAmount)
        {
            GameObject newObject = Instantiate(prefab, Utility.lineCanvas);
            Line line = newObject.GetComponent<Line>();
            projectController.resolutionChange.AddListener(line.ResolutionReset);
            pooledObjects.Add(line);
            objectAvailable.Add(true);
            newObject.SetActive(false);
        }
    }
}
