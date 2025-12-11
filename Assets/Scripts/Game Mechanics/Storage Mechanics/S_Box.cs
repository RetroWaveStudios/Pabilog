using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_Box : MonoBehaviour
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

    [Header("UI Settings")]
    public GameObject theItem;
    public TextMeshProUGUI countText;

    private Plants[] plantnames = (Plants[])System.Enum.GetValues(typeof(Plants));
    private Fruits[] fruitnames = (Fruits[])System.Enum.GetValues(typeof(Fruits));
    private AProducts[] APnames = (AProducts[])System.Enum.GetValues(typeof(AProducts));
    private Products[] ProductNames = (Products[])System.Enum.GetValues(typeof(Products));
    private Items[] itemNames = (Items[])System.Enum.GetValues(typeof(Items));
    private a_f_types[] FoodNames = (a_f_types[])System.Enum.GetValues(typeof(a_f_types));

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
        Storage storage = GetComponentInParent<Storage>();
        Image image = theItem.GetComponent<Image>();

        if (a_item is Plants)
        {
            plant = (Plants)a_item;
            this.count = c;
            for (int i = 0; i < plantnames.Length; i++)
                if(plant == plantnames[i]) image.sprite = Sprites.instance.sprites.plants.Find(e => e.plant == plantnames[i]).sprite;
            category = Category.Plants;
        }
        else if (a_item is Fruits)
        {
            fruit = (Fruits)a_item;
            this.count = c;
            for (int i = 0; i < fruitnames.Length; i++)
                if(fruit == fruitnames[i]) image.sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == fruitnames[i]).sprite;
            category = Category.Fruits;
        }
        else if (a_item is AProducts)
        {
            animal_product = (AProducts)a_item;
            this.count = c;
            for (int i = 0; i < APnames.Length; i++)
                if (animal_product == APnames[i]) image.sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == APnames[i]).sprite;
            category = Category.AProducts;
        }
        else if(a_item is Products)
        {
            product = (Products)a_item;
            this.count = c;
            for (int i = 0; i < ProductNames.Length; i++)
                if (product == ProductNames[i]) image.sprite = Sprites.instance.sprites.products.Find(e => e.product == ProductNames[i]).sprite;
            category = Category.Products;
        }
        else if(a_item is Items)
        {
            item = (Items)a_item;
            this.count = c;
            for (int i = 0; i < itemNames.Length; i++)
                if (item == itemNames[i]) image.sprite = Sprites.instance.sprites.items.Find(e => e.item == itemNames[i]).sprite;
            category = Category.Items;
        }
        else if(a_item is a_f_types)
        {
            Food = (a_f_types)a_item;
            this.count = c;
            for (int i = 0; i < FoodNames.Length; i++)
                if (Food == FoodNames[i]) image.sprite = Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food == FoodNames[i]).sprite;
            category = Category.AnimalFood;
        }
        else Debug.LogWarning("Item not allowed in this box!");
    }

    public void UpdateCount()
    {
        if (category == Category.Plants)
        {
            count = StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == plant).count;
            Debug.Log(plant + " count changed to " + count);
        }
        else if (category == Category.AProducts)
        {
            count = StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == animal_product).count;
            Debug.Log(animal_product + " count changed to " + count);
        }
        else if (category == Category.Products)
        {
            count = StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == product).count;
            Debug.Log(product + " count changed to " + count);
        }
        else if (category == Category.Items)
        {
            count = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == item).count;
            Debug.Log(item + " count changed to " + count);
        }
        else if (category == Category.AnimalFood)
        {
            count = StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == Food).amount;
            Debug.Log(Food + " count changed to " + count);
        }
        countText.text = count.ToString();
    }
}