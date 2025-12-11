using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LuckyBox : MonoBehaviour
{
    public float baseChance = 5f;       // Starting chance
    public float chanceIncrease = 0.3f; // Per fail
    public float maxChance = 65f;       // Hard cap
    public float currentChance = 0f;

    public List<ItemCount> ItProPers; // Item Probability Persentage
    public List<Items> itemPool;
    public List<float> openedNumbers;
    public List<Items> openedItems;

    public List<ItemCount> repeatedTimes;

    public Items TheItem;
    public Image itemImage;

    public int openedBoxs = 0;

    private void Awake()
    {
        BuildItemPool();
    }

    private void BuildItemPool()
    {
        itemPool = new List<Items>();
        foreach (var it in ItProPers)
        {
            for (int i = 0; i < it.count; i++)
                itemPool.Add(it.item);
        }
        Shuffle(itemPool);
    }

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
    }
    public bool TryToFindBox()
    {
        if (Random.value < currentChance * 0.01f)
        {
            openedNumbers.Add(currentChance);
            Debug.Log($"opened at {currentChance} try");
            SetBox();
            return true;
        }

        currentChance = Mathf.Min(currentChance + chanceIncrease, maxChance);
        return false;
    }

    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
    private void SetBox()
    {
        Shuffle(itemPool);
        PickAItem();
        Debug.Log("Box Opened");
        currentChance = baseChance;
    }

    private void PickAItem()
    {
        TheItem = itemPool[Random.Range(0, itemPool.Count)];
        itemImage.sprite = Sprites.instance.sprites.items.Find(e => e.item == TheItem).sprite;
        openedItems.Add(TheItem);
    }
}