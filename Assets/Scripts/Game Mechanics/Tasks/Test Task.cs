using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestTask : MonoBehaviour
{
    private Button btn;
    public Task task;
    public GameObject reqPrefab;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => GetTask());
        btn.onClick.AddListener(() => PopulateInfoHolder());
    }

    public void GetTask()
    {
        task = TasksLogic.instance.SendRequest(TaskType.Hard, TasksLogic.instance.totalPoints[TaskType.Hard]);
        SetDetails();
    }

    public void PopulateInfoHolder()
    {
        Transform ih = transform.parent.parent.Find("Info Holder");

        ih.Find("Difficulty").GetComponent<TextMeshProUGUI>().text = task.difficulty.ToString();

        ih.Find("Money/Icon").GetComponent<Image>().sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == task.currency).sprite;
        ih.Find("Money/Reward").GetComponent<TextMeshProUGUI>().text = task.Money.ToString();
        ih.Find("Xp/Reward").GetComponent<TextMeshProUGUI>().text = task.Xp.ToString();

        #region Filling Reqs Holder
        foreach (Transform item in ih.Find("Reqs")) Destroy(item.gameObject);
        // fill required plants
        if (task.Plants != null || task.Plants.Count > 0)
        {
            for (int i = 0; i < task.Plants.Count; i++)
            {
                GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 10);
                req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -7);

                req.GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == task.Plants[i].Plant).sprite;
                req.transform.Find("Details/Count").gameObject.SetActive(true);

                if (Storage.instance.hasEnought(task.Plants[i].Plant, task.Plants[i].count, false))
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = task.Plants[i].count.ToString() + " / " + task.Plants[i].count.ToString();
                else
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(task.Plants[i].Plant) + " / " + task.Plants[i].count.ToString();
                Destroy(req.GetComponent<Button>());
            }
        }

        // fill required fruits
        if (task.Fruits != null || task.Fruits.Count > 0)
        {
            for (int i = 0; i < task.Fruits.Count; i++)
            {
                GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 10);
                req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -7);

                req.GetComponent<Image>().sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == task.Fruits[i].Fruit).sprite;
                req.transform.Find("Details/Count").gameObject.SetActive(true);

                if (Storage.instance.hasEnought(task.Fruits[i].Fruit, task.Fruits[i].count, false))
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = task.Fruits[i].count.ToString() + " / " + task.Fruits[i].count.ToString();
                else
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(task.Fruits[i].Fruit) + " / " + task.Fruits[i].count.ToString();
                Destroy(req.GetComponent<Button>());
            }
        }

        // fill required Animal Foods
        if (task.AnimalFoods != null || task.AnimalFoods.Count > 0)
        {
            for (int i = 0; i < task.AnimalFoods.Count; i++)
            {
                GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 10);
                req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -7);

                req.GetComponent<Image>().sprite = Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food == task.AnimalFoods[i].food).sprite;
                req.transform.Find("Details/Count").gameObject.SetActive(true);

                if (Storage.instance.hasEnought(task.AnimalFoods[i].food, task.AnimalFoods[i].amount, false))
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = task.AnimalFoods[i].amount.ToString() + " / " + task.AnimalFoods[i].amount.ToString();
                else
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(task.AnimalFoods[i].food) + " / " + task.AnimalFoods[i].amount.ToString();
                Destroy(req.GetComponent<Button>());
            }
        }

        // fill required Animal Products
        if (task.AnimalProducts != null || task.AnimalProducts.Count > 0)
        {
            for (int i = 0; i < task.AnimalProducts.Count; i++)
            {
                GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 10);
                req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -7);

                req.GetComponent<Image>().sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == task.AnimalProducts[i].animal_products).sprite;
                req.transform.Find("Details/Count").gameObject.SetActive(true);

                if (Storage.instance.hasEnought(task.AnimalProducts[i].animal_products, task.AnimalProducts[i].count, false))
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = task.AnimalProducts[i].count.ToString() + " / " + task.AnimalProducts[i].count.ToString();
                else
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(task.AnimalProducts[i].animal_products) + " / " + task.AnimalProducts[i].count.ToString();
                Destroy(req.GetComponent<Button>());
            }
        }

        // fill required Products
        if (task.Products != null || task.Products.Count > 0)
        {
            for (int i = 0; i < task.Products.Count; i++)
            {
                GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 10);
                req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -7);

                req.GetComponent<Image>().sprite = Sprites.instance.sprites.products.Find(e => e.product == task.Products[i].product).sprite;
                req.transform.Find("Details/Count").gameObject.SetActive(true);

                if (Storage.instance.hasEnought(task.Products[i].product, task.Products[i].count, false))
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = task.Products[i].count.ToString() + " / " + task.Products[i].count.ToString();
                else
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(task.Products[i].product) + " / " + task.Products[i].count.ToString();
                Destroy(req.GetComponent<Button>());
            }
        }

        // fill required Items
        if (task.Items != null || task.Items.Count > 0)
        {
            for (int i = 0; i < task.Items.Count; i++)
            {
                GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 10);
                req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -7);

                req.GetComponent<Image>().sprite = Sprites.instance.sprites.items.Find(e => e.item == task.Items[i].item).sprite;
                req.transform.Find("Details/Count").gameObject.SetActive(true);

                if (Storage.instance.hasEnought(task.Items[i].item, task.Items[i].count, false))
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = task.Items[i].count.ToString() + " / " + task.Items[i].count.ToString();
                else
                    req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = Storage.instance.GetCountOf(task.Items[i].item) + " / " + task.Items[i].count.ToString();
                Destroy(req.GetComponent<Button>());
            }
        }
        #endregion

        CheckReqs(out bool met);
        if (met)
        {
            ih.Find("Send").GetComponent<Button>().onClick.RemoveAllListeners();
            ih.Find("Send").GetComponent<Button>().onClick.AddListener(() => CompleteTask());
        }
    }

    public void SetDetails()
    {
        transform.Find("Money/Icon").GetComponent<Image>().sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == task.currency).sprite;

        transform.Find("Money/Reward").GetComponent<TextMeshProUGUI>().text = task.Money.ToString();
        transform.Find("Xp/Reward").GetComponent<TextMeshProUGUI>().text = task.Xp.ToString();
    }

    private void CheckReqs(out bool met)
    {
        met = false;
        bool[] has = new bool[6] { true, true, true, true, true, true }; bool hasAll = true;
        if (task.Plants != null || task.Plants.Count > 0)
        {
            for (int i = 0; i < task.Plants.Count; i++)
                if (!Storage.instance.hasEnought(task.Plants[i].Plant, task.Plants[i].count, false)) has[0] = false;
        }
        else has[0] = false;
        if (task.Fruits != null || task.Fruits.Count > 0)
        {
            for (int i = 0; i < task.Fruits.Count; i++)
                if (!Storage.instance.hasEnought(task.Fruits[i].Fruit, task.Fruits[i].count, false)) has[1] = false;
        }
        else has[1] = false;


        if (task.AnimalProducts != null || task.AnimalProducts.Count > 0)
        {
            for (int i = 0; i < task.AnimalProducts.Count; i++)
                if (!Storage.instance.hasEnought(task.AnimalProducts[i].animal_products, task.AnimalProducts[i].count, false)) has[2] = false;
        }
        else has[2] = false;

        if (task.AnimalFoods != null || task.AnimalFoods.Count > 0)
        {
            for (int i = 0; i < task.AnimalFoods.Count; i++)
                if (!Storage.instance.hasEnought(task.AnimalFoods[i].food, task.AnimalFoods[i].amount, false)) has[3] = false;
        }
        else has[3] = false;

        if (task.Products != null || task.Products.Count > 0)
        {
            for (int i = 0; i < task.Products.Count; i++)
                if (!Storage.instance.hasEnought(task.Products[i].product, task.Products[i].count, false)) has[4] = false;
        }
        else has[4] = false;

        if (task.Items != null || task.Items.Count > 0)
        {
            for (int i = 0; i < task.Items.Count; i++)
                if (!Storage.instance.hasEnought(task.Items[i].item, task.Items[i].count, false)) has[5] = false;
        }
        else has[5] = false;

        for (int i = 0; i < has.Length; i++) if (!has[i]) { Debug.Log("has " + i + has[i]); hasAll = false; }

        if (hasAll)
        {
            transform.GetComponent<Image>().color = new Color32(20, 160, 0, 255);
            met = true;
        }
    }

    private void CompleteTask()
    {
        Debug.Log($"Task Completed. Rewards:\nMoney: {task.Money}\nXp: {task.Xp}");
    }
}
