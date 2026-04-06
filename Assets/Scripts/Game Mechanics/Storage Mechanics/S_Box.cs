using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_Box : MonoBehaviour
{
    [Header("Box Settings")]
    public Category category;
    public int count;
    public object theItem;

    private void Awake()
    {
        RectTransform irts = gameObject.transform.Find("Item Icon").GetComponent<RectTransform>();
        irts.sizeDelta = new Vector2(85, 85);
        irts.localPosition = new Vector3(0, (float)-42.5, 0);
        RectTransform crts = gameObject.transform.Find("Count").GetComponent<RectTransform>();
        crts.sizeDelta = new Vector2(0, 30);
        crts.localPosition = new Vector3(0, 15, 0);
    }

    public void AddItem(object a_item, int c)
    {
        Image image = transform.Find("Item Icon").GetComponent<Image>();
        bool allowed = true;
        if (a_item is Plants)           { category = Category.Plants; theItem = (Plants)a_item; }
        else if (a_item is Fruits)      { category = Category.Fruits; theItem = (Fruits)a_item; }
        else if (a_item is AProducts)   { category = Category.AProducts; theItem = (AProducts)a_item; }
        else if (a_item is Products)    { category = Category.Products; theItem = (Products)a_item; }
        else if (a_item is Items)       { category = Category.Items; theItem = (Items)a_item; }
        else if (a_item is a_f_types)   { category = Category.AnimalFood; theItem = (a_f_types)a_item; }
        else { Debug.LogWarning("Item not allowed in this box!"); allowed = false; }

        if(allowed)
        {
            this.count = c;
            Debug.Log($"Allowed for {theItem} and count {c}");
            image.sprite = Sprites.instance.GetSpriteFromSource(theItem);
        }
    }

    public void UpdateCount()
    {
        if (category == Category.Plants)
            count = StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == (Plants)theItem).count;
        else if (category == Category.Fruits)
            count = StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == (Fruits)theItem).count;
        else if (category == Category.AProducts)
            count = StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == (AProducts)theItem).count;
        else if (category == Category.Products)
            count = StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == (Products)theItem).count;
        else if (category == Category.Items)
            count = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == (Items)theItem).count;
        else if (category == Category.AnimalFood)
            count = StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == (a_f_types)theItem).amount;
        transform.Find("Count").GetComponent<TextMeshProUGUI>().text = count.ToString();
    }
}