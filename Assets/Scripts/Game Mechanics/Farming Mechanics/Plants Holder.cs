using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantsHolder : MonoBehaviour
{
    public static PlantsHolder instance;
    public Transform ph;
    public List<GameObject> PlantsInPH = new();
    public List<GameObject> L_PlantsInPH = new();

    public Transform Farm;
    private void Awake()
    {
        instance = this;
        StaticDatas.LoadDatas();
        PopulatePlantsHolder();
    }

    public void PopulatePlantsHolder()
    {
        PlantsInPH = new();
        L_PlantsInPH = new();
        int hindex = 0;
        foreach (Transform item in ph) Destroy(item.gameObject);

        Plants[] plant_names = (Plants[])Enum.GetValues(typeof(Plants));
        int indexToRemove = 0;
        for (int f = 0; f < plant_names.Length; f++) if (plant_names[f] == Plants.None) indexToRemove = f;

        Plants[] newArr = new Plants[plant_names.Length - 1];

        for (int i = 0, j = 0; i < plant_names.Length; i++)
        {
            if (i == indexToRemove) continue;
            newArr[j++] = plant_names[i];
        }

        plant_names = newArr;
        if (StaticDatas.PlayerData.unlocked_items.u_plants != null || StaticDatas.PlayerData.unlocked_items.u_plants.Count > 0)
        {
            for (int i = 0; i < plant_names.Length; i++)
            {
                if (StaticDatas.PlayerData.unlocked_items.u_plants.Contains(plant_names[i]))
                {
                    Plants p = plant_names[i];
                    Debug.Log("working on Plant " + p);
                    GameObject dublicate = Instantiate(Sprites.instance.HolderPrefab, ph);
                    dublicate.transform.name = p.ToString();

                    Button button = dublicate.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    int chosen = hindex;
                    button.onClick.AddListener(() => PHChoosePlant(chosen, p));

                    dublicate.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(p);

                    #region Info Button detailing
                        GameObject ib = Instantiate(Sprites.instance.InfoButtonPrefab, dublicate.transform);
                        RectTransform ibrts = ib.GetComponent<RectTransform>();
                        ibrts.anchoredPosition = new Vector2(0, 10);
                        ibrts.anchorMax = new Vector2(0.5f, 1);
                        ibrts.anchorMin = new Vector2(0.5f, 1);
                        ib.GetComponent<InfoDetails>().btn.onClick.RemoveAllListeners();
                        ib.GetComponent<InfoDetails>().btn.onClick.AddListener(() => ib.GetComponent<InfoDetails>().DetailsOnOff("CT", "Item", p, null, chosen));
                    #endregion

                    PlantsInPH.Add(dublicate);
                    UpdateCountOfPlants();
                    hindex++;
                }
                else
                {
                    Plants p = plant_names[i];
                    GameObject dublicate = Instantiate(Sprites.instance.LockedHolderPrefab, ph);
                    dublicate.transform.name = "locked " + p.ToString();

                    dublicate.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(p);
                    dublicate.GetComponent<Image>().color = new Color32(77, 77, 77, 255);

                    int level = 0;
                    for (int l = 0; l < PlayerProfile.instance.rewards.Count; l++)
                        if (PlayerProfile.instance.rewards[l].Plant.Contains(p)) level = l;

                    Debug.Log($"locked: working on Plant {p} and level = {level}");
                    dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = "Lvl " + level;

                    L_PlantsInPH.Add(dublicate);
                }
            }
        }

        GridLayoutGroup glg = ph.GetComponent<GridLayoutGroup>();
        RectTransform rts = ph.GetComponent<RectTransform>();
        int totalCount = PlantsInPH.Count + L_PlantsInPH.Count;
        Debug.Log($"totalCount = {totalCount}");
        Debug.Log($"glg.cellSize.x = {glg.cellSize.x}");
        Debug.Log($"glg.spacing.x = {glg.spacing.x}");
        rts.sizeDelta = new Vector2((totalCount * glg.cellSize.x) + ((totalCount - 1) * glg.spacing.x) + glg.padding.left + glg.padding.right + 280, 160);
        if (rts.sizeDelta.x < 1000) rts.sizeDelta = new Vector2(1000, rts.sizeDelta.y);
    }

    private void PHChoosePlant(int chosen, Plants plant)
    {
        for (int s = 0; s < PlantsInPH.Count; s++)
        {
            if (s == chosen) PlantsInPH[s].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            else PlantsInPH[s].GetComponent<Image>().color = new Color32(100, 100, 100, 255);
        }
        for (int i = 0; i < FarmLogic.instance.Slots.Count; i++)
        {
            FarmingTS script = FarmLogic.instance.Slots[i].GetComponent<FarmingTS>();
            if (script.landstate == LandState.Empty && script.ThePlant.state == PlantState.None)
            {
                script.btn.onClick.RemoveAllListeners();
                script.btn.onClick.AddListener(() => script.ChoosePlant(plant));
                script.HighlightToPlant(true);
            }
        }
    }

    public void UpdateCountOfPlants()
    {
        for (int s = 0; s < PlantsInPH.Count; s++)
        {
            GameObject plant = PlantsInPH[s];

            if(Enum.TryParse(plant.name, out Plants p))

            plant.transform.Find("Details").gameObject.SetActive(true);
            foreach (Transform item in plant.transform.Find("Details")) item.gameObject.SetActive(false);

            plant.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
            plant.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20, 0);

            if (Storage.instance.hasEnought(p, 1, false))
            {
                plant.transform.Find("Details/Count").gameObject.SetActive(true);
                plant.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(p).ToString();
            }
            else
            {
                plant.transform.Find("Details/Price").gameObject.SetActive(true);
                plant.transform.Find("Details/Icon").gameObject.SetActive(true);
                plant.transform.Find("Details/Icon").GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(Currency.Crystal);
                plant.transform.Find("Details/Price").GetComponent<TextMeshProUGUI>().text = "1";
            }
        }
    }
}
