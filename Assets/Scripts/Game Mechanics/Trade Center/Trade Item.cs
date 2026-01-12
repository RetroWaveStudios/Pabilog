using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeItem : MonoBehaviour
{
    public string place = "";
    public object item;
    public int price;
    public int TotalPrice;
    public int count;

    public int TradeCount = 1;

    public bool chosen = true;

    public void GetDetails(object i, int price, int count)
    {
        this.item = i; this.price = price; this.count = count;
        if(place == "market")
            transform.Find("Button Cover/Count").gameObject.SetActive(false);
        UpdateState();
        SetDetials();
    }

    private void SetDetials()
    {
        transform.Find("Button Cover/The Item").GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(item);
        transform.Find("Button Cover/Price Holder/Price").GetComponent<TextMeshProUGUI>().text = price.ToString();
        transform.Find("Button Cover/Count").GetComponent<TextMeshProUGUI>().text = count.ToString();

        ChangeTradeCount(0);
    }

    public void ChangeTradeCount(int amount)
    {
        Transform chd = transform.Find("Count Changer/Dec");
        Transform chi = transform.Find("Count Changer/Inc");
        chd.gameObject.SetActive(true);
        chi.gameObject.SetActive(true);
        TradeCount += amount;
        if (TradeCount <= 1)
        {
            chd.gameObject.SetActive(false);
            TradeCount = 1;
        }
        if (place == "yi" && TradeCount >= count)
        {
            chi.gameObject.SetActive(false);
            TradeCount = count;
        }

        if(place == "market" && (TradeCenter.instance.TotalMoneyOnDeal - TradeCenter.instance.TotalMoneyOnMarket) < TotalPrice && TradeCount <= 1)
        {
            chd.gameObject.SetActive(false);
            chi.gameObject.SetActive(false);
            TradeCount = 1;
        }
        TotalPrice = price * TradeCount;

        transform.Find("Count Changer/Count").GetComponent<TextMeshProUGUI>().text = TradeCount.ToString();

        ThingCount ic = new ThingCount()
        {
            item = item,
            count = TradeCount
        };

        TradeCenter.instance.UpdateItemCount(ic, place, chosen);
    }

    public void UpdateState()
    {
        chosen = !chosen;
        transform.Find("Count Changer").gameObject.SetActive(chosen);
        if (!chosen) { TradeCount = 1; TotalPrice = price * TradeCount; }
        ChangeTradeCount(0);
    }
}
