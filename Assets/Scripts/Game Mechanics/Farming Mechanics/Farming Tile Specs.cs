using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmingTS : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI infoText;

    public GameObject waterTimerPot;
    public GameObject waterTimer;

    public TextMeshProUGUI timer;

    public GameObject ThePlantImage;
    private int w; // water amount to contuniue watering with full need of water
    private int decide;

    [Header("Game Settings")]
    public Button btn;
    private Animator anim;
    public int SlotNumber;
    public LandState landstate;
    public PD ThePlant;

    [Header("Growing")]
    public GameObject Stages;
    public GameObject ready;
    public GameObject PauseBG;

    public Sprite noWater;
    public Sprite WCan;

    public bool is_watering = false, is_plowing = false;

    private void Awake()
    {
        btn = GetComponent<Button>();
        anim = GetComponent<Animator>();
        infoText.text = "Empty";
    }

    private void Start()
    {
        if (landstate == LandState.Planted && ThePlant.state == PlantState.Growing && !ThePlant.hasWater) CalculateReqWater();
        LoadUI();
    }

    private void Update()
    {
        if (ThePlant.state == PlantState.Growing) { if (ThePlant.hasWater) { CheckWaterTimer(); CheckforGrowth(); UpdateTimer(); } }
        else if (ThePlant.state == PlantState.ReadyToHarvest) rtharvest();
    }

    public void LoadUI()
    {
        infoText.gameObject.SetActive(false);
        Stages.SetActive(false); PauseBG.SetActive(false); waterTimerPot.SetActive(false);
        ready.SetActive(false); infoText.text = "Empty"; ThePlantImage.SetActive(false);
        HighlightToPlant(false);

        Image pImage = PauseBG.transform.Find("Icon").GetComponent<Image>();
        if  (landstate == LandState.Empty)
            infoText.gameObject.SetActive(true);
        else if (landstate == LandState.Dry)
        {
            btn.onClick.RemoveAllListeners();
            if(!is_watering)
                btn.onClick.AddListener(() => WaterLand());
            PauseBG.SetActive(true);
            PauseBG.transform.Find("Count").gameObject.SetActive(true);
            if (StaticDatas.PlayerData.PlayerInfos.Water.amount >= 2 || is_watering)
            {
                PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = "2";
                pImage.sprite = WCan;
                pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
            }
            else
            {
                if(!is_watering)
                {
                    PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = "1";
                    pImage.sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Crystal).sprite;
                    pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                }
            }
        }
        else if (landstate == LandState.Plow)
        {
            btn.onClick.RemoveAllListeners();
            if(!is_plowing)
                btn.onClick.AddListener(() => PlowLand());
            PauseBG.SetActive(true);
            PauseBG.transform.Find("Count").gameObject.SetActive(true);
            if (Storage.instance.hasEnought(Items.Rake, 1, false) || is_plowing)
            {
                PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = "1";
                pImage.sprite = Sprites.instance.sprites.items.Find(e => e.item == Items.Rake).sprite;
                pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
            }
            else
            {
                if(!is_plowing)
                {
                    PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = "1";
                    pImage.sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Crystal).sprite;
                    pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                }
            }
        }
        else
        {
            ThePlantImage.SetActive(true);
            ThePlantImage.transform.Find("The Plant").GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == ThePlant.plant).sprite;
            PauseBG.transform.Find("Count").gameObject.SetActive(true);
            infoText.gameObject.SetActive(false); waterTimerPot.SetActive(true);
            if (ThePlant.state == PlantState.Growing)
            {
                if (ThePlant.hasWater) { PauseBG.SetActive(false); btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => ShowTimer()); }
                else
                {
                    PauseBG.SetActive(true); pImage.sprite = noWater;
                    pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);

                    transform.Find("Info Holder/Watering Pot/Inc").gameObject.SetActive(true);
                    transform.Find("Info Holder/Watering Pot/Dec").gameObject.SetActive(true);
                    if (StaticDatas.PlayerData.PlayerInfos.Water.amount < decide || decide >= w)
                        transform.Find("Info Holder/Watering Pot/Inc").gameObject.SetActive(false);
                    if (decide < 2)
                        transform.Find("Info Holder/Watering Pot/Dec").gameObject.SetActive(false);
                }

                Stages.SetActive(true);
            }
            else if (ThePlant.state == PlantState.ReadyToHarvest)
            { Stages.SetActive(false); ready.SetActive(true); PauseBG.SetActive(false); }
        }
    }

    private void PlowLand()
    {
        bool enought = true;
        if (Storage.instance.hasEnought(Items.Rake, 1, false))
            Storage.instance.UpdateItemCount(Items.Rake, -1);
        else MoneySystem.instance.UpdateCyrstal(-1, out enought);

        if(enought)
        {
            btn.onClick.RemoveAllListeners();
            anim.SetTrigger("Plow Land");
            is_plowing = true;

            StaticDatas.PlayerData.FarmSlots[SlotNumber].plowed++;
        }
    }

    private void WaterLand()
    {
        bool enought = true;
        if (StaticDatas.PlayerData.PlayerInfos.Water.amount >= 2) WaterSL.instance.TriggerAmount(-2);
        else MoneySystem.instance.UpdateCyrstal(-1, out enought);

        if (enought)
        {
            btn.onClick.RemoveAllListeners();
            anim.SetTrigger("Water Land");
            is_watering = true;

            StaticDatas.PlayerData.FarmSlots[SlotNumber].dried++;
        }
    }

    public void ChoosePlant(Plants plant)
    {
        if (ThePlant.plant == Plants.None)
        {
            bool enought = true;
            if (Storage.instance.hasEnought(plant, 1, false))
                Storage.instance.UpdatePlantCount(plant, -FarmLogic.instance.PlantDetails.Find(e => e.plant == plant).price);
            else MoneySystem.instance.UpdateCyrstal(-1, out enought);
            if (enought)
            {
                Debug.Log("slotNumber: " + SlotNumber);
                var proto = FarmLogic.instance.PlantDetails.Find(e => e.plant == plant);
                ThePlant = proto.Clone(); // each slot gets its own copy
                ThePlant.wTimer = StaticDatas.PlayerData.PlayerInfos.Water.WateringTimer;
                Debug.Log($"The.Plant.plant = {ThePlant.plant}");
                Debug.Log($"The.Plant.plant.ToString() = {ThePlant.plant.ToString()}");
                ThePlant.name = ThePlant.plant.ToString();
                transform.name = ThePlant.name;
                PlantsHolder.instance.UpdateCountOfPlants();
                Plant();
            }
        }
    }

    private void Plant()
    {
        Debug.Log("Planting " + ThePlant.plant);

        ThePlant.hasWater = false;
        ThePlant.state = PlantState.Growing;
        Debug.Log("added time to waterTime: " + ThePlant.waterTime);
        landstate = LandState.Planted;
        StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
        StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
        StaticDatas.SaveDatas(); ShowWatering(); LoadUI(); CalculateReqWater();
    }

    private void CheckforGrowth()
    {
        double slices = ThePlant.GrowthTime / Sprites.instance.sprites.StageSprites.Find(e => e.plant == ThePlant.plant).stages.Count;

        // Precompute stage thresholds (in minutes)
        double[] stagesTime = new double[Sprites.instance.sprites.StageSprites.Find(e => e.plant == ThePlant.plant).stages.Count];
        for (int i = 0; i < Sprites.instance.sprites.StageSprites.Find(e => e.plant == ThePlant.plant).stages.Count; i++)
        {
            stagesTime[i] = (i + 1) * slices;
        }

        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(ThePlant.waterTime, "plant slot " + SlotNumber.ToString(), out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes + ThePlant.pauseTime;

        // --- Update plant stages ---
        for (int i = 0; i < stagesTime.Length; i++)
        {
            // Check if plant is in this stage
            if (elapsedMinutes <= stagesTime[i])
            {
                // Set correct sprite
                Image image = Stages.GetComponent<Image>();
                for (int p = 0; p < Sprites.instance.sprites.StageSprites.Count; p++)
                {
                    if (Sprites.instance.sprites.StageSprites[p].plant == ThePlant.plant)
                    {
                        image.sprite = Sprites.instance.sprites.StageSprites[p].stages[i];
                    }
                }
                break;
            }
        }

        if (elapsedMinutes >= ThePlant.GrowthTime)
        {
            rtharvest();
            PauseGrowth(true);
            StaticDatas.SaveDatas();
        }
    }

    private void CheckWaterTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(ThePlant.waterTime, "plant slot " + SlotNumber.ToString(), out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes;
        double elapsedSeconds = elapsed.TotalSeconds;

        float progress = Mathf.Clamp01((float)(elapsedSeconds / (ThePlant.wTimer * 60)));
        // GrowthTime assumed in minutes → multiply by 60 for seconds

        Image filler = waterTimer.GetComponent<Image>();
        filler.fillAmount = 1f - progress;

        // --- Check if ready to harvest ---
        if (elapsedMinutes >= ThePlant.wTimer && ((ThePlant.GrowthTime - ThePlant.pauseTime) > ThePlant.wTimer))
        {
            Debug.Log("Calling pause");
            PauseGrowth(false);
            StaticDatas.SaveDatas();
        }
    }

    private void PauseGrowth(bool end)
    {
        ThePlant.hasWater = false;
        ThePlant.pauseTime += ThePlant.wTimer;
        ThePlant.waterTime = "";
        btn.onClick.RemoveAllListeners();
        if(!end)
            CalculateReqWater();
        else
            anim.SetBool("Show Timer", false);
        LoadUI();
        StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
    }

    private void CalculateReqWater()
    {
        Debug.Log("calculating");
        double req = (ThePlant.GrowthTime - ThePlant.pauseTime) / StaticDatas.PlayerData.PlayerInfos.Water.WateringTimer;
        w = (int)Math.Ceiling(req);

        Debug.Log("opening show water");
        anim.SetBool("Show Watering", true);
        btn.onClick.RemoveAllListeners();
        //calculate max can use to water
        if (StaticDatas.PlayerData.PlayerInfos.Water.amount >= w)
            btn.onClick.AddListener(() => ResumeGrowth(w));
        else
        {
            w = StaticDatas.PlayerData.PlayerInfos.Water.amount;
            btn.onClick.AddListener(() => ResumeGrowth(StaticDatas.PlayerData.PlayerInfos.Water.amount));
        }
        decide = w;
        Debug.Log($"decide = {decide}");
        PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = decide.ToString();
        LoadUI();
    }

    public void ChangeWatering(int amount)
    {
        decide += amount;
        Debug.Log($"decide = {decide}");
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ResumeGrowth(decide));
        PauseBG.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = decide.ToString();
        LoadUI();
    }

    public void ResumeGrowth(int amount)
    {
        if (WaterSL.instance.hasEnoughWater(amount))
        {
            ThePlant.hasWater = true;
            ThePlant.wTimer = StaticDatas.PlayerData.PlayerInfos.Water.WateringTimer * amount;
            ThePlant.waterTime = DateTime.UtcNow.ToString("o");
            WaterSL.instance.TriggerAmount(-amount);
            Debug.Log("Saving details");
            StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
            StaticDatas.SaveDatas(); LoadUI();
            LuckyBox.instance.TryToFindBox();
            anim.SetBool("Show Watering", false);
        }
    }

    private void UpdateTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(ThePlant.waterTime, "plant slot " + SlotNumber.ToString(), out startTime)) return;

        double totalSecondsRequired = ThePlant.GrowthTime * 60;
        double elapsedSeconds = (ThePlant.pauseTime * 60);
        if (ThePlant.hasWater) elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds + (ThePlant.pauseTime * 60);
        else if (!ThePlant.hasWater) elapsedSeconds = (ThePlant.pauseTime * 60);
        string timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        timer.text = timeString;
    }

    public void ShowTimer()
    {
        for (int i = 0; i < FarmLogic.instance.Slots.Count; i++)
        {
            FarmingTS fts = FarmLogic.instance.Slots[i].GetComponent<FarmingTS>();
            if (i != SlotNumber)
            {
                fts.anim.SetBool("Show Timer", false);
            }
            Debug.Log("disabling timer for" + i);
        }
        anim.SetBool("Show Timer", !anim.GetBool("Show Timer"));
        LoadUI();
    }

    private void ShowWatering()
    {
        anim.SetBool("Show Watering", !anim.GetBool("Show Watering"));
    }

    private void rtharvest() //ready to harvest
    {
        ThePlant.state = PlantState.ReadyToHarvest;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => Harvest());
        StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
        Image rimage = ready.GetComponent<Image>();
        rimage.sprite = Sprites.instance.sprites.readySprites.Find(e => e.plant == ThePlant.plant).sprite;
    }

    private void Harvest()
    {
        if (ThePlant.state == PlantState.ReadyToHarvest && Storage.instance.hasEnStorage(ThePlant.harvestAmount))
        {
            anim.SetBool("Show Timer", false);
            Storage.instance.UpdatePlantCount(ThePlant.plant, ThePlant.harvestAmount);
            MoneySystem.instance.UpdateXp(ThePlant.xp);

            btn.onClick.RemoveAllListeners();
            landstate = LandState.Empty;
            ThePlant = new PD()
            {
                plant = Plants.None
            };
            StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].usage++;
            CheckUsage();

            StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
            PlantsHolder.instance.UpdateCountOfPlants();
            StaticDatas.SaveDatas(); LoadUI();
            LuckyBox.instance.TryToFindBox();
        }
    }

    private void CheckUsage()
    {
        if (StaticDatas.PlayerData.FarmSlots[SlotNumber].usage == 10)
            landstate = LandState.Plow;
        else if (StaticDatas.PlayerData.FarmSlots[SlotNumber].usage == 5)
            landstate = LandState.Dry;
    }

    public void HighlightToPlant(bool ready)
    {
        if(!ready)
            transform.Find("Info Holder").GetComponent<Image>().color = new Color32(0, 0, 0, 255);
        else
            transform.Find("Info Holder").GetComponent<Image>().color = new Color32(0, 255, 0, 255);
    }

    #region Animation Events
        public void A_Plow()
        {
            landstate = LandState.Dry;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].usage = 0;
            btn.onClick.RemoveAllListeners();
            is_plowing = false;
            StaticDatas.SaveDatas(); LoadUI();
            LuckyBox.instance.TryToFindBox();
        }

        public void A_Water()
        {
            landstate = LandState.Empty;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
            btn.onClick.RemoveAllListeners();

            is_watering = false;
            StaticDatas.SaveDatas(); LoadUI();
            LuckyBox.instance.TryToFindBox();
        }
    #endregion
}