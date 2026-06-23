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
    public Vector2 SampleDimension => _sampleDimension;
}
