using UnityEngine;

public class TileGenerationManager : MonoBehaviour
{
    public TileGenerationSettings tileSettings;

    public TileType GetRandomTileType()
    {
        float randomValue = Random.value * 100f;

        if (randomValue < tileSettings.waterPercentage) return TileType.Water;
        if (randomValue < tileSettings.waterPercentage + tileSettings.mountainPercentage) return TileType.Mountain;
        if (randomValue < tileSettings.waterPercentage + tileSettings.mountainPercentage + tileSettings.minePercentage) return TileType.Mine;
        if (randomValue < tileSettings.waterPercentage + tileSettings.mountainPercentage + tileSettings.minePercentage + tileSettings.sawmillPercentage) return TileType.Sawmill;
        if (randomValue < tileSettings.waterPercentage + tileSettings.mountainPercentage + tileSettings.minePercentage + tileSettings.sawmillPercentage + tileSettings.stoneQuarryPercentage) return TileType.StoneQuarry;
        return TileType.Grass;
    }
}