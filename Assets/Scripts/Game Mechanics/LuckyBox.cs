using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LuckyBox : MonoBehaviour
{
    public static LuckyBox instance;

    public float baseChance = 0f;       // Starting chance
    public float chanceIncrease = 0.25f; // Per fail
    public float maxChance = 65f;       // Hard cap
    public float currentChance = 0f;

    public TextMeshProUGUI chanceText;

    public List<ItemCount> ItProPers; // Item Probability Persentage
    public List<Items> itemPool;
    public List<float> openedNumbers;
    public List<Items> openedItems;

    public List<ItemCount> repeatedTimes;

    public Items TheItem = Items.None;
    public Image itemImage;

    public int openedBoxs = 0;

    private Animator anim;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        chanceText.text = Math.Round(currentChance, 2).ToString();
        chanceText.color = Color.white;
        anim = GetComponent<Animator>();
        currentChance = StaticDatas.PlayerData.PlayerInfos.currentChanceOfLB;
        BuildItemPool();
    }

    private void BuildItemPool()
    {
        itemPool = new List<Items>();
        foreach (var it in ItProPers)
            for (int i = 0; i < it.count; i++)
                itemPool.Add(it.item);
        StaticDatas.Shuffle(itemPool);
    }

    /*
    public void RepeatIntTimes()
    {
        openedNumbers.Clear();
        for (int i = 0; i < openedBoxs; i++)
        {
            OpenIntTimes();
        }
        openedNumbers.Sort((a, b) => b.CompareTo(a));
        openedItems.Sort((a, b) => a.ToString().CompareTo(b.ToString()));
        for (int i = 0; i < openedItems.Count; i++)
        {
            if (repeatedTimes.Find(e => e.item == openedItems[i]) != null) repeatedTimes.Find(e => e.item == openedItems[i]).count++;
            else
            {
                repeatedTimes.Add(new ItemCount()
                {
                    item = openedItems[i],
                    count = 1
                });
            }
        }
        string numbers = string.Join("\n", openedNumbers);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "numbers.txt"), numbers);
    }

    public void OpenIntTimes()
    {
        for (int i = 0; i < 1000; i++)
        {
            TryToFindBox();
        }
    }*/

    public void TryToFindBox(float increas)
    {
        float chance = UnityEngine.Random.value * 100;
        currentChance = Mathf.Min(currentChance + increas, maxChance);

        if (chance > 14f && chance < currentChance)
        {
            openedNumbers.Add(currentChance);
            Debug.Log($"opened at {currentChance} try");
            SetBox();
        }

        chanceText.text = Math.Round(currentChance, 2).ToString();
        chanceText.color = Color.white;
        StaticDatas.PlayerData.PlayerInfos.currentChanceOfLB = currentChance;
        StaticDatas.SaveDatas();
    }

    private void SetBox()
    {
        StaticDatas.Shuffle(itemPool);
        PickAItem();
        Debug.Log("Box Opened");
        chanceText.text = currentChance.ToString();
        chanceText.color = Color.green;
        currentChance = baseChance;
        StaticDatas.PlayerData.PlayerInfos.currentChanceOfLB = currentChance;
        StaticDatas.SaveDatas();
    }

    private void PickAItem()
    {
        TheItem = itemPool[UnityEngine.Random.Range(0, itemPool.Count)];
        openedItems.Add(TheItem);
        anim.SetTrigger("Drop Box");
    }

    public void OpenTheBox()
    {
        itemImage.sprite = Sprites.instance.sprites.items.Find(e => e.item == TheItem).sprite;
        anim.SetTrigger("Open Box");
    }

    public void TakeItem()
    {
        anim.SetTrigger("Close Screen");
        Storage.instance.UpdateThingCount(TheItem, 1);
        StaticDatas.SaveDatas();
    }
}