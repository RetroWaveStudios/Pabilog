using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoDetails : MonoBehaviour
{
    private Animator anim;
    private string aDr;
    private int index;
    private object item, sourceInfos;

    public Button btn;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        btn = GetComponent<Button>();
    }

    public void DetailsOnOff(string direction, string type, object i, object sinfo, int fsIndex)
    {
        //Debug.Log($"{type} info loaded:\ni = {i}\nsinfo = {sinfo}\nfsIndex = {fsIndex}");
        anim.SetBool(direction, !anim.GetBool(direction));
        aDr = direction;
        item = i;
        sourceInfos = sinfo;
        index = fsIndex;
        if (type == "Item")
        {
            OnOffOthers("Item");
            SetItemInfos();
            foreach (Transform item in transform.Find("Info Panel")) if (item.name != "Product Infos") item.gameObject.SetActive(false); else item.gameObject.SetActive(true);
        }
        else if (type == "Land")
        {
            OnOffOthers("Land");
            GetLandDetails();
            foreach (Transform item in transform.Find("Info Panel")) if (item.name != "Land Infos") item.gameObject.SetActive(false); else item.gameObject.SetActive(true);
        }
        else if (type == "Machine")
        {
            OnOffOthers("Machine");
            GetMachineDetails();
            foreach (Transform item in transform.Find("Info Panel")) if (item.name != "Machine Infos") item.gameObject.SetActive(false); else item.gameObject.SetActive(true);
        }
    }

    public void TimerAnim()
    {
        if(item != null)
            transform.Find("Info Panel/Product Infos/Timer Holder/Timer Prefab").GetComponent<Animator>().SetTrigger("Spin it");
    }

    private void SetItemInfos()
    {
        Transform details = transform.Find("Info Panel/Product Infos");
        string name = "";
        double timeInfo = 0;
        double countInfo = 0;
        if (item is Plants)
        {
            name = FarmLogic.instance.PlantDetails.Find(e => e.plant == (Plants)item).plant.ToString();
            timeInfo = FarmLogic.instance.PlantDetails.Find(e => e.plant == (Plants)item).GrowthTime;
        }
        else if(item is Fruits)
        {
            TreeD td = ForestLogic.instance.TreeDetails.Find(e => e.fruit == (Fruits)item);
            name = td.fruit.ToString();
            double time = 0;
            for (int i = 0; i < td.GrowthTimeByStage.Count; i++)
                time += td.GrowthTimeByStage[i];
            timeInfo = time;
        }
        else if(item is AProducts)
        {
            APD apd = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == (Animals)sourceInfos);
            name = apd.products.Find(e => e == (AProducts)item).ToString();
            int index = apd.products.IndexOf((AProducts)item);
            timeInfo = apd.prTimes[index];
        }
        else if(item is Products)
        {
            PrD prd = ProductionLogic.instance.MachineDetails.Find(e => e.MachineName == sourceInfos.ToString()).products.Find(e => e.product == (Products)item);

            name = prd.name;
            timeInfo = prd.prTimer;
        }
        else if(item is Items)
        {
            name = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == (Items)item).ToString();
            details.Find("Timer Holder").gameObject.SetActive(false);
        }

        countInfo = Storage.instance.GetCountOf(item);

        timeInfo = timeInfo * 60;
        details.Find("Name Holder/Name").GetComponent<TextMeshProUGUI>().text = name;
        TimeSpan remaining = TimeSpan.FromSeconds(timeInfo);
        string timeString;
        if(remaining.Hours > 0) timeString = string.Format("{0:D2}:{1:D2}:{2:D2}", remaining.Hours, remaining.Minutes, remaining.Seconds);
        else timeString  = string.Format("{0:D2}:{1:D2}", remaining.Minutes, remaining.Seconds);
        details.Find("Timer Holder/Time").GetComponent<TextMeshProUGUI>().text = timeString;
        details.Find("Storage Count/Count").GetComponent<TextMeshProUGUI>().text = countInfo.ToString();
    }

    private void OnOffOthers(string type)
    {
        if (type == "Item")
        {
            if (item is Plants)
            {
                List<GameObject> plants = PlantsHolder.instance.PlantsInPH;
                for (int i = 0; i < plants.Count; i++)
                {
                    if (i != index && plants[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool(aDr))
                        plants[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool(aDr, false);
                }
            }
            else if (item is Fruits)
            {
                List<GameObject> trees = TreeHolder.instance.Trees;
                for (int i = 0; i < trees.Count; i++)
                {
                    if (i != index && trees[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool(aDr))
                        trees[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool(aDr, false);
                }
            }
            else if (item is AProducts)
            {
                List<GameObject> aprs = AHolder.instance.allProducts;
                for (int i = 0; i < aprs.Count; i++)
                {
                    if (i != index && aprs[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool(aDr))
                        aprs[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool(aDr, false);
                }
            }
            else if (item is Products)
            {
                List<GameObject> products = MachinePH.instance.prs;
                for (int i = 0; i < products.Count; i++)
                {
                    if (i != index && products[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool(aDr))
                        products[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool(aDr, false);
                }
            }
        }
        else if (type == "Land")
            for (int i = 0; i < FarmLogic.instance.Slots.Count; i++)
            {
                Transform main = FarmLogic.instance.transform.Find("Info Buttons");
                if (i != index && main.GetChild(i).Find("Info Button(Clone)").GetComponent<Animator>().GetBool(aDr))
                    main.GetChild(i).Find("Info Button(Clone)").GetComponent<Animator>().SetBool(aDr, false);
            }
        else if (type == "Machine")
            for (int i = 0; i < ProductionLogic.instance.ActiveMachines.Count; i++)
            {
                Transform main = ProductionLogic.instance.ActiveMachines[i].transform.Find("Name/Info Button");
                if (main == null) Debug.Log("Machine info button not found");
                if (ProductionLogic.instance.ActiveMachines[i].name != sourceInfos.ToString() && main.GetComponent<Animator>().GetBool(aDr))
                    main.GetComponent<Animator>().SetBool(aDr, false);
            }
    }

    private void GetLandDetails()
    {
        Transform LandInfos = transform.Find("Info Panel/Land Infos");
        LandInfos.transform.Find("Usage/Count").GetComponent<TextMeshProUGUI>().text = StaticDatas.PlayerData.FarmSlots[index].usage.ToString();
        LandInfos.transform.Find("Plowed/Count").GetComponent<TextMeshProUGUI>().text = StaticDatas.PlayerData.FarmSlots[index].plowed.ToString();
        LandInfos.transform.Find("Dried/Count").GetComponent<TextMeshProUGUI>().text = StaticDatas.PlayerData.FarmSlots[index].dried.ToString();
    }

    private void GetMachineDetails()
    {
        Debug.Log($"sourceInfo = {sourceInfos}");
        Transform LandInfos = transform.Find("Info Panel/Machine Infos");
        LandInfos.transform.Find("Usage/Count").GetComponent<TextMeshProUGUI>().text = StaticDatas.PlayerData.MachineStats.
            Find(e => e.MachineName == sourceInfos.ToString()).usage.ToString();
        LandInfos.transform.Find("Fixed/Count").GetComponent<TextMeshProUGUI>().text = StaticDatas.PlayerData.MachineStats.
            Find(e => e.MachineName == sourceInfos.ToString()).Fixed.ToString();
    }
}