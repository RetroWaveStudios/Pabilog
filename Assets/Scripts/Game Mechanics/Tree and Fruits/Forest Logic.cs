using System.Collections.Generic;
using UnityEngine;

public class ForestLogic : MonoBehaviour
{
    public static ForestLogic instance;
    public List<TreeD> TreeDetails;
    public int maxSlotCount = 32;

    [Header("UI Elements")]
    public GameObject SlotPrefab;
    public List<GameObject> Slots;
    public Transform SlotsHolder;

    public Dictionary<Fruits, int> TreePrice = new Dictionary<Fruits, int>()
    {
        { Fruits.Apple, 250 },      { Fruits.Cherry, 500 },     { Fruits.Berry, 400 },
        { Fruits.Banana, 1000 },    { Fruits.Orange, 1500 },    { Fruits.Cacao, 2000 },
    };

    private void Awake()
    {
        instance = this;
        StaticDatas.LoadDatas();
        if(StaticDatas.PlayerData.TreeSpots == null)
        {
            StaticDatas.PlayerData.TreeSpots.Add(new TreeSpotStats()
            {
                state = LandState.Empty,
                TreeDetails = new TreeD()
                {
                    fruit = Fruits.None,
                    state = PlantState.None,
                    usage = 0
                }
            });
        }
        for (int i = StaticDatas.PlayerData.TreeSpots.Count; i < 32; i++)
        {
            StaticDatas.PlayerData.TreeSpots.Add(new TreeSpotStats()
            {
                state = LandState.Empty,
                TreeDetails = new TreeD()
                {
                    fruit = Fruits.None,
                    state = PlantState.None,
                    usage = 0
                }
            });
        }
        StaticDatas.SaveDatas();
        PopulateForest();
    }

    private void PopulateForest()
    {
        for (int i = 0; i < maxSlotCount; i++)
        {
            GameObject dublicate = Instantiate(SlotPrefab, SlotsHolder);
            TreeSlot ts = dublicate.GetComponent<TreeSlot>();
            ts.TheTree = StaticDatas.PlayerData.TreeSpots[i].TreeDetails;
            ts.transform.name = ts.TheTree.fruit.ToString();
            ts.landstate = StaticDatas.PlayerData.TreeSpots[i].state;
            ts.btn.onClick.RemoveAllListeners();
            ts.btn.onClick.AddListener(() => ts.SlotClicked());
            ts.SlotNumber = i;
            ts.LoadUI();
            ts.UpdateFruitImages();
            Slots.Add(dublicate);
        }
    }
}
