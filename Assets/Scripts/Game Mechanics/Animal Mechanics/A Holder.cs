using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AHolder : MonoBehaviour
{
    public static AHolder instance;
    public Transform Holder;
    public GameObject A_P_Prefab;

    public List<GameObject> allAnimals;
    public List<GameObject> allProducts;

    public int spotnumber;

    private Dictionary<Animals, (AProducts, AProducts)> products = new Dictionary<Animals, (AProducts, AProducts)>()
    {
        { Animals.Chicken, (AProducts.Egg, AProducts.Ch_Meat) },
        { Animals.Cow, (AProducts.Milk, AProducts.Cow_Meat) },
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
        allAnimals = new();
        foreach (Transform item in Holder) Destroy(item.gameObject);
        for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_animals.Count; i++)
        {
            Animals a = StaticDatas.PlayerData.unlocked_items.u_animals[i].animal;
            if (StaticDatas.PlayerData.unlocked_items.u_animals[i].owned)
            {
                GameObject dublicate = Instantiate(A_P_Prefab, Holder);
                dublicate.transform.name = a.ToString();

                Image image = dublicate.GetComponent<Image>();
                image.sprite = Sprites.instance.sprites.animals.Find(e => e.animal == a).sprite;

                RectTransform rts = dublicate.GetComponent<RectTransform>();
                rts.sizeDelta = new Vector2(110, 110);

                dublicate.transform.Find("Details").gameObject.SetActive(true);
                dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
                
                dublicate.transform.Find("Details/Price").gameObject.SetActive(true);
                dublicate.transform.Find("Details/Icon").gameObject.SetActive(true);
                dublicate.transform.Find("Details/Icon").GetComponent<Image>().sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Coin).sprite;
                dublicate.transform.Find("Details/Price").GetComponent<TextMeshProUGUI>().text = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == a).a_price.ToString();

                RectTransform rt = dublicate.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100);

                Debug.Log("spotnumber = " + spotnumber);
                AnimalSpot ans = AnimalsLogic.instance.Spots[spotnumber].GetComponent<AnimalSpot>();

                Button button = dublicate.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ans.BuyAnimal(a));
                allAnimals.Add(dublicate);
            }
        }
    }

    private void ChooseProduct(Animals animal)
    {
        allProducts = new();
        foreach (Transform item in Holder) Destroy(item.gameObject);
        foreach (var item in products)
        {
            if (item.Key == animal)
            {
                AProducts[] animalProducts = { item.Value.Item1, item.Value.Item2 };
                int index = 0;
                foreach (var ap in animalProducts)
                {
                    if(ap != AProducts.None){
                        GameObject dublicate = Instantiate(A_P_Prefab, Holder);
                        dublicate.transform.name = ap.ToString();

                        AnimalSpot ans = AnimalsLogic.instance.Spots[spotnumber].GetComponent<AnimalSpot>();

                        Image image = dublicate.GetComponent<Image>();
                        image.sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == ap).sprite;

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
                            ibrts.anchorMax = new Vector2((float)0.5, 1);
                            ibrts.anchorMin = new Vector2((float)0.5, 1);

                            ib.GetComponent<InfoDetails>().item = ap;
                            ib.GetComponent<InfoDetails>().sourceInfos = animal;
                            ib.GetComponent<InfoDetails>().index = index;
                        #endregion

                        Button button = dublicate.GetComponent<Button>();
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => ans.Product(ap));
                        allProducts.Add(dublicate);
                        index++;
                    }
                }
            }
        }
        AddEraseButton();
    }

    private void AddEraseButton()
    {
        GameObject ei = Instantiate(A_P_Prefab, Holder);
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
