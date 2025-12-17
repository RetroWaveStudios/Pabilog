using System;
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

    private bool showtimer;

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
        LoadUI();
    }

    private void Update()
    {
        LoadUI();
        if (ThePlant.state == PlantState.Growing) { if (ThePlant.hasWater) { CheckWaterTimer(); CheckforGrowth(); UpdateTimer(); } }
        else if (ThePlant.state == PlantState.ReadyToHarvest) rtharvest();
    }

    public void LoadUI()
    {
        infoText.gameObject.SetActive(false);
        Stages.SetActive(false); PauseBG.SetActive(false); waterTimerPot.SetActive(false);
        ready.SetActive(false); infoText.text = "Empty"; ThePlantImage.SetActive(false);

        Image pImage = PauseBG.transform.Find("Icon").GetComponent<Image>();
        if  (landstate == LandState.Empty)
            infoText.gameObject.SetActive(true);
        else if (landstate == LandState.Dry)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => WaterLand());
            PauseBG.SetActive(true);
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
            btn.onClick.AddListener(() => PlowLand());
            PauseBG.SetActive(true);
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
            infoText.gameObject.SetActive(false); waterTimerPot.SetActive(true);
            if (ThePlant.state == PlantState.Growing)
            {
                if (ThePlant.hasWater) { PauseBG.SetActive(false); btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => ShowTimer()); }
                else
                {
                    PauseBG.SetActive(true); pImage.sprite = noWater;
                    pImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
                    btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => ResumeGrowth());
                }

                Stages.SetActive(true);
            }
            else if (ThePlant.state == PlantState.ReadyToHarvest)
            { Stages.SetActive(false); ready.SetActive(true); PauseBG.SetActive(false); }
        }
    }

    private void PlowLand()
    {
        anim.SetTrigger("Plow Land");
        is_plowing = true;
        if (Storage.instance.hasEnought(Items.Rake, 1, false))
            Storage.instance.UpdateItemCount(Items.Rake, -1);
        else MoneySystem.instance.UpdateCyrstal(-1);
    }

    private void WaterLand()
    {
        anim.SetTrigger("Water Land");
        is_watering = true;

        if (StaticDatas.PlayerData.PlayerInfos.Water.amount > 2) WaterSL.instance.TriggerAmount(-2);
        else MoneySystem.instance.UpdateCyrstal(-1);
    }

    public void ChoosePlant(Plants plant)
    {
        if (ThePlant.plant == Plants.None && WaterSL.instance.hasEnoughWater(1))
        {
            Debug.Log("slotNumber: " + SlotNumber);
            var proto = FarmLogic.instance.PlantDetails.Find(e => e.plant == plant);
            ThePlant = proto.Clone(); // each slot gets its own copy
            ThePlant.wTimer = StaticDatas.PlayerData.PlayerInfos.Water.WateringTimer;
            Debug.Log($"The.Plant.plant = {ThePlant.plant}");
            Debug.Log($"The.Plant.plant.ToString() = {ThePlant.plant.ToString()}");
            ThePlant.name = ThePlant.plant.ToString();
            transform.name = ThePlant.name;
            if (Storage.instance.hasEnought(plant, 1, false))
                Storage.instance.UpdatePlantCount(ThePlant.plant, -ThePlant.price);
            else MoneySystem.instance.UpdateCyrstal(-1);
            PlantsHolder.instance.PopulatePlantsHolder();
            Plant();
        }
    }

    private void Plant()
    {
        Debug.Log("Planting " + ThePlant.plant);

        ThePlant.waterTime = DateTime.UtcNow.ToString("o");
        ThePlant.wTimer = StaticDatas.PlayerData.PlayerInfos.Water.WateringTimer;
        ThePlant.state = PlantState.Growing;
        ThePlant.hasWater = true;
        WaterSL.instance.TriggerAmount(-1);
        Debug.Log("added time to waterTime: " + ThePlant.waterTime);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ShowTimer());
        landstate = LandState.Planted;
        StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
        StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
        StaticDatas.SaveDatas(); ShowTimer();
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
            StaticDatas.SaveDatas();
            PauseGrowth();
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
            PauseGrowth();
            StaticDatas.SaveDatas();
        }
    }

    private void PauseGrowth()
    {
        ThePlant.hasWater = false;
        anim.SetBool("Show Timer", false);
        Debug.Log("Time deleted");
        ThePlant.pauseTime += ThePlant.wTimer;
        ThePlant.waterTime = "";
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ResumeGrowth());
        StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
    }

    public void ResumeGrowth()
    {
        if (WaterSL.instance.hasEnoughWater(1))
        {
            ThePlant.hasWater = true;
            ThePlant.waterTime = DateTime.UtcNow.ToString("o");
            WaterSL.instance.TriggerAmount(-1);
            Debug.Log("Saving details");
            StaticDatas.PlayerData.FarmSlots[SlotNumber].PlantDetails = ThePlant;
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
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
                fts.showtimer = false;
            }
            else showtimer = !showtimer;
            Debug.Log("disabling timer for" + i);
        }
        if (showtimer)
        {
            Debug.Log("expanting showtimer items");
            anim.SetBool("Show Timer", true);

        }
        else if (!showtimer)
        {
            Debug.Log("shrinking showtimer items");
            anim.SetBool("Show Timer", false);
        }
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
            anim.SetBool("ShowTimer", false);
            Storage.instance.UpdatePlantCount(ThePlant.plant, ThePlant.harvestAmount);
            Storage.instance.UpdateBoxItems();

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
            PlantsHolder.instance.PopulatePlantsHolder();
            StaticDatas.SaveDatas();
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

    #region Animation Events
        public void A_Plow()
        {
            landstate = LandState.Dry;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].usage = 0;
            btn.onClick.RemoveAllListeners();
            is_plowing = false;
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
        }

        public void A_Water()
        {
            landstate = LandState.Empty;
            StaticDatas.PlayerData.FarmSlots[SlotNumber].state = landstate;
            btn.onClick.RemoveAllListeners();

            is_watering = false;
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
        }
    #endregion
}