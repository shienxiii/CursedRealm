using ProceduralInterior;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// These are content that fills a room and may or may not consume sample spaces
/// </summary>
public class RoomContent : MonoBehaviour
{
    [SerializeField, Tooltip("Sample points this object need to occupy, relative to this object's transform")]
    private List<Vector2Int> _consume;
    [SerializeField, Tooltip("Sample points around this object that needs to be reserved to ensure player can reach this object")]
    private List<Vector2Int> _reserve;

    private void OnDrawGizmos()
    {
        ProceduralInteriorSettings settings = InteriorManager.Settings;
        if (settings == null) return;

        Vector3 sampleSize = new Vector3(settings.SampleDimension.x - 0.1f, 0.0f, settings.SampleDimension.x - 0.1f);

        // draw consumed
        Color color = Color.yellow;
        color.a = 0.5f;

        if (_consume != null)
        {
            Gizmos.color = color;
            foreach (Vector2Int consume in _consume)
            {
                Vector3 positionOffset = new Vector3(consume.x * sampleSize.x, 0.0f, consume.y * sampleSize.x);
                Vector3 pos = transform.position + positionOffset;

                Gizmos.DrawCube(pos, sampleSize);
            }
        }

        if (_reserve != null)
        {
            color = Color.green;
            color.a = 0.5f;

            Gizmos.color = color;
            foreach (Vector2Int reserve in _reserve)
            {
                Vector3 offset = new Vector3(reserve.x * sampleSize.x, 0.0f, reserve.y * sampleSize.x);
                Vector3 pos = transform.position + offset;

                Gizmos.DrawCube(pos, sampleSize);
            }
        }
    }
}
