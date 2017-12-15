using UnityEngine;
using DragAndDrop;

public class DragAndDropUnity
{
    public static void Enable(OnDropFileDelegate callback)
    {
        DragAndDrop.DragAndDrop.Enable(callback, Application.productName);
    }
    public static void Disable()
    {
        DragAndDrop.DragAndDrop.Disable();
    }
}
