using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProductionLogic : MonoBehaviour
{
    public static ProductionLogic instance;
    private CanvasGroup canvasGroup;
    public List<MachineProduct> MachineDetails;

    public GameObject machPrefab;
    public List<GameObject> Machines;
    public List<Sprite> MachinesSprites;

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
        Machines.Clear();
        if (StaticDatas.PlayerData.unlocked_items.u_machines != null || StaticDatas.PlayerData.unlocked_items.u_machines.Count > 0)
            for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_machines.Count; i++)
            {
                string m = StaticDatas.PlayerData.unlocked_items.u_machines[i];
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

                Machines.Add(dublicate);
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
        for (int i = 0; i < Machines.Count; i++)
        {
            Machines[i].GetComponent<Machine>().prChoosed = false;
            Machines[i].GetComponent<Machine>().LoadUI();
        }
    }
}