using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantsHolder : MonoBehaviour
{
    public static PlantsHolder instance;
    public Transform ph;
    public GameObject PlantPrefab;
    public List<GameObject> Plants = new();

    public Transform Farm;
    private void Awake()
    {
        instance = this;
        StaticDatas.LoadDatas();
        PopulatePlantsHolder();
    }

    public void PopulatePlantsHolder()
    {
        Plants = new();
        int hindex = 0;
        foreach (Transform item in ph) Destroy(item.gameObject);
        for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_plants.Count; i++)
        {
            if (StaticDatas.PlayerData.unlocked_items.u_plants[i].owned)
            {
                Plants p = StaticDatas.PlayerData.unlocked_items.u_plants[i].plant;
                Debug.Log("working on Plant " + p);
                GameObject dublicate = Instantiate(PlantPrefab, ph);
                dublicate.transform.name = p.ToString();

                Button button = dublicate.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                int chosen = hindex;
                button.onClick.AddListener(() => PHChoosePlant(chosen, p));

                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == p).sprite;

                dublicate.transform.Find("Details").gameObject.SetActive(true);
                dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
                dublicate.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20, 0);

                if (Storage.instance.hasEnought(p, 1, false))
                {
                    dublicate.transform.Find("Details/Count").gameObject.SetActive(true);
                    dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.Boxes.Find(e => e.GetComponent<S_Box>().plant == p).GetComponent<S_Box>().count.ToString();
                }
                else
                {
                    dublicate.transform.Find("Details/Price").gameObject.SetActive(true);
                    dublicate.transform.Find("Details/Icon").gameObject.SetActive(true);
                    dublicate.transform.Find("Details/Icon").GetComponent<Image>().sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == Currency.Crystal).sprite;
                    dublicate.transform.Find("Details/Price").GetComponent<TextMeshProUGUI>().text = "1";
                }

                #region Info Button detailing
                    GameObject ib = Instantiate(Sprites.instance.InfoButtonPrefab, dublicate.transform);
                    RectTransform ibrts = ib.GetComponent<RectTransform>();
                    ibrts.anchoredPosition = new Vector2(0, 10);
                    ibrts.anchorMax = new Vector2((float)0.5, 1);
                    ibrts.anchorMin = new Vector2((float)0.5, 1);
                    ib.GetComponent<InfoDetails>().item = p;
                    ib.GetComponent<InfoDetails>().index = hindex;
                #endregion

                Plants.Add(dublicate);
                hindex++;
            }
        }

        GridLayoutGroup glg = ph.GetComponent<GridLayoutGroup>();
        RectTransform rts = ph.GetComponent<RectTransform>();
        rts.sizeDelta = new Vector2((Plants.Count * glg.cellSize.x) + ((Plants.Count - 1) * glg.spacing.x) + glg.padding.left + glg.padding.right + 115, 160);
        if (rts.sizeDelta.x < 1000) rts.sizeDelta = new Vector2(1000, rts.sizeDelta.y);
    }

    private void PHChoosePlant(int chosen, Plants plant)
    {
        for (int s = 0; s < Plants.Count; s++)
        {
            if (s == chosen) Plants[s].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            else Plants[s].GetComponent<Image>().color = new Color32(100, 100, 100, 255);
        }
        for (int i = 0; i < FarmLogic.instance.Slots.Count; i++)
        {
            FarmingTS script = FarmLogic.instance.Slots[i].GetComponent<FarmingTS>();
            if (script.landstate == LandState.Empty && script.ThePlant.state == PlantState.None)
            {
                script.btn.onClick.RemoveAllListeners();
                script.btn.onClick.AddListener(() => script.ChoosePlant(plant));
            }
        }
    }
}
