using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoDetails : MonoBehaviour
{
    private Animator anim;
    private int index;
    private object item, sourceInfos;

    public Button btn;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        btn = GetComponent<Button>();
    }

    public void DetailsOnOff(string type, object i, object sinfo, int fsIndex)
    {
        anim.SetBool("Details", !anim.GetBool("Details"));
        if(type == "Item")
        {
            item = i;
            sourceInfos = sinfo;
            index = fsIndex;
            OnOffOthers("Item");
            SetItemInfos();
            foreach (Transform item in transform.Find("Info Panel")) if (item.name != "Product Infos") item.gameObject.SetActive(false); else item.gameObject.SetActive(true);
        }
        else if (type == "Land")
        {
            index = fsIndex;
            OnOffOthers("Land");
            GetLandDetails();
            foreach (Transform item in transform.Find("Info Panel")) if (item.name != "Land Infos") item.gameObject.SetActive(false); else item.gameObject.SetActive(true);
        }
    }

    public void TimerAnim()
    {
        if(item != null)
            transform.Find("Info Panel/Timer Holder/Timer Prefab").GetComponent<Animator>().SetTrigger("Spin it");
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

            if (StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == (Plants)item) != null)
                countInfo = StaticDatas.PlayerData.Storage.PlantsInStorage.Find(e => e.Plant == (Plants)item).count;
            else countInfo = 0;
        }
        else if(item is Fruits)
        {
            TreeD td = ForestLogic.instance.TreeDetails.Find(e => e.fruit == (Fruits)item);
            name = td.fruit.ToString();
            double time = 0;
            for (int i = 0; i < td.GrowthTimeByStage.Count; i++)
                time += td.GrowthTimeByStage[i];
            timeInfo = time;

            if (StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == (Fruits)item) != null)
                countInfo = StaticDatas.PlayerData.Storage.FruitInStorage.Find(e => e.Fruit == (Fruits)item).count;
            else countInfo = 0;
        }
        else if(item is AProducts)
        {
            APD apd = AnimalsLogic.instance.AnimalsDetails.Find(e => e.animal == (Animals)sourceInfos);
            name = apd.products.Find(e => e == (AProducts)item).ToString();
            int index = apd.products.IndexOf((AProducts)item);
            timeInfo = apd.prTimes[index];

            if (StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == (AProducts)item) != null)
                countInfo = StaticDatas.PlayerData.Storage.a_p_inStorage.Find(e => e.animal_products == (AProducts)item).count;
            else countInfo = 0;
        }
        else if(item is Products)
        {
            PrD prd = ProductionLogic.instance.MachineDetails.Find(e => e.MachineName == sourceInfos.ToString()).products.Find(e => e.product == (Products)item);

            name = prd.name;
            timeInfo = prd.prTimer;

            if (StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == (Products)item) != null)
                countInfo = StaticDatas.PlayerData.Storage.ProductsInStorage.Find(e => e.product == (Products)item).count;
            else countInfo = 0;
        }
        else if(item is Items)
        {
            name = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == (Items)item).ToString();
            details.Find("Timer Holder").gameObject.SetActive(false);

            if (StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == (Items)item) != null)
                countInfo = StaticDatas.PlayerData.Storage.ItemsInStorage.Find(e => e.item == (Items)item).count;
            else countInfo = 0;
        }
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
                    if (i != index && plants[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool("Details"))
                        plants[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool("Details", false);
                }
            }
            else if (item is Fruits)
            {
                List<GameObject> trees = TreeHolder.instance.Trees;
                for (int i = 0; i < trees.Count; i++)
                {
                    if (i != index && trees[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool("Details"))
                        trees[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool("Details", false);
                }
            }
            else if (item is AProducts)
            {
                List<GameObject> aprs = AHolder.instance.allProducts;
                for (int i = 0; i < aprs.Count; i++)
                {
                    if (i != index && aprs[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool("Details"))
                        aprs[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool("Details", false);
                }
            }
            else if (item is Products)
            {
                List<GameObject> products = MachinePH.instance.prs;
                for (int i = 0; i < products.Count; i++)
                {
                    if (i != index && products[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool("Details"))
                        products[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool("Details", false);
                }
            }
        }
        else if (type == "Land")
            for (int i = 0; i < FarmLogic.instance.Slots.Count; i++)
                if (i != index && FarmLogic.instance.Slots[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().GetBool("Details"))
                    FarmLogic.instance.Slots[i].transform.Find("Info Button(Clone)").GetComponent<Animator>().SetBool("Details", false);
    }

    private void GetLandDetails()
    {
        int usage = 0;
        int plowed = 0;
        int dried = 0;

        usage = StaticDatas.PlayerData.FarmSlots[index].usage;
        plowed = StaticDatas.PlayerData.FarmSlots[index].plowed;
        dried = StaticDatas.PlayerData.FarmSlots[index].dried;

        Transform LandInfos = transform.Find("Info Panel/Land Infos");

        LandInfos.transform.Find("Usage/Count").GetComponent<TextMeshProUGUI>().text = usage.ToString();
        LandInfos.transform.Find("Plowed/Count").GetComponent<TextMeshProUGUI>().text = plowed.ToString();
        LandInfos.transform.Find("Dried/Count").GetComponent<TextMeshProUGUI>().text = dried.ToString();
    }
}