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

    private bool showtimer;
    private int stimer;
    private Animator anim;

    private Dictionary<Animals, (Vector2, Vector2)> a_s_pos = new Dictionary<Animals, (Vector2, Vector2)>()
    {
        { Animals.Chicken, (new Vector2(0, 0), new Vector2(135, 135)) },
        { Animals.Cow, (new Vector2(0, 24), new Vector2(245, 245)) },
        { Animals.Sheep, (new Vector2(0, 24), new Vector2(245, 245)) },
        { Animals.Pig, (new Vector2(-15, 24), new Vector2(245, 245)) },
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
        LoadUI();
	}

    private void Update()
    {
        if (spotState == ASpotState.HasAnimal)
        {
            btn.onClick.RemoveAllListeners();
            if (TheAnimal.state == AState.Fertilizing && TheAnimal.hasFood)
            {
                CheckFoodTimer();
                btn.onClick.AddListener(() => ShowTimer());
                CheckFertilizing();
                UpdateTimer();
            }
            else if (TheAnimal.state == AState.Fertilizing && !TheAnimal.hasFood)
            {
                btn.onClick.AddListener(() => ResumeFertilizing());
            }
            else if (TheAnimal.state == AState.ReadyToCollect)
            {
                ReadyToCollect();
            }
            else btn.onClick.AddListener(() => SpotDetails());
        }
    }

    private void LoadUI()
    {
        infoText.gameObject.SetActive(false);
        if (spotState == ASpotState.Empty)
        {
            infoText.gameObject.SetActive(true); infoText.text = "Empty"; AnimalImage.SetActive(false); noFood.SetActive(false);
            ChickenSpot.SetActive(false); FeedingPot.SetActive(false); tPro.gameObject.SetActive(false); tProBg.gameObject.SetActive(false);
            ProgressSpot.SetActive(false); timer.gameObject.SetActive(false);
        }
        else
        {
            if (TheAnimal.theProduct != AProducts.None)
            {
                FeedingPot.SetActive(false);
                tPro.gameObject.SetActive(true); tProBg.gameObject.SetActive(true);
                tPro.sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == TheAnimal.theProduct).sprite;
            }
            infoText.gameObject.SetActive(false); timer.gameObject.SetActive(false); AnimalImage.SetActive(true);
            if (TheAnimal.animal == Animals.Chicken) { ChickenSpot.SetActive(true); FeedingPot.SetActive(false); }
            else { ChickenSpot.SetActive(false); FeedingPot.SetActive(true); }

            if (TheAnimal.state == AState.Fertilizing)
            {
                ProgressSpot.SetActive(true);
                if (TheAnimal.hasFood) noFood.SetActive(false);
                else { noFood.SetActive(true); noFoodTimer.text = StaticDatas.convertToTimer(TheAnimal.productTime * 60, TheAnimal.pauseTime * 60); }
                tProBg.color = new Color32(0, 110, 215, 255);
            }

            else if (TheAnimal.state == AState.ReadyToCollect) { ProgressSpot.SetActive(false);}

            Image image = AnimalImage.GetComponent<Image>();
            image.sprite = Sprites.instance.sprites.animals.Find(e => e.animal == TheAnimal.animal).sprite;
            RectTransform rts = AnimalImage.GetComponent<RectTransform>();
            rts.localPosition = a_s_pos[TheAnimal.animal].Item1;
            rts.sizeDelta = a_s_pos[TheAnimal.animal].Item2;

            RectTransform PPrts = ProgressSpot.GetComponent<RectTransform>();
            PPrts.sizeDelta = new Vector2(240, 244);
        }
    }

    public void SpotDetails()
    {
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

            StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
            StaticDatas.PlayerData.AnimalSpots[SpotNumber].state = spotState;
            StaticDatas.SaveDatas();
            AHolder.instance.SpotClicked(spotState, TheAnimal.animal);
            LoadUI();
        }
    }

    public void Product(AProducts product)
    {
        if (FoodPL.instance.hasEnoughFood(AnimalsLogic.instance.TheFood, 1, true))
        {
            TheAnimal.fTimer = StaticDatas.PlayerData.PlayerInfos.Food.materials.Find(e => e.Food == AnimalsLogic.instance.TheFood).foodTimer;
            TheAnimal.theProduct = product;
            TheAnimal.productTime = TheAnimal.prTimes[TheAnimal.products.IndexOf(product)];
            TheAnimal.feedTime = DateTime.UtcNow.ToString("o");
            TheAnimal.hasFood = true;
            TheAnimal.state = AState.Fertilizing;
            for(int i = 0; i < TheAnimal.products.Count; i++)
                if (product == TheAnimal.products[i]) { TheAnimal.xp = TheAnimal.prXp[i]; TheAnimal.amount = TheAnimal.prAmounts[i]; }
            tPro.sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == TheAnimal.theProduct).sprite;
            StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
            Storage.instance.UpdateAnimalFood(AnimalsLogic.instance.TheFood, -1);
            StaticDatas.SaveDatas();
            foreach (Transform item in AHolder.instance.Holder) Destroy(item.gameObject);
            LoadUI();
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

        if (elapsedMinutes >= TheAnimal.productTime)
        {
            PauseFertilizing();
            ReadyToCollect();
            LoadUI();
            StaticDatas.SaveDatas();
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
            PauseFertilizing();
            LoadUI();
            StaticDatas.SaveDatas();
        }
    }

    private void PauseFertilizing()
    {
        TheAnimal.hasFood = false;
        TheAnimal.pauseTime += TheAnimal.fTimer;
        TheAnimal.feedTime = "";
        anim.SetBool("ShowTimer", false);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ResumeFertilizing());
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
        LoadUI();
    }

    private void ResumeFertilizing()
    {
        if (FoodPL.instance.hasEnoughFood(AnimalsLogic.instance.TheFood, 1, true))
        {
            TheAnimal.fTimer = StaticDatas.PlayerData.PlayerInfos.Food.materials.Find(e => e.Food == AnimalsLogic.instance.TheFood).foodTimer;
            TheAnimal.hasFood = true;
            DateTime time = DateTime.UtcNow;
            time.AddSeconds(-(TheAnimal.fTimer * 60));
            TheAnimal.feedTime = time.ToString("o");
            anim.SetBool("ShowTimer", false);
            StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
            Storage.instance.UpdateAnimalFood(AnimalsLogic.instance.TheFood, -1);
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
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
        {
            AnimalSpot ans = AnimalsLogic.instance.Spots[i].GetComponent<AnimalSpot>();
            if (i != SpotNumber)
            {
                ans.anim.SetBool("ShowTimer", false);
                ans.showtimer = false;
            }
        }
        anim.SetBool("ShowTimer", !anim.GetBool("ShowTimer"));
        LoadUI();
    }

    private void ReadyToCollect()
    {
        TheAnimal.state = AState.ReadyToCollect;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => CollectProduct());
        tProBg.color = new Color32(0, 100, 0, 255);
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
    }

    private void CollectProduct()
    {
        if (TheAnimal.state == AState.ReadyToCollect && Storage.instance.hasEnStorage(TheAnimal.amount))
        {
            Storage.instance.UpdateAPCount(TheAnimal.theProduct, TheAnimal.amount);
            MoneySystem.instance.UpdateXp(TheAnimal.xp);
            anim.SetBool("ShowTimer", false);
            spotState = ASpotState.HasAnimal;
            TheAnimal.theProduct = AProducts.None;
            TheAnimal.state = AState.None;
            TheAnimal.feedTime = "";
            TheAnimal.hasFood = false;
            TheAnimal.pauseTime = 0;
            tProBg.gameObject.SetActive(false);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SpotDetails());

            StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
            StaticDatas.PlayerData.AnimalSpots[SpotNumber].state = spotState;
            StaticDatas.SaveDatas();
            LuckyBox.instance.TryToFindBox();
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
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
        StaticDatas.PlayerData.AnimalSpots[SpotNumber].state = spotState;
        StaticDatas.SaveDatas();
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


        StaticDatas.PlayerData.AnimalSpots[SpotNumber].AnimalProductDetails = TheAnimal;
        StaticDatas.SaveDatas();
        LoadUI();
    }
}