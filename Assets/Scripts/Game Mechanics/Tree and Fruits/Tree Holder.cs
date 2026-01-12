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
        if(StaticDatas.PlayerData.unlocked_items.u_fruits != null || StaticDatas.PlayerData.unlocked_items.u_fruits.Count > 0)
            for (int i = 0; i < StaticDatas.PlayerData.unlocked_items.u_fruits.Count; i++)
            {
                Fruits f = StaticDatas.PlayerData.unlocked_items.u_fruits[i];
                Debug.Log(f + " tree added to Holder to be able to buy");
                GameObject dublicate = Instantiate(TreePrefab, Holder);
                dublicate.transform.name = f.ToString();

                dublicate.GetComponent<Image>().sprite = Sprites.instance.sprites.trees.Find(e => e.fruit == f).sprite;
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
                pImage.sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == ForestLogic.instance.TreeDetails.Find(e => e.fruit == f).currency).sprite;

                Trees.Add(dublicate);
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
