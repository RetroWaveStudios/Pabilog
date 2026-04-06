using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterSL : MonoBehaviour
{
    public static WaterSL instance;
    public Animator anim;

    public List<WaterSystem> WaterLevels;

    public TextMeshProUGUI priceText;
    public Image priceImage;

    public TextMeshProUGUI maxAmountText;
    public List<Sprite> StoneWellImages;

    public Image Icon;
    public Image TheWell;
    public Image TheWellBg;

    [Header("Level Infos")]
    public Image NextWell;
    public TextMeshProUGUI cFillingTimer;
    public TextMeshProUGUI cWateringTimer;
    public TextMeshProUGUI cMaxCapacity;
    public TextMeshProUGUI nFillingTimer;
    public TextMeshProUGUI nWateringTimer;
    public TextMeshProUGUI nMaxCapacity;
    public TextMeshProUGUI timer;

    public GameObject UpgradeButton;

    public TextMeshProUGUI WaterText;
    public TextMeshProUGUI u_waterAmount;


    [Header("Tap Tap Parameters")]
    public float cooldown;
    public bool tcd;
    public float increasement = 0.12f;
    public float baseInc = 0.12f;

    public float finalReturn = 0;
    public int tapCount;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        instance = this;
        StaticDatas.LoadDatas();
        SetImages(); CheckLevel(); Calculate(out int cost);
        transform.Find("Water Well/Holder Colored/Details/Tap Value").gameObject.SetActive(false);
    }

    private void Update()
    {
        if (StaticDatas.PlayerData.PlayerInfos.Water.amount < StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount)
        {
            WaterFillingSystem(); UpdateTimer();
        }
        else if (StaticDatas.PlayerData.PlayerInfos.Water.amount == StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount)
            StaticDatas.PlayerData.PlayerInfos.Water.fillTime = "";

        if (StaticDatas.PlayerData.PlayerInfos.WellLevel >= WaterLevels.Count) UpgradeButton.SetActive(false);
        if(tcd)
            cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            tapCount = 0;
            finalReturn = 0;
            increasement = baseInc;
            tcd = false;
            transform.Find("Water Well/Holder Colored/Details/Tap Value").gameObject.SetActive(false);
        }
    }

    public void OpenDetails()
    {
        int id = Animator.StringToHash("Open Water Details");
        if (FoodPL.instance.anim.GetBool("Open Details"))
            FoodPL.instance.anim.SetBool("Open Details", false);
        if (Storage.instance.anim.GetBool("Open Storage"))
            Storage.instance.anim.SetBool("Open Storage", false);
        if (TasksLogic.instance.anim.GetBool("Open Task Board"))
            TasksLogic.instance.anim.SetBool("Open Task Board", false);
        if (isAnTrue("Open Water Details"))
            CloseUpgrade();
        anim.SetBool(id, !anim.GetBool(id));

        if(StaticDatas.PlayerData.PlayerInfos.WellLevel >= StoneWellImages.Count)
            transform.Find("Water Well/Holder Colored/Details/Upgrade Button").gameObject.SetActive(false);
    }

    public void OpenUpgrade()
    {
        if (!isAnTrue("Open Water Upgrade"))
        {
            Debug.Log($"Setting to {!isAnTrue("Open Water Upgrade")}");
            anim.SetBool("Open Water Upgrade", true);
            priceImage.sprite = Sprites.instance.GetSpriteFromSource(WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency);
            priceText.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price.ToString();
        }
        else
            UpgradeWellLevel();
    }

    public void CloseUpgrade()
    {
        if (isAnTrue("Open Water Upgrade"))
        {
            Debug.Log($"Setting to {!isAnTrue("Open Water Upgrade")}");
            anim.SetBool("Open Water Upgrade", false);
        }
    }

    private bool isAnTrue(string name)
    {
        int index = Animator.StringToHash(name);
        if (anim.GetBool(index))
            return true;
        else
            return false;
    }

    public void UpgradeWellLevel()
    {
        if (MoneySystem.instance.hasEnough(WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency, WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price))
        {
            if (StaticDatas.PlayerData.PlayerInfos.WellLevel < StoneWellImages.Count)
            {
                StaticDatas.PlayerData.PlayerInfos.WellLevel++;
                if (StaticDatas.PlayerData.PlayerInfos.WellLevel < StoneWellImages.Count)
                {
                    CheckLevel();
                    SetImages();
                    StaticDatas.SaveDatas();
                    priceImage.sprite = Sprites.instance.GetSpriteFromSource(WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency);
                    priceText.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price.ToString();
                }
                else FinishLevel();
                if (WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].currency == Currency.Coin)
                    MoneySystem.instance.UpdateCoin(-WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].price, out bool s);
                else if (WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].currency == Currency.Crystal)
                    MoneySystem.instance.UpdateCyrstal(-WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].price, out bool s);
                MoneySystem.instance.UpdateXp(30 * StaticDatas.PlayerData.PlayerInfos.WellLevel);
                LuckyBox.instance.TryToFindBox(0.5f * StaticDatas.PlayerData.PlayerInfos.WellLevel);
            }
            else return;
        }
    }

    private void SetImages()
    {
        Icon.sprite = StoneWellImages[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1];
        TheWell.sprite = StoneWellImages[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1];
        TheWellBg.sprite = StoneWellImages[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1];

        if(StaticDatas.PlayerData.PlayerInfos.WellLevel < StoneWellImages.Count)
        {
            cFillingTimer.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].fTimer.ToString();
            cWateringTimer.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].WateringTimer.ToString();
            cMaxCapacity.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].MaxAmount.ToString();

            nFillingTimer.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].fTimer.ToString();
            nWateringTimer.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].WateringTimer.ToString();
            nMaxCapacity.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].MaxAmount.ToString();

            NextWell.sprite = StoneWellImages[StaticDatas.PlayerData.PlayerInfos.WellLevel];
        }
    }

    private void CheckLevel()
    {
        WaterSystem ws = new WaterSystem()
        {
            fillTime = StaticDatas.PlayerData.PlayerInfos.Water.fillTime,
            fTimer = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].fTimer,
            WateringTimer = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].WateringTimer,
            MaxAmount = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].MaxAmount,
            amount = StaticDatas.PlayerData.PlayerInfos.Water.amount,
            currency = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].currency,
            price = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].price,
        };
        StaticDatas.PlayerData.PlayerInfos.Water = ws;
        StaticDatas.SaveDatas();
        WaterText.text = StaticDatas.PlayerData.PlayerInfos.Water.amount.ToString();
        maxAmountText.text = StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount.ToString();
        if (StaticDatas.PlayerData.PlayerInfos.WellLevel < 5)
        {
            priceImage.sprite = Sprites.instance.GetSpriteFromSource(WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency);
            priceText.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price.ToString();
        }
    }

    private void FinishLevel()
    {
        Icon.sprite = StoneWellImages[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1];
        TheWell.sprite = StoneWellImages[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1];
        WaterSystem ws = new WaterSystem()
        {
            fillTime = StaticDatas.PlayerData.PlayerInfos.Water.fillTime,
            fTimer = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].fTimer,
            WateringTimer = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].WateringTimer,
            MaxAmount = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].MaxAmount,
            amount = StaticDatas.PlayerData.PlayerInfos.Water.amount,
            currency = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].currency,
            price = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel - 1].price,
        };
        StaticDatas.PlayerData.PlayerInfos.Water = ws;
        StaticDatas.SaveDatas();
        WaterText.text = StaticDatas.PlayerData.PlayerInfos.Water.amount.ToString();
        maxAmountText.text = StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount.ToString();
        CloseUpgrade();
        transform.Find("Water Well/Holder Colored/Details/Upgrade Button").gameObject.SetActive(false);
    }

    public void WaterFillingSystem()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(StaticDatas.PlayerData.PlayerInfos.Water.fillTime, "water refilling system", out startTime))
        { startTime = DateTime.UtcNow; StaticDatas.PlayerData.PlayerInfos.Water.fillTime = startTime.ToString("o"); StaticDatas.SaveDatas(); }

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes;

        int round = (int)(elapsedMinutes / StaticDatas.PlayerData.PlayerInfos.Water.fTimer);
        double remainingminutes = elapsedMinutes % StaticDatas.PlayerData.PlayerInfos.Water.fTimer;
        double elapsedSeconds = elapsed.TotalSeconds;

        float progress = Mathf.Clamp01((float)(elapsedSeconds / (StaticDatas.PlayerData.PlayerInfos.Water.fTimer * 60)));

        Image iimage = Icon.GetComponent<Image>();
        iimage.fillAmount = progress;

        if (round > 0 && StaticDatas.PlayerData.PlayerInfos.Water.amount < StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount)
        {
            Debug.Log(round + " offline waters addded ");
            DateTime time = DateTime.UtcNow;
            time.AddSeconds(-(remainingminutes * 60));
            StaticDatas.PlayerData.PlayerInfos.Water.fillTime = time.ToString("o");
            TriggerAmount(round);
            StaticDatas.SaveDatas();
        }
    }

    private void UpdateTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(StaticDatas.PlayerData.PlayerInfos.Water.fillTime, "water refilling system", out startTime)) return;
        double totalSecondsRequired = StaticDatas.PlayerData.PlayerInfos.Water.fTimer * 60;
        double elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

        double remainingSeconds = totalSecondsRequired - elapsedSeconds;
        string timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        timer.text = timeString;
    }

    public void Calculate(out int cost)
    {
        double price = 25.0 / StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount;
        cost = (int)Math.Ceiling(price * (StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount - StaticDatas.PlayerData.PlayerInfos.Water.amount));
        Debug.Log($"water filling cost = {cost} and price {price}");
        transform.Find("Water Well/Holder Colored/Details/Fill Quick/Price").GetComponent<TextMeshProUGUI>().text = cost.ToString();
    }

    public void FillQuick()
    {
        Calculate(out int cost);
        MoneySystem.instance.UpdateCyrstal(-cost, out bool enought);
        if(enought) TriggerAmount(StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount - StaticDatas.PlayerData.PlayerInfos.Water.amount);

    }

    public void TriggerAmount(int amount)
    {
        u_waterAmount.text = amount.ToString();
        if (amount + StaticDatas.PlayerData.PlayerInfos.Water.amount >= StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount)
        {
            StaticDatas.PlayerData.PlayerInfos.Water.amount = StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount;
            PushNotice.instance.Push("Water Well Full", PushType.Notice);
        }
        else
            StaticDatas.PlayerData.PlayerInfos.Water.amount += amount;
        if (amount > 0)
            anim.SetTrigger("Add Water");
        else if (amount < 0)
            anim.SetTrigger("Remove Water");
        UpdateWaterAmount();
        for (int i = 0; i < FarmLogic.instance.Slots.Count; i++)
            if (FarmLogic.instance.Slots[i].GetComponent<FarmingTS>().landstate == LandState.Planted && !FarmLogic.instance.Slots[i].GetComponent<FarmingTS>().ThePlant.hasWater)
                FarmLogic.instance.Slots[i].GetComponent<FarmingTS>().CalculateReqWater();
            else
                FarmLogic.instance.Slots[i].GetComponent<FarmingTS>().LoadUI();
        for (int i = 0; i < ForestLogic.instance.Slots.Count; i++)
            ForestLogic.instance.Slots[i].GetComponent<TreeSlot>().LoadUI();
        Calculate(out int cost);
    }

    public void UpdateWaterAmount()
    {
        WaterText.text = StaticDatas.PlayerData.PlayerInfos.Water.amount.ToString();
        StaticDatas.SaveDatas();
    }

    public bool hasEnoughWater(int amount)
    {
        if (StaticDatas.PlayerData.PlayerInfos.Water.amount >= amount) return true;
        else { PushNotice.instance.Push("No Enough Water", PushType.Alert); return false; }
    }

    public void TapTapWell()
    {
        if(StaticDatas.PlayerData.PlayerInfos.Water.amount < StaticDatas.PlayerData.PlayerInfos.Water.MaxAmount)
        {
            tapCount++;
            tcd = true;

            finalReturn += increasement; // each tap contributes
            increasement += 0.01f;
            float showFR = MathF.Round(finalReturn, 2);
            transform.Find("Water Well/Holder Colored/Details/Tap Value").gameObject.SetActive(true);
            transform.Find("Water Well/Holder Colored/Details/Tap Value").GetComponent<TextMeshProUGUI>().text = showFR.ToString();
            if (finalReturn >= 1f)
            {
                int waterGained = Mathf.FloorToInt(finalReturn);
                TriggerAmount(waterGained);
                MoneySystem.instance.UpdateXp(1);
                finalReturn -= waterGained;
                increasement -= 0.1f;
            }
            cooldown = 0.5f;
        }
    }
}
