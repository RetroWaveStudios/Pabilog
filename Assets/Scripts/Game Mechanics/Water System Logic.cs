using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterSL : MonoBehaviour
{
    public static WaterSL instance;

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

    private Animator anim;

    [Header("Tap Tap Parameters")]
    public float cooldown;
    public bool tcd;
    public float increasement = 0.12f;

    public float finalReturn = 0;
    public int tapCount;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        instance = this;
        StaticDatas.LoadDatas();
        SetImages(); CheckLevel();
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
            tcd = false;
        }
    }

    public void OpenDetails()
    {
        int id = Animator.StringToHash("Open Water Details");
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
            priceImage.sprite = Sprites.instance.sprites.currencies.
                Find(e => e.Currency == WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency).sprite;
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
                    priceImage.sprite = Sprites.instance.sprites.currencies.
                    Find(e => e.Currency == WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency).sprite;
                    priceText.text = WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price.ToString();
                }
                else FinishLevel();
                LuckyBox.instance.TryToFindBox();
            }
            else return;

            if (WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency == Currency.Coin)
                MoneySystem.instance.UpdateCoin(-WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price);
            else if (WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency == Currency.Crystal)
                MoneySystem.instance.UpdateCyrstal(-WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].price);
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
            priceImage.sprite = Sprites.instance.sprites.currencies.
                Find(e => e.Currency == WaterLevels[StaticDatas.PlayerData.PlayerInfos.WellLevel].currency).sprite;
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
            transform.Find("Water Well/Holder Colored/Details/Tap Value").GetComponent<TextMeshProUGUI>().text = finalReturn.ToString();
            if (finalReturn >= 1f)
            {
                int waterGained = Mathf.FloorToInt(finalReturn);
                TriggerAmount(waterGained);
                finalReturn -= waterGained;
            }
            cooldown = 1f;
        }
    }
}
