using Unity.VisualScripting;
using UnityEngine;

public static class ArrayOperatorFunctions
{
    /// <summary>
    /// Sets all value within the specified block in an array
    /// to the specified value
    /// </summary>
    /// <typeparam name="T">the type in the array</typeparam>
    /// <param name="array">the array</param>
    /// <param name="value">the value to set</param>
    /// <param name="start">starting index (inclusive)</param>
    /// <param name="end">final index (inclusive)</param>
    public static void MutateBlockOnArray<T>(T[] array, T value, int start, int end)
    {
        if (array == null || start < 0) return;

        int max = array.Length;

        for (int i = start; i <= end && i < max; i++)
            array[i] = value;
    }

    /// <summary>
    /// Sets all value within the specified block in an array
    /// to the specified value
    /// </summary>
    /// <typeparam name="T">the type in the array</typeparam>
    /// <param name="array">the array</param>
    /// <param name="value">the value to set</param>
    /// <param name="startX">starting index on X (inclusive)</param>
    /// <param name="endX">final index on X (inclusive)</param>
    /// <param name="startY">starting index on Y (inclusive)</param>
    /// <param name="endY">final index on Y (inclusive)</param>
    public static void MutateBlockOn2DArray<T>(T[,] array, T value, int startX, int endX, int startY, int endY)
    {
        if (array == null || startX < 0 || startY < 0) return;

        int maxX  = array.GetLength(0);
        int maxY = array.GetLength(1);

        for (int x = startX; x <= endX && x < maxX; x++)
        {
            for(int y = startY; y <= endY && y < maxY; y++)
                array[x, y] = value;
        }
    }
}
