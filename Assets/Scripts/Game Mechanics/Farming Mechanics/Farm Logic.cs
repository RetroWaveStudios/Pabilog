using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmLogic : MonoBehaviour
{
    public static FarmLogic instance;
    private CanvasGroup canvasGroup;
    public List<PD> PlantDetails;
    public List<int> SlotPrices = new();
    public List<int> BuyXP = new();
    int p;
    int xp;
    [Header("UI Elements")]
    public GameObject SlotPrefab;
    public GameObject BuySlotPrefab;
    public List<GameObject> Slots;
    public Transform SlotsHolder;

    private void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        StaticDatas.LoadDatas();
        int price = 100;
        int axp = 5;
        for (int i = 0; i < 32; i++)
        {
            SlotPrices.Add(price * (i + 1));
            BuyXP.Add(axp * (i + 1));
        }
        p = SlotPrices[StaticDatas.PlayerData.land_slot_count - 1];
        xp = BuyXP[StaticDatas.PlayerData.land_slot_count - 1];
        for (int i = StaticDatas.PlayerData.FarmSlots.Count; i < StaticDatas.PlayerData.land_slot_count; i++)
        {
            StaticDatas.PlayerData.FarmSlots.Add(new FarmSlotStats()
            {
                state = LandState.Empty,
                PlantDetails = new PD()
                {
                    plant = Plants.None
                }
            });
        }
        StaticDatas.SaveDatas();
        for (int i = 0; i < StaticDatas.PlayerData.FarmSlots.Count; i++)
        {
            PopulateSlots(i);
        }
        StaticDatas.SaveDatas();
        AddBuySlot();
    }

    private void PopulateSlots(int i)
    {
        GameObject dublicate = Instantiate(SlotPrefab, SlotsHolder);
        FarmingTS fts = dublicate.GetComponent<FarmingTS>();
        dublicate.name = StaticDatas.PlayerData.FarmSlots[i].PlantDetails.plant.ToString();

        fts.SlotNumber = i;
        fts.ThePlant = StaticDatas.PlayerData.FarmSlots[i].PlantDetails;
        fts.landstate = StaticDatas.PlayerData.FarmSlots[i].state;
        fts.AddSkip();
        fts.LoadUI();
        Slots.Add(dublicate);

        #region Clean Info Button
            GameObject cib = Instantiate(SlotPrefab, transform.Find("Info Buttons"));
            cib.name = StaticDatas.PlayerData.FarmSlots[i].PlantDetails.plant.ToString() + " Info Button";
        
            GameObject ib = Instantiate(Sprites.instance.InfoButtonPrefab, cib.transform);
            RectTransform ibrts = ib.GetComponent<RectTransform>();
            ibrts.anchoredPosition = new Vector2(0, 0);
            ibrts.anchorMax = new Vector2(1, 1);
            ibrts.anchorMin = new Vector2(1, 1);
            ib.GetComponent<InfoDetails>().btn.onClick.RemoveAllListeners();
            ib.GetComponent<InfoDetails>().btn.onClick.AddListener(() => ib.GetComponent<InfoDetails>().DetailsOnOff(GetAnimDirection(i), "Land", null, null, i));
            #region cleaning inside SlotPrefab
                foreach (Transform item in cib.transform) if (item.name != "Info Button(Clone)") Destroy(item.gameObject);
                Destroy(cib.GetComponent<Image>());
                Destroy(cib.GetComponent<Animator>());
            #endregion
        #endregion
    }

    private string GetAnimDirection(int i)
    {
        if (i == 28 || i == 29 || i == 30 || i == 31) return "LT";

        //if (i == 0 || i == 1 || i == 2 || i == 3) return "LB";
        else return "LB";
    }

    private void AddBuySlot()
    {
        if (StaticDatas.PlayerData.land_slot_count < 32)
        {
            GameObject buySlot = Instantiate(BuySlotPrefab, SlotsHolder);

            Button button = buySlot.GetComponent<Button>();
            button.onClick.AddListener(() => BuySlot());

            Transform price = buySlot.transform.Find("Price");
            Transform ptext = price.transform.Find("Price Text");
            TextMeshProUGUI text = ptext.GetComponent<TextMeshProUGUI>();
            text.text = p.ToString();
        }
    }

    private void BuySlot()
    {
        if (MoneySystem.instance.hasEnough(Currency.Coin, p))
        {
            StaticDatas.PlayerData.land_slot_count++;
            StaticDatas.UpdateFarmSlotDatas();
            MoneySystem.instance.UpdateCoin(-p, out bool s);
            MoneySystem.instance.UpdateXp(xp);
            p = SlotPrices[StaticDatas.PlayerData.land_slot_count - 1];
            xp = BuyXP[StaticDatas.PlayerData.land_slot_count - 1];
            Transform child = SlotsHolder.Find("Farm Buy Slot(Clone)");
            if (child != null)
            {
                Destroy(child.gameObject);
            }

            PopulateSlots(Slots.Count);
            AddBuySlot();
            LuckyBox.instance.TryToFindBox(0.2f * StaticDatas.PlayerData.land_slot_count);
        }
    }

    public void ResetSituation()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            FarmingTS fts = Slots[i].GetComponent<FarmingTS>();
            if (fts.landstate == LandState.Empty)
                fts.btn.onClick.RemoveAllListeners();
            else if (fts.landstate == LandState.Plow)
                fts.is_plowing = false;
            else if (fts.landstate == LandState.Dry)
                fts.is_watering = false;
        }
        if (canvasGroup != null) StaticDatas.AdjustCanvasGroup(canvasGroup, false);
    }
}
