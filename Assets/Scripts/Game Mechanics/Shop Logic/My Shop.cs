using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MyShop : MonoBehaviour
{
    public static MyShop instance;
    private CanvasGroup canvasGroup;

    [Header("Shop Window")]
    public Transform ShopHolder;
    public GameObject ShopItemPrefab;
    public List<GameObject> itemsinShop;

    [Header("Selling Window")]
    public GameObject SellWindow;
    public Transform SellItemHolder;
    public GameObject SellingPrefab;
    public List<GameObject> SellingItems;

    public TextMeshProUGUI PriceParameterText;
    public TextMeshProUGUI CountParameterText;

    public Dictionary<Plants, int> PlantPR = new Dictionary<Plants, int>()
    {
        { Plants.Wheat, 6 },            { Plants.Corn, 8 },                 { Plants.Potato, 25 },
        { Plants.Carrot, 10 },          { Plants.Tomato, 34 },              { Plants.Onion, 50 },
        { Plants.SugarCane, 24 },       { Plants.Herbs, 20 },               { Plants.Spice, 40 },
        { Plants.Cotton, 60}
    };
    public Dictionary<Fruits, int> FruitPR = new Dictionary<Fruits, int>()
    {
        { Fruits.Apple, 15 },           { Fruits.Berry, 25 },               { Fruits.Banana, 50 },
        { Fruits.Orange, 60 },          { Fruits.Cacao, 65 },               { Fruits.Cherry, 30}
    };
    public Dictionary<AProducts, int> AProductPR = new Dictionary<AProducts, int>()
    {
        { AProducts.Egg, 8 },           { AProducts.Ch_Meat, 150 },         { AProducts.Milk, 10 },
        { AProducts.Cow_Meat, 250 },    { AProducts.Wool, 20 },             { AProducts.Bacon, 120 },
    };
    public Dictionary<Products, int> ProductPR = new Dictionary<Products, int>()
    {
        //  Dairy Churn Products
        { Products.Cream, 20 },            { Products.Butter, 40 },           { Products.Cheese, 60 },

        //  Press / Juicer Productk
        { Products.Sugar, 40 },            { Products.AppleJuice, 25 },       { Products.BerryJuice, 40 },

        //  Oven Products
        { Products.Bread, 18 },            { Products.CornBread, 25 },        { Products.Cookie, 80 },
        { Products.Pizza, 110 },            { Products.ChickenPizza, 220 },    { Products.SpicyPizza, 150 },

        //  PopCorn Products
        { Products.Popcorn, 30 },          { Products.ButterPopcorn, 50 },    { Products.SpicyPopcorn, 60 },

        //  Cake House Products
        { Products.CarrotCake, 60 },       { Products.CreamCake, 65 },        { Products.BerryCake, 75 },
        { Products.CherryCake, 75 },

        //  Pie Oven Products
        { Products.ApplePie, 60 },         { Products.CarrotPie, 70 },        { Products.BaconPie, 120 },


        //  Stove Products
        { Products.Soup, 80 },             { Products.ChickenSoup, 200 },     { Products.Stew, 250 },
        { Products.Steak, 300 },           { Products.RoastedCord, 40 },      { Products.Gratin, 80 },

        // Color Pigment Machine
        { Products.PigmentRed, 40 },       { Products.PigmentBlue, 40 },      { Products.PigmentGreen, 35 },
        { Products.PigmentYellow, 20 },    { Products.PigmentOrange, 25 },    { Products.PigmentBrown, 60 },
        { Products.PigmentPurple, 50 },

        //  Color Producer
        { Products.Red, 50 },              { Products.Blue, 50 },             { Products.Green, 45 },
        { Products.Yellow, 35 },           { Products.Orange, 40 },           { Products.Brown, 90 },
        { Products.Purple, 80 },

        //  Icecream Stand
        { Products.VanillaIcecream, 50 },  { Products.CherryIcecream, 80 },   { Products.OilyIcecream, 90 },
        { Products.ChocolatteIcecream, 120 },

        //  Simple Sewing Machine
        { Products.SimpleTShirt, 100 },    { Products.Hat, 80 },              { Products.Sweater, 120 },
    };
    public Dictionary<Items, int> ItemPR = new Dictionary<Items, int>()
    {
        { Items.Rake, 120 },                { Items.Bolt, 200 },                { Items.Screw, 200 },
        { Items.Axe, 120 },                 { Items.Hammer, 200 },              { Items.Tape, 400},
        { Items.Drill, 400 },               { Items.Pliers, 400 },              { Items.ToolSet, 1000},
    };
    public Dictionary<a_f_types, int> afPR = new Dictionary<a_f_types, int>()
    {
        { a_f_types.Wheat, 20 },               { a_f_types.Corn, 40 },
        { a_f_types.Carrot, 60 },              { a_f_types.Potato, 80 },
    };

    private Vector2 PriceRange = new Vector2(0, 0);
    private Vector2 TempPriceRange = new Vector2(0, 0);
    private int price, count, maxcount;
    private object TheItem = null;
    public int ssNumber;

    private void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        SellWindow.SetActive(false);
        PopulateShop();
    }

    private void PopulateShop()
    {
        for (int i = 0; i < StaticDatas.PlayerData.ShopSlotCount; i++)
        {
            GameObject dublicate = Instantiate(ShopItemPrefab, ShopHolder);
            ShopItem si = dublicate.GetComponent<ShopItem>();
            si.ssNumber = i;
            Button button = dublicate.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => si.SetItem());
            itemsinShop.Add(dublicate);
        }
    }

    public void PopulateHodler(Transform holder)
    {
        SellingItems.Clear();
        foreach (Transform item in holder) Destroy(item.gameObject);
        int slotn = 0;
        for (int i = 0; i < StaticDatas.PlayerData.Storage.PlantsInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.PlantsInStorage, i, slotn, out slotn, holder);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.FruitInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.FruitInStorage, i, slotn, out slotn, holder);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.a_p_inStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.a_p_inStorage, i, slotn, out slotn, holder);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.ProductsInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.ProductsInStorage, i, slotn, out slotn, holder);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.ItemsInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.ItemsInStorage, i, slotn, out slotn, holder);

        for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.PlayerInfos.Food.Amounts, i, slotn, out slotn, holder);

        RectTransform rts = SellItemHolder.GetComponent<RectTransform>();
        GridLayoutGroup glg = SellItemHolder.GetComponent<GridLayoutGroup>();
        if (SellItemHolder.childCount > 4) rts.sizeDelta = new Vector2(728, glg.padding.top + glg.padding.bottom +
                ((SellItemHolder.childCount / 4) * 174) + (float)(((SellItemHolder.childCount - 1) / 4) * 35.3));
        else rts.sizeDelta = new Vector2(728, glg.padding.top + glg.padding.bottom + 174);

        transform.Find("Sell Image/Sell Parameters").gameObject.SetActive(false);
    }

    private void CreatePrefabInHolder<T>(List<T> checkOn, int i, int insn, out int outsn, Transform holder) where T : IStorageItem
    {
        outsn = insn;
        GameObject dublicate = Instantiate(SellingPrefab, holder);
        Button button = dublicate.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        if ((checkOn != null && checkOn.Count > 0) && checkOn[i].Count > 0)
        {
            object item = checkOn[i].Item;
            dublicate.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = checkOn[i].Count.ToString();
            dublicate.transform.Find("Item Icon").GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(item);
            button.onClick.AddListener(() => TheItem = item);
            int s = insn;
            button.onClick.AddListener(() => SellParameters(s, TheItem));
            outsn++;
            SellingItems.Add(dublicate);
        }
        else Destroy(dublicate);
    }

    private void SellParameters(int snumb, object item)
    {
        transform.Find("Sell Image/Sell Parameters").gameObject.SetActive(true);

        for (int i = 0; i < SellingItems.Count; i++)
        {
            if (i == snumb)
                SellingItems[i].GetComponent<Image>().color = new Color32(0, 0, 0, 140);
            else
                SellingItems[i].GetComponent<Image>().color = new Color32(0, 0, 0, 60);
        }
        if (item is Plants)
        {
            Plants p = (Plants)item;
            PriceRange = new Vector2(1, PlantPR[p]);
            if (StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == p).count < 10)
                maxcount = StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == p).count;
            else maxcount = 10;
        }
        else if (item is Fruits)
        {
            Fruits f = (Fruits)item;
            PriceRange = new Vector2(1, FruitPR[f]);
            if (StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == f).count < 10)
                maxcount = StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == f).count;
            else maxcount = 10;
        }
        else if (item is AProducts)
        {
            AProducts ap = (AProducts)item;
            PriceRange = new Vector2(1, AProductPR[ap]);
            if (StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == ap).count < 10)
                maxcount = StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == ap).count;
            else maxcount = 10;
        }
        else if (item is Products)
        {
            Products ap = (Products)item;
            PriceRange = new Vector2(1, ProductPR[ap]);
            if (StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == ap).count < 10)
                maxcount = StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == ap).count;
            else
                maxcount = 10;
        }
        else if (item is Items)
        {
            Items it = (Items)item;
            PriceRange = new Vector2(1, ItemPR[it]);
            if (!Storage.instance.hasEnought(it, 10, false)) maxcount = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == it).count;
            else maxcount = 10;
        }
        else if (item is a_f_types)
        {
            a_f_types af = (a_f_types)item;
            PriceRange = new Vector2(1, afPR[af]);
            if (!Storage.instance.hasEnought(af, 10, false)) maxcount = StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == af).amount;
            else maxcount = 10;
        }
        TempPriceRange = PriceRange;
        count = 1;
        UpdateCount(0);
        UpdatePrice((int)PriceRange.y);
    }

    public void UpdatePrice(int p)
    {
        price += p;
        if (p < 0 && price < TempPriceRange.x) price = (int)TempPriceRange.x;
        else if (p >= 0 && price >= TempPriceRange.y) price = (int)TempPriceRange.y;
        PriceParameterText.text = price.ToString();
    }

    public void PriceToMaxOrMin(int i)
    {
        if (i < 0) price = (int)TempPriceRange.x;
        else if (i > 0) price = (int)TempPriceRange.y;
        PriceParameterText.text = price.ToString();
    }

    public void CountToMaxOrMin(int i)
    {
        if (i < 0) count = 1;
        else if (i > 0) count = 10;
        CountParameterText.text = count.ToString();
        UpdateCount(0);
    }

    public void UpdateCount(int c)
    {
        count += c;
        if (count < 1) count = 1;
        else if (count >= maxcount) count = maxcount;
        TempPriceRange.x = PriceRange.x * count;
        TempPriceRange.y = PriceRange.y * count;
        CountParameterText.text = count.ToString();
        price = (int)TempPriceRange.y;
        if (price <= TempPriceRange.x) price = (int)TempPriceRange.x;
        if (price >= TempPriceRange.y) price = (int)TempPriceRange.y;
        UpdatePrice(0);
    }

    public void Sell()
    {
        for (int i = 0; i < itemsinShop.Count; i++)
        {
            Debug.Log("Sell Window ssNumber = " + ssNumber);
            ShopItem si = itemsinShop[i].GetComponent<ShopItem>();
            if (si.ssNumber == ssNumber)
            {
                Debug.Log("Match found = " + si.ssNumber);
                si.TheItem = TheItem;
                si.price = price;
                si.count = count;
                si.sell = true;
                si.LoadUI();
                si.btn.onClick.RemoveAllListeners();
                DeleteFromStorage();
                break;
            }
        }
        count = 0;
        maxcount = 0;
        price = 0;
        PriceRange = new Vector2(0, 0);
        TempPriceRange = new Vector2(0, 0);
        SellWindowState (false);

        transform.Find("Sell Image/Sell Parameters").gameObject.SetActive(false);
    }

    public void SellWindowState(bool tf)
    {
        SellWindow.SetActive(tf);
    }

    private void DeleteFromStorage()
    {
        Storage.instance.UpdateThingCount(TheItem, -count);
        if(PlantsHolder.instance != null) PlantsHolder.instance.UpdateCountOfPlants();
        StaticDatas.SaveDatas();
    }

    public void ResetSituation()
    {
        SellWindowState(false);
        if (canvasGroup != null) StaticDatas.AdjustCanvasGroup(canvasGroup, false);
    }

    public int GetPriceOfItem(object item)
    {
        if (item is Plants)
            return PlantPR[(Plants)item];
        else if (item is Fruits)
            return FruitPR[(Fruits)item];
        else if (item is a_f_types)
            return afPR[(a_f_types)item];
        else if (item is AProducts)
            return AProductPR[(AProducts)item];
        else if (item is Products)
            return ProductPR[(Products)item];
        else if (item is Items)
            return ItemPR[(Items)item];
        else return 0;
    }
}