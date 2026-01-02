using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachinePH : MonoBehaviour
{
    public static MachinePH instance;
    public MachineProduct mp;
    public int mnumber;

    public Transform Holder;
    public GameObject prPrefab;
    public List<GameObject> prs;

    private void Awake()
    {
        instance = this;
    }
    public void PopulateHolder()
    {
        prs = new();
        foreach(Transform item in Holder) Destroy(item.gameObject);
        for (int i = 0; i < mp.products.Count; i++)
        {
            if (StaticDatas.PlayerData.unlocked_items.u_Products.Find(e => e.Product == mp.products[i].product) != null &&
                StaticDatas.PlayerData.unlocked_items.u_Products.Find(e => e.Product == mp.products[i].product).owned &&
                Sprites.instance.sprites.products.Find(e => e.product == mp.products[i].product) != null)
            {
                Products p = mp.products[i].product;
                Debug.Log("product is " + p);
                GameObject dublicate = Instantiate(prPrefab, Holder);
                Image image = dublicate.GetComponent<Image>();
                image.sprite = Sprites.instance.sprites.products.Find(e => e.product == p).sprite;
                dublicate.transform.name = p.ToString();
                dublicate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                dublicate.transform.Find("Details").gameObject.SetActive(true);
                dublicate.transform.Find("Details").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
                dublicate.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20, 0);
                dublicate.transform.Find("Details/Time Info").gameObject.SetActive(true);
                dublicate.transform.Find("Details/Time Info").GetComponent<TextMeshProUGUI>().text = mp.products.Find(e => e.product == p).prTimer.ToString() + " min";

                Button button = dublicate.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ProductionLogic.instance.Machines.Find(e => e.name == mp.MachineName).GetComponent<Machine>().PickProduct(mp.products.Find(e => e.product == p).Clone()));

                #region Info Button detailing
                    GameObject ib = Instantiate(Sprites.instance.InfoButtonPrefab, dublicate.transform);
                    RectTransform ibrts = ib.GetComponent<RectTransform>();
                    ibrts.anchoredPosition = new Vector2(0, 10);
                    ibrts.anchorMax = new Vector2(0.5f, 1);
                    ibrts.anchorMin = new Vector2(0.5f, 1);

                    int index = i;
                    ib.GetComponent<InfoDetails>().btn.onClick.RemoveAllListeners();
                    ib.GetComponent<InfoDetails>().btn.onClick.AddListener(() => ib.GetComponent<InfoDetails>().DetailsOnOff("CT", "Item", p, mp.MachineName, index));
                #endregion
                prs.Add(dublicate);
            }
        }
    }
}
