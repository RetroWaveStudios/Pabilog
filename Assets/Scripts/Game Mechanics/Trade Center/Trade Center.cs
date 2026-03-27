using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeCenter : MonoBehaviour
{
    public static TradeCenter instance;

    public GameObject ItemPrefab;
    public List<GameObject> yourItems;
    public List<GameObject> onMarket;

    public int AddedMoney = 0;

    public int TotalMoneyOnDeal;
    public int TotalMoneyOnMarket;

    public int YIPersentage = 10;
    public int OMPersentage = 8;

    public List<ThingCount> c_SI = new();
    public List<ThingCount> c_MI = new();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PopulateYourItems(); PopulateTradeableItems(); CheckForLimit();
    }

    private void PopulateYourItems()
    {
        Transform yiHodler = transform.Find("Your Items/Items/Viewport/Content");
        yourItems.Clear();
        foreach (Transform item in yiHodler) Destroy(item.gameObject);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.PlantsInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.PlantsInStorage, i, yiHodler, "yi", yourItems);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.FruitInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.FruitInStorage, i, yiHodler, "yi", yourItems);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.a_p_inStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.a_p_inStorage, i, yiHodler, "yi", yourItems);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.ProductsInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.ProductsInStorage, i, yiHodler, "yi", yourItems);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.ItemsInStorage.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.Storage.ItemsInStorage, i, yiHodler, "yi", yourItems);

        for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i++)
            CreatePrefabInHolder(StaticDatas.PlayerData.PlayerInfos.Food.Amounts, i, yiHodler, "yi", yourItems);

        RectTransform rts = yiHodler.GetComponent<RectTransform>();
        GridLayoutGroup glg = yiHodler.GetComponent<GridLayoutGroup>();
        float count = (float)yourItems.Count / 5;
        float row = (float)Math.Ceiling(count);

        rts.sizeDelta = new Vector2(rts.sizeDelta.x, glg.padding.top + glg.padding.bottom + (row * glg.cellSize.y) + ((row - 1) * glg.spacing.y) + 50);

    }

    private void PopulateTradeableItems()
    {
        Transform omHolder = transform.Find("On Market/Items/Viewport/Content");
        onMarket.Clear();
        foreach (Transform item in omHolder) Destroy(item.gameObject);

        List<ThingCount> itemCountInStorage = new();
        for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_plants.Count; i++)
            itemCountInStorage.Add(FillThingCount(StaticDatas.PlayerData.unlocked_items.u_plants[i], Storage.instance.GetCountOf(StaticDatas.PlayerData.unlocked_items.u_plants[i])));
        for (int i = 0; i < itemCountInStorage.Count; i++)
            CreatePrefabInHolder(itemCountInStorage, i, omHolder, "market", onMarket);
        itemCountInStorage.Clear();

        for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_fruits.Count; i++)
            itemCountInStorage.Add(FillThingCount(StaticDatas.PlayerData.unlocked_items.u_fruits[i], Storage.instance.GetCountOf(StaticDatas.PlayerData.unlocked_items.u_fruits[i])));
        for (int i = 0; i < itemCountInStorage.Count; i++)
            CreatePrefabInHolder(itemCountInStorage, i, omHolder, "market", onMarket);
        itemCountInStorage.Clear();

        List<a_f_types> foodnames = new List<a_f_types>() { a_f_types.Wheat, a_f_types.Corn, a_f_types.Carrot, a_f_types.Potato };
        List<a_f_types> uFoods = TasksLogic.instance.GetUnlockedFood(foodnames);
        for (int i = 0; i < uFoods.Count; i++)
            itemCountInStorage.Add(FillThingCount(uFoods[i], Storage.instance.GetCountOf(uFoods[i])));
        for (int i = 0; i < uFoods.Count; i++)
            CreatePrefabInHolder(itemCountInStorage, i, omHolder, "market", onMarket);
        itemCountInStorage.Clear();

        for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_a_products.Count; i++)
            itemCountInStorage.Add(FillThingCount(StaticDatas.PlayerData.unlocked_items.u_a_products[i], Storage.instance.GetCountOf(StaticDatas.PlayerData.unlocked_items.u_a_products[i])));
        for (int i = 0; i < itemCountInStorage.Count; i++)
            CreatePrefabInHolder(itemCountInStorage, i, omHolder, "market", onMarket);
        itemCountInStorage.Clear();

        for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_Products.Count; i++)
            itemCountInStorage.Add(FillThingCount(StaticDatas.PlayerData.unlocked_items.u_Products[i], Storage.instance.GetCountOf(StaticDatas.PlayerData.unlocked_items.u_Products[i])));
        for (int i = 0; i < itemCountInStorage.Count; i++)
            CreatePrefabInHolder(itemCountInStorage, i, omHolder, "market", onMarket);
        itemCountInStorage.Clear();

        Items[] uItems = (Items[])Enum.GetValues(typeof(Items));
        uItems = uItems.Where(x => x != Items.None && x != Items.ToolSet).ToArray();
        for (int i = 0; i < uItems.Length; i++)
            itemCountInStorage.Add(FillThingCount(uItems[i], Storage.instance.GetCountOf(uItems[i])));
        for (int i = 0; i < uItems.Length; i++)
            CreatePrefabInHolder(itemCountInStorage, i, omHolder, "market", onMarket);
        itemCountInStorage.Clear();

        RectTransform rts = omHolder.GetComponent<RectTransform>();
        GridLayoutGroup glg = omHolder.GetComponent<GridLayoutGroup>();

        float count = (float)onMarket.Count / 5;
        float row = (float)Math.Ceiling(count);

        rts.sizeDelta = new Vector2(rts.sizeDelta.x, glg.padding.top + glg.padding.bottom + (row * glg.cellSize.y) + ((row - 1) * glg.spacing.y) + 50);
    }

    private ThingCount FillThingCount(object i, int c)
    {
        return new ThingCount()
        {
            item = i,
            count = c
        };
    }

    private void CreatePrefabInHolder<T>(List<T> checkOn, int i, Transform holder, string locate, List<GameObject> list) where T : IStorageItem
    {
        GameObject dublicate = Instantiate(ItemPrefab, holder);
        Debug.Log($"for {locate} item is {checkOn[i].Item}");
        dublicate.name = checkOn[i].Item.ToString();
        TradeItem ti = dublicate.GetComponent<TradeItem>();
        Button button = dublicate.transform.Find("Button Cover").GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        int price = 0;
        if (locate == "market"){
            price = (int)Math.Ceiling(MyShop.instance.GetPriceOfItem(checkOn[i].Item) + (((double)MyShop.instance.GetPriceOfItem(checkOn[i].Item) / 100) * OMPersentage));
            ti.place = locate;
            ti.GetDetails(checkOn[i].Item, price, Storage.instance.GetCountOf(checkOn[i].Item));
            button.onClick.AddListener(() => ti.UpdateState());

            list.Add(dublicate);
        }
        else if(locate == "yi"){
            price = (int)Math.Floor(MyShop.instance.GetPriceOfItem(checkOn[i].Item) - (((double)MyShop.instance.GetPriceOfItem(checkOn[i].Item) / 100) * YIPersentage));
            if ((checkOn != null && checkOn.Count > 0) && checkOn[i].Count > 0)
            {
                ti.place = locate;
                ti.GetDetails(checkOn[i].Item, price, Storage.instance.GetCountOf(checkOn[i].Item));
                button.onClick.AddListener(() => ti.UpdateState());

                list.Add(dublicate);
            }
            else Destroy(dublicate);
        }
    }

    public void UpdateTMOnDeal()
    {
        TotalMoneyOnDeal = 0;
        if (yourItems.Count > 0)
            for (int i = 0; i < yourItems.Count; i++) { if (yourItems[i].GetComponent<TradeItem>().chosen) TotalMoneyOnDeal += yourItems[i].GetComponent<TradeItem>().TotalPrice; }
        else
            TotalMoneyOnDeal = 0;
        transform.Find("Your Items/Dealable Money/Money").GetComponent<TextMeshProUGUI>().text = TotalMoneyOnDeal.ToString();
    }

    public void UpdateTMOnMarket()
    {
        TotalMoneyOnMarket = 0;
        if (onMarket.Count > 0)
            for (int i = 0; i < onMarket.Count; i++) { if (onMarket[i].GetComponent<TradeItem>().chosen) TotalMoneyOnMarket += onMarket[i].GetComponent<TradeItem>().TotalPrice; }
        else
            TotalMoneyOnMarket = 0;
        transform.Find("On Market/Dealable Money/Money").GetComponent<TextMeshProUGUI>().text = TotalMoneyOnMarket.ToString();
    }

    public void UpdateItemCount(IStorageItem items, string place, bool keeping)
    {
        ThingCount tc = new ThingCount() { item = items.Item, count = items.Count };
        if (place == "market")
        {
            if(keeping){
                var existing = c_MI.Find(e => e.item.Equals(tc.item));

                if (existing != null)
                    existing.count = tc.count;
                else
                    c_MI.Add(tc);
            }
            else
                c_MI.Remove(c_MI.Find(e => e.item == tc.item));
            UpdateTMOnMarket();
        }
        else if(place == "yi")
        {
            if(keeping){
                var existing = c_SI.Find(e => e.item.Equals(tc.item));

                if (existing != null)
                    existing.count = tc.count;
                else
                    c_SI.Add(tc);
            }
            else
                c_SI.Remove(c_SI.Find(e => e.item == tc.item));
            UpdateTMOnDeal();
        }

        ChangeAddedMoney(0);
    }

    public void ChangeAddedMoney(int amount)
    {
        AddedMoney += amount;

        if (AddedMoney >= TotalMoneyOnMarket - TotalMoneyOnDeal) AddedMoney = TotalMoneyOnMarket - TotalMoneyOnDeal;

        if (AddedMoney < 0) AddedMoney = 0;
        else if (AddedMoney >= StaticDatas.PlayerData.PlayerInfos.Coin) AddedMoney = StaticDatas.PlayerData.PlayerInfos.Coin;

        transform.Find("Your Items/Money Deal/Count").GetComponent<TextMeshProUGUI>().text = AddedMoney.ToString();
        CheckForLimit();
    }

    private void CheckForLimit()
    {
        for (int i = 0; i < onMarket.Count; i++)
        {
            TradeItem ti = onMarket[i].GetComponent<TradeItem>();
            Transform BC = onMarket[i].transform.Find("Button Cover");
            Transform inc = onMarket[i].transform.Find("Count Changer/Inc");
            int availableMoney = TotalMoneyOnDeal - TotalMoneyOnMarket + AddedMoney;

            if (availableMoney < ti.price)
            {
                if (!ti.chosen)
                {
                    BC.GetComponent<Button>().interactable = false;
                    BC.GetComponent<Image>().color = new Color32(255, 255, 255, 100);
                }
                inc.gameObject.SetActive(false);
            }
            else if (availableMoney >= ti.price)
            {
                BC.GetComponent<Button>().interactable = true;
                BC.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                inc.gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < yourItems.Count; i++)
        {
            TradeItem ti = yourItems[i].GetComponent<TradeItem>();
            Transform BC = yourItems[i].transform.Find("Button Cover");
            Transform dec = yourItems[i].transform.Find("Count Changer/Dec");
            int availableMoney = TotalMoneyOnDeal - TotalMoneyOnMarket;
            if (ti.chosen && availableMoney <= 0){
                BC.GetComponent<Button>().interactable = false;
                dec.gameObject.SetActive(false);
            }
            else{
                BC.GetComponent<Button>().interactable = true;
                dec.gameObject.SetActive(true);
            }

        }

        CalculateLeftOver();
    }

    private void CalculateLeftOver()
    {
        int leftover = TotalMoneyOnDeal - TotalMoneyOnMarket + AddedMoney;
        Transform makeADeal = transform.Find("Make A Deal");
        makeADeal.Find("Leftover Money/Money").GetComponent<TextMeshProUGUI>().text = leftover.ToString();
        makeADeal.GetComponent<Button>().onClick.RemoveAllListeners();
        if (leftover >= 0 && c_SI.Count > 0)
            makeADeal.GetComponent<Button>().onClick.AddListener(() => MakeADeal());
        else
            makeADeal.GetComponent<Button>().onClick.AddListener(() => PushNotice.instance.Push($"Change trade items to match deal money - {leftover}", PushType.Notice));
    }

    private void MakeADeal()
    {
        // removing chosen items from storage
        for(int i = 0; i < c_SI.Count; i++)
            Storage.instance.UpdateThingCount(c_SI[i].item, -c_SI[i].count);

        // adding chosen items to storage
        for (int i = 0; i < c_MI.Count; i++)
            Storage.instance.UpdateThingCount(c_MI[i].item, c_MI[i].count);
        int finalMoney = (TotalMoneyOnDeal - TotalMoneyOnMarket) + AddedMoney;
        MoneySystem.instance.UpdateCoin((TotalMoneyOnDeal - TotalMoneyOnMarket) + AddedMoney, out bool e);
        AddedMoney = 0;
        ChangeAddedMoney(0);
        c_SI.Clear(); c_MI.Clear();
        PopulateYourItems();
        PopulateTradeableItems();
    }

    public void ResetSituation()
    {
        TotalMoneyOnMarket = 0;
        TotalMoneyOnDeal = 0;
        AddedMoney = 0;
        c_SI.Clear();
        c_MI.Clear();
        yourItems.Clear();
        onMarket.Clear();
        PopulateYourItems();
        PopulateTradeableItems();
    }
}
