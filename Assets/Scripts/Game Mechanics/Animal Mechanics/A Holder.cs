using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AHolder : MonoBehaviour
{
    public static AHolder instance;
    public Transform Holder;

    public List<GameObject> allAnimals;
    public List<GameObject> allProducts;

    public List<GameObject> l_animals;
    public List<GameObject> l_products;

    public int spotnumber;

    private Dictionary<Animals, (AProducts, AProducts)> products = new Dictionary<Animals, (AProducts, AProducts)>()
    {
        { Animals.Chicken, (AProducts.Egg, AProducts.ChickenMeat) },
        { Animals.Cow, (AProducts.Milk, AProducts.Beef) },
        { Animals.Sheep, (AProducts.Wool, AProducts.None) },
        { Animals.Pig, (AProducts.Bacon, AProducts.None) },
    };

    private void Awake()
    {
        instance = this;
    }

    public void SpotClicked(ASpotState spotstate, Animals animal)
    {
        if (spotstate == ASpotState.Empty) AHolder.instance.ChooseAnimal();
        else if (spotstate == ASpotState.HasAnimal) AHolder.instance.ChooseProduct(animal);
    }

    private void ChooseAnimal()
    {
        allAnimals = new(); l_animals = new();
        foreach (Transform item in Holder) Destroy(item.gameObject);

        Animals[] animal_names = (Animals[])Enum.GetValues(typeof(Animals));
        int indexToRemove = 0;
        for (int f = 0; f < animal_names.Length; f++) if (animal_names[f] == Animals.None) indexToRemove = f;

        Animals[] newArr = new Animals[animal_names.Length - 1];

        for (int i = 0, j = 0; i < animal_names.Length; i++)
        {
            if (i == indexToRemove) continue;
            newArr[j++] = animal_names[i];
        }
        animal_names = newArr;

        if (StaticDatas.PlayerData.unlocked_items.u_animals != null || StaticDatas.PlayerData.unlocked_items.u_animals.Count > 0)
        {
            for (int i = 0; i < animal_names.Length; i++)
            {
                if (StaticDatas.PlayerData.unlocked_items.u_animals.Contains(animal_names[i]))
                {
                    Animals a = StaticDatas.PlayerData.unlocked_items.u_animals[i];
                    GameObject dublicate = Instantiate(Sprites.instance.HolderPrefab, Holder);
                    dublicate.transform.name = a.ToString();

                    Image image = dublicate.GetComponent<Image>();
                    image.sprite = Sprites.instance.GetSpriteFromSource(a);

                    dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                    dublicate.transform.Find("Details").gameObject.SetActive(true);
                    dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);

                    dublicate.transform.Find("Details/Price").gameObject.SetActive(true);
                    dublicate.transform.Find("Details/Icon").gameObject.SetActive(true);
                    dublicate.transform.Find("Details/Icon").GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(Currency.Coin);
                    dublicate.transform.Find("Details/Price").GetComponent<TextMeshProUGUI>().text = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == a).a_price.ToString();

                    Debug.Log("spotnumber = " + spotnumber);
                    AnimalSpot ans = AnimalsLogic.instance.Spots[spotnumber].GetComponent<AnimalSpot>();

                    Button button = dublicate.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ans.BuyAnimal(a));
                    allAnimals.Add(dublicate);
                }
                else
                {
                    Animals a = animal_names[i];
                    GameObject dublicate = Instantiate(Sprites.instance.LockedHolderPrefab, Holder);
                    dublicate.transform.name = "locked " + a.ToString();

                    dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                    dublicate.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(a);
                    dublicate.GetComponent<Image>().color = new Color32(77, 77, 77, 255);
                    dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);

                    int level = 0;
                    for (int l = 0; l < PlayerProfile.instance.rewards.Count; l++) if (PlayerProfile.instance.rewards[l].Animal.Contains(a)) level = l;

                    Debug.Log($"locked: working on Animal {a} and level = {level}");
                    dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = "Lvl " + level;

                    l_animals.Add(dublicate);
                }
            }
        }
    }

    private void ChooseProduct(Animals animal)
    {
        Debug.Log($"{animal} sent for product choosing");
        allProducts = new(); l_products = new();
        foreach (Transform item in Holder) Destroy(item.gameObject);

        foreach (var item in products)
        {
            if (item.Key == animal)
            {
                AProducts[] animalProducts = { item.Value.Item1, item.Value.Item2 };
                int index = 0;
                foreach (var ap in animalProducts)
                {
                    Debug.Log($"checking if animal product {ap} is unlocked");
                    if (ap != AProducts.None && StaticDatas.PlayerData.unlocked_items.u_a_products.Contains(ap))
                    {
                        Debug.Log($"animal product {ap} is unlocked");
                        GameObject dublicate = Instantiate(Sprites.instance.HolderPrefab, Holder);
                        dublicate.transform.name = ap.ToString();

                        AnimalSpot ans = AnimalsLogic.instance.Spots[spotnumber].GetComponent<AnimalSpot>();

                        Image image = dublicate.GetComponent<Image>();
                        image.sprite = Sprites.instance.GetSpriteFromSource(ap);

                        dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                        dublicate.transform.Find("Details").gameObject.SetActive(true);
                        dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
                        dublicate.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20, 0);
                        dublicate.transform.Find("Details/Time Info").gameObject.SetActive(true);
                        dublicate.transform.Find("Details/Time Info").GetComponent<TextMeshProUGUI>().text =
                            AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == animal).prTimes[index].ToString() + " min";

                        #region Info Button detailing
                            GameObject ib = Instantiate(Sprites.instance.InfoButtonPrefab, dublicate.transform);
                            RectTransform ibrts = ib.GetComponent<RectTransform>();
                            ibrts.anchoredPosition = new Vector2(0, 10);
                            ibrts.anchorMax = new Vector2(0.5f, 1);
                            ibrts.anchorMin = new Vector2(0.5f, 1);

                            ib.GetComponent<InfoDetails>().btn.onClick.RemoveAllListeners();
                            int i = index;
                            ib.GetComponent<InfoDetails>().btn.onClick.AddListener(() => ib.GetComponent<InfoDetails>().DetailsOnOff("CT", "Item", ap, animal, i));
                        #endregion

                        Button button = dublicate.GetComponent<Button>();
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => ans.Product(ap));
                        allProducts.Add(dublicate);
                        index++;
                    }
                    else
                    {
                        AProducts a = ap;
                        GameObject dublicate = Instantiate(Sprites.instance.LockedHolderPrefab, Holder);
                        dublicate.transform.name = "locked " + a.ToString();

                        dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                        dublicate.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(a);
                        dublicate.GetComponent<Image>().color = new Color32(77, 77, 77, 255);
                        dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);

                        int level = 0;
                        for (int l = 0; l < PlayerProfile.instance.rewards.Count; l++) if (PlayerProfile.instance.rewards[l].AnimalProduct.Contains(ap)) level = l;

                        Debug.Log($"locked: working on Animal {a} and level = {level}");
                        dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = "Lvl " + level;

                        l_products.Add(dublicate);
                    }
                }
            }
        }
        AddEraseButton();
    }

    private void AddEraseButton()
    {
        GameObject ei = Instantiate(Sprites.instance.HolderPrefab, Holder);
        RectTransform rts = ei.GetComponent<RectTransform>();
        rts.sizeDelta = new Vector2(130, 130);

        ei.transform.Find("Details").gameObject.SetActive(false);

        ei.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

        AnimalSpot ans = AnimalsLogic.instance.Spots[spotnumber].GetComponent<AnimalSpot>();
        Image Image = ei.GetComponent<Image>();
        Image.sprite = Sprites.instance.EraseItem;
        Button button = ei.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ans.CutTheAnimal());
    }
}
