using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Machine : MonoBehaviour
{
    public MachineProduct mProducts;
    public string machineName;
    public int mNumber { get; private set; }
    public PrD TheProduct;
    public bool prChoosed = false;

    public TextMeshProUGUI mName;

    public GameObject BrokenScreen;
    public Transform prHolder;
    public GameObject prPrefab;
    public List<GameObject> prs;

    private Transform ShelfHolder;
    public GameObject productPrefab;

    public List<Sprite> SwitchOnOff;
    public GameObject switchBtn;
    public GameObject timerPot;
    public TextMeshProUGUI timer;

    private Button btn;
    public MachineStats mStats;

    public GameObject qPrefab;
    public Transform qHolder;
    public List<GameObject> queue;

    public Sprite buyButton;

    private bool qEditing;

    public void Awake()
    {
        queue = new List<GameObject>();
        btn = GetComponent<Button>();
    }

    public void Init(PrD product, MachineStats stats, MachineProduct details)
    {
        mProducts = details;
        TheProduct = product;
        mStats = stats;

        machineName = mStats.MachineName;
        gameObject.transform.name = mProducts.MachineName;
        mName.text = mProducts.MachineName.ToString();
        gameObject.GetComponent<Image>().sprite = ProductionLogic.instance.MachinesSprites.Find(e => e.name == mProducts.MachineName);
        ShelfHolder = gameObject.transform.Find("Shelf Holder");
        ShelfHolder.gameObject.SetActive(false);
        PopulateShelf();
        PopulateQueue();
        if (mStats.state == ASpotState.Broken) PopulateFixers();
    }

    private void Update()
    {
        LoadUI();
        if (mStats.state == ASpotState.HasAnimal)
        {
            if (TheProduct.state == AState.Fertilizing) { CheckTimer(); UpdateTimer(); }
            else if (TheProduct.state == AState.ReadyToCollect) rtCollect();
        }
    }

    public void LoadUI()
    {
        btn.onClick.RemoveAllListeners();
        if (mStats.state == ASpotState.Empty && !prChoosed)
        {
            switchBtn.SetActive(false); timerPot.SetActive(false); BrokenScreen.SetActive(false);
            prHolder.gameObject.SetActive(false); btn.onClick.AddListener(() => MachineClicked());
        }
        else if (mStats.state == ASpotState.Broken)
        {
            BrokenScreen.SetActive(true);
        }
        else
        {
            BrokenScreen.SetActive(false);
            if (mStats.queue.Count < mStats.qLimit)
            {
                btn.onClick.AddListener(() => MachineClicked());
            }
            if (!prChoosed)
            {
                if (TheProduct.state == AState.None)
                {
                    prHolder.gameObject.SetActive(true); switchBtn.SetActive(true);
                }
                else if (TheProduct.state == AState.Fertilizing)
                {
                    if (!qEditing && queue.Count > 0)
                        queue[0].transform.Find("BG").GetComponent<Image>().color = Color.darkBlue;
                    prHolder.gameObject.SetActive(false); switchBtn.SetActive(false); timerPot.SetActive(true);
                }
                else if (TheProduct.state == AState.ReadyToCollect)
                {
                    if (!qEditing && queue.Count > 0)
                        queue[0].transform.Find("BG").GetComponent<Image>().color = Color.green;
                    timerPot.SetActive(false);
                }
            }
            else
            {
                prHolder.gameObject.SetActive(true); switchBtn.SetActive(true);
                if (TheProduct.state == AState.Fertilizing)
                {
                    if (!qEditing && queue.Count > 0)
                        queue[0].transform.Find("BG").GetComponent<Image>().color = Color.darkBlue;
                    timerPot.SetActive(true);
                }
                else if (TheProduct.state == AState.ReadyToCollect)
                {
                    if (!qEditing && queue.Count > 0)
                        queue[0].transform.Find("BG").GetComponent<Image>().color = Color.green;
                    timerPot.SetActive(false);
                }
            }
        }
    }

    private void MachineClicked()
    {
        ProductionLogic.instance.DeSelectProductAtAllMachines();
        MachinePH.instance.mp = mProducts;
        MachinePH.instance.PopulateHolder();
        Debug.Log("sent to populateHolder");
    }

    private void PopulateQueue()
    {
        qEditing = true;

        if (queue == null) queue = new List<GameObject>();
        foreach (Transform t in qHolder) Destroy(t.gameObject);
        Debug.Log("holder emptied");

        queue.Clear();
        Debug.Log("queue cleared");

        int qCount = mStats.queue?.Count ?? 0;
        Debug.Log($"qCount = {qCount}");

        for (int i = 0; i < mStats.qLimit; i++)
        {
            GameObject dublicate = Instantiate(qPrefab, qHolder);

            if (i < qCount)
            {
                Debug.Log($"mStats.queue[i].product = {mStats.queue[i].product}");
                dublicate.transform.name = mStats.queue[i].name;
                dublicate.transform.Find("Item").GetComponent<Image>().enabled = true;
                dublicate.transform.Find("Item").GetComponent<Image>().sprite =
                    Sprites.instance.sprites.products.Find(e => e.product == mStats.queue[i].product).sprite;

                dublicate.transform.Find("Price").gameObject.SetActive(false);
            }
            else
            {
                dublicate.transform.name = "Empty Slot";
                dublicate.transform.Find("Item").GetComponent<Image>().enabled = false;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
            }

            dublicate.transform.Find("BG").GetComponent<Image>().color = Color.darkBlue;
            dublicate.GetComponent<RectTransform>().localScale =
                new Vector3(1 - (float)(i * 0.07), 1 - (float)(i * 0.07), 1);

            queue.Add(dublicate);
        }

        int add = 0;
        if (mStats.qLimit < 4)
        {
            AddBuySlot();
            add = 1;
        }

        RectTransform rts = qHolder.GetComponent<RectTransform>();
        HorizontalLayoutGroup hlg = qHolder.GetComponent<HorizontalLayoutGroup>();

        rts.sizeDelta = new Vector2(
            ((mStats.qLimit + add) * 80) + hlg.padding.left + ((mStats.qLimit - 1) * hlg.spacing),
            100
        );

        qEditing = false;
    }

    private void AddBuySlot()
    {
        if (mStats.qLimit < 5)
        {
            int i = mStats.qLimit + 1;
            GameObject buy = Instantiate(qPrefab, qHolder);
            buy.transform.name = "Buy Slot";
            buy.transform.Find("Item").GetComponent<Image>().sprite = buyButton;
            buy.transform.Find("Price").gameObject.SetActive(true);
            if (FoodPL.instance != null)
                buy.transform.Find("Price/Price Text").GetComponent<TextMeshProUGUI>().text = FoodPL.instance.sPrices[mStats.qLimit - 1].ToString();
            buy.GetComponent<RectTransform>().localScale = new Vector3(1 - (float)(i * 0.07), 1 - (float)(i * 0.07), 1 - (float)(i * 0.07));

            Button btn = buy.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => BuySlot());
        }
    }

    private void BuySlot()
    {
        if (MoneySystem.instance.hasEnough(Currency.Coin, FoodPL.instance.sPrices[mStats.qLimit - 1]))
        {
            mStats.qLimit++;
            MoneySystem.instance.UpdateCoin(-FoodPL.instance.sPrices[mStats.qLimit - 2]);
            Transform child = qHolder.Find("Buy Slot");
            if (child != null) Destroy(child.gameObject);
            PopulateQueue();
            LuckyBox.instance.TryToFindBox();
            SaveStats();
        }
    }

    public void PickProduct(PrD p)
    {
        Debug.Log($"in PickProduct: Product = {p}");
        PopulateHolder(p);
        switchBtn.GetComponent<Image>().sprite = SwitchOnOff[0];
        switchBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        switchBtn.GetComponent<Button>().onClick.AddListener(() => SelectProduct(p));
        prChoosed = true;
    }

    private void PopulateHolder(PrD product)
    {
        Debug.Log($"in PopulateHolder: Product = {product}");
        PrD c_product = product.Clone();
        Debug.Log($"in PopulateHolder: c_product = {c_product}");
        foreach (Transform item in prHolder) Destroy(item.gameObject);
        prs = new();

        if (mStats.queue.Count == 0 && mStats.queue == null)
            foreach (Transform item in ShelfHolder) Destroy(item.gameObject); ShelfHolder.gameObject.SetActive(false);
        //  Highlighting by count
        for (int i = 0; i < c_product.p_Used.Count; i++)
        {
            GameObject dublicate = Instantiate(prPrefab, prHolder);
            dublicate.transform.name = c_product.p_Used[i].Plant.ToString();
            dublicate.transform.Find("The Product").GetComponent<Image>().sprite =
                Sprites.instance.sprites.plants.Find(e => e.plant == c_product.p_Used[i].Plant).sprite;

            Transform count = dublicate.transform.Find("Count Text");
            count.GetComponent<TextMeshProUGUI>().text = c_product.p_Used[i].count.ToString();

            Transform tbg = dublicate.transform.Find("Background");
            if (Storage.instance.hasEnought(c_product.p_Used[i].Plant, c_product.p_Used[i].count, false))
            { tbg.GetComponent<Image>().color = new Color32(43, 135, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255); }
            else { tbg.GetComponent<Image>().color = new Color32(135, 14, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255); }

            prs.Add(dublicate);
        }
        for (int i = 0; i < c_product.f_used.Count; i++)
        {
            GameObject dublicate = Instantiate(prPrefab, prHolder);
            dublicate.transform.name = c_product.f_used[i].Fruit.ToString();
            Transform tpr = dublicate.transform.Find("The Product");
            dublicate.transform.Find("The Product").GetComponent<Image>().sprite =
                Sprites.instance.sprites.fruits.Find(e => e.fruit == c_product.f_used[i].Fruit).sprite;

            Transform count = dublicate.transform.Find("Count Text");
            count.GetComponent<TextMeshProUGUI>().text = c_product.f_used[i].count.ToString();

            Transform tbg = dublicate.transform.Find("Background");
            if (Storage.instance.hasEnought(c_product.f_used[i].Fruit, c_product.f_used[i].count, false))
            { tbg.GetComponent<Image>().color = new Color32(43, 135, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255); }
            else { tbg.GetComponent<Image>().color = new Color32(135, 14, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255); }

            prs.Add(dublicate);
        }
        for (int i = 0; i < c_product.ap_Used.Count; i++)
        {
            GameObject dublicate = Instantiate(prPrefab, prHolder);
            dublicate.transform.name = c_product.ap_Used[i].animal_products.ToString();
            Transform tpr = dublicate.transform.Find("The Product");
            dublicate.transform.Find("The Product").GetComponent<Image>().sprite =
                Sprites.instance.sprites.a_products.Find(e => e.a_product == c_product.ap_Used[i].animal_products).sprite;

            Transform count = dublicate.transform.Find("Count Text");
            count.GetComponent<TextMeshProUGUI>().text = c_product.ap_Used[i].count.ToString();

            Transform tbg = dublicate.transform.Find("Background");
            if (Storage.instance.hasEnought(c_product.ap_Used[i].animal_products, c_product.ap_Used[i].count, false))
            { tbg.GetComponent<Image>().color = new Color32(43, 135, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255); }
            else { tbg.GetComponent<Image>().color = new Color32(135, 14, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255); }

            prs.Add(dublicate);
        }
        for (int i = 0; i < c_product.pr_Used.Count; i++)
        {
            GameObject dublicate = Instantiate(prPrefab, prHolder);
            dublicate.transform.name = c_product.pr_Used[i].product.ToString();
            dublicate.transform.Find("The Product").GetComponent<Image>().sprite =
                Sprites.instance.sprites.products.Find(e => e.product == c_product.pr_Used[i].product).sprite;

            Transform count = dublicate.transform.Find("Count Text");
            count.GetComponent<TextMeshProUGUI>().text = c_product.pr_Used[i].count.ToString();

            Transform tbg = dublicate.transform.Find("Background");
            if (Storage.instance.hasEnought(c_product.pr_Used[i].product, c_product.pr_Used[i].count, false))
            { tbg.GetComponent<Image>().color = new Color32(43, 135, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255); }
            else { tbg.GetComponent<Image>().color = new Color32(135, 14, 0, 220); count.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255); }

            prs.Add(dublicate);
        }

        GridLayoutGroup glg = prHolder.GetComponent<GridLayoutGroup>();
        RectTransform rts = prHolder.GetComponent<RectTransform>();
        float tpCount = c_product.p_Used.Count + c_product.f_used.Count + c_product.ap_Used.Count + c_product.pr_Used.Count; // total product count
        rts.sizeDelta = new Vector2((tpCount * glg.cellSize.x) + ((tpCount - 1) * glg.spacing.x) + glg.padding.left + glg.padding.right, 100);
    }

    private void PopulateShelf()
    {
        if (TheProduct.product != Products.None)
        {
            foreach (Transform item in ShelfHolder) Destroy(item.gameObject);
            for (int i = 0; i < TheProduct.p_Used.Count; i++)
            {
                GameObject pr = Instantiate(productPrefab, ShelfHolder);
                pr.transform.name = TheProduct.p_Used[i].Plant.ToString();
                pr.GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == TheProduct.p_Used[i].Plant).sprite;
                pr.transform.Find("Details").gameObject.SetActive(false);
            }
            for (int i = 0; i < TheProduct.f_used.Count; i++)
            {
                GameObject pr = Instantiate(productPrefab, ShelfHolder);
                pr.transform.name = TheProduct.f_used[i].Fruit.ToString();
                pr.GetComponent<Image>().sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == TheProduct.f_used[i].Fruit).sprite;
                pr.transform.Find("Details").gameObject.SetActive(false);
            }
            for (int i = 0; i < TheProduct.ap_Used.Count; i++)
            {
                GameObject pr = Instantiate(productPrefab, ShelfHolder);
                pr.transform.name = TheProduct.ap_Used[i].animal_products.ToString();
                pr.GetComponent<Image>().sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == TheProduct.ap_Used[i].animal_products).sprite;
                pr.transform.Find("Details").gameObject.SetActive(false);
            }
            for (int i = 0; i < TheProduct.pr_Used.Count; i++)
            {
                GameObject pr = Instantiate(productPrefab, ShelfHolder);
                pr.transform.name = TheProduct.pr_Used[i].product.ToString();
                pr.GetComponent<Image>().sprite = Sprites.instance.sprites.products.Find(e => e.product == TheProduct.pr_Used[i].product).sprite;
                pr.transform.Find("Details").gameObject.SetActive(false);
            }
        }
    }

    private void SelectProduct(PrD product)
    {
        Debug.Log($"in SelectProduct: Product = {product}");
        PrD c_product = product.Clone();
        bool[] has = new bool[4] { true, true, true, true }; bool hasAll = true;
        for (int i = 0; i < c_product.p_Used.Count; i++)
            if (!Storage.instance.hasEnought(c_product.p_Used[i].Plant, c_product.p_Used[i].count, true)) has[0] = false;
        for (int i = 0; i < c_product.f_used.Count; i++)
            if (!Storage.instance.hasEnought(c_product.f_used[i].Fruit, c_product.f_used[i].count, true)) has[1] = false;
        for (int i = 0; i < c_product.ap_Used.Count; i++)
            if (!Storage.instance.hasEnought(c_product.ap_Used[i].animal_products, c_product.ap_Used[i].count, true)) has[2] = false;
        for (int i = 0; i < c_product.pr_Used.Count; i++)
            if (!Storage.instance.hasEnought(c_product.pr_Used[i].product, c_product.pr_Used[i].count, true)) has[3] = false;
        for (int i = 0; i < has.Length; i++) if (!has[i]) { Debug.Log("has " + i + has[i]); hasAll = false; }
        if (hasAll)
        {
            if (mStats.queue.Count >= mStats.qLimit) return;
            Debug.Log("All resources met reqs");
            for (int i = 0; i < c_product.p_Used.Count; i++)
                Storage.instance.UpdatePlantCount(c_product.p_Used[i].Plant, -c_product.p_Used[i].count);
            for (int i = 0; i < c_product.f_used.Count; i++)
                Storage.instance.UpdateFruitCount(c_product.f_used[i].Fruit, -c_product.f_used[i].count);
            for (int i = 0; i < c_product.ap_Used.Count; i++)
                Storage.instance.UpdateAPCount(c_product.ap_Used[i].animal_products, -c_product.ap_Used[i].count);
            for (int i = 0; i < c_product.pr_Used.Count; i++)
                Storage.instance.UpdateProductCount(c_product.pr_Used[i].product, -c_product.pr_Used[i].count);

            // new tf processed item
            PrD newItem = c_product.Clone();
            newItem.state = AState.Fertilizing;

            List<PrD> inqueue = mStats.queue;

            if (inqueue.Count == 0)
                newItem.Time = DateTime.UtcNow.ToString("o");
            else
            {
                DateTime lastFinish = DateTime.Parse(inqueue[inqueue.Count - 1].Time, null, System.Globalization.DateTimeStyles.RoundtripKind);
                newItem.Time = lastFinish.AddMinutes(inqueue[inqueue.Count - 1].prTimer).ToString("o");
            }
            newItem.name = c_product.product.ToString();

            inqueue.Add(newItem); // IMPORTANT: add the clone
            queue[mStats.queue.Count - 1].transform.Find("Item").GetComponent<Image>().sprite =
                Sprites.instance.sprites.products.Find(e => e.product == c_product.product).sprite;
            queue[mStats.queue.Count - 1].transform.Find("Item").GetComponent<Image>().enabled = true;
            mStats.queue = inqueue;

            mStats.state = ASpotState.HasAnimal;
            TheProduct = inqueue[0];
            PopulateQueue();
            PopulateShelf();
            SaveStats();
            if (mStats.queue.Count == mStats.qLimit)
                foreach (Transform item in MachinePH.instance.Holder) Destroy(item.gameObject);
            if (PlantsHolder.instance != null) PlantsHolder.instance.UpdateCountOfPlants();
            ShelfHolder.gameObject.SetActive(true);
            prChoosed = false;
        }
    }

    private void CheckTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheProduct.Time, "machine " + mProducts.MachineName, out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes;
        double totalSeconds = elapsed.TotalSeconds;

        /*        
        float progress = Mathf.Clamp01((float)(totalSeconds / (TheProduct.prTimer * 60)));
        Image filler = prImage.GetComponent<Image>();
        filler.fillAmount = progress;
        */

        if (elapsedMinutes >= TheProduct.prTimer)
        {
            rtCollect();
            SaveStats();
        }
    }

    private void UpdateTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheProduct.Time, "machine " + mProducts.MachineName, out startTime)) return;

        double totalSecondsRequired = TheProduct.prTimer * 60;
        double elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
        string timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        timer.text = timeString;
    }

    public void rtCollect()
    {
        mStats.queue[0].state = AState.ReadyToCollect;
        btn.onClick.AddListener(() => CollectProduct());
    }

    private void CollectProduct()
    {
        if (TheProduct.state == AState.ReadyToCollect && Storage.instance.hasEnStorage(TheProduct.amount))
        {
            foreach (Transform item in ShelfHolder) Destroy(item.gameObject); ShelfHolder.gameObject.SetActive(false);

            Storage.instance.UpdateProductCount(TheProduct.product, TheProduct.amount);
            Storage.instance.UpdateBoxItems();
            Debug.Log($"The Product Xp: {TheProduct.xp}");
            MoneySystem.instance.UpdateXp(TheProduct.xp);

            mStats.queue.RemoveAt(0);
            PopulateQueue();
            btn.onClick.RemoveAllListeners();
            if (mStats.queue.Count <= 0 || mStats.queue == null)
            {
                TheProduct = new PrD()
                {
                    state = AState.None,
                    product = Products.None
                };
                mStats.state = ASpotState.Empty;
                btn.onClick.AddListener(() => MachineClicked());
            }
            else
            {
                btn.onClick.AddListener(() => CollectProduct());
                TheProduct = mStats.queue[0];
            }

            foreach (Transform item in MachinePH.instance.Holder) Destroy(item.gameObject);
            PopulateShelf();
            LoadUI();
            if (mStats.queue.Count > 0) CheckTimer();
            CheckBroken();
            SaveStats();
            LuckyBox.instance.TryToFindBox();
        }
    }

    private void CheckBroken()
    {
        int usageRate = mStats.qLimit * 4;
        if (mStats.usage > usageRate)
        {
            List<bool> chance = new List<bool>() { false, false, false, false, false, false, false, false, false, false };

            for (int i = 0; i < mStats.usage - usageRate; i++)
                chance[i] = true;
            StaticDatas.Shuffle(chance);

            bool rand = chance[UnityEngine.Random.Range(0, chance.Count)];
            if (rand) { mStats.state = ASpotState.Broken; BrokeMachine(); }
        }
    }

    private void SaveStats()
    {
        StaticDatas.PlayerData.MachineStats.Find(e => e.MachineName == mProducts.MachineName).state = mStats.state;
        StaticDatas.PlayerData.MachineStats.Find(e => e.MachineName == mProducts.MachineName).queue = mStats.queue;
        StaticDatas.PlayerData.MachineStats.Find(e => e.MachineName == mProducts.MachineName).qLimit = mStats.qLimit;
        StaticDatas.PlayerData.MachineStats.Find(e => e.MachineName == mProducts.MachineName).usage = mStats.usage;
        StaticDatas.SaveDatas();
    }

    #region Broken Details
    private void BrokeMachine()
    {
        if (mStats.queue.Count > 0)
        {
            DateTime starttime;
            StaticDatas.TryGetStartTime(mStats.queue[0].Time, machineName, out starttime);
            for (int i = 0; i < mStats.queue.Count; i++)
            {
                if (i == 0 && mStats.queue[i].state == AState.Fertilizing)
                {
                    TimeSpan elapsed = DateTime.UtcNow - starttime;
                    mStats.queue[i].prTimer -= elapsed.TotalMinutes;
                }
                mStats.queue[i].Time = "";
            }
        }
        SaveStats();
        PopulateFixers();
    }

    private void PopulateFixers()
    {
        List<Items> fixers = new List<Items>() { Items.Screw, Items.Hammer, Items.Bolt };
        for (int i = 0; i < fixers.Count; i++)
        {
            GameObject fixer = Instantiate(prPrefab, transform.Find("Broken Screen/Fixers"));

            fixer.transform.Find("The Product").GetComponent<Image>().sprite =
                Sprites.instance.sprites.items.Find(e => e.item == fixers[i]).sprite;
            fixer.transform.Find("The Product").GetComponent<RectTransform>().sizeDelta = new Vector2(55, 55);

            fixer.transform.Find("Count Text").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(fixers[i]) + " / 1";
            fixer.transform.Find("Count Text").GetComponent<TextMeshProUGUI>().fontSizeMax = 20f;
            if (Storage.instance.hasEnought(fixers[i], 1, false))
                fixer.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
            else 
                fixer.GetComponent<Image>().color = new Color32(255, 0, 0, 255);

            Button btn = fixer.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int index = i;
            if (Storage.instance.hasEnought(fixers[i], 1, false))
                btn.onClick.AddListener(() => FixMachine(fixers[index]));
        }
    }

    private void FixMachine(Items item)
    {
        mStats.usage = 0;
        if (mStats.queue.Count > 0){
            mStats.state = ASpotState.HasAnimal;
            for (int i = 0; i < mStats.queue.Count; i++) mStats.queue[i].Time = DateTime.UtcNow.ToString("o");
        }
        else
            mStats.state = ASpotState.Empty;

        Storage.instance.UpdateItemCount(item, -1);
        SaveStats();
    }
    #endregion
}