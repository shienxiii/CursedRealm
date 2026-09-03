using UnityEngine;

public static class DragAndDropHelperLibrary
{
    private static Object _draggedObj = null;

    public static void SetDraggedObject(Object inDragged)
    {
        _draggedObj = inDragged;
    }

    public static void Clear()
    {
        _draggedObj = null;
    }
}
