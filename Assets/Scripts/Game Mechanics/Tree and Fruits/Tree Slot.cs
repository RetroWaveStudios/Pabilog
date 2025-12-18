using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreeSlot : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI infoText;

    public GameObject ProgressPot;
    public GameObject waterTimer;
    public GameObject progressTimer;

    public GameObject timerPot;
    public GameObject wateringPot;
    public Button reqWaterButton;
    public TextMeshProUGUI wamount;
    public TextMeshProUGUI timer;
    public GameObject TheFruitImage;

    public float animationSpeed = 1f;

    [Header("Game Settings")]
    public Button btn;
    public int SlotNumber;
    public LandState landstate;
    public TreeD TheTree;
    private bool showtimer;
    private int stimer = 0;
    private int w; // water amount to contuniue watering with full need of water
    private float pieceforMinute = 0; // how many used

    private List<Vector3> fPos = new List<Vector3>() { new Vector3(-32, 25), new Vector3(-4, -(float)5.5), new Vector3(31, 16) };
    private Animator anim;

    [Header("Growing")]
    public Mask stageMask;
    public GameObject Stages;
    public GameObject ready;
    public List<GameObject> fruits;

    public int dropindex;

    public GameObject PauseBG;

    [Header("Stumped")]
    public Sprite noWater;
    public Sprite StumpSpr;
    public bool isAxing = false;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        infoText.text = "Empty"; 
    }

    private void Update()
    {
        LoadUI();
        if (landstate == LandState.Planted)
        {
            if (TheTree.pauseTime != 0)
                pieceforMinute = (float)TheTree.pauseTime / (float)(TheTree.wTimerByStages[TheTree.stage] / TheTree.WaterAmoutByStage[TheTree.stage]);
            w = TheTree.WaterAmoutByStage[TheTree.stage] - (int)pieceforMinute;
            if (TheTree.state == PlantState.Growing)
            {
                btn.onClick.RemoveAllListeners();
                if (TheTree.hasWater)
                {
                    btn.onClick.AddListener(() => ShowTimer()); 
                    CheckForWatering(); CheckForGrowth(); UpdateTimer();
                }
                else if (!TheTree.hasWater)
                {
                    reqWaterButton.onClick.RemoveAllListeners();
                    reqWaterButton.onClick.AddListener(() => ResumeGrowth(w));
                    wamount.text = w.ToString();
                    btn.onClick.AddListener(() => ShowWatering());
                }
            }
            else if (TheTree.state == PlantState.ReadyToHarvest) rtCollect();
        }
    }

    public void LoadUI()
    {
        ProgressPot.SetActive(false); timerPot.SetActive(false); timerPot.SetActive(false);
        Stages.SetActive(false); ready.SetActive(false); PauseBG.SetActive(false); wateringPot.SetActive(false); TheFruitImage.SetActive(false);

        Image pImage = PauseBG.transform.Find("Icon").GetComponent<Image>();
        if (landstate == LandState.Empty)
            infoText.gameObject.SetActive(true);
        else if(landstate == LandState.Stumped)
        {
            PauseBG.transform.Find("Count").gameObject.SetActive(true);
            PauseBG.SetActive(true);
            Stages.SetActive(true);

            Stages.GetComponent<Image>().sprite = StumpSpr;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => AxingStump());
            if (Storage.instance.hasEnought(Items.Axe, 1, false) || isAxing)
            {
                pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
                PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = "1";
                pImage.sprite = Sprites.instance.sprites.items.Find(e => e.item == Items.Axe).sprite;
            }
            else
            {
                if (!isAxing)
                {
                    pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                    PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = "2";
                    pImage.sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Crystal).sprite;
                }
            }
        }
        else if (landstate == LandState.Planted)
        {
            PauseBG.transform.Find("Count").gameObject.SetActive(false);
            Stages.SetActive(true);
            Stages.GetComponent<Image>().sprite = Sprites.instance.sprites.TreeStageSprites.Find(e => e.tree == TheTree.fruit).stages[TheTree.stage];
            TheFruitImage.SetActive(true);

            TheFruitImage.transform.Find("The Fruit").GetComponent<Image>().sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == TheTree.fruit).sprite;
            if (TheTree.state == PlantState.Growing)
            {
                ready.SetActive(false); infoText.gameObject.SetActive(false); ProgressPot.SetActive(true);
                timerPot.SetActive(true);
                if (TheTree.hasWater) { PauseBG.SetActive(false); wateringPot.SetActive(false); timer.gameObject.SetActive(true); }
                else
                {
                    PauseBG.SetActive(true); pImage.sprite = noWater; wateringPot.SetActive(true); timer.gameObject.SetActive(false); }
            }
            else if (TheTree.state == PlantState.ReadyToHarvest)
            {
                Stages.GetComponent<Image>().sprite = Sprites.instance.sprites.TreeStageSprites.Find(e => e.tree == TheTree.fruit).stages[TheTree.stage];
                ready.SetActive(true); ProgressPot.SetActive(false);
            }
        }
    }

    private void AxingStump()
    {
        anim.SetTrigger("Axe Stump");
        isAxing = true;
        if (!Storage.instance.hasEnought(Items.Axe, 1, false)) MoneySystem.instance.UpdateCyrstal(-2);
    }

    public void SlotClicked()
    {
        if (landstate == LandState.Empty)
        {
            TreeHolder.instance.slotnumber = SlotNumber;
            TreeHolder.instance.PopulateHodler();
        }
    }

    public void PickATree(Fruits t)
    {
        if (TheTree.fruit == Fruits.None &&
            (MoneySystem.instance.hasEnough(Currency.Coin, ForestLogic.instance.TreeDetails.Find(e => e.fruit == t).price)) &&
            WaterSL.instance.hasEnoughWater(ForestLogic.instance.TreeDetails.Find(e => e.fruit == t).WaterAmoutByStage[TheTree.stage]))
        {
            Debug.Log("slotNumber: " + SlotNumber);
            var proto = ForestLogic.instance.TreeDetails.Find(e => e.fruit == t);
            TheTree = proto.Clone(); // each slot gets its own copy
            Plant();
        }
    }

    private void Plant()
    {
        Debug.Log("Planting " + TheTree.fruit + " tree");
        TheTree.waterTime = DateTime.UtcNow.ToString("o");
        TheTree.name = TheTree.fruit.ToString();
        TheTree.state = PlantState.Growing;
        TheTree.hasWater = true;
        TheTree.wTimer = TheTree.wTimerByStages[TheTree.stage];
        TheTree.hFrutis = new List<int>() { 0, 1, 2 };
        WaterSL.instance.TriggerAmount(-TheTree.WaterAmoutByStage[TheTree.stage]);
        Debug.Log("added time to waterTime: " + TheTree.waterTime);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ShowTimer());
        landstate = LandState.Planted;
        StaticDatas.PlayerData.TreeSpots[SlotNumber].state = landstate;
        StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
        MoneySystem.instance.UpdateCoin(-TheTree.price);
        StaticDatas.SaveDatas();
        wamount.text = TheTree.WaterAmoutByStage[TheTree.stage].ToString();
        reqWaterButton.onClick.RemoveAllListeners();
        reqWaterButton.onClick.AddListener(() => ResumeGrowth(TheTree.WaterAmoutByStage[TheTree.stage]));
        transform.name = TheTree.name;
        ShowTimer();
    }

    private void CheckForGrowth()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheTree.waterTime, "tree slot: " + SlotNumber.ToString(), out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes + TheTree.pauseTime;
        double totalSeconds = elapsed.TotalSeconds + (TheTree.pauseTime * 60);

        float progress = Mathf.Clamp01((float)(totalSeconds / (TheTree.GrowthTimeByStage[TheTree.stage] * 60)));

        Image filler = progressTimer.GetComponent<Image>();
        // update UI (0 = empty, 1 = full, or invert if needed)
        filler.fillAmount = progress;
        Stages.GetComponent<Image>().sprite = Sprites.instance.sprites.TreeStageSprites.Find(e => e.tree == TheTree.fruit).stages[TheTree.stage];

        if (elapsedMinutes >= TheTree.GrowthTimeByStage[TheTree.stage]) UpdateStage();
    }

    private void CheckForWatering()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheTree.waterTime, "tree slot: " + SlotNumber.ToString(), out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes;
        double elapsedSeconds = elapsed.TotalSeconds;

        float progress = Mathf.Clamp01((float)(elapsedSeconds / (TheTree.wTimer * 60)));
        // GrowthTime assumed in minutes → multiply by 60 for seconds

        Image filler = waterTimer.GetComponent<Image>();
        filler.fillAmount = 1f - progress;

        // --- Check if ready to harvest ---
        if (elapsedMinutes >= TheTree.wTimer && ((TheTree.GrowthTimeByStage[TheTree.stage] - TheTree.pauseTime) > TheTree.wTimer))
        {
            Debug.Log("Calling pause");
            PauseGrowth(false);
        }
    }

    private void PauseGrowth(bool next)
    {
        btn.onClick.RemoveAllListeners();
        if (!next) { TheTree.pauseTime += TheTree.wTimer; }
        else { TheTree.stage++; TheTree.pauseTime = 0; }
        btn.onClick.AddListener(() => ShowWatering());
        if (TheTree.pauseTime != 0)
            pieceforMinute = (float)TheTree.pauseTime / (float)(TheTree.wTimerByStages[TheTree.stage] / TheTree.WaterAmoutByStage[TheTree.stage]);
        w = TheTree.WaterAmoutByStage[TheTree.stage] - (int)pieceforMinute;
        anim.SetBool("Show Timer", false);

        if (SlotNumber < ForestLogic.instance.maxSlotCount - 4)
        {
            TreeSlot ts = ForestLogic.instance.Slots[SlotNumber + 4].GetComponent<TreeSlot>();
            ts.stageMask.enabled = false;
        }
        wamount.text = w.ToString();
        TheTree.hasWater = false;
        TheTree.waterTime = "";
        StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
        StaticDatas.SaveDatas();
    }

    public void ResumeGrowth(int wateramount)
    {
        if (WaterSL.instance.hasEnoughWater(wateramount))
        {
            TheTree.hasWater = true;
            TheTree.waterTime = DateTime.UtcNow.ToString("o");
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ShowTimer());
            if (TheTree.pauseTime != 0)
                pieceforMinute = (float)TheTree.pauseTime / (float)(TheTree.wTimerByStages[TheTree.stage] / TheTree.WaterAmoutByStage[TheTree.stage]);
            w = TheTree.WaterAmoutByStage[TheTree.stage] - (int)pieceforMinute;
            if (wateramount == w) TheTree.wTimer = TheTree.wTimerByStages[TheTree.stage] - TheTree.pauseTime;
            else TheTree.wTimer = TheTree.wTimerByStages[TheTree.stage] / TheTree.WaterAmoutByStage[TheTree.stage];
            anim.SetBool("Show Watering", false);

            if (SlotNumber < ForestLogic.instance.maxSlotCount - 4)
            {
                TreeSlot ts = ForestLogic.instance.Slots[SlotNumber + 4].GetComponent<TreeSlot>();
                ts.stageMask.enabled = false;
            }
            WaterSL.instance.TriggerAmount(-wateramount);
            StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
        }
    }

    private void UpdateTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheTree.waterTime, "tree slot: " + SlotNumber.ToString(), out startTime)) return;

        string timeString = "";
        if (stimer == 0) // fruit timer
        {
            double totalSecondsRequired = TheTree.GrowthTimeByStage[TheTree.stage] * 60;
            double elapsedSeconds = (TheTree.pauseTime * 60);
            if (TheTree.hasWater) elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds + (TheTree.pauseTime * 60);
            else if (!TheTree.hasWater) elapsedSeconds = (TheTree.pauseTime * 60);
            timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        }
        else if (stimer == 1) // water timer
        {
            double totalSecondsRequired = TheTree.wTimer * 60;
            double elapsedSeconds = TheTree.wTimer * 60;
            if (TheTree.hasWater) elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        }
        timer.text = timeString;
    }

    private void UpdateStage()
    {
        if (TheTree.stage != 2)
        {
            pieceforMinute = 0;
            w = 0;
            PauseGrowth(true);
            ShowWatering();
        }
        else
        {
            TheTree.stage = 2;
            rtCollect(); UpdateFruitImages();
            PauseGrowth(false);
        }
        StaticDatas.SaveDatas();
    }

    private void ShowTimer()
    {
        Debug.Log("showtimer called");
        for (int i = 0; i < ForestLogic.instance.Slots.Count; i++)
        {
            TreeSlot ts = ForestLogic.instance.Slots[i].GetComponent<TreeSlot>();
            if (i != SlotNumber)
            {
                ts.anim.SetBool("Show Timer", false);
                ts.showtimer = false;
            }
            else showtimer = !showtimer;
        }
        if (showtimer)
        {
            Debug.Log("expanting showtimer items");
            anim.SetBool("Show Timer", true);

            if (SlotNumber < ForestLogic.instance.maxSlotCount - 4){
                TreeSlot ts = ForestLogic.instance.Slots[SlotNumber + 4].GetComponent<TreeSlot>();
                ts.stageMask.enabled = true;
            }
        }
        else if (!showtimer)
        {
            Debug.Log("shrinking showtimer items");
            anim.SetBool("Show Timer", false);
            if (SlotNumber < ForestLogic.instance.maxSlotCount - 4)
            {
                TreeSlot ts = ForestLogic.instance.Slots[SlotNumber + 4].GetComponent<TreeSlot>();
                ts.stageMask.enabled = false;
            }
        }
    }

    public void SwitchTimer(int f)
    {
        stimer = f;
    }

    private void ShowWatering()
    {
        showtimer = false;
        Debug.Log("watering called");
        Debug.Log("expanting showWatering items");
        RectTransform tp_rts = timerPot.GetComponent<RectTransform>();
        RectTransform pp_rts = ProgressPot.GetComponent<RectTransform>();
        anim.SetBool("Show Watering", true);

        if (SlotNumber < ForestLogic.instance.maxSlotCount - 4)
        {
            TreeSlot ts = ForestLogic.instance.Slots[SlotNumber + 4].GetComponent<TreeSlot>();
            ts.stageMask.enabled = true;
        }
    }

    private void rtCollect()
    {
        TheTree.state = PlantState.ReadyToHarvest;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => Collect());
        StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
    }

    private void Collect()
    {
        if (TheTree.state == PlantState.ReadyToHarvest && Storage.instance.hasEnStorage(1))
        {
            TheTree.pauseTime = 0;
            TheTree.waterTime = "";
            Storage.instance.UpdateFruitCount(TheTree.fruit, 1);
            MoneySystem.instance.UpdateXp(TheTree.xp);
            Storage.instance.UpdateBoxItems();
            btn.onClick.RemoveAllListeners();
            TheTree.stage = 2;
            CollectAFruit();
            if (TheTree.harvestAmount <= 0)
            {
                TheTree.usage--; TheTree.state = PlantState.Growing;
                TheTree.harvestAmount = ForestLogic.instance.TreeDetails.Find(e => e.fruit == TheTree.fruit).harvestAmount;
                TheTree.hFrutis = new List<int>() { 0, 1, 2 }; for (int i = 0; i < fruits.Count; i++) fruits[i].SetActive(true);
                pieceforMinute = 0; w = 0;
                TheTree.hasWater = false;
                TheTree.waterTime = "";
                StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
                StaticDatas.SaveDatas();
                for (int i = 0; i < fruits.Count; i++) fruits[i].GetComponent<RectTransform>().localPosition = fPos[i];
                if (TheTree.usage <= 0)
                {
                    landstate = LandState.Stumped;
                    TheTree = new TreeD()
                    {
                        fruit = Fruits.None,
                        state = PlantState.None,
                    };
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => AxingStump());
                }
                else ShowWatering();

            }
            StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
            StaticDatas.PlayerData.TreeSpots[SlotNumber].state = landstate;
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
        }
        else PushNotice.instance.Push("No Space in Storage", PushType.Alert);
    }

    private void CollectAFruit()
    {
        dropindex = UnityEngine.Random.Range(0, TheTree.hFrutis.Count);
        while (true)
        {
            Debug.Log("dropindex = " + dropindex);
            if (fruits[TheTree.hFrutis[dropindex]].activeInHierarchy)
            {
                Debug.Log("TheTree.hFrutis[dropindex] = " + TheTree.hFrutis[dropindex]);
                Debug.Log("fruits[TheTree.hFrutis[dropindex]].activeInHierarchy = " + fruits[TheTree.hFrutis[dropindex]].activeInHierarchy);
                fruits[TheTree.hFrutis[dropindex]].GetComponent<FruitDrop>().slotNumber = SlotNumber;
                Debug.Log($"fruits[{TheTree.hFrutis[dropindex]}].slotNubmer changed to {SlotNumber}");
                Animator an = fruits[TheTree.hFrutis[dropindex]].GetComponent<Animator>();
                /*if (an != null)
                {
                    Debug.Log("Animating");
                    an.SetTrigger("Drop Fruit");
                    Debug.Log("Animated");
                }
                else Debug.Log("an = null");
                Debug.Log($"animation done");*/
                fruits[TheTree.hFrutis[dropindex]].SetActive(false);
                TheTree.hFrutis.RemoveAt(dropindex);
                TheTree.harvestAmount--;
                break;
            }
            else dropindex = UnityEngine.Random.Range(0, TheTree.hFrutis.Count);
        }
        StaticDatas.PlayerData.TreeSpots[SlotNumber].TreeDetails = TheTree;
        StaticDatas.SaveDatas();
    }

    public void UpdateFruitImages()
    {
        if(TheTree.state == PlantState.ReadyToHarvest)
        {
            for (int i = 0; i < fruits.Count; i++)
            {
                Debug.Log($"i = {i}");
                fruits[i].SetActive(false);
                for (int f = 0; f < TheTree.hFrutis.Count; f++)
                {
                    if (i == TheTree.hFrutis[f])
                    {
                        Debug.Log($"matched f = {f}");
                        fruits[i].SetActive(true);
                        Image image = fruits[i].GetComponent<Image>();
                        image.sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == TheTree.fruit).sprite;
                        break;
                    }
                }
            }
        }
    }

    #region Animation Done
    public void A_AxingDone()
    {
        landstate = LandState.Empty;
        Debug.Log($"RemoveAt(dropindex)");
        StaticDatas.PlayerData.TreeSpots[SlotNumber].state = landstate;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SlotClicked());
        if (Storage.instance.hasEnought(Items.Axe, 1, false))
            Storage.instance.UpdateItemCount(Items.Axe, -1);
        isAxing = false;
        StaticDatas.SaveDatas();
        LuckyBox.instance.TryToFindBox();
    }
    #endregion
}