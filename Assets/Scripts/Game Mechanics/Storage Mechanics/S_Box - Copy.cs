using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_Box_BackUp : MonoBehaviour
{
    [Header("Box Settings")]
    public Category category;
    public int count;
    public Plants plant = Plants.None;
    public Fruits fruit = Fruits.None;
    public AProducts animal_product = AProducts.None;
    public Products product = Products.None;
    public Items item = Items.None;
    public a_f_types Food = a_f_types.None;
    public object theItem = null;

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

        if (a_item is Plants)
        {
            plant = (Plants)a_item;
            category = Category.Plants;
        }
        else if (a_item is Fruits)
        {
            fruit = (Fruits)a_item;
            category = Category.Fruits;
        }
        else if (a_item is AProducts)
        {
            animal_product = (AProducts)a_item;
            category = Category.AProducts;
        }
        else if (a_item is Products)
        {
            product = (Products)a_item;
            category = Category.Products;
        }
        else if (a_item is Items)
        {
            item = (Items)a_item;
            category = Category.Items;
        }
        else if (a_item is a_f_types)
        {
            Food = (a_f_types)a_item;
            category = Category.AnimalFood;
        }
        else Debug.LogWarning("Item not allowed in this box!");

        this.count = c;
        image.sprite = Sprites.instance.GetSpriteFromSource(plant);
    }

    public void UpdateCount()
    {
        if (category == Category.Plants)
            count = StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == plant).count;
        else if (category == Category.Fruits)
            count = StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == fruit).count;
        else if (category == Category.AProducts)
            count = StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == animal_product).count;
        else if (category == Category.Products)
            count = StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == product).count;
        else if (category == Category.Items)
            count = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == item).count;
        else if (category == Category.AnimalFood)
            count = StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == Food).amount;
        transform.Find("Count").GetComponent<TextMeshProUGUI>().text = count.ToString();
    }
}