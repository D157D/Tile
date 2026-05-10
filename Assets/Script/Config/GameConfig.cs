using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "TileMatch/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Board Settings")]
    public float tileWidth = 70f;
    public float tileHeight = 80f;
    
    [Header("Tray Settings")]
    public int maxTraySlots = 7;
    public float animationDuration = 0.2f;
    
    [Header("Assets")]
    public Sprite[] icons;
    public GameObject tilePrefab;
}
