using UnityEngine;

[CreateAssetMenu(fileName = "TileGenerationSettings", menuName = "Tile Generation Settings", order = 1)]
public class TileGenerationSettings : ScriptableObject
{
    [Header("Tile Generation Percentages")]
    [Tooltip("Percentage of water tiles in the generated grid.")]
    [Range(0, 100)]
    public float waterPercentage = 20f;

    [Tooltip("Percentage of mountain tiles in the generated grid.")]
    [Range(0, 100)]
    public float mountainPercentage = 10f;

    [Tooltip("Percentage of mine tiles in the generated grid.")]
    [Range(0, 100)]
    public float minePercentage = 5f;

    [Tooltip("Percentage of sawmill tiles in the generated grid.")]
    [Range(0, 100)]
    public float sawmillPercentage = 5f;

    [Tooltip("Percentage of stone quarry tiles in the generated grid.")]
    [Range(0, 100)]
    public float stoneQuarryPercentage = 10f;

    [Header("Water Proximity Factor")]
    [Tooltip("Proximity factor for water. This multiplier affects the probability of water based on nearby tiles.")]
    [Range(0f, 1f)]
    public float waterProximityFactor = 0.1f;
}