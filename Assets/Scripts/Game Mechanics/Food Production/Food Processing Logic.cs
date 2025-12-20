using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodPL : MonoBehaviour
{
    public static FoodPL instance;
    public List<TheFood> materials;
    public List<FoodLevelSystem> lSystem;
    public Animator anim;

    public TextMeshProUGUI AnimalFoodText;

    public Sprite foodImage;
    public TextMeshProUGUI timer;

    [Header("Material Holder")]
    public Transform mholder;
    public GameObject materialPrefab;
    public List<GameObject> Materials;

    [Header("Schedule Holder")]
    public Transform qholder;
    public GameObject qPrefab;
    public List<GameObject> queue;
    public Sprite buyButton;
    public Button mBtn;

    [Header("Upgrade System")]
    public List<Sprite> MachineImages;
    public Image priceImage;
    public TextMeshProUGUI priceText;

    public Image Icon;
    public Image TheWell;
    public Image NextWell;

    [Header("Upgrade Parameters")]
    public TextMeshProUGUI cPTD;
    public TextMeshProUGUI nPTD;
    public TextMeshProUGUI cFTI;
    public TextMeshProUGUI nFTI;
    public TextMeshProUGUI cPrX;
    public TextMeshProUGUI nPrX;

    public List<int> sPrices = new List<int>() { 300, 500, 750 };

    private void Awake()
    {
        instance = this;

        anim = GetComponent<Animator>();
        AnimalFoodText.text = StaticDatas.PlayerData.PlayerInfos.Food.sumAmount.ToString();

        mBtn.onClick.RemoveAllListeners();
        mBtn.onClick.AddListener(() => PopulateMatHolder());
        SetImages();
        changeFood();
        PopulateSchedule();
        PopulateFoodList();
    }

    public void UpdateAnimalFood(a_f_types food, int amount)
    {
        if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food) == null)
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Add(new afAmount() { food = food, amount = amount });
        else
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == food).amount += amount;
        CalSumAmount();
        AnimalFoodText.text = StaticDatas.PlayerData.PlayerInfos.Food.sumAmount.ToString();
        if(AnimalsLogic.instance != null)
            AnimalsLogic.instance.PopulateFoodChooser();
        Storage.instance.UpdateBoxItems();
        StaticDatas.SaveDatas();
    }

    private void Update()
    {
        if (StaticDatas.PlayerData.PlayerInfos.Food.MachState == LandState.Planted)
        {
            if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState == PlantState.Growing)
            { CheckGrowth(); UpdateTimer(); }
            else if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState == PlantState.ReadyToHarvest)
                rtCollect();
        }
        else
        {
            mBtn.onClick.RemoveAllListeners(); mBtn.onClick.AddListener(() => PopulateMatHolder()); 
        }
        LoadUI();
    }

    private void LoadUI()
    {
        if (StaticDatas.PlayerData.PlayerInfos.Food.MachState == LandState.Empty)
        {
            timer.gameObject.SetActive(false);
            queue[0].transform.Find("BG").GetComponent<Image>().color = Color.darkBlue;
        }
        else
        {
            timer.gameObject.SetActive(true);
            if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState == PlantState.Growing)
            {
                if(queue.Count > 0)
                    queue[0].transform.Find("BG").GetComponent<Image>().color = Color.darkBlue;
            }
            else if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState == PlantState.ReadyToHarvest)
            {
                if (queue.Count > 0)
                    queue[0].transform.Find("BG").GetComponent<Image>().color = Color.green;
            }
        }
    }

    #region Animations
        public void OpenDetails()
        {
            if (WaterSL.instance.anim.GetBool("Open Water Details"))
                WaterSL.instance.anim.SetBool("Open Water Details", false);
            if (isAnTrue("Open Upgrade"))
                CloseUpgrade();
            else if (isAnTrue("Open MH"))
                OpenMH(false);
            if(StaticDatas.PlayerData.PlayerInfos.FoodLevel >= MachineImages.Count)
                transform.Find("Food Producer/Holder Colored/Details/Upgrade Button").gameObject.SetActive(false);

            int id = Animator.StringToHash("Open Details");
            Debug.Log($"Open Details is {anim.GetBool(id)}");
            Debug.Log($"Setting to {!anim.GetBool(id)}");

            anim.SetBool(id, !anim.GetBool(id));
        }

        public void OpenMH(bool tf)
        {
            if (!isAnTrue("Open Upgrade"))
            {
                Debug.Log($"Setting to {tf}");
                anim.SetBool("Open MH", tf);
                mBtn.onClick.RemoveAllListeners();
                mBtn.onClick.AddListener(() => PopulateMatHolder());
            }
        }

        public void OpenUpgrade()
        {
            if (!isAnTrue("Open MH"))
            {
                if (!isAnTrue("Open Upgrade"))
                {
                    Debug.Log($"Setting to {!isAnTrue("Open Upgrade")}");
                    anim.SetBool("Open Upgrade", true);
                    mBtn.onClick.RemoveAllListeners();
                    priceImage.sprite = Sprites.instance.sprites.currencies.
                        Find(e => e.Currency == lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].currency).sprite;
                    priceText.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].price.ToString();
                }
                else
                    UpgradeWellLevel();
            }
        }

        public void CloseUpgrade()
        {
            if (isAnTrue("Open Upgrade"))
            {
                Debug.Log($"Setting to {!isAnTrue("Open Upgrade")}");
                anim.SetBool("Open Upgrade", false);
                mBtn.onClick.RemoveAllListeners();
                mBtn.onClick.AddListener(() => PopulateMatHolder());
            }
        }

        private bool isAnTrue(string name)
    {
        int index = Animator.StringToHash(name);
        if(anim.GetBool(index))
            return true;
        else
            return false;
    }
    #endregion

    #region Production Section
        private void ChooseProduct(TheFood tf)
        {
            if (Storage.instance.hasEnought(tf.material, tf.reqAmount, true) && StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count < StaticDatas.PlayerData.PlayerInfos.Food.qLimit)
            {
                Storage.instance.UpdatePlantCount(tf.material, -tf.reqAmount);

                TheFood newItem = tf.Clone();
                newItem.PrState = PlantState.Growing;

                List<TheFood> inqueue = StaticDatas.PlayerData.PlayerInfos.Food.InQueue;

                if (inqueue.Count == 0)
                {
                    newItem.fillTime = DateTime.UtcNow.ToString("o");
                }
                else
                {
                    DateTime lastFinish = DateTime.Parse(inqueue[inqueue.Count - 1].fillTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    newItem.fillTime = lastFinish.AddMinutes(inqueue[inqueue.Count - 1].pTimer).ToString("o");
                }

                inqueue.Add(newItem);
                StaticDatas.PlayerData.PlayerInfos.Food.InQueue = inqueue;
                StaticDatas.PlayerData.PlayerInfos.Food.MachState = LandState.Planted;
                StaticDatas.SaveDatas();

                queue[StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count - 1].transform.Find("Item").GetComponent<Image>().sprite =
                    Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food == tf.Food).sprite;
                queue[StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count - 1].transform.Find("Item").GetComponent<Image>().enabled = true;
                if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count >= StaticDatas.PlayerData.PlayerInfos.Food.qLimit){
                    PopulateMatHolder();
                }
                PopulateSchedule();
                PlantsHolder.instance.UpdateCountOfPlants();
            }
        }

        private void CheckGrowth()
        {
            DateTime startTime;
            if (!StaticDatas.TryGetStartTime(StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].fillTime, "Food Timer", out startTime)) return;

            TimeSpan elapsed = DateTime.UtcNow - startTime;
            double elapsedMinutes = elapsed.TotalMinutes;
            if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState == PlantState.Growing && elapsedMinutes > StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].pTimer)
            {
                StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState = PlantState.ReadyToHarvest;
                Debug.Log("Plant state set to ready");
                StaticDatas.SaveDatas();
            }
        }

        private void UpdateTimer()
        {
            DateTime startTime;
            if (!StaticDatas.TryGetStartTime(StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].fillTime, "Food Timer", out startTime)) return;
            float time = 0;
            for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count; i++)
            {
                if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue[i].PrState != PlantState.ReadyToHarvest)
                    { time = StaticDatas.PlayerData.PlayerInfos.Food.InQueue[i].pTimer; break; }
            }
            double totalSecondsRequired = time * 60;
            double elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

            string timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
            timer.text = timeString;
        }

        private void PopulateMatHolder()
        {
            if(!isAnTrue("Open Upgrade"))
            {
                foreach (Transform item in mholder) Destroy(item.gameObject);
                if (StaticDatas.PlayerData.PlayerInfos.Food.qLimit > StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count)
                {
                    for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.materials.Count; i++)
                    {
                        var proto = materials.Find(e => e.material == StaticDatas.PlayerData.PlayerInfos.Food.materials[i].material);
                        TheFood tf = proto.Clone();
                        Debug.Log($"in popmatholder: Food {tf.Food} \n material {tf.material}");
                        if (StaticDatas.PlayerData.unlocked_items.u_plants.Find(e => e.plant == tf.material).owned)
                        {
                            GameObject dublicate = Instantiate(materialPrefab, mholder);
                            dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == tf.material).sprite;
                            dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(65, 65);
                            dublicate.transform.Find("Price").gameObject.SetActive(false);

                            Button button = dublicate.GetComponent<Button>();
                            button.onClick.RemoveAllListeners();
                            button.onClick.AddListener(() => ChooseProduct(tf));
                        }
                    }
                    OpenMH(true);
                }
                else
                {
                    OpenMH(false);
                    mBtn.onClick.RemoveAllListeners();
                }
            }
        }

        private void PopulateSchedule()
        {
            foreach (Transform item in qholder) Destroy(item.gameObject);
            queue.Clear();
            for (int i = queue.Count; i < StaticDatas.PlayerData.PlayerInfos.Food.qLimit; i++)
            {
                Debug.Log($"qLimit is {StaticDatas.PlayerData.PlayerInfos.Food.qLimit}");
                GameObject dublicate = Instantiate(qPrefab, qholder);
                if (i < StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count)
                {
                    dublicate.transform.name = "Animal Food";
                    dublicate.transform.Find("Item").GetComponent<Image>().enabled = true;
                    dublicate.transform.Find("Item").GetComponent<Image>().sprite = Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food ==
                        StaticDatas.PlayerData.PlayerInfos.Food.InQueue[i].Food).sprite;
                    dublicate.transform.Find("Price").gameObject.SetActive(false);
                }
                else
                {
                    dublicate.transform.name = "Empty Slot";
                    dublicate.transform.Find("Item").GetComponent<Image>().enabled = false;
                    dublicate.transform.Find("Price").gameObject.SetActive(false);
                }
                dublicate.transform.Find("BG").GetComponent<Image>().color = Color.darkBlue;
                dublicate.GetComponent<RectTransform>().localScale = new Vector3(1 - (float)(i * 0.08), 1 - (float)(i * 0.08), 1 - (float)(i * 0.08));
                Debug.Log($"slot added to queue");
                queue.Add(dublicate);
            }
            int add = 0;
            if (StaticDatas.PlayerData.PlayerInfos.Food.qLimit < 4)
            {
                Debug.Log($"adding addbuyslot");
                AddBuySlot(); add = 1;
            }
            RectTransform rts = qholder.GetComponent<RectTransform>();
            HorizontalLayoutGroup hlg = qholder.GetComponent<HorizontalLayoutGroup>();
            rts.sizeDelta = new Vector2(((StaticDatas.PlayerData.PlayerInfos.Food.qLimit + add) * 80) + (hlg.padding.left * 2) + (StaticDatas.PlayerData.PlayerInfos.Food.qLimit * hlg.spacing), 100);
        }

        private void AddBuySlot()
        {
            if(StaticDatas.PlayerData.PlayerInfos.Food.qLimit < 5)
            {
                int i = StaticDatas.PlayerData.PlayerInfos.Food.qLimit + 1;
                GameObject buy = Instantiate(qPrefab, qholder);
                buy.transform.name = "Buy Slot";
                buy.transform.Find("Item").GetComponent<Image>().sprite = buyButton;
                buy.transform.Find("Price").gameObject.SetActive(true);
                buy.transform.Find("Price/Price Text").GetComponent<TextMeshProUGUI>().text = sPrices[StaticDatas.PlayerData.PlayerInfos.Food.qLimit - 1].ToString();
                buy.GetComponent<RectTransform>().localScale = new Vector3(1 - (float)(i * 0.08), 1 - (float)(i * 0.08), 1 - (float)(i * 0.08));

                Button btn = buy.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => BuySlot());
            }
        }

        private void BuySlot()
        {
            if(MoneySystem.instance.hasEnough(Currency.Coin, sPrices[StaticDatas.PlayerData.PlayerInfos.Food.qLimit - 1]))
            {
                StaticDatas.PlayerData.PlayerInfos.Food.qLimit++;
                MoneySystem.instance.UpdateCoin(-sPrices[StaticDatas.PlayerData.PlayerInfos.Food.qLimit - 2], out bool s);
                Transform child = qholder.Find("Buy Slot");
                StaticDatas.SaveDatas();
                if (child != null) Destroy(child.gameObject);
                PopulateSchedule();
                LuckyBox.instance.TryToFindBox();
            }
        }

        private void rtCollect()
        {
            mBtn.onClick.RemoveAllListeners();
            mBtn.onClick.AddListener(() => Collect());
        }

        private void Collect()
        {
            if (!isAnTrue("Open Upgrade") && StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].PrState == PlantState.ReadyToHarvest &&
                Storage.instance.hasEnStorage(StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].collectAmount))
            {
                UpdateAnimalFood(StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].Food, StaticDatas.PlayerData.PlayerInfos.Food.InQueue[0].collectAmount);
                MoneySystem.instance.UpdateXp(StaticDatas.PlayerData.PlayerInfos.Food.xp);
                queue[StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count - 1].transform.Find("Item").GetComponent<Image>().enabled = false;
                StaticDatas.PlayerData.PlayerInfos.Food.InQueue.RemoveAt(0);
                if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count <= 0)
                {
                    StaticDatas.PlayerData.PlayerInfos.Food.MachState = LandState.Empty;
                }
                if (AnimalsLogic.instance != null)
                    AnimalsLogic.instance.PopulateFoodChooser();
                PopulateSchedule();
                StaticDatas.SaveDatas();
                mBtn.onClick.RemoveAllListeners();
                mBtn.onClick.AddListener(() => PopulateMatHolder());
                LuckyBox.instance.TryToFindBox();
            }
            //else if (StaticDatas.PlayerData.PlayerInfos.Food.InQueue.Count < StaticDatas.PlayerData.PlayerInfos.Food.qLimit) PopulateMatHolder();
        }

        public bool hasEnoughFood(a_f_types type, int amount, bool push)
    {
        if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == type) != null &&
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == type).amount >= amount)
            return true;
        else{
            if(push) PushNotice.instance.Push($"No Enough {type.ToString()} Animal Food", PushType.Alert);
            return false;
        }
    }
    #endregion

    #region Upgrage System
        public void UpgradeWellLevel()
        {
            if (MoneySystem.instance.hasEnough(lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].currency, lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].price))
            {
                if (StaticDatas.PlayerData.PlayerInfos.FoodLevel < MachineImages.Count)
                {
                    StaticDatas.PlayerData.PlayerInfos.FoodLevel++;
                    if (StaticDatas.PlayerData.PlayerInfos.FoodLevel < MachineImages.Count)
                    {
                        //CheckLevel();
                        changeFood();
                        SetImages();
                        StaticDatas.SaveDatas();
                        priceImage.sprite = Sprites.instance.sprites.currencies.
                            Find(e => e.Currency == lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].currency).sprite;
                        priceText.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].price.ToString();
                    }
                    else FinishLevel();
                    LuckyBox.instance.TryToFindBox();
                }
                else return;

                if (lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].currency == Currency.Coin)
                    MoneySystem.instance.UpdateCoin(-lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].price, out bool s);
                else if (lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].currency == Currency.Crystal)
                    MoneySystem.instance.UpdateCyrstal(-lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].price, out bool s);
            }
        }

        private void SetImages()
        {
            Icon.sprite = MachineImages[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1];
            TheWell.sprite = MachineImages[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1];

            if(StaticDatas.PlayerData.PlayerInfos.FoodLevel < MachineImages.Count)
            {
                cPTD.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].progTimerDec.ToString() + "%";
                cFTI.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].foodTimerInc.ToString() + "%";
                cPrX.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].productionX.ToString() + "x";

                nPTD.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].progTimerDec.ToString() + "%";
                nFTI.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].foodTimerInc.ToString() + "%";
                nPrX.text = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel].productionX.ToString() + "x";

                NextWell.sprite = MachineImages[StaticDatas.PlayerData.PlayerInfos.FoodLevel];
            }
        }

        private void FinishLevel()
        {
            Icon.sprite = MachineImages[StaticDatas.PlayerData.PlayerInfos.FoodLevel -1];
            TheWell.sprite = MachineImages[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1];
            changeFood();
            CloseUpgrade();
            transform.Find("Food Producer/Holder Colored/Details/Upgrade Button").gameObject.SetActive(false);
        }
    #endregion

    private void PopulateFoodList()
    {
        for (int i = StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i < StaticDatas.PlayerData.PlayerInfos.Food.materials.Count; i++)
        {
            StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Add(new afAmount()
            {
                food = StaticDatas.PlayerData.PlayerInfos.Food.materials[i].Food,
                amount = 0
            });
            StaticDatas.SaveDatas();
        }
    }

    private void changeFood()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            TheFood f = new TheFood()
            {
                pTimer = materials[i].pTimer - (((materials[i].pTimer) / 100) * lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].progTimerDec),
                foodTimer = materials[i].foodTimer + (((materials[i].foodTimer) / 100) * lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].foodTimerInc),
                collectAmount = lSystem[StaticDatas.PlayerData.PlayerInfos.FoodLevel - 1].productionX
            };
            materials[i].pTimer = f.pTimer;
            materials[i].foodTimer = f.foodTimer;
            materials[i].collectAmount = f.collectAmount;
        }
        CalSumAmount();
        StaticDatas.PlayerData.PlayerInfos.Food.materials = materials;
        StaticDatas.SaveDatas();
    }

    private void CalSumAmount()
    {
        StaticDatas.PlayerData.PlayerInfos.Food.sumAmount = 0;
        for (int i = 0; i < StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Count; i++)
        {
            StaticDatas.PlayerData.PlayerInfos.Food.sumAmount += StaticDatas.PlayerData.PlayerInfos.Food.Amounts[i].amount;
        }
    }
}
