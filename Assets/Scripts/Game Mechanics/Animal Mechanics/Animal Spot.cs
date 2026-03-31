using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalSpot : MonoBehaviour
{
    [Header("Game Settings")]
    public Button btn;
    public int SpotNumber;
    public APD TheAnimal;
    public ASpotState spotState;

    public TextMeshProUGUI infoText;
    public GameObject ProgressSpot;
    public TextMeshProUGUI timer;
    public GameObject foodTimer;

    public GameObject FeedingPot;
    public GameObject feedingFoodTimer;

    public GameObject productTimer;

    public GameObject AnimalImage;
    public GameObject ChickenSpot;

    public Image tProBg;    //The Product Background
    public Image tPro;      //The Product

    public GameObject noFood;
    public TextMeshProUGUI noFoodTimer;

    private int f; // water amount to contuniue watering with full need of water
    private int decide;

    private int stimer;
    private Animator anim;

    public string listeners = "";

    private Dictionary<Animals, (Vector2, Vector2)> a_s_pos = new Dictionary<Animals, (Vector2, Vector2)>()
    {
        { Animals.Chicken, (new Vector2(0, 0), new Vector2(120, 120)) },
        { Animals.Cow, (new Vector2(0, 18), new Vector2(220, 220)) },
        { Animals.Sheep, (new Vector2(0, 18), new Vector2(220, 220)) },
        { Animals.Pig, (new Vector2(-15, 26), new Vector2(200, 200)) },
    };

    private void Awake()
    {
        anim = GetComponent<Animator>();
        stimer = 0;
    }

    public void Init()
	{
		if (TheAnimal.animal != Animals.None)
			SetAnimalInfos();
        if (spotState == ASpotState.HasAnimal && TheAnimal.state == AState.Fertilizing && !TheAnimal.hasFood) CalculateFoodCount();
        #region
            Transform skip = transform.Find("Skip Button");
            skip.Find("Price/Icon").GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(Currency.Crystal);
            skip.GetComponent<Button>().onClick.RemoveAllListeners();
            skip.GetComponent<Button>().onClick.AddListener(() => SkipOnOff());

            skip.transform.Find("Price").GetComponent<Button>().onClick.RemoveAllListeners();
            skip.transform.Find("Price").GetComponent<Button>().onClick.AddListener(() => SkipProduct());

            if (TheAnimal.state != AState.Fertilizing) skip.gameObject.SetActive(false);
        #endregion
        LoadUI();
	}

    private void Update()
    {
        if (spotState == ASpotState.HasAnimal)
        {
            if (TheAnimal.state == AState.Fertilizing && TheAnimal.hasFood)
            {
                if (TheAnimal.hasFood)
                {
                    CheckFoodTimer();
                    CheckFertilizing();
                    UpdateTimer();
                }                    
            }
            else if (TheAnimal.state == AState.ReadyToCollect) ReadyToCollect();
        }
    }

    private void LoadUI()
    {
        infoText.gameObject.SetActive(false); AnimalImage.SetActive(false); noFood.SetActive(false);
        ChickenSpot.SetActive(false); FeedingPot.SetActive(false); tPro.gameObject.SetActive(false); tProBg.gameObject.SetActive(false);
        ProgressSpot.SetActive(false); timer.gameObject.SetActive(false);
        transform.Find("Skip Button").gameObject.SetActive(false);
        listeners = "";
        if (spotState == ASpotState.Empty)
        {
            infoText.gameObject.SetActive(true); infoText.text = "Empty";
            btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => SpotDetails()); listeners = "SpotDetails";
        }
        else
        {
            if (TheAnimal.theProduct != AProducts.None)
            {
                FeedingPot.SetActive(false);
                tPro.gameObject.SetActive(true); tProBg.gameObject.SetActive(true);
                tPro.sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == TheAnimal.theProduct).sprite;
            }
            else
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SpotDetails()); listeners = "SpotDetails";
            }
            infoText.gameObject.SetActive(false); timer.gameObject.SetActive(false); AnimalImage.SetActive(true);
            if (TheAnimal.animal == Animals.Chicken) { ChickenSpot.SetActive(true); FeedingPot.SetActive(false); }
            else { ChickenSpot.SetActive(false); FeedingPot.SetActive(true); }

            if (TheAnimal.state == AState.Fertilizing)
            {
                ProgressSpot.SetActive(true);
                if (TheAnimal.hasFood) { noFood.SetActive(false);
                    transform.Find("Skip Button").gameObject.SetActive(true);
                    btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => ShowTimer()); listeners = "Show Timer"; }
                else { noFood.SetActive(true); noFoodTimer.text = StaticDatas.convertToTimer(TheAnimal.productTime * 60, TheAnimal.pauseTime * 60);
                    Debug.Log($"at spot {SpotNumber} {TheAnimal.animal} state is {TheAnimal.state} and hasFood = {TheAnimal.hasFood}");;

                    noFood.transform.Find("Count Details/Inc").gameObject.SetActive(true);
                    noFood.transform.Find("Count Details/Dec").gameObject.SetActive(true);
                    if (StaticDatas.PlayerData.PlayerInfos.Water.amount < decide || decide >= f)
                        noFood.transform.Find("Count Details/Inc").gameObject.SetActive(false);
                    if (decide < 2)
                        noFood.transform.Find("Count Details/Dec").gameObject.SetActive(false);
                }
                tProBg.color = new Color32(0, 110, 215, 255);
            }

            else if (TheAnimal.state == AState.ReadyToCollect) { ProgressSpot.SetActive(false);
                transform.Find("Skip Button").gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => CollectProduct()); }

            Image image = AnimalImage.GetComponent<Image>();
            image.sprite = Sprites.instance.sprites.animals.Find(e => e.animal == TheAnimal.animal).sprite;
            RectTransform rts = AnimalImage.GetComponent<RectTransform>();
            rts.localPosition = a_s_pos[TheAnimal.animal].Item1;
            rts.sizeDelta = a_s_pos[TheAnimal.animal].Item2;

            RectTransform PPrts = ProgressSpot.GetComponent<RectTransform>();
            PPrts.sizeDelta = new Vector2(240, 244);
        }
    }

    public void SkipOnOff()
    {
        Animator anim = transform.Find("Skip Button").GetComponent<Animator>();
        int id = Animator.StringToHash("SkipOnOff");
        anim.SetBool("SkipOnOff", !anim.GetBool(id));
    }

    public void SpotDetails()
    {
        Debug.Log($"sending details. State = {spotState}");
        AHolder.instance.spotnumber = SpotNumber;
        if (TheAnimal.theProduct == AProducts.None)
            AHolder.instance.SpotClicked(spotState, TheAnimal.animal);
    }

    public void BuyAnimal(Animals animal)
    {
        if (MoneySystem.instance.hasEnough(Currency.Coin, AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == animal).a_price))
        {
            Debug.Log("StaticDatas.PlayerData.PlayerInfos.Coin = " + StaticDatas.PlayerData.PlayerInfos.Coin);
            Debug.Log("TheAnimal.a_price = " + AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == animal).a_price);
            AnimalImage.SetActive(true);
            Image image = AnimalImage.GetComponent<Image>();
            image.sprite = Sprites.instance.sprites.animals.Find(e => e.animal == animal).sprite;
            RectTransform rts = AnimalImage.GetComponent<RectTransform>();
            rts.localPosition = a_s_pos[animal].Item1;
            rts.sizeDelta = a_s_pos[animal].Item2;

            var proto = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == animal);
            TheAnimal = proto.Clone(); // each slot gets its own copy
            MoneySystem.instance.UpdateCoin(-TheAnimal.a_price, out bool s);
            MoneySystem.instance.UpdateXp(TheAnimal.a_Xp);
            spotState = ASpotState.HasAnimal;
            
            SaveState();
            AHolder.instance.SpotClicked(spotState, TheAnimal.animal);
            LoadUI();
        }
    }

    public void Product(AProducts product)
    {
        if (Storage.instance.hasEnought(AnimalsLogic.instance.TheFood, 1, true))
        {
            TheAnimal.fTimer = StaticDatas.PlayerData.PlayerInfos.Food.materials.Find(e => e.Food == AnimalsLogic.instance.TheFood).foodTimer;
            TheAnimal.theProduct = product;
            TheAnimal.productTime = TheAnimal.prTimes[TheAnimal.products.IndexOf(product)];
            TheAnimal.feedTime = "";
            TheAnimal.hasFood = false;
            TheAnimal.state = AState.Fertilizing;
            for(int i = 0; i < TheAnimal.products.Count; i++)
                if (product == TheAnimal.products[i]) { TheAnimal.xp = TheAnimal.prXp[i]; TheAnimal.amount = TheAnimal.prAmounts[i]; }
            tPro.sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == TheAnimal.theProduct).sprite;
            SaveState();
            foreach (Transform item in AHolder.instance.Holder) Destroy(item.gameObject);
            LoadUI(); CalculateFoodCount();
        }
    }

    private void CheckFertilizing()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheAnimal.feedTime, "animal slot: " + SpotNumber.ToString(), out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes + TheAnimal.pauseTime;
        double totalSeconds = elapsed.TotalSeconds + (TheAnimal.pauseTime * 60);

        float progress = Mathf.Clamp01((float)(totalSeconds / (TheAnimal.productTime * 60)));
        Image filler = productTimer.GetComponent<Image>();
        filler.fillAmount = progress;
        CalculateSkipCost(out int cost);
        if (elapsedMinutes >= TheAnimal.productTime)
        {
            PauseFertilizing(true);
            ReadyToCollect();
            SaveState();
            LoadUI();
        }
    }

    private void CheckFoodTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheAnimal.feedTime, "animal slot: " + SpotNumber.ToString(), out startTime)) return;

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes;
        double elapsedSeconds = elapsed.TotalSeconds;

        float progress = Mathf.Clamp01((float)(elapsedSeconds / (TheAnimal.fTimer * 60)));
        // GrowthTime assumed in minutes → multiply by 60 for seconds

        Image filler = foodTimer.GetComponent<Image>();
        filler.fillAmount = 1f - progress;

        Image fftimer = feedingFoodTimer.GetComponent<Image>();
        fftimer.fillAmount = 1f - progress;

        if (elapsedMinutes >= TheAnimal.fTimer && ((TheAnimal.productTime - TheAnimal.pauseTime) > TheAnimal.fTimer))
        {
            PauseFertilizing(false);
            SaveState();
        }
    }

    private void PauseFertilizing(bool end)
    {
        TheAnimal.hasFood = false;
        TheAnimal.pauseTime += TheAnimal.fTimer;
        TheAnimal.feedTime = "";
        if (end)
            CalculateFoodCount();
        else
            anim.SetBool("Show Timer", false);
        btn.onClick.RemoveAllListeners();
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
        LoadUI();
    }

    public void CalculateFoodCount()
    {
        Debug.Log("calculating");
        double req = (TheAnimal.productTime - TheAnimal.pauseTime) / StaticDatas.PlayerData.PlayerInfos.Food.materials.Find(e => e.Food.Equals(AnimalsLogic.instance.TheFood)).foodTimer;
        f = (int)Math.Ceiling(req);
        
        btn.onClick.RemoveAllListeners();
        //calculate max can use to water
        if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food.Equals(AnimalsLogic.instance.TheFood)).amount >= f)
            btn.onClick.AddListener(() => ResumeFertilizing(f));
        else
        {
            f = StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food.Equals(AnimalsLogic.instance.TheFood)).amount;
            if (StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food.Equals(AnimalsLogic.instance.TheFood)).amount > 0)
                btn.onClick.AddListener(() => ResumeFertilizing(StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food.Equals(AnimalsLogic.instance.TheFood)).amount));
            else btn.onClick.RemoveAllListeners();
        }
        decide = f;
        Debug.Log($"decide = {decide}");
        noFood.transform.Find("Count Details/Count").GetComponent<TextMeshProUGUI>().text = decide.ToString();
        LoadUI();
    }

    public void ChangeFooding(int amount)
    {
        decide += amount;
        Debug.Log($"decide = {decide}");
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ResumeFertilizing(decide));
        noFood.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = decide.ToString();
        LoadUI();
    }

    private void ResumeFertilizing(int amount)
    {
        Debug.Log($"for spot {SpotNumber} Resume clicked");
        if (Storage.instance.hasEnought(AnimalsLogic.instance.TheFood, amount, true))
        {
            Debug.Log($"for spot {SpotNumber} resume reqs met");
            TheAnimal.fTimer = StaticDatas.PlayerData.PlayerInfos.Food.materials.Find(e => e.Food.Equals(AnimalsLogic.instance.TheFood)).foodTimer * amount;
            TheAnimal.hasFood = true;
            DateTime time = DateTime.UtcNow;
            time.AddSeconds(-(TheAnimal.fTimer * 60));
            TheAnimal.feedTime = time.ToString("o");
            Storage.instance.UpdateThingCount(AnimalsLogic.instance.TheFood, -amount);
            MoneySystem.instance.UpdateXp(5 * amount);
            LuckyBox.instance.TryToFindBox(0.1f * amount);
            SaveState();
            LoadUI();
        }
    }

    private void CalculateSkipCost(out int cost)
    {
        cost = 0;
        cost = StaticDatas.FindSkipCost(TheAnimal.feedTime, "Food", TheAnimal.productTime);
        Debug.Log($"cost = {cost}");
        transform.Find("Skip Button/Price/Cost").GetComponent<TextMeshProUGUI>().text = cost.ToString();
    }

    private void SkipProduct()
    {
        CalculateSkipCost(out int cost);
        if (MoneySystem.instance.hasEnough(Currency.Crystal, cost))
        {
            MoneySystem.instance.UpdateCyrstal(-cost, out bool enought);
            TheAnimal.state = AState.ReadyToCollect;
            LoadUI();
        }
    }

    private void UpdateTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TheAnimal.feedTime, "animal slot: " + SpotNumber.ToString(), out startTime)) return;

        string timeString = "";
        if (stimer == 0)
        {
            double totalSecondsRequired = TheAnimal.productTime * 60;
            double elapsedSeconds = (TheAnimal.pauseTime * 60);
            if (TheAnimal.hasFood) elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds + (TheAnimal.pauseTime * 60);
            else if (!TheAnimal.hasFood) elapsedSeconds = (TheAnimal.pauseTime * 60);
            timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        }
        else if (stimer == 1)
        {
            double totalSecondsRequired = TheAnimal.fTimer * 60;
            double elapsedSeconds = TheAnimal.fTimer * 60;
            if (TheAnimal.hasFood) elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        }
        timer.text = timeString;
    }

    public void switchTimer(int i)
    {
        stimer = i;
    }

    public void ShowTimer()
    {
        for (int i = 0; i < AnimalsLogic.instance.Spots.Count; i++)
            if (i != SpotNumber) AnimalsLogic.instance.Spots[i].GetComponent<AnimalSpot>().anim.SetBool("Show Timer", false);
        anim.SetBool("Show Timer", !anim.GetBool("Show Timer"));
        LoadUI();
    }

    private void ReadyToCollect()
    {
        TheAnimal.state = AState.ReadyToCollect;
        tProBg.color = new Color32(0, 100, 0, 255);
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
    }

    private void CollectProduct()
    {
        if (TheAnimal.state == AState.ReadyToCollect && Storage.instance.hasEnStorage(TheAnimal.amount))
        {
            Storage.instance.UpdateThingCount(TheAnimal.theProduct, TheAnimal.amount);
            MoneySystem.instance.UpdateXp(TheAnimal.xp);
            anim.SetBool("Show Timer", false);
            spotState = ASpotState.HasAnimal;
            TheAnimal.theProduct = AProducts.None;
            TheAnimal.state = AState.None;
            TheAnimal.feedTime = "";
            TheAnimal.hasFood = false;
            TheAnimal.pauseTime = 0;
            tProBg.gameObject.SetActive(false);

            SaveState();
            LuckyBox.instance.TryToFindBox(0.3f);
            LoadUI();
        }
    }

    public void CutTheAnimal()
    {
        spotState = ASpotState.Empty;
        TheAnimal = new APD()
        {
            animal = Animals.None,
            theProduct = AProducts.None
        };
        SaveState();
        foreach (Transform item in AHolder.instance.Holder) Destroy(item.gameObject);
        LoadUI();
    }

    private void SetAnimalInfos()
    {
		Debug.Log("instance: " + (AnimalsLogic.instance != null));
		Debug.Log("AnimalsDetails: " + (AnimalsLogic.instance?.AnimalsDetails != null));
		Debug.Log("TheAnimal: " + (TheAnimal != null));
		Debug.Log("TheAnimal.animal = " + TheAnimal?.animal);

		var found = AnimalsLogic.instance?.AnimalsDetails?.Find(e => e.animal == TheAnimal.animal);
		Debug.Log("Found? " + (found != null));

		TheAnimal.a_price = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == TheAnimal.animal).a_price;
		TheAnimal.a_Xp = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == TheAnimal.animal).a_Xp;
		TheAnimal.prTimes = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == TheAnimal.animal).prTimes;
		TheAnimal.products = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == TheAnimal.animal).products;
        TheAnimal.prXp = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == TheAnimal.animal).prXp;
        TheAnimal.prAmounts = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == TheAnimal.animal).prAmounts;

        SaveState();
        LoadUI();
    }

    private void SaveState()
    {
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].state = spotState;
        StaticDatas.SaveDatas();
    }
}