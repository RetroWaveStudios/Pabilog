using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreeHolder : MonoBehaviour
{
    public static TreeHolder instance;
    public Transform Holder;
    public GameObject TreePrefab;
    public List<GameObject> Trees = new();
    public List<GameObject> L_Trees = new();

    public int slotnumber;

    private void Awake()
    {
        instance = this;
        StaticDatas.LoadDatas();
    }

    public void PopulateHodler()
    {
        foreach (Transform item in Holder) Destroy(item.gameObject);
        Debug.Log("Populate Holder Called in TH");
        Fruits[] tree_names = (Fruits[])Enum.GetValues(typeof(Fruits));

        int indexToRemove = 0;
        for (int f = 0; f < tree_names.Length; f++) if (tree_names[f] == Fruits.None) indexToRemove = f;

        Fruits[] newArr = new Fruits[tree_names.Length - 1];

        for (int i = 0, j = 0; i < tree_names.Length; i++)
        {
            if (i == indexToRemove) continue;
            newArr[j++] = tree_names[i];
        }

        if (StaticDatas.PlayerData.unlocked_items.u_fruits != null || StaticDatas.PlayerData.unlocked_items.u_fruits.Count > 0)
        {
            for (int i = 0; i < tree_names.Length; i++)
            {
                if (StaticDatas.PlayerData.unlocked_items.u_fruits.Contains(tree_names[i]))
                {
                    Fruits f = StaticDatas.PlayerData.unlocked_items.u_fruits[i];
                    Debug.Log(f + " tree added to Holder to be able to buy");
                    GameObject dublicate = Instantiate(TreePrefab, Holder);
                    dublicate.transform.name = f.ToString();

                    dublicate.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(f);
                    Button button = dublicate.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ChooseTree(f));

                    Transform ih = dublicate.transform.Find("Info Holder");
                    ih.gameObject.SetActive(true);
                    Transform price = ih.transform.Find("Price");
                    price.GetComponent<TextMeshProUGUI>().text = ForestLogic.instance.TreeDetails.Find(e => e.fruit == f).price.ToString();

                    Transform tName = ih.transform.Find("Tree Name");
                    tName.GetComponent<TextMeshProUGUI>().text = f.ToString();

                    Transform pIcon = ih.transform.Find("Icon");
                    Image pImage = pIcon.GetComponent<Image>();
                    pImage.sprite = Sprites.instance.GetSpriteFromSource(ForestLogic.instance.TreeDetails.Find(e => e.fruit == f).currency);

                    Trees.Add(dublicate);
                }
                else
                {
                    Fruits f = tree_names[i];
                    GameObject dublicate = Instantiate(Sprites.instance.LockedHolderPrefab, Holder);
                    dublicate.transform.name = "locked " + f.ToString();

                    dublicate.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(f);
                    dublicate.GetComponent<Image>().color = new Color32(77, 77, 77, 255);

                    int level = 0;
                    for (int l = 0; l < PlayerProfile.instance.rewards.Count; l++)
                        if (PlayerProfile.instance.rewards[l].Trees.Contains(f)) level = l;

                    Debug.Log($"locked: working on Tree {f} and level = {level}");
                    dublicate.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = "Lvl " + level;

                    L_Trees.Add(dublicate);
                }
            }
        }
    }

    private void ChooseTree(Fruits t)
    {
        for (int i = 0; i < ForestLogic.instance.Slots.Count; i++)
        {
            TreeSlot ts = ForestLogic.instance.Slots[i].GetComponent<TreeSlot>();
            if (ts.SlotNumber == slotnumber)
            {
                ts.PickATree(t);
            }
        }
    }
}
