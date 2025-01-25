using UnityEngine;

[CreateAssetMenu(fileName = "TileGenerationSettings", menuName = "Tile Generation Settings", order = 1)]
public class TileGenerationSettings : ScriptableObject
{
    [Header("Tile Generation Percentages")]

    [Tooltip("Percentage of mountain tiles in the generated grid.")]
    [Range(0, 100)]
    public float mountainPercentage = 3f;

    [Tooltip("Percentage of mine tiles in the generated grid.")]
    [Range(0, 100)]
    public float minePercentage = 2f;

    [Tooltip("Percentage of sawmill tiles in the generated grid.")]
    [Range(0, 100)]
    public float sawmillPercentage = 2f;

    [Tooltip("Percentage of stone quarry tiles in the generated grid.")]
    [Range(0, 100)]
    public float stoneQuarryPercentage = 2f;

    [Header("Water Parameters")]
    [Tooltip("Percentage of water tiles in the generated grid.")]
    [Range(0, 100)]
    public float waterPercentage = 0.1f;
    [Tooltip("Proximity factor for water. This multiplier affects the probability of water based on nearby tiles.")]
    [Range(0f, 1f)]
    public float waterProximityFactor = 0.01f;
    
    [Header("Desert Parameters")]
    [Tooltip("Percentage of desert tiles in the generated grid.")]
    [Range(0, 100)]
    public float desertPercentage = 0.1f;
    [Tooltip("Proximity factor for desert. This multiplier affects the probability of desert based on nearby tiles.")]
    [Range(0f, 1f)]
    public float desertProximityFactor = 0.01f;

    [Tooltip("Probability of creating a city cluster when conditions are met.")]
    [Range(0f, 1f)]
    public static float cityClusterProbability = 0.01f;

    [Header("Cluster Settings")]
    [Tooltip("Size of clusters for the generation process.")]
    [Range(1, 10)]
    public int[] clusterSizes = new int[3] { 3, 5, 7 };

    [Tooltip("Maximum number of clusters to create.")]
    [Range(1, 10)]
    public int maxClusters = 5;
}
