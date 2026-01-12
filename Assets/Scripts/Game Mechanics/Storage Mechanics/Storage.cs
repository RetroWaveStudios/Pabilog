using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Storage : MonoBehaviour
{
    public static Storage instance;
    public Animator anim;
    [Header("UI Elements")]
    public GameObject BoxPrefab;
    public List<GameObject> Boxes;
    public Transform BoxHolder;

    public TextMeshProUGUI currentCount;
    public TextMeshProUGUI capacityCount;

    [Header("Expand Details")]
    public List<AStorageLevel> LevelReqs;
    public Transform TheHolder;
    public GameObject itemPrefab;

    public List<Sprite> ExOnOffSprite;
    public Image ExOnOff;

    private void Awake()
    {
        if (instance == null) instance = this;
        PopulateLevelReqs(30);
        StaticDatas.LoadDatas();
        PopulateBoxItems();
    }

    public void OpenStorage()
    {
        if (WaterSL.instance.anim.GetBool("Open Water Details"))
            WaterSL.instance.anim.SetBool("Open Water Details", false);
        if (FoodPL.instance.anim.GetBool("Open Details"))
            FoodPL.instance.anim.SetBool("Open Details", false);
        if (TasksLogic.instance.anim.GetBool("Open Task Board"))
            TasksLogic.instance.anim.SetBool("Open Task Board", false);
        int id = Animator.StringToHash("Open Storage");
        anim.SetBool("Open Storage", !anim.GetBool(id));
    }

    public void PopulateBoxItems()
    {
        int count = 0;
        foreach (Transform holder in BoxHolder)
        {
            if (!holder.name.Contains("Text"))
                foreach (Transform item in holder)
                { 
                    Debug.Log($"inside {holder.name}: destroying {item.name}");
                    Destroy(item.gameObject);
                }
        }
        Boxes.Clear();

        for (int i = 0; i < StaticDatas.PlayerData.Storage.PlantsInStorage.Count; i++)
            AddBox(StaticDatas.PlayerData.Storage.PlantsInStorage[i].Plant, StaticDatas.PlayerData.Storage.PlantsInStorage[i].count, Category.Plants);
        count = StaticDatas.PlayerData.Storage.PlantsInStorage?.Count ?? 0;
        AdjustHolderSize(BoxHolder.Find("Plants"), count);

        int f = 0;
        for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i++)
            if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts[i].amount > 0){
                f++;
                AddBox(StaticDatas.PlayerData.PlayerInfos.Food.Amounts[i].food, StaticDatas.PlayerData.PlayerInfos.Food.Amounts[i].amount, Category.AnimalFood);
            }
        AdjustHolderSize(BoxHolder.Find("Foods"), f);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.FruitInStorage.Count; i++)
            AddBox(StaticDatas.PlayerData.Storage.FruitInStorage[i].Fruit, StaticDatas.PlayerData.Storage.FruitInStorage[i].count, Category.Fruits);
        count = StaticDatas.PlayerData.Storage.FruitInStorage?.Count ?? 0;
        AdjustHolderSize(BoxHolder.Find("Fruits"), count);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.a_p_inStorage.Count; i++)
            AddBox(StaticDatas.PlayerData.Storage.a_p_inStorage[i].animal_products, StaticDatas.PlayerData.Storage.a_p_inStorage[i].count, Category.AProducts);
        count = StaticDatas.PlayerData.Storage.a_p_inStorage?.Count ?? 0;
        AdjustHolderSize(BoxHolder.Find("Animal Products"), count);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.ProductsInStorage.Count; i++)
            AddBox(StaticDatas.PlayerData.Storage.ProductsInStorage[i].product, StaticDatas.PlayerData.Storage.ProductsInStorage[i].count, Category.Products);
        count = StaticDatas.PlayerData.Storage.ProductsInStorage?.Count ?? 0;
        AdjustHolderSize(BoxHolder.Find("Products"), count);

        for (int i = 0; i < StaticDatas.PlayerData.Storage.ItemsInStorage.Count; i++)
            AddBox(StaticDatas.PlayerData.Storage.ItemsInStorage[i].item, StaticDatas.PlayerData.Storage.ItemsInStorage[i].count, Category.Items);
        count = StaticDatas.PlayerData.Storage.ItemsInStorage?.Count ?? 0;
        AdjustHolderSize(BoxHolder.Find("Items"), count);

        AdjustMainHolder();
        UpdateCountInBox();
        if(PlantsHolder.instance != null)
            PlantsHolder.instance.PopulatePlantsHolder();
        if (TasksLogic.instance != null)
            TasksLogic.instance.CheckTasks();
    }

    private void reArrangeItem(Transform item)
    {
        RectTransform iRts = item.transform.Find("Item Icon").GetComponent<RectTransform>();
        iRts.sizeDelta = new Vector2(110, 110);
        iRts.localPosition = new Vector2(0, 30);
        iRts.anchorMax = new Vector2(1, 1); 
        iRts.anchorMin = new Vector2(1, 1);
        iRts.pivot = new Vector2((float)0.5, (float)0.5);

        RectTransform cRts = item.transform.Find("Count").GetComponent<RectTransform>();
        cRts.sizeDelta = new Vector2(0, 40);
        cRts.localPosition = new Vector2(0, -80);
        cRts.anchorMax = new Vector2(1, 0);
        cRts.anchorMin = new Vector2(0, 0);
        cRts.pivot = new Vector2((float)0.5, 0);
    }

    private void AddBox(object item, int count, Category category)
    {
        Transform holder = BoxHolder;
        if (category == Category.Plants) holder = BoxHolder.Find("Plants");
        else if (category == Category.AnimalFood) holder = BoxHolder.Find("Foods");
        else if (category == Category.AProducts) holder = BoxHolder.Find("Animal Products");
        else if (category == Category.Products) holder = BoxHolder.Find("Products");
        else if (category == Category.Items) holder = BoxHolder.Find("Items");
        else if (category == Category.Fruits) holder = BoxHolder.Find("Fruits");

        GameObject i = Instantiate(BoxPrefab, holder);
        if (holder.name != "Foods")
            i.transform.name = item.ToString();
        else
            i.transform.name = item.ToString() + " Animal Food";
        i.GetComponent<S_Box>().AddItem(item, count);
        reArrangeItem(i.transform);
        Boxes.Add(i);
    }
    
    private void AdjustHolderSize(Transform holder, int childCount)
    {
        Debug.Log($"in {holder.name} there are {childCount} different things");
        BoxHolder.Find(holder.name + " Text").gameObject.SetActive(true);
        holder.gameObject.SetActive(true);
        int rows = 0;
        Debug.Log($"childCount = {childCount}");
        if(childCount > 0)
        {
            if (childCount > 4) rows = (int)Math.Ceiling((double)childCount / 4);
            else rows = 1;
        }
        else
        {
            BoxHolder.Find(holder.name + " Text").gameObject.SetActive(false);
            holder.gameObject.SetActive(false);
            rows = 0;
        }
        Debug.Log($"rows = {rows}");

        GridLayoutGroup vlg = holder.GetComponent<GridLayoutGroup>();
        holder.GetComponent<RectTransform>().sizeDelta = new Vector2(660, (rows * vlg.cellSize.y) + ((rows - 1) * vlg.spacing.y) + + vlg.padding.top + 65);
    }

    private void AdjustMainHolder()
    {
        RectTransform rts = BoxHolder.GetComponent<RectTransform>();
        float size = 0;
        for (int i = 0; i < BoxHolder.childCount; i++)
            if(BoxHolder.transform.GetChild(i).gameObject.activeInHierarchy)
                size += BoxHolder.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
        rts.sizeDelta = new Vector2(660, size);
    }

    public bool hasEnStorage(int reqAmount)
    {
        if (LevelReqs[StaticDatas.PlayerData.StorageLevel - 1].Capacity - SumCount() >= reqAmount) return true;
        else PushNotice.instance.Push("No Space in Storage", PushType.Alert); return false;
    }

    public int SumCount()
    {
        return CatSumCount(Category.Plants) + CatSumCount(Category.Fruits) + CatSumCount(Category.AProducts) + CatSumCount(Category.Products) + CatSumCount(Category.Items) + CatSumCount(Category.AnimalFood);
    }

    public int CatSumCount(Category cat)
    {
        int count = 0;
        if (cat == Category.AnimalFood)
        {
            for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i++)
                count += StaticDatas.PlayerData.PlayerInfos.Food.Amounts[i].amount;
        }
        else
        {
            for (int i = 0; i < Boxes.Count; i++)
            {
                S_Box s = Boxes[i].GetComponent<S_Box>();
                if (s.category == cat) count += s.count;
            }
        }
        return count;
    }

    public int GetCountOf(object item)
    {
        for (int i = 0; i < Boxes.Count; i++)
        {
            S_Box s = Boxes[i].GetComponent<S_Box>();
            if (s.category == Category.Plants && item is Plants)
            {
                if (s.plant == (Plants)item) { Debug.Log($"item found: {(Plants)item} count: {s.count}"); return s.count; }
            }

            else if (s.category == Category.Fruits && item is Fruits)
            {
                if (s.fruit == (Fruits)item) { Debug.Log($"item found: {(Fruits)item} count: {s.count}"); return s.count; }
            }

            else if (s.category == Category.AnimalFood && item is a_f_types)
            {
                if(s.Food == (a_f_types)item) { Debug.Log($"item found: {(a_f_types)item} count: {s.count}"); return s.count; }
            }

            else if (s.category == Category.AProducts && item is AProducts)
            {
                if (s.animal_product == (AProducts)item) { Debug.Log($"item found: {(AProducts)item} count: {s.count}"); return s.count; }
            }

            else if (s.category == Category.Products && item is Products)
            {
                if (s.product == (Products)item) { Debug.Log($"item found: {(Products)item} count: {s.count}"); return s.count; }
            }

            else if (s.category == Category.Items && item is Items)
            {
                if (s.item == (Items)item) { Debug.Log($"item found: {(Items)item} count: {s.count}"); return s.count; }
            }
        }
        return 0;
    }

    public void UpdateThingCount(object item, int count)
    {
        if (item is Plants)
            UpdatePlantCount((Plants)item, count);
        else if (item is Fruits)
            UpdateFruitCount((Fruits)item, count);
        else if (item is a_f_types)
            UpdateAnimalFood((a_f_types)item, count);
        else if (item is AProducts)
            UpdateAPCount((AProducts)item, count);
        else if (item is Products)
            UpdateProductCount((Products)item, count);
        else if (item is Items)
            UpdateItemCount((Items)item, count);
    }

    private void UpdatePlantCount(Plants plant, int count)
    {
        // Find existing entry
        if (StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == plant) == null)
        {
            StaticDatas.PlayerData.Storage.PlantsInStorage.Add(new PlantCount { Plant = plant, count = count });
            AddBox(plant, StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == plant).count, Category.Plants);
        }
        else
            StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == plant).count += count;
        StaticDatas.SaveDatas();
        PopulateBoxItems();
    }

    private void UpdateFruitCount(Fruits fruit, int count)
    {
        // Find existing entry
        if (StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == fruit) == null)
        {
            StaticDatas.PlayerData.Storage.FruitInStorage.Add(new FruitCount { Fruit = fruit, count = count });
            AddBox(fruit, StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == fruit).count, Category.Fruits);
        }
        else
            StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == fruit).count += count;
        StaticDatas.SaveDatas();
        PopulateBoxItems();
    }

    private void UpdateAPCount(AProducts ap, int count)
    {
        // Find existing entry
        if (StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == ap) == null)
        {
            // Add new if not found
            StaticDatas.PlayerData.Storage.a_p_inStorage.Add(new APCount { animal_products = ap, count = count });
            AddBox(ap, StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == ap).count,Category.AProducts);
        }
        else 
            StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == ap).count += count;
        StaticDatas.SaveDatas();
        PopulateBoxItems();
    }

    private void UpdateProductCount(Products pr, int count)
    {
        // Find existing entry
        if (StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == pr) == null)
        {
            // Add new if not found
            StaticDatas.PlayerData.Storage.ProductsInStorage.Add(new ProductCount { product = pr, count = count });
            AddBox(pr, StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == pr).count, Category.Products);
        }
        else
            StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == pr).count += count;
        StaticDatas.SaveDatas();
        PopulateBoxItems();
    }

    private void UpdateItemCount(Items item, int count)
    {
        // Find existing entry
        if (StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == item) == null)
        {
            // Add new if not found
            StaticDatas.PlayerData.Storage.ItemsInStorage.Add(new ItemCount { item = item, count = count });
            AddBox(item, StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == item).count, Category.Items);
        }
        else
            StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == item).count += count;

        StaticDatas.SaveDatas();
        PopulateBoxItems();
        for (int i = 0; i < ProductionLogic.instance.Machines.Count; i++)
            if (ProductionLogic.instance.Machines[i].GetComponent<Machine>().mStats.state == ASpotState.Broken) ProductionLogic.instance.Machines[i].GetComponent<Machine>().PopulateFixers();
    }

    private void UpdateAnimalFood(a_f_types food, int amount)
    {
        bool add = false;
        if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food) == null){
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Add(new afAmount() { food = food, amount = amount }); add = true;
        }
        else if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food).amount == 0){
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food).amount += amount; add = true;
        }
        else
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food).amount += amount;

        if (add)
            AddBox(food, StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food).amount, Category.AnimalFood);

        StaticDatas.SaveDatas();
        PopulateBoxItems();
        CalSumOfFood();
        FoodPL.instance.AnimalFoodText.text = StaticDatas.PlayerData.PlayerInfos.Food.sumAmount.ToString();
        if (AnimalsLogic.instance != null) AnimalsLogic.instance.PopulateFoodChooser();

    }

    public void CalSumOfFood()
    {
        StaticDatas.PlayerData.PlayerInfos.Food.sumAmount = 0;
        for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i++)
            StaticDatas.PlayerData.PlayerInfos.Food.sumAmount += StaticDatas.PlayerData.PlayerInfos.Food.Amounts[i].amount;
    }

    public void UpdateCountInBox()
    {
        Debug.Log("updating item's count");
        for (int i = 0; i < Boxes.Count; i++)
        {
            S_Box s = Boxes[i].GetComponent<S_Box>();
            s.UpdateCount();
            if (s.count <= 0)
            {
                if (s.category == Category.Plants)StaticDatas.PlayerData.Storage.PlantsInStorage.
                        Remove(StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == s.plant));
                else if (s.category == Category.Fruits)StaticDatas.PlayerData.Storage.FruitInStorage.
                        Remove(StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == s.fruit));
                else if (s.category == Category.AProducts)StaticDatas.PlayerData.Storage.a_p_inStorage.
                        Remove(StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == s.animal_product));
                else if (s.category == Category.Products)StaticDatas.PlayerData.Storage.ProductsInStorage.
                        Remove(StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == s.product));
                else if (s.category == Category.Items)StaticDatas.PlayerData.Storage.ItemsInStorage.
                        Remove(StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == s.item));
                Destroy(Boxes[i].gameObject);
                Boxes.RemoveAt(i);
            }
        }
        StaticDatas.SaveDatas();
        currentCount.text = SumCount().ToString();
        capacityCount.text = LevelReqs[StaticDatas.PlayerData.StorageLevel - 1].Capacity.ToString();
    }

    public bool hasEnought(object item, int count, bool push)
    {
        for (int i = 0; i < Boxes.Count; i++)
        {
            if (Boxes[i] == null) { Debug.Log("Boxes[i] == null for " + item); return false; }
            S_Box s = Boxes[i].GetComponent<S_Box>();
            if (s == null) { Debug.Log("s is null"); return false; }
            if (s.category == Category.Plants && item is Plants) { if (s.plant == (Plants)item && s.count >= count) return true; }
            else if (s.category == Category.Fruits && item is Fruits) { if (s.fruit == (Fruits)item && s.count >= count) return true; }
            else if (s.category == Category.AnimalFood && item is a_f_types) { if (s.Food == (a_f_types)item && s.count >= count) return true; }
            else if (s.category == Category.AProducts && item is AProducts){ if (s.animal_product == (AProducts)item && s.count >= count) return true; }
            else if (s.category == Category.Products && item is Products){ if (s.product == (Products)item && s.count >= count)  return true; }
            else if (s.category == Category.Items && item is Items){ if(s.item == (Items)item && s.count >= count) return true; }
        }
        if(push) PushNotice.instance.Push("No Enough Resources", PushType.Alert);
        return false;
    }

    #region Storage Expand
    private void PopulateLevelReqs(int maxSLevel)
    {
        var proto = new AStorageLevel();
        for (int i = 0; i < maxSLevel; i++)
        {
            AStorageLevel nlevel = proto.Clone();
            int tbr = 1;
            
            if (i > 0) tbr = (i / 10) + 1;
            nlevel = new AStorageLevel()
            {
                LevelNumber = i,
                ItemCount = (i - 1) + 3,

                ToolSet = tbr,
                Capacity = 150 + (i * 25)
            };
            LevelReqs.Add(nlevel);
        }
    }

    public void ExpandOnOff()
    {
        PopulateExpand();
        TheHolder.transform.Find("Storage Items").gameObject.SetActive(!TheHolder.transform.Find("Storage Items").gameObject.activeInHierarchy);
        TheHolder.transform.Find("Expand Screen").gameObject.SetActive(!TheHolder.transform.Find("Expand Screen").gameObject.activeInHierarchy);

        ExOnOff.sprite = ExOnOffSprite[TheHolder.transform.Find("Storage Items").gameObject.activeInHierarchy ? 0 : 1];
    }

    private void PopulateExpand()
    {
        // settting Items Option details
        Transform itemOption = TheHolder.transform.Find("Expand Screen/Items Option");
        foreach(Transform item in itemOption.transform.Find("Reqs Holder")) Destroy(item.gameObject);
        itemOption.transform.Find("From To/Current Capacity").GetComponent<TextMeshProUGUI>().text = LevelReqs[StaticDatas.PlayerData.StorageLevel - 1].Capacity.ToString();
        itemOption.transform.Find("From To/Next Capacity").GetComponent<TextMeshProUGUI>().text = LevelReqs[StaticDatas.PlayerData.StorageLevel].Capacity.ToString();

        List<Items> rItems = new List<Items>() { Items.Tape, Items.Pliers, Items.Drill };
        for (int i = 0; i < 3; i++)
        {
            GameObject dublicate = Instantiate(itemPrefab, itemOption.transform.Find("Reqs Holder"));
            Destroy(dublicate.GetComponent<Button>());
            dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
            dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.items.Find(e => e.item == rItems[i]).sprite;
            dublicate.transform.Find("Details").GetComponent<Image>().color = new Color32(0, 0, 0, 150);
            dublicate.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(140, 40);
            dublicate.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -30, 0);

            dublicate.transform.Find("Details/Count").gameObject.SetActive(true);
            if (hasEnought(rItems[i], LevelReqs[StaticDatas.PlayerData.StorageLevel].ItemCount, false))
                dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().color = new Color32(50, 255, 0, 255);
            else
                dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);

            dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = GetCountOf(rItems[i]) + " / " + LevelReqs[StaticDatas.PlayerData.StorageLevel].ItemCount.ToString();
            dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().fontSizeMax = 40f;
        }

        // settting Tool Set Option details
        Transform tbOption = TheHolder.transform.Find("Expand Screen/Tool Set Option");
        foreach (Transform item in tbOption.transform.Find("Reqs Holder")) Destroy(item.gameObject);
        tbOption.transform.Find("From To/Current Capacity").GetComponent<TextMeshProUGUI>().text = LevelReqs[StaticDatas.PlayerData.StorageLevel - 1].Capacity.ToString();
        tbOption.transform.Find("From To/Next Capacity").GetComponent<TextMeshProUGUI>().text = LevelReqs[StaticDatas.PlayerData.StorageLevel + 1].Capacity.ToString();

        GameObject tbdublicate = Instantiate(itemPrefab, tbOption.transform.Find("Reqs Holder"));
        Destroy(tbdublicate.GetComponent<Button>());
        tbdublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        tbdublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.items.Find(e => e.item == Items.ToolSet).sprite;
        tbdublicate.transform.Find("Details").GetComponent<Image>().color = new Color32(0, 0, 0, 150);
        tbdublicate.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(140, 40);
        tbdublicate.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -30, 0);

        tbdublicate.transform.Find("Details/Count").gameObject.SetActive(true);
        if (hasEnought(Items.ToolSet, LevelReqs[StaticDatas.PlayerData.StorageLevel].ToolSet, false))
            tbdublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().color = new Color32(50, 255, 0, 255);
        else
            tbdublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);

        tbdublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = GetCountOf(Items.ToolSet) + " / " + LevelReqs[StaticDatas.PlayerData.StorageLevel].ToolSet.ToString();
        tbdublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().fontSizeMax = 40f;

        // set up update buttons
        bool[] has = new bool[3] { true, true, true };
        bool hasAll = true;
        for (int i = 0; i < rItems.Count; i++)
            if (GetCountOf(rItems[i]) < LevelReqs[StaticDatas.PlayerData.StorageLevel].ItemCount) has[i] = false;

        for (int i = 0; i < has.Length; i++) if (!has[i]) { Debug.Log("has " + i + has[i]); hasAll = false; }

        Transform update = itemOption.transform.Find("Update");
        update.GetComponent<Button>().onClick.RemoveAllListeners();
        if (hasAll){
            Debug.Log($"All reqs met");
            update.GetComponent<Button>().onClick.AddListener(() => ExpandStorage(0));
            update.transform.Find("Update").GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255);
        }
        else
        {
            Debug.Log($"Not all reqs met");
            update.transform.Find("Update").GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 100);
        }

        Transform tbupdate = tbOption.transform.Find("Update");
        tbupdate.GetComponent<Button>().onClick.RemoveAllListeners();
        if (GetCountOf(Items.ToolSet) >= LevelReqs[StaticDatas.PlayerData.StorageLevel].ToolSet)
        {
            Debug.Log($"All reqs met");
            tbupdate.GetComponent<Button>().onClick.AddListener(() => ExpandStorage(1));
            tbupdate.transform.Find("Update").GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255);
        }
        else
        {
            Debug.Log($"Not all reqs met");
            tbupdate.transform.Find("Update").GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 100);
        }
    }

    private void ExpandStorage(int index)
    {
        List<Items> rItems = new List<Items>() { Items.Tape, Items.Pliers, Items.Drill };
        if (index == 0){
            for (int i = 0; i < rItems.Count; i++)
                UpdateItemCount(rItems[i], -LevelReqs[StaticDatas.PlayerData.StorageLevel].ItemCount);
            StaticDatas.PlayerData.StorageLevel++;
        }
        else if (index == 1){
            UpdateItemCount(Items.ToolSet, -LevelReqs[StaticDatas.PlayerData.StorageLevel].ToolSet);
            StaticDatas.PlayerData.StorageLevel += 2;
        }
        StaticDatas.SaveDatas();
        PopulateExpand();
    }
    #endregion
}