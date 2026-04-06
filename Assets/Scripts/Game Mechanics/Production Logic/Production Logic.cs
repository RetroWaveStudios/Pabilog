using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionLogic : MonoBehaviour
{
    public static ProductionLogic instance;
    private CanvasGroup canvasGroup;
    public List<MachineProduct> MachineDetails;

    public GameObject machPrefab;
    public GameObject buyMachPrefab;
    public List<GameObject> ActiveMachines;
    public List<Sprite> MachinesSprites;

    public Dictionary<Machines, int> MachinePrices = new Dictionary<Machines, int>()
    {
        { Machines.Oven, 500 }, { Machines.DairyChurn, 2500 }, { Machines.Presser, 5000 }, { Machines.PopcornMachine, 10000 },
        { Machines.SewingMachine, 25000 }, { Machines.PieOven, 40000 }
    };

    private void Awake()
    {
        StaticDatas.LoadDatas();
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        PopulateMachines();
    }

    public void PopulateMachines()
    {
        foreach (Transform machine in transform.Find("Machinary")) Destroy(machine.gameObject);
        ActiveMachines.Clear();
        if (StaticDatas.PlayerData.unlocked_items.u_machines != null || StaticDatas.PlayerData.unlocked_items.u_machines.Count > 0)
            for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_machines.Count; i++)
            {
                string m = StaticDatas.PlayerData.unlocked_items.u_machines[i].MachineName;

                if (!StaticDatas.PlayerData.unlocked_items.u_machines[i].bought)
                {
                    GameObject dublicate = Instantiate(buyMachPrefab, transform.Find("Machinary"));
                    dublicate.transform.Find("Machine Image").GetComponent<Image>().sprite = MachinesSprites.Find(e => e.name == m + " Image");

                    dublicate.transform.Find("Buy Screen/Buying/Details/Price").GetComponent<TextMeshProUGUI>().text = MachinePrices[StaticDatas.PlayerData.unlocked_items.u_machines.Find(e => e.MachineName == m).machEnum].ToString();

                    Debug.Log(StaticDatas.PlayerData.unlocked_items.u_machines[i].machEnum);
                    int index = i;
                    dublicate.transform.Find("Buy Screen/Buying/Details").GetComponent<Button>().onClick.RemoveAllListeners();
                    dublicate.transform.Find("Buy Screen/Buying/Details").GetComponent<Button>().onClick.AddListener(() => BuyTheMachine(StaticDatas.PlayerData.unlocked_items.u_machines[index].machEnum));

                    dublicate.transform.Find("Buy Screen/Buying/Buy Icon").GetComponent<Button>().onClick.RemoveAllListeners();
                    dublicate.transform.Find("Buy Screen/Buying/Buy Icon").GetComponent<Button>().onClick.AddListener(() => BuyTheMachine(StaticDatas.PlayerData.unlocked_items.u_machines[index].machEnum));
                }
                else
                {
                    if (StaticDatas.PlayerData.MachineStats.Find(e => e.MachineName == m) == null)
                    {
                        MachineStats ms = new MachineStats()
                        {
                            MachineName = m,
                            state = ASpotState.Empty,
                            qLimit = 1,
                        };
                        StaticDatas.PlayerData.MachineStats.Add(ms);
                        StaticDatas.SaveDatas();
                    }


                    GameObject dublicate = Instantiate(machPrefab, transform.Find("Machinary"));
                    Machine machine = dublicate.GetComponent<Machine>();

                    var stats = StaticDatas.PlayerData.MachineStats.Find(e => e.MachineName == m);
                    MachineProduct details = MachineDetails.Find(e => e.MachineName == m);
                    if (stats.qLimit == 0) stats.qLimit = 1;
                    StaticDatas.SaveDatas();
                    PrD product = (stats.queue != null && stats.queue.Count > 0)
                        ? stats.queue[0]
                        : new PrD() { state = AState.None, product = Products.None };

                    machine.Init(product, stats, details);

                    ActiveMachines.Add(dublicate);
                }
            }
    }

    private void BuyTheMachine(Machines machEnum)
    {
        MoneySystem.instance.UpdateCoin(-MachinePrices[machEnum], out bool enought);
        if (enought)
        {
            StaticDatas.PlayerData.unlocked_items.u_machines.Find(e => e.machEnum == machEnum).bought = true;
            StaticDatas.SaveDatas();
            PopulateMachines();
        }
    }

    public void ResetSituation()
    {
        foreach (Transform item in MachinePH.instance.Holder) Destroy(item.gameObject);
        DeSelectProductAtAllMachines();
        if (canvasGroup != null) StaticDatas.AdjustCanvasGroup(canvasGroup, false);
    }

    public void DeSelectProductAtAllMachines()
    {
        for (int i = 0; i < ActiveMachines.Count; i++)
        {
            ActiveMachines[i].GetComponent<Machine>().prChoosed = false;
            ActiveMachines[i].GetComponent<Machine>().LoadUI();
        }
    }
}