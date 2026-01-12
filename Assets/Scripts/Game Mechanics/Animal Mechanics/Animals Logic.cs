using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalsLogic : MonoBehaviour
{
    public static AnimalsLogic instance;
    private CanvasGroup canvasGroup;
    public List<APD> AnimalsDetails;

    public List<int> SlotPrices = new();
    public List<int> BuyXP = new();

    int p;
    int xp;

    public Transform Holder;
    public GameObject SpotPrefab;
    public GameObject BuySpotPrefab;
    public List<GameObject> Spots;
    [Header("Food Logic")]
    private Animator anim;

    public Transform FoodsHolder;
    public float foodChooserStartY = -400f;     // Panel hidden position
    public float foodChooserTargetY = 0f;       // Will be calculated dynamically

    private int animParam_FoodChooserT;         // hashed animator float ID

    public GameObject FoodPrefab;
    public List<GameObject> Foods;
    public Image TheFoodImage;
    public a_f_types TheFood = a_f_types.Wheat;

    private List<a_f_types> foodnames = new List<a_f_types>()
    {
        a_f_types.Wheat, a_f_types.Corn, a_f_types.Carrot, a_f_types.Potato
    };


    public void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        anim = GetComponent<Animator>();
        animParam_FoodChooserT = Animator.StringToHash("FoodChooser_T");

        StaticDatas.LoadDatas();
        int price = 200, axp = 10;
        for (int i = 0; i < 18; i++)
        {
            SlotPrices.Add(price * (i + 1));
            BuyXP.Add(axp * (i + 1));
        }
        p = SlotPrices[StaticDatas.PlayerData.animal_slot_count - 1];
        xp = BuyXP[StaticDatas.PlayerData.animal_slot_count - 1];
        if (StaticDatas.PlayerData.AnimalSpots.Count < StaticDatas.PlayerData.animal_slot_count)
        {
            Debug.Log("StaticDatas.PlayerData.AnimalSpots.Count " + StaticDatas.PlayerData.AnimalSpots.Count +
                " < StaticDatas.PlayerData.animal_slot_count " + StaticDatas.PlayerData.animal_slot_count);
            for (int i = StaticDatas.PlayerData.AnimalSpots.Count; i < StaticDatas.PlayerData.animal_slot_count; i++)
            {
                StaticDatas.PlayerData.AnimalSpots.Add(new AnimalsSpotStats()
                {
                    state = ASpotState.Empty,
                    AnimalProductDetails = new APD()
                    {
                        animal = Animals.None,
                        theProduct = AProducts.None
                    }
                });
                Debug.Log("New data added to unassigned spot");
            }
            StaticDatas.SaveDatas();
        }
        for (int i = 0; i < StaticDatas.PlayerData.AnimalSpots.Count; i++)
            PopulateSpots(i);
        PopulateFoodChooser();
        AddBuySpot();
        StaticDatas.SaveDatas();
    }

    private void Update()
    {
        AnimateFoodChooserPanel();
    }

    public void PopulateSpots(int i)
    {
        GameObject dublicate = Instantiate(SpotPrefab, Holder);
        AnimalSpot ans = dublicate.GetComponent<AnimalSpot>();
        ans.SpotNumber = i;
        ans.TheAnimal = StaticDatas.PlayerData.AnimalSpots[i].AnimalProductDetails;
        if (string.IsNullOrEmpty(ans.TheAnimal.name)) ans.TheAnimal.name = ans.TheAnimal.animal.ToString();
        ans.spotState = StaticDatas.PlayerData.AnimalSpots[i].state;
        dublicate.name = ans.TheAnimal.name;

        Button button = dublicate.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ans.SpotDetails());

        ans.Init();
        Spots.Add(dublicate);
    }

    private void AddBuySpot()
    {
        if (StaticDatas.PlayerData.animal_slot_count < 18)
        {
            GameObject buySlot = Instantiate(BuySpotPrefab, Holder);

            Button button = buySlot.GetComponent<Button>();
            button.onClick.AddListener(() => BuySpot());

            Transform price = buySlot.transform.Find("Price");
            Transform ptext = price.transform.Find("Price Text");
            TextMeshProUGUI text = ptext.GetComponent<TextMeshProUGUI>();
            text.text = p.ToString();
        }
    }

    private void BuySpot()
    {
        if (MoneySystem.instance.hasEnough(Currency.Coin, p))
        {
            StaticDatas.PlayerData.animal_slot_count++;
            StaticDatas.UpdateAnimalSpotDatas();
            MoneySystem.instance.UpdateCoin(-p, out bool s);
            MoneySystem.instance.UpdateXp(xp);
            p = SlotPrices[StaticDatas.PlayerData.animal_slot_count - 1];
            xp = BuyXP[StaticDatas.PlayerData.animal_slot_count - 1];
            Transform child = Holder.Find("Animal Buy Spot(Clone)");
            if (child != null)
            {
                Destroy(child.gameObject);
            }
            PopulateSpots(Spots.Count);

            GameObject dublicate = Spots[Spots.Count - 1];
            AnimalSpot ans = dublicate.GetComponent<AnimalSpot>();
            ans.TheAnimal.theProduct = AProducts.None;
            AddBuySpot();
            LuckyBox.instance.TryToFindBox(0.2f * StaticDatas.PlayerData.animal_slot_count);
            StaticDatas.SaveDatas();
        }
    }

    private void AnimateFoodChooserPanel()
    {
        float t = anim.GetFloat(animParam_FoodChooserT); // 0→1 curve from animator

        Vector2 pos = FoodsHolder.GetComponent<RectTransform>().anchoredPosition;
        pos.y = Mathf.Lerp(foodChooserStartY, foodChooserTargetY, t);
        FoodsHolder.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    public void OpenFoodChooser(bool preset)
    {
        if (preset)
            anim.SetBool("Food Chooser", false);
        else
        {
            bool isOpen = anim.GetBool("Food Chooser");

            if (!isOpen)
            {
                // recalc before opening
                PopulateFoodChooser();
                anim.SetBool("Food Chooser", true);
            }
            else anim.SetBool("Food Chooser", false);
        }
    }

    private void RecalculateFoodChooserTarget()
    {
        int count = Foods.Count;

        // Example formula — adjust how you like
        foodChooserTargetY = (((count * 105) + ((count - 1) * 50)) / 2) + (80 * count) - 11.9f;
        Debug.Log($"foodChooserTargetY = {foodChooserTargetY}");
        Debug.Log($"after cal target: localposition = {FoodsHolder.GetComponent<RectTransform>().localPosition}");
    }
    
    public void PopulateFoodChooser()
    {
        TheFoodImage.sprite = Sprites.instance.sprites.AnimalFoodSprites
            .Find(e => e.food == TheFood).sprite;

        foreach (Transform item in FoodsHolder) { Destroy(item.gameObject); Foods.Clear(); }
        foreach (Transform fitem in FoodPL.instance.transform.Find("Food Producer/Foods in Storage/Holder")) Destroy(fitem.gameObject);
        for (int i = 0; i < foodnames.Count; i++)
        {
            if (StaticDatas.PlayerData.unlocked_items.u_plants.Contains(StaticDatas.PlayerData.PlayerInfos.Food.materials[i].material))
            {
                GameObject dublicate = Instantiate(FoodPrefab, FoodsHolder);
                dublicate.transform.name = StaticDatas.PlayerData.PlayerInfos.Food.materials[i].material.ToString() + " Animal Food";
                dublicate.GetComponent<Image>().sprite =
                    Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food == foodnames[i]).sprite;
                int index = i;
                if (Storage.instance.hasEnought(foodnames[i], 1, false))
                {
                    dublicate.GetComponent<Button>().onClick.AddListener(() => FoodChooser(foodnames[index]));
                    dublicate.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                }
                else dublicate.GetComponent<Image>().color = new Color32(120, 120, 120, 255);

                dublicate.transform.Find("Minute").GetComponent<TextMeshProUGUI>().text =
                    StaticDatas.PlayerData.PlayerInfos.Food.materials.Find(e => e.Food == foodnames[i]).foodTimer.ToString() + " min";

                dublicate.transform.Find("Count/Count Text").GetComponent<TextMeshProUGUI>().text = 
                    StaticDatas.PlayerData.PlayerInfos.Food.Amounts.Find(e => e.food == foodnames[i]).amount.ToString();

                Foods.Add(dublicate);
                Instantiate(dublicate, FoodPL.instance.transform.Find("Food Producer/Foods in Storage/Holder"));
            }
        }
        FoodPL.instance.reSizeFoodsHolder();
        VerticalLayoutGroup vlg = FoodsHolder.GetComponent<VerticalLayoutGroup>();
        RectTransform rts = FoodsHolder.GetComponent<RectTransform>();
        rts.sizeDelta = new Vector2(160, vlg.padding.top + (Foods.Count * 105) + ((Foods.Count - 1) * vlg.spacing));
        // Recalculate animation target now that list created
        RecalculateFoodChooserTarget();
        for (int i = 0; i < Spots.Count; i++)
            if (Spots[i].GetComponent<AnimalSpot>().TheAnimal.state == AState.Fertilizing && !Spots[i].GetComponent<AnimalSpot>().TheAnimal.hasFood) Spots[i].GetComponent<AnimalSpot>().CalculateFoodCount();
    }

    public void FoodChooser(a_f_types food)
    {
        if (Storage.instance.hasEnought(food, 1, true))
        {
            TheFood = food;
            TheFoodImage.sprite = Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food == TheFood).sprite;
            OpenFoodChooser(false);
            for (int i = 0; i < Spots.Count; i++)
                if (Spots[i].GetComponent<AnimalSpot>().TheAnimal.state == AState.Fertilizing && !Spots[i].GetComponent<AnimalSpot>().TheAnimal.hasFood) Spots[i].GetComponent<AnimalSpot>().CalculateFoodCount();
        }
    }

    public void ResetSituation()
    {
        foreach (Transform item in AHolder.instance.Holder) Destroy(item.gameObject);
        OpenFoodChooser(true);
        if (canvasGroup != null) StaticDatas.AdjustCanvasGroup(canvasGroup, false);
    }
}