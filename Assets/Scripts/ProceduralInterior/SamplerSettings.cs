using UnityEngine;

[CreateAssetMenu(fileName = "SamplerSettings", menuName = "Scriptable Objects/Procedural Interior/SamplerSettings")]
public class SamplerSettings : ScriptableObject
{
    /// <summary>
    /// Dimension of each sample when sampling.
    /// x = Horizontal dimension
    /// y = Vertical dimension
    /// </summary>
    [SerializeField]
    private Vector2 _sampleDimension = new Vector2(2.0f, 3.0f);
    [SerializeField] private int _minRoomCount = 5;
    [SerializeField] private int _maxRoomCount = 10;
    [SerializeField] private int _offset = 5;
    [SerializeField] private int _minSamplesPerRoom = 4;

    public Vector2 SampleDimension => _sampleDimension;
    public int MinRoomCount => _minRoomCount;
    public int MaxRoomCount => _maxRoomCount;
    public int Offset => _offset;
    public int MinSamplesPerRoom => _minSamplesPerRoom;
}
