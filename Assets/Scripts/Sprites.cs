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
    public Sprite GetSpriteFromSource(object item, string category = "", int stage = 100)
    {
        if (item is Plants)
        {
            if (category == "plant ready")
                return sprites.readySprites.Find(e => e.plant == (Plants)item).sprite;
            else if (category == "plant stages")
                return sprites.StageSprites.Find(e => e.plant == (Plants)item).stages[stage];
            else
                return sprites.plants.Find(e => e.plant == (Plants)item).sprite;
        }
        else if (item is Fruits)
        {
            if(category == "tree stages")
                return sprites.TreeStageSprites.Find(e => e.tree == (Fruits)item).stages[stage];
            else if(category == "tree")
                return sprites.trees.Find(e => e.fruit == (Fruits)item).sprite;
            else
                return sprites.fruits.Find(e => e.fruit == (Fruits)item).sprite;
        }
        else if(item is Animals)
            return sprites.animals.Find(e => e.animal == (Animals)item).sprite;
        else if (item is a_f_types)
            return sprites.AnimalFoodSprites.Find(e => e.food == (a_f_types)item).sprite;
        else if (item is AProducts)
            return sprites.a_products.Find(e => e.a_product == (AProducts)item).sprite;
        else if (item is Products)
            return sprites.products.Find(e => e.product == (Products)item).sprite;
        else if (item is Items)
            return sprites.items.Find(e => e.item == (Items)item).sprite;
        else if (item is Machines)
            return sprites.machines.Find(e => e.machine == (Machines)item).sprite;
        else if (item is Currency)
            return sprites.currencies.Find(e => e.Currency == (Currency)item).sprite;
        else return null;
    }
}