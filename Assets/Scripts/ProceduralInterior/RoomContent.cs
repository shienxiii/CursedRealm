using ProceduralInterior;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// These are content that fills a room and may or may not consume sample spaces
/// </summary>
public class RoomContent : MonoBehaviour
{
    [SerializeField, Tooltip("Sample points this object need to occupy, relative to this object's transform")]
    private List<Vector2Int> _consumed;
    [SerializeField, Tooltip("Sample points around this object that needs to be reserved to ensure player can reach this object")]
    private List<Vector2Int> _reserve;

    // Calculate and return the grid space consumed, taking into consideration the rotation of the GameObject
    public List<Vector2Int> GetConsumedGridSpace()
    {
        return LocalOffsetToWorld(transform, _consumed);
    }
    
    public List<Vector2Int> GetReservedGridSpace()
    {
        return LocalOffsetToWorld(transform, _reserve);
    }

    private static List<Vector2Int> LocalOffsetToWorld(Transform transform, List<Vector2Int> localOffset)
    {
        float angle = transform.rotation.eulerAngles.y;
        
        List<Vector2Int> worldOffset = new List<Vector2Int>();
        
        foreach (Vector2Int offset in localOffset)
            worldOffset.Add(CalculateOffset(offset, angle));

        return worldOffset;
    }

    private static Vector2Int CalculateOffset(Vector2Int offset, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        
        // Calculate standard sin and cos
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        // Apply rotation matrix and round to nearest integer
        int newX = Mathf.RoundToInt(offset.x * cos + offset.y * sin);
        int newY = Mathf.RoundToInt(-offset.x * sin + offset.y * cos);
        
        return new Vector2Int(newX, newY);
    }
    private void OnDrawGizmos()
    {
        ProceduralInteriorSettings settings = InteriorManager.Settings;
        if (!settings) return;

        Vector3 sampleSize = new Vector3(settings.SampleDimension.x - 0.1f, 0.0f, settings.SampleDimension.x - 0.1f);

        // draw consumed
        Color color = Color.yellow;
        color.a = 0.5f;
        Gizmos.color = color;

        List<Vector2Int> offsets = GetConsumedGridSpace();

        if (offsets != null)
        {
            foreach (Vector2Int consume in offsets)
            {
                Vector3 positionOffset = new Vector3(consume.x * sampleSize.x, 0.0f, consume.y * sampleSize.x);
                Vector3 pos = transform.position + positionOffset;

                Gizmos.DrawCube(pos, sampleSize);
            }
        }

        color = Color.green;
        color.a = 0.5f;
        Gizmos.color = color;
        
        offsets = GetReservedGridSpace();
        
        if (offsets != null)
        {
            foreach (Vector2Int reserve in offsets)
            {
                Vector3 offset = new Vector3(reserve.x * sampleSize.x, 0.0f, reserve.y * sampleSize.x);
                Vector3 pos = transform.position + offset;

                Gizmos.DrawCube(pos, sampleSize);
            }
        }
    }
}
