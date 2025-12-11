using UnityEngine;

public class Sprites : MonoBehaviour
{
    public static Sprites instance;
    public StorageSprites sprites;
    public Sprite EraseItem;

    public GameObject InfoButtonPrefab;

    private void Awake()
    {
        instance = this;
    }
}