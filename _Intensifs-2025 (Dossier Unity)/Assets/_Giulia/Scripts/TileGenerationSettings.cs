using UnityEngine;

[CreateAssetMenu(fileName = "TileGenerationSettings", menuName = "Tile Generation Settings", order = 1)]
public class TileGenerationSettings : ScriptableObject
{
    public float grassPercentage = 50f;
    public float waterPercentage = 20f;
    public float mountainPercentage = 10f;
    public float minePercentage = 5f;
    public float sawmillPercentage = 5f;
    public float stoneQuarryPercentage = 10f;
}