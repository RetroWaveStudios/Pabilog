using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile instance;
    public LevelSystem ls;
    public Level level;
    public List<LevelRewards> rewards;

    [Header("Username Settings")]
    public TMP_InputField input;
    public GameObject unWindow; // username set window;

    [Header("Level Info Settings")]
    public TextMeshProUGUI mainLevelText;

    public GameObject infoWindow;
    public TextMeshProUGUI cLevel; //current Level
    public TextMeshProUGUI nLevel; //next Level

    public Image levelBar; // detailed level bar
    public Image MainLevelBar;

    public TextMeshProUGUI cXp; //current XP
    public TextMeshProUGUI nXP; //needed XP

    public Transform irHolder; //item rewards holder
    public Transform mrHolder; // money rewards holder

    public GameObject PlantPrefab;
    public GameObject AnimalPrefab;
    public GameObject AProductPrefab;

    public GameObject CoinPrefab;
    public GameObject CyristalPrefab;

    public void Awake()
    {
        instance = this;
        //unWindow.SetActive(false);
        infoWindow.SetActive(true);
        infoWindow.SetActive(false);
        StaticDatas.LoadDatas();
        Debug.Log("creating ls");
        ls = new LevelSystem(17);
        level = ls.GetLevelByXP(StaticDatas.PlayerData.PlayerInfos.XP);

        Debug.Log($"Current Level: {level.LevelNumber}, XP needed: {level.reqXP}");
        CheckLevelUp();
        //CheckForName();
    }

    #region Level infos
    public void InfoWindow()
    {
        infoWindow.SetActive(!infoWindow.activeInHierarchy);
        foreach (Transform child in irHolder) Destroy(child.gameObject);
        foreach (Transform child in mrHolder) Destroy(child.gameObject);

        PopulateItemRewards();
        PopulateMoneyRewards();
        UpdateLevelBar();
    }

    public void UpdateLevelBar()
    {
        cLevel.text = level.LevelNumber.ToString();
        nLevel.text = (level.LevelNumber + 1).ToString();

        // Clamp to last level if max
        int currentIndex = Mathf.Max(level.LevelNumber - 1, 0);
        int nextIndex = Mathf.Min(currentIndex + 1, ls.levels.Count - 1);

        int currentXPNeeded = ls.levels[currentIndex].reqXP;
        int nextXPNeeded = ls.levels[nextIndex].reqXP;

        int playerXP = StaticDatas.PlayerData.PlayerInfos.XP;

        // Progress within current level
        int gainedXP = playerXP - currentXPNeeded;
        int requiredXP = nextXPNeeded - currentXPNeeded;

        float fill = (float)gainedXP / requiredXP;
        fill = Mathf.Clamp01(fill);

        // Apply to UI
        mainLevelText.text = level.LevelNumber.ToString();
        levelBar.fillAmount = fill;
        MainLevelBar.fillAmount = fill;

        cXp.text = gainedXP.ToString();
        nXP.text = requiredXP.ToString();
    }

    public void PopulateItemRewards()
    {
        if (rewards[level.LevelNumber - 1].Plant.Count > 0)
        {
            for (int i = 0; i < rewards[level.LevelNumber - 1].Plant.Count; i++)
            {
                Debug.Log("Level " + (level.LevelNumber + 1) + " has plant to unlock");
                GameObject dublicate = Instantiate(PlantPrefab, irHolder);
                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == rewards[level.LevelNumber - 1].Plant[i]).sprite;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            }
        }
        if (rewards[level.LevelNumber - 1].Trees.Count > 0)
        {
            for (int i = 0; i < rewards[level.LevelNumber - 1].Trees.Count; i++)
            {
                Debug.Log("Level " + (level.LevelNumber + 1) + " has fruit to unlock");
                GameObject dublicate = Instantiate(PlantPrefab, irHolder);
                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == rewards[level.LevelNumber - 1].Trees[i]).sprite;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            }
        }
        if (rewards[level.LevelNumber - 1].Animal.Count > 0)
        {
            for (int i = 0; i < rewards[level.LevelNumber - 1].Animal.Count; i++)
            {
                Debug.Log("Level " + (level.LevelNumber + 1) + " has animal to unlock");
                GameObject dublicate = Instantiate(AnimalPrefab, irHolder);
                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.animals.Find(e => e.animal == rewards[level.LevelNumber - 1].Animal[i]).sprite;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            }
        }
        if (rewards[level.LevelNumber - 1].AnimalProduct.Count > 0)
        {
            for (int i = 0; i < rewards[level.LevelNumber - 1].AnimalProduct.Count; i++)
            {
                Debug.Log("Level " + (level.LevelNumber + 1) + " has animal product to unlock");
                GameObject dublicate = Instantiate(AProductPrefab, irHolder);
                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == rewards[level.LevelNumber - 1].AnimalProduct[i]).sprite;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            }
        }
        if (rewards[level.LevelNumber - 1].Product.Count > 0)
        {
            for (int i = 0; i < rewards[level.LevelNumber - 1].Product.Count; i++)
            {
                Debug.Log("Level " + (level.LevelNumber + 1) + " has product to unlock");
                GameObject dublicate = Instantiate(AProductPrefab, irHolder);
                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.products.Find(e => e.product == rewards[level.LevelNumber - 1].Product[i]).sprite;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            }
        }
        if (rewards[level.LevelNumber - 1].Machine.Count > 0)
        {
            for (int i = 0; i < rewards[level.LevelNumber - 1].Machine.Count; i++)
            {
                Debug.Log("Level " + (level.LevelNumber + 1) + " has product to unlock");
                GameObject dublicate = Instantiate(AProductPrefab, irHolder);
                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.machines.Find(e => e.machine == rewards[level.LevelNumber - 1].Machine[i]).sprite;
                dublicate.transform.Find("Price").gameObject.SetActive(false);
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            }
        }
    }

    public void PopulateMoneyRewards()
    {
        if (rewards[level.LevelNumber-1].Coin != 0)
        {
            GameObject dublicate = Instantiate(CoinPrefab, mrHolder);
            Image image = dublicate.GetComponent<Image>();
            image.sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Coin).sprite;
            TextMeshProUGUI t = dublicate.GetComponentInChildren<TextMeshProUGUI>();
            t.text = rewards[level.LevelNumber - 1].Coin.ToString();
        }
        if (rewards[level.LevelNumber-1].Crystal != 0)
        {
            GameObject dublicate = Instantiate(CoinPrefab, mrHolder);
            Image image = dublicate.GetComponent<Image>();
            image.sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Crystal).sprite;
            TextMeshProUGUI t = dublicate.GetComponentInChildren<TextMeshProUGUI>();
            t.text = rewards[level.LevelNumber - 1].Crystal.ToString();
        }
    }

    public void CheckLevelUp()
    {
        Level newLevel = ls.GetLevelByXP(StaticDatas.PlayerData.PlayerInfos.XP);
        Debug.Log($"Current level {level.LevelNumber}");
        
        for (int lvl = 0; lvl <= newLevel.LevelNumber-2; lvl++)
        {
            GiveRewards(lvl);
        }
        if(level.LevelNumber < newLevel.LevelNumber)
        {
            TasksLogic.instance.DetermineTaskLimit();
            PlantsHolder.instance.PopulatePlantsHolder();
            ProductionLogic.instance.PopulateMachines();
            StaticDatas.SaveDatas();
        }
        level = newLevel;
        UpdateLevelBar();
    }

    private void GiveRewards(int newLevel)
    {
        Debug.Log($"Granding rewards");
        LevelRewards reward = rewards[newLevel]; // zero-based index
        bool given = true;
        if (reward.Plant.Count > 0)
            for(int i = 0; i < reward.Plant.Count; i++){
                if (!StaticDatas.PlayerData.unlocked_items.u_plants.Contains(reward.Plant[i])){
                    StaticDatas.PlayerData.unlocked_items.u_plants.Add(reward.Plant[i]);
                    Debug.Log($"{reward.Plant[i]} unlocked");
                    Storage.instance.UpdateThingCount(reward.Plant[i], 3);
                    given = false;
                }
            }

        if (reward.Trees.Count > 0)
            for (int i = 0; i < reward.Trees.Count; i++)
                if (!StaticDatas.PlayerData.unlocked_items.u_fruits.Contains(reward.Trees[i])){
                    StaticDatas.PlayerData.unlocked_items.u_fruits.Add(reward.Trees[i]);
                    Debug.Log($"{reward.Trees[i]} unlocked");
                    given = false;
                }

        if (reward.Animal.Count > 0)
            for (int i = 0; i < reward.Animal.Count; i++)
                if (!StaticDatas.PlayerData.unlocked_items.u_animals.Contains(reward.Animal[i])){
                    StaticDatas.PlayerData.unlocked_items.u_animals.Add(reward.Animal[i]);
                    Debug.Log($"{reward.Animal[i]} unlocked");
                    given = false;
                }

        if (reward.AnimalProduct.Count > 0)
            for (int i = 0; i < reward.AnimalProduct.Count; i++)
                if (!StaticDatas.PlayerData.unlocked_items.u_a_products.Contains(reward.AnimalProduct[i])){
                    StaticDatas.PlayerData.unlocked_items.u_a_products.Add(reward.AnimalProduct[i]);
                    Debug.Log($"{reward.AnimalProduct[i]} unlocked");
                    given = false;
                }

        if (reward.Product.Count > 0)
            for (int i = 0; i < reward.Product.Count; i++)
                if (!StaticDatas.PlayerData.unlocked_items.u_Products.Contains(reward.Product[i])){
                    StaticDatas.PlayerData.unlocked_items.u_Products.Add(reward.Product[i]);
                    Debug.Log($"{reward.Product[i]} unlocked");
                    given = false;
                }

        if (reward.Machine.Count > 0)
            for (int i = 0; i < reward.Machine.Count; i++)
            {
                if (!StaticDatas.PlayerData.unlocked_items.u_machines.Contains(StaticDatas.FormatEnum(reward.Machine[i])))
                {
                    StaticDatas.PlayerData.unlocked_items.u_machines.Add(StaticDatas.FormatEnum(reward.Machine[i]));
                    Debug.Log($"{reward.Machine[i]} unlocked");
                    given = false;
                }
            }

        if (reward.Coin > 0 && !given)
            StaticDatas.PlayerData.PlayerInfos.Coin += reward.Coin;
        if (reward.Crystal > 0 && !given)
            StaticDatas.PlayerData.PlayerInfos.Crystal += reward.Crystal;
    }

    #endregion

    #region Username Handling

    private void CheckForName()
    {
        if (StaticDatas.PlayerData.PlayerInfos.name == null && StaticDatas.PlayerData.PlayerInfos.IDnumber == 0)
        {
            unWindow.SetActive(true);
        }
    }

    public void GetInput()
    {
        if (input.text != "" || input.text.Length > 3)
        {
            StaticDatas.PlayerData.PlayerInfos.name = input.text;
            SetID();
        }
    }

    private void SetID()
    {
        StaticDatas.PlayerData.PlayerInfos.IDnumber = Random.Range(10000000, 99999999);
        StaticDatas.SaveDatas();
        unWindow.SetActive(false);
    }
    #endregion
}