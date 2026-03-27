using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TasksLogic : MonoBehaviour
{
    public static TasksLogic instance;
    public Animator anim;
    private static string filePath;
    private static TaskSystem default_taskList = new TaskSystem()
    {
        Tasks = new List<TaskDetails>()
        {
            new TaskDetails()
            {
                state = TaskState.HasTask,
                timeToSpawn = 30f,
                Task = new Task()
                {
                    Plants = new List<PlantCount>()
                    {
                        new PlantCount()
                        {
                            Plant = Plants.Wheat,
                            count = 2
                        }
                    },
                    currency = Currency.Coin,
                    Money = 20,
                    Xp = 5,
                }
            }
        }
    };
    public List<TasksAlgorithm> Parameters;
    
    public TaskSystem TaskList = new();

    public GameObject taskPrefab;
    public Transform Holder;
    public List<GameObject> tasks;

    public GameObject reqPrefab;

    private Dictionary<TaskType, int> TaskPoint = new Dictionary<TaskType, int>()
    {
        { TaskType.Easy, 10 },   { TaskType.Medium, 20 },  { TaskType.Hard, 50 },   { TaskType.Legendary, 100 },
    };

    private Dictionary<int, TaskType> PointTask = new Dictionary<int, TaskType>()
    {
        { 10, TaskType.Easy },   { 20, TaskType.Medium },  { 50, TaskType.Hard },   { 100, TaskType.Legendary },
    };

    public Dictionary<TaskType, int> totalPoints = new Dictionary<TaskType, int>()
    {
        { TaskType.Easy, 30 },   { TaskType.Medium, 50 },  { TaskType.Hard, 100 },   { TaskType.Legendary, 100 },
    };

    public Dictionary<TaskType, Dictionary<Category, int>> chanceRateForDiff = new Dictionary<TaskType, Dictionary<Category, int>>()
    {
        { TaskType.Easy, new Dictionary<Category, int>() { { Category.Plants, 35 }, { Category.Fruits, 25 }, { Category.AnimalFood, 10 }, { Category.AProducts, 15 }, { Category.Products, 15 } }},
        { TaskType.Medium, new Dictionary<Category, int>() { { Category.Plants, 25 }, { Category.Fruits, 20 }, { Category.AnimalFood, 15 }, { Category.AProducts, 15 }, { Category.Products, 15 }, { Category.Items, 10 }, }},
        { TaskType.Hard, new Dictionary<Category, int>() { { Category.Plants, 15 }, { Category.Fruits, 15 }, { Category.AnimalFood, 20 }, { Category.AProducts, 15 }, { Category.Products, 20 }, { Category.Items, 15 }, }},
        { TaskType.Legendary, new Dictionary<Category, int>() { { Category.AnimalFood, 20 }, { Category.AProducts, 20 }, { Category.Products, 30 }, { Category.Items, 30 }, }},
    };

    private void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        LoadDatas();

        DetermineTaskLimit();
        PopulateBoard(); CheckTasks();
    }

    public void DetermineTaskLimit()
    {
        //changing all task slots' spawn timer depending on level
        float timer = 0f;
        if (PlayerProfile.instance.level.LevelNumber < 5) timer = 30f;
        else if (PlayerProfile.instance.level.LevelNumber >= 5 && PlayerProfile.instance.level.LevelNumber < 10)
            timer = 60f;
        else if (PlayerProfile.instance.level.LevelNumber >= 10 && PlayerProfile.instance.level.LevelNumber < 15)
            timer = 120f;
        else timer = 180f;
        if (TaskList.Tasks.Count < PlayerProfile.instance.level.LevelNumber)
        {
            for (int i = TaskList.Tasks.Count; i < PlayerProfile.instance.level.LevelNumber; i++)
            {
                if (PlayerProfile.instance.level.LevelNumber > 15) break;

                TaskDetails ts = new TaskDetails()
                {
                    state = TaskState.HasTask,
                    EmptyTime = "",
                    timeToSpawn = timer,
                    Task = new Task()
                    {
                        difficulty = TaskType.None
                    }
                };
                if(i == PlayerProfile.instance.level.LevelNumber)
                    ts.Task = SendRequest(CreateTaskToRequest(i, true), 30);
                TaskList.Tasks.Add(ts);
            }
        }

        for (int i = 0; i < TaskList.Tasks.Count; i++) if(TaskList.Tasks[i].state != TaskState.Empty) TaskList.Tasks[i].timeToSpawn = timer;        
        SaveDatas();
    }

    public void OpenTaskBoard()
    {
        if (WaterSL.instance.anim.GetBool("Open Water Details"))
            WaterSL.instance.anim.SetBool("Open Water Details", false);
        if (FoodPL.instance.anim.GetBool("Open Details"))
            FoodPL.instance.anim.SetBool("Open Details", false);
        if (Storage.instance.anim.GetBool("Open Storage"))
            Storage.instance.anim.SetBool("Open Storage", false);
        anim.SetBool("Open Task Board", !anim.GetBool("Open Task Board"));
        if (anim.GetBool("Open Task Board")) PopulateInfoHolder(1000);
    }

    //fix the issue in function
    /*private void UpdatePricesOfTasks()
    {
        
         // Fix current task's price x rate and xp x rate history to future price fixation issue on global market 
         
        Task newTask = new Task();
        for (int t = 0; t < TaskList.Tasks.Count; t++)
        {
            Task cTask = TaskList.Tasks[t].Task;
            Dictionary<object, (float priceM, int xp, int count)> ChosenItems = new Dictionary<object, (float priceM, int xp, int count)>();
            #region fillin ChosenItems
                TasksAlgorithm typePar = Parameters.Find(e => e.difficulty == TaskList.Tasks[t].Task.difficulty);
                if (cTask.Plants != null && cTask.Plants.Count > 0)
                {
                    for(int i = 0; i < cTask.Plants.Count; i++)
                    {
                        var key = cTask.Plants[i];
                        if (!ChosenItems.ContainsKey(key))
                            ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedPlants.priceRange.x, typePar.AllowedPlants.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedPlants.XpRange.x, (int)typePar.AllowedPlants.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedPlants.countRange.x, (int)typePar.AllowedPlants.countRange.y)
                            );
                    }
                }
                if (typePar.AllowedFruits.fruits != null && typePar.AllowedFruits.fruits.Count > 0)
                {
                    Fruits p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_fruits, typePar.AllowedFruits.fruits, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedFruits.priceRange.x, typePar.AllowedFruits.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedFruits.XpRange.x, (int)typePar.AllowedFruits.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedFruits.countRange.x, (int)typePar.AllowedFruits.countRange.y)
                            );
                }
                if (typePar.AllowedAnimalFoods.animal_foods != null && typePar.AllowedAnimalFoods.animal_foods.Count > 0)
                {
                    a_f_types p = a_f_types.None;
                    List<Plants> fplants = new List<Plants>() { Plants.Wheat, Plants.Corn, Plants.Carrot, Plants.Potato };
                    List<a_f_types> foodnames = new List<a_f_types>() { a_f_types.Wheat, a_f_types.Corn, a_f_types.Carrot, a_f_types.Potato };
                    List<a_f_types> unlockedfoods = new();
                    List<a_f_types> afoods = new();
                    for (int i = 0; i < fplants.Count; i++)
                        if (StaticDatas.PlayerData.unlocked_items.u_plants.Contains(fplants[i])) unlockedfoods.Add(foodnames[i]);

                    for (int i = 0; i < typePar.AllowedAnimalFoods.animal_foods.Count; i++)
                        if (unlockedfoods.Contains(typePar.AllowedAnimalFoods.animal_foods[i])) afoods.Add(typePar.AllowedAnimalFoods.animal_foods[i]);

                    p = GetProduct(afoods, typePar.AllowedAnimalFoods.animal_foods, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedAnimalFoods.priceRange.x, typePar.AllowedAnimalFoods.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedAnimalFoods.XpRange.x, (int)typePar.AllowedAnimalFoods.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedAnimalFoods.countRange.x, (int)typePar.AllowedAnimalFoods.countRange.y)
                            );     // write back
                }
                if (typePar.AllowedAnimalProducts.animal_product != null && typePar.AllowedAnimalProducts.animal_product.Count > 0)
                {
                    AProducts p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_a_products, typePar.AllowedAnimalProducts.animal_product, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedAnimalProducts.priceRange.x, typePar.AllowedAnimalProducts.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedAnimalProducts.XpRange.x, (int)typePar.AllowedAnimalProducts.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedAnimalProducts.countRange.x, (int)typePar.AllowedAnimalProducts.countRange.y)
                            );     // write back
                }
                if (typePar.AllowedProduts.products != null && typePar.AllowedProduts.products.Count > 0)
                {
                    Products p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_Products, typePar.AllowedProduts.products, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedProduts.priceRange.x, typePar.AllowedProduts.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedProduts.XpRange.x, (int)typePar.AllowedProduts.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedProduts.countRange.x, (int)typePar.AllowedProduts.countRange.y)
                            );      // write back
                }
                if (typePar.AllowedItems.items != null && typePar.AllowedItems.items.Count > 0)
            {
                Items[] ItemsArray = (Items[])Enum.GetValues(typeof(Items));
                List<Items> allItems = new();
                for (int i = 0; i < ItemsArray.Length; i++)
                {
                    if (ItemsArray[i] != Items.None)
                        allItems.Add(ItemsArray[i]);
                }
                Items item = GetProduct(allItems, typePar.AllowedItems.items, ChosenItems);
                Debug.Log($"p = {item}");
                var key = item;
                if (!ChosenItems.ContainsKey(key))
                    ChosenItems[key] =
                        (
                        priceM: UnityEngine.Random.Range(typePar.AllowedItems.priceRange.x, typePar.AllowedItems.priceRange.y),
                        xp: UnityEngine.Random.Range((int)typePar.AllowedItems.XpRange.x, (int)typePar.AllowedItems.XpRange.y),
                        count: UnityEngine.Random.Range((int)typePar.AllowedItems.countRange.x, (int)typePar.AllowedItems.countRange.y)
                        );  // write back
            }
            #endregion
            newTask = SetTaskDetails(TaskList.Tasks[t].Task.difficulty, ChosenItems);


        }
    }
    */
    private void PopulateBoard()
    {
        Debug.Log("Populating Task Board");

        for (int i = 0; i < TaskList.Tasks.Count; i++)
        {
            Transform parent =
                (i == 12)
                    ? transform.Find("Tasks Window/Main Holder/Legendary Holder")
                    : transform.Find("Tasks Window/Main Holder/Holder");

            GameObject instance = Instantiate(taskPrefab, parent);
            instance.name = TaskList.Tasks[i].Task.difficulty.ToString();

            ATask t = instance.GetComponent<ATask>();
            t.task = TaskList.Tasks[i].Task;
            t.slotState = TaskList.Tasks[i].state;
            t.slotNumber = i;

            tasks.Add(instance);
        }

        if (tasks.Count < 9)
        {
            for (int i = 0; i < tasks.Count; i++) tasks[i].GetComponent<RectTransform>().localScale = new Vector3(1.4f, 1.4f, 1.4f);
            GridLayoutGroup glg = transform.Find("Tasks Window/Main Holder/Holder").GetComponent<GridLayoutGroup>();
            glg.padding.top = 15;
            glg.spacing = new Vector2(60f, 40);
        }
        else
        {
            for (int i = 0; i < tasks.Count; i++) tasks[i].GetComponent<RectTransform>().localScale = new Vector3(1.1f, 1.1f, 1.1f);
            GridLayoutGroup glg = transform.Find("Tasks Window/Main Holder/Holder").GetComponent<GridLayoutGroup>();
            glg.padding.top = 10;
            glg.spacing = new Vector2(10f, 13f);
        }
    }

    public void PopulateInfoHolder(int slotNumber)
    {
        Transform ih = transform.Find("Tasks Window/Main Holder/Info Holder");

        foreach (Transform child in ih)
        {
            if (child.name == "slotNumber") child.GetComponent<RectTransform>().localPosition = new Vector3(-510, slotNumber, 0); 
            child.gameObject.SetActive(false);
        }

        if (slotNumber != 1000)
        {
            Task cTask = TaskList.Tasks[slotNumber].Task;
            foreach (Transform child in ih)
                if (child.name != "slotNumber") child.gameObject.SetActive(true);
                else child.GetComponent<RectTransform>().localPosition = new Vector3(-510, slotNumber, 0);
            ih.Find("Difficulty").GetComponent<TextMeshProUGUI>().text = cTask.difficulty.ToString();

            ih.Find("Money/Icon").GetComponent<Image>().sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == cTask.currency).sprite;
            ih.Find("Money/Reward").GetComponent<TextMeshProUGUI>().text = cTask.Money.ToString();
            ih.Find("Xp/Reward").GetComponent<TextMeshProUGUI>().text = cTask.Xp.ToString();

            #region Filling Reqs Holder
            foreach (Transform item in ih.Find("Reqs")) Destroy(item.gameObject);
            // fill required plants
            if (cTask.Plants != null && cTask.Plants.Count > 0)
            {
                for (int i = 0; i < cTask.Plants.Count; i++)
                {
                    GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                    req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 13);
                    req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -11);

                    req.GetComponent<Image>().sprite = Sprites.instance.sprites.plants.Find(e => e.plant == cTask.Plants[i].Plant).sprite;
                    req.transform.Find("Details/Count").gameObject.SetActive(true);

                    if (Storage.instance.hasEnought(cTask.Plants[i].Plant, cTask.Plants[i].count, false))
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = cTask.Plants[i].count.ToString() + " / " + cTask.Plants[i].count.ToString();
                    else
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text =
                            Storage.instance.GetCountOf(cTask.Plants[i].Plant) + " / " + cTask.Plants[i].count.ToString();
                    Destroy(req.GetComponent<Button>());
                }
            }

            // fill required fruits
            if (cTask.Fruits != null && cTask.Fruits.Count > 0)
            {
                for (int i = 0; i < cTask.Fruits.Count; i++)
                {
                    GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                    req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 13);
                    req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -11);

                    req.GetComponent<Image>().sprite = Sprites.instance.sprites.fruits.Find(e => e.fruit == cTask.Fruits[i].Fruit).sprite;
                    req.transform.Find("Details/Count").gameObject.SetActive(true);

                    if (Storage.instance.hasEnought(cTask.Fruits[i].Fruit, cTask.Fruits[i].count, false))
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = cTask.Fruits[i].count.ToString() + " / " + cTask.Fruits[i].count.ToString();
                    else
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text =
                            Storage.instance.GetCountOf(cTask.Fruits[i].Fruit) + " / " + cTask.Fruits[i].count.ToString();
                    Destroy(req.GetComponent<Button>());
                }
            }

            // fill required Animal Foods
            if (cTask.AnimalFoods != null && cTask.AnimalFoods.Count > 0)
            {
                for (int i = 0; i < cTask.AnimalFoods.Count; i++)
                {
                    GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                    req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 13);
                    req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -11);

                    req.GetComponent<Image>().sprite = Sprites.instance.sprites.AnimalFoodSprites.Find(e => e.food == cTask.AnimalFoods[i].food).sprite;
                    req.transform.Find("Details/Count").gameObject.SetActive(true);

                    if (Storage.instance.hasEnought(cTask.AnimalFoods[i].food, cTask.AnimalFoods[i].amount, false))
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = cTask.AnimalFoods[i].amount.ToString() + " / " + cTask.AnimalFoods[i].amount.ToString();
                    else
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text =
                            Storage.instance.GetCountOf(cTask.AnimalFoods[i].food) + " / " + cTask.AnimalFoods[i].amount.ToString();
                    Destroy(req.GetComponent<Button>());
                }
            }

            // fill required Animal Products
            if (cTask.AnimalProducts != null && cTask.AnimalProducts.Count > 0)
            {
                for (int i = 0; i < cTask.AnimalProducts.Count; i++)
                {
                    GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                    req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 13);
                    req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -11);

                    req.GetComponent<Image>().sprite = Sprites.instance.sprites.a_products.Find(e => e.a_product == cTask.AnimalProducts[i].animal_products).sprite;
                    req.transform.Find("Details/Count").gameObject.SetActive(true);

                    if (Storage.instance.hasEnought(cTask.AnimalProducts[i].animal_products, cTask.AnimalProducts[i].count, false))
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = cTask.AnimalProducts[i].count.ToString() + " / " + cTask.AnimalProducts[i].count.ToString();
                    else
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text =
                            Storage.instance.GetCountOf(cTask.AnimalProducts[i].animal_products) + " / " + cTask.AnimalProducts[i].count.ToString();
                    Destroy(req.GetComponent<Button>());
                }
            }

            // fill required Products
            if (cTask.Products != null && cTask.Products.Count > 0)
            {
                for (int i = 0; i < cTask.Products.Count; i++)
                {
                    GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                    req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 13);
                    req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -11);

                    req.GetComponent<Image>().sprite = Sprites.instance.sprites.products.Find(e => e.product == cTask.Products[i].product).sprite;
                    req.transform.Find("Details/Count").gameObject.SetActive(true);

                    if (Storage.instance.hasEnought(cTask.Products[i].product, cTask.Products[i].count, false))
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = cTask.Products[i].count.ToString() + " / " + cTask.Products[i].count.ToString();
                    else
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text =
                            Storage.instance.GetCountOf(cTask.Products[i].product) + " / " + cTask.Products[i].count.ToString();
                    Destroy(req.GetComponent<Button>());
                }
            }

            // fill required Items
            if (cTask.Items != null && cTask.Items.Count > 0)
            {
                for (int i = 0; i < cTask.Items.Count; i++)
                {
                    GameObject req = Instantiate(reqPrefab, ih.Find("Reqs"));
                    req.transform.Find("Details").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 13);
                    req.transform.Find("Details").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -11);

                    req.GetComponent<Image>().sprite = Sprites.instance.sprites.items.Find(e => e.item == cTask.Items[i].item).sprite;
                    req.transform.Find("Details/Count").gameObject.SetActive(true);

                    if (Storage.instance.hasEnought(cTask.Items[i].item, cTask.Items[i].count, false))
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text = cTask.Items[i].count.ToString() + " / " + cTask.Items[i].count.ToString();
                    else
                        req.transform.Find("Details/Count").GetComponent<TextMeshProUGUI>().text =
                            Storage.instance.GetCountOf(cTask.Items[i].item) + " / " + cTask.Items[i].count.ToString();
                    Destroy(req.GetComponent<Button>());
                }
            }
            #endregion

            CheckReqs(slotNumber, out bool met);
            if (met)
            {
                ih.Find("Buttons/Send").GetComponent<Button>().onClick.RemoveAllListeners();
                ih.Find("Buttons/Send").GetComponent<Button>().onClick.AddListener(() => tasks[slotNumber].GetComponent<ATask>().CompleteTask());
                Debug.Log($"Reqs met for task {slotNumber}");
            }
            ih.Find("Buttons/Delete Task").GetComponent<Button>().onClick.RemoveAllListeners();
            ih.Find("Buttons/Delete Task").GetComponent<Button>().onClick.AddListener(() => tasks[slotNumber].GetComponent<ATask>().DeleteTask());
        }
    }

    public void CompleteTask()
    {
        transform.Find("Tasks Window/Main Holder/Info Holder/Buttons/Send").GetComponent<Button>().onClick.RemoveAllListeners();
        transform.Find("Tasks Window/Main Holder/Info Holder/Buttons/Delete Task").GetComponent<Button>().onClick.RemoveAllListeners();
    }

    public void CheckTasks()
    {
        for (int i = 0; i < TaskList.Tasks.Count; i++) 
            if(TaskList.Tasks[i].state == TaskState.HasTask)
            {
                CheckReqs(i, out bool met);
                PopulateInfoHolder((int)transform.Find("Tasks Window/Main Holder/Info Holder/slotNumber").GetComponent<RectTransform>().localPosition.y);
            }
    }

    public void CheckReqs(int slotNumber, out bool met)
    {
        met = false;
        bool[] has = new bool[6] { true, true, true, true, true, true }; bool hasAll = true;
        bool[] empty = new bool[6] { false, false, false, false, false, false }; bool emptyAll = true;
        if (TaskList.Tasks[slotNumber].Task.Plants != null && TaskList.Tasks[slotNumber].Task.Plants.Count > 0)
        {
            for (int i = 0; i < TaskList.Tasks[slotNumber].Task.Plants.Count; i++)
                if (!Storage.instance.hasEnought(TaskList.Tasks[slotNumber].Task.Plants[i].Plant, TaskList.Tasks[slotNumber].Task.Plants[i].count, false)) has[0] = false;
        }
        else empty[0] = true;
        if (TaskList.Tasks[slotNumber].Task.Fruits != null && TaskList.Tasks[slotNumber].Task.Fruits.Count > 0)
        {
            for (int i = 0; i < TaskList.Tasks[slotNumber].Task.Fruits.Count; i++)
                if (!Storage.instance.hasEnought(TaskList.Tasks[slotNumber].Task.Fruits[i].Fruit, TaskList.Tasks[slotNumber].Task.Fruits[i].count, false)) has[1] = false;
        }
        else empty[1] = true;


        if (TaskList.Tasks[slotNumber].Task.AnimalProducts != null && TaskList.Tasks[slotNumber].Task.AnimalProducts.Count > 0)
        {
            for (int i = 0; i < TaskList.Tasks[slotNumber].Task.AnimalProducts.Count; i++)
                if (!Storage.instance.hasEnought(TaskList.Tasks[slotNumber].Task.AnimalProducts[i].animal_products, TaskList.Tasks[slotNumber].Task.AnimalProducts[i].count, false)) has[2] = false;
        }
        else empty[2] = true;

        if (TaskList.Tasks[slotNumber].Task.AnimalFoods != null && TaskList.Tasks[slotNumber].Task.AnimalFoods.Count > 0)
        {
            for (int i = 0; i < TaskList.Tasks[slotNumber].Task.AnimalFoods.Count; i++)
                if (!Storage.instance.hasEnought(TaskList.Tasks[slotNumber].Task.AnimalFoods[i].food, TaskList.Tasks[slotNumber].Task.AnimalFoods[i].amount, false)) has[3] = false;
        }
        else empty[3] = true;

        if (TaskList.Tasks[slotNumber].Task.Products != null && TaskList.Tasks[slotNumber].Task.Products.Count > 0)
        {
            for (int i = 0; i < TaskList.Tasks[slotNumber].Task.Products.Count; i++)
                if (!Storage.instance.hasEnought(TaskList.Tasks[slotNumber].Task.Products[i].product, TaskList.Tasks[slotNumber].Task.Products[i].count, false)) has[4] = false;
        }
        else empty[4] = true;

        if (TaskList.Tasks[slotNumber].Task.Items != null && TaskList.Tasks[slotNumber].Task.Items.Count > 0)
        {
            for (int i = 0; i < TaskList.Tasks[slotNumber].Task.Items.Count; i++)
                if (!Storage.instance.hasEnought(TaskList.Tasks[slotNumber].Task.Items[i].item, TaskList.Tasks[slotNumber].Task.Items[i].count, false)) has[5] = false;
        }
        else empty[5] = true;

        for (int i = 0; i < has.Length; i++) if (!has[i]) { Debug.Log("has " + i + has[i]); hasAll = false; }
        for (int i = 0; i < empty.Length; i++) if (!empty[i]) { emptyAll = false; }

        if (hasAll && !emptyAll)
        {
            tasks[slotNumber].GetComponent<Image>().color = new Color32(20, 160, 0, 255);
            met = true;
        }
        else{
            Debug.Log($"slotNumber = {slotNumber}");
            tasks[slotNumber].GetComponent<Image>().color = new Color32(200, 170, 0, 255);
        }
    }

    public TaskType CreateTaskToRequest(int slotNumber, bool fromMain)
    {
        List<TaskType> pool = new();
        Dictionary<string, Dictionary<TaskType, int>> taskPool = new Dictionary<string, Dictionary<TaskType, int>>()
        {
            { "Early",          new Dictionary<TaskType, int>(){ { TaskType.Easy, 100 } } },
            { "Past-Early",     new Dictionary<TaskType, int>(){ { TaskType.Easy, 70 }, { TaskType.Medium, 30 } } },
            { "Intermediet",    new Dictionary<TaskType, int>(){ { TaskType.Easy, 50 }, { TaskType.Medium, 30 }, { TaskType.Hard, 20 } } },
            { "Advanced",       new Dictionary<TaskType, int>(){ { TaskType.Easy, 30 }, { TaskType.Medium, 40 }, { TaskType.Hard, 30 } } }
        };

        string state = "";
        TaskType chosen = TaskType.None;
        if (PlayerProfile.instance.level.LevelNumber < 5) state = "Early";
        else if (PlayerProfile.instance.level.LevelNumber >= 5 && PlayerProfile.instance.level.LevelNumber < 10) state = "Past-Early";
        else if (PlayerProfile.instance.level.LevelNumber >= 10 && PlayerProfile.instance.level.LevelNumber < 15) state = "Intermediet";
        else state = "Advanced";

        foreach (var item in taskPool) if (item.Key == state) foreach (var dif in item.Value) for (int i = 0; i < dif.Value; i++) pool.Add(dif.Key);
        if (!fromMain)
        {
            if (slotNumber == 15) chosen = TaskType.Legendary;
            else chosen = pool[UnityEngine.Random.Range(0, pool.Count)];
        }
        else
        {
            if (slotNumber == TaskList.Tasks.Count - 1 && PlayerProfile.instance.level.LevelNumber >= 15) chosen = TaskType.Legendary;
            else chosen = pool[UnityEngine.Random.Range(0, pool.Count)];
        }
        { }
        return chosen;
    }

    #region Algorithm of Task Creation

    public Task SendRequest(TaskType diff, int point)
    {
        Debug.Log($"Difficulty = {diff}");
        List<TaskType> diffPool = new List<TaskType>();
        if (diff == TaskType.Easy && point < 31)
        {
            Debug.Log($"before point = {point}");
            diffPool.Add(TaskType.Easy);
            point -= TaskPoint[TaskType.Easy];
            Debug.Log($"after point = {point}");
        }
        else if(diff == TaskType.Legendary)
            diffPool.Add(TaskType.Legendary);
        else
        {
            Debug.Log($"before point = {point}");
            if (diff == TaskType.Hard) { diffPool.Add(TaskType.Hard); point -= TaskPoint[TaskType.Hard]; }
            if(diff == TaskType.Medium) { diffPool.Add(TaskType.Medium); point -= TaskPoint[TaskType.Medium]; }
            Debug.Log($"after prechoosing diff point = {point}");
            List<int> d_points = new List<int>() { 10, 20, 50 };
            for (int i = 0; i < 5; i++)
            {
                if (d_points == null || d_points.Count == 0) break;
                int itemSum = 0;
                for (int s = 0; s < d_points.Count; s++) itemSum += d_points[s];

                Debug.Log($"itemSum = {itemSum}");
                if (point >= itemSum || point >= d_points[d_points.Count - 1])
                {
                    int p = d_points[UnityEngine.Random.Range(0, d_points.Count)];
                    Debug.Log($"before point = {point}");
                    diffPool.Add(PointTask[p]);
                    point -= p;
                    Debug.Log($"after point = {point}");
                    if (point < d_points[d_points.Count - 1]) { Debug.Log($"in removing {d_points[d_points.Count - 1]}"); d_points.RemoveAt(d_points.Count - 1); }
                }
                else
                    if (point < d_points[d_points.Count - 1]) { Debug.Log($"removing {d_points[d_points.Count - 1]}"); d_points.RemoveAt(d_points.Count - 1); }
            }
        }
        return CreateATask(diff, diffPool);
    }

    private Task CreateATask(TaskType mainDif, List<TaskType> difs)
    {
        Debug.Log($"inside CreateATask");
        Dictionary<object, (float priceM, int xp, int count)> ChosenItems = new Dictionary<object, (float priceM, int xp, int count)>();
        List<Category> categories = new();
        List<TaskType> t_difs = new List<TaskType>(difs);
        for (int i = 0; i < difs.Count; i++)
        {
            TasksAlgorithm tal = Parameters.Find(e => e.difficulty == difs[i]);
            if (StaticDatas.PlayerData.unlocked_items.u_plants != null && StaticDatas.PlayerData.unlocked_items.u_plants.Count > 0 && tal.AllowedPlants.plants != null && tal.AllowedPlants.plants.Count > 0)
                { if (!categories.Contains(Category.Plants)) categories.Add(Category.Plants); }
            else if (StaticDatas.PlayerData.unlocked_items.u_plants == null || StaticDatas.PlayerData.unlocked_items.u_plants.Count == 0) Debug.Log("There is no Plants unlocked");

            if (StaticDatas.PlayerData.unlocked_items.u_fruits != null && StaticDatas.PlayerData.unlocked_items.u_fruits.Count > 0 && tal.AllowedFruits.fruits != null && tal.AllowedFruits.fruits.Count > 0)
                { if (!categories.Contains(Category.Fruits)) categories.Add(Category.Fruits); }
            else if (StaticDatas.PlayerData.unlocked_items.u_fruits == null || StaticDatas.PlayerData.unlocked_items.u_fruits.Count == 0) Debug.Log("There is no Fruits unlocked");

            List<a_f_types> afoods = GetUnlockedFood(tal.AllowedAnimalFoods.animal_foods);
            if (afoods != null && afoods.Count > 0 && tal.AllowedAnimalFoods.animal_foods != null && tal.AllowedAnimalFoods.animal_foods.Count > 0)
                { if (!categories.Contains(Category.AnimalFood)) categories.Add(Category.AnimalFood); }
            else if (afoods == null || afoods.Count == 0) Debug.Log("There is no Foods unlocked");

            if (StaticDatas.PlayerData.unlocked_items.u_a_products != null && StaticDatas.PlayerData.unlocked_items.u_a_products.Count > 0 && tal.AllowedAnimalProducts.animal_product != null && tal.AllowedAnimalProducts.animal_product.Count > 0)
                { if (!categories.Contains(Category.AProducts)) categories.Add(Category.AProducts); }
            else if (StaticDatas.PlayerData.unlocked_items.u_a_products == null || StaticDatas.PlayerData.unlocked_items.u_a_products.Count == 0) Debug.Log("There is no Animal Products unlocked");

            if (StaticDatas.PlayerData.unlocked_items.u_Products != null && StaticDatas.PlayerData.unlocked_items.u_Products.Count > 0 && tal.AllowedProduts.products != null && tal.AllowedProduts.products.Count > 0)
                { if (!categories.Contains(Category.Products)) categories.Add(Category.Products); }
            else if (StaticDatas.PlayerData.unlocked_items.u_Products == null || StaticDatas.PlayerData.unlocked_items.u_Products.Count == 0) Debug.Log("There is no Product unlocked");

            if (tal.AllowedItems.items != null && tal.AllowedItems.items.Count > 0)
                if(!categories.Contains(Category.Items)) categories.Add(Category.Items);
        }
        for(int d = 0; d < difs.Count; d++)
        {
            int dindex = UnityEngine.Random.Range(0, t_difs.Count);
            Debug.Log($"dindex = {dindex}");
            object dif = t_difs[dindex]; //take difficulty from pool
            t_difs.RemoveAt(dindex);

            Debug.Log($"dif = {(TaskType)dif}");
            TasksAlgorithm typePar = Parameters.Find(e => e.difficulty == (TaskType)dif);
            List<Category> catOfDif = new();
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i] == Category.Plants && (TaskType)dif != TaskType.Legendary) { if ((typePar.AllowedPlants.plants != null && typePar.AllowedPlants.plants.Count > 0)) catOfDif.Add(categories[i]); }
                else if (categories[i] == Category.Fruits && (TaskType)dif != TaskType.Legendary) { if (typePar.AllowedFruits.fruits != null && typePar.AllowedFruits.fruits.Count > 0) catOfDif.Add(categories[i]); }
                else if (categories[i] == Category.AnimalFood) { if (typePar.AllowedAnimalFoods.animal_foods != null && typePar.AllowedAnimalFoods.animal_foods.Count > 0) catOfDif.Add(categories[i]); }
                else if (categories[i] == Category.AProducts) { if (typePar.AllowedAnimalProducts.animal_product != null && typePar.AllowedAnimalProducts.animal_product.Count > 0) catOfDif.Add(categories[i]); }
                else if (categories[i] == Category.Products) { if (typePar.AllowedProduts.products != null && typePar.AllowedProduts.products.Count > 0) catOfDif.Add(categories[i]); }
                else if (categories[i] == Category.Items && (TaskType)dif != TaskType.Easy) { if (typePar.AllowedItems.items != null && typePar.AllowedItems.items.Count > 0) catOfDif.Add(categories[i]); }
            }

            for (int i = 0; i < catOfDif.Count; i++) Debug.Log($"Accepted Categories: {catOfDif[i]}");

            List<int> pool = new();
            for (int item = 0; item < catOfDif.Count; item++)
                for (int i = 0; i < chanceRateForDiff[(TaskType)dif][catOfDif[item]]; i++) pool.Add((int)catOfDif[item]);
            Category c = (Category)pool[UnityEngine.Random.Range(0, pool.Count)];
            Debug.Log($"Category = {c}");
            if (c == Category.Plants)
            {
                if (typePar.AllowedPlants.plants != null && typePar.AllowedPlants.plants.Count > 0)
                {
                    Plants p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_plants, typePar.AllowedPlants.plants, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] = 
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedPlants.priceRange.x, typePar.AllowedPlants.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedPlants.XpRange.x, (int)typePar.AllowedPlants.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedPlants.countRange.x, (int)typePar.AllowedPlants.countRange.y)
                            );
                }
            }
            else if (c == Category.Fruits)
            {
                if (typePar.AllowedFruits.fruits != null && typePar.AllowedFruits.fruits.Count > 0)
                {
                    Fruits p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_fruits, typePar.AllowedFruits.fruits, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedFruits.priceRange.x, typePar.AllowedFruits.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedFruits.XpRange.x, (int)typePar.AllowedFruits.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedFruits.countRange.x, (int)typePar.AllowedFruits.countRange.y)
                            );
                }
            }
            else if (c == Category.AnimalFood)
            {
                if (typePar.AllowedAnimalFoods.animal_foods != null && typePar.AllowedAnimalFoods.animal_foods.Count > 0)
                {
                    a_f_types p = a_f_types.None;
                    p = GetProduct(GetUnlockedFood(typePar.AllowedAnimalFoods.animal_foods), typePar.AllowedAnimalFoods.animal_foods, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedAnimalFoods.priceRange.x, typePar.AllowedAnimalFoods.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedAnimalFoods.XpRange.x, (int)typePar.AllowedAnimalFoods.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedAnimalFoods.countRange.x, (int)typePar.AllowedAnimalFoods.countRange.y)
                            );     // write back
                }
            }
            else if (c == Category.AProducts)
            {
                if (typePar.AllowedAnimalProducts.animal_product != null && typePar.AllowedAnimalProducts.animal_product.Count > 0)
                {
                    AProducts p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_a_products, typePar.AllowedAnimalProducts.animal_product, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedAnimalProducts.priceRange.x, typePar.AllowedAnimalProducts.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedAnimalProducts.XpRange.x, (int)typePar.AllowedAnimalProducts.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedAnimalProducts.countRange.x, (int)typePar.AllowedAnimalProducts.countRange.y)
                            );     // write back
                }
            }
            else if (c == Category.Products)
            {
                if (typePar.AllowedProduts.products != null && typePar.AllowedProduts.products.Count > 0)
                {
                    Products p = GetProduct(StaticDatas.PlayerData.unlocked_items.u_Products, typePar.AllowedProduts.products, ChosenItems);
                    Debug.Log($"p = {p}");
                    var key = p;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedProduts.priceRange.x, typePar.AllowedProduts.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedProduts.XpRange.x, (int)typePar.AllowedProduts.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedProduts.countRange.x, (int)typePar.AllowedProduts.countRange.y)
                            );      // write back
                }
            }
            else if (c == Category.Items)
            {
                if (typePar.AllowedItems.items != null && typePar.AllowedItems.items.Count > 0)
                {
                    Items[] ItemsArray = (Items[])Enum.GetValues(typeof(Items));
                    List<Items> allItems = new();
                    for (int i = 0; i < ItemsArray.Length; i++)
                    {
                        if (ItemsArray[i] != Items.None)
                            allItems.Add(ItemsArray[i]);
                    }
                    Items item = GetProduct(allItems, typePar.AllowedItems.items, ChosenItems);
                    Debug.Log($"p = {item}");
                    var key = item;
                    if (!ChosenItems.ContainsKey(key))
                        ChosenItems[key] =
                            (
                            priceM: UnityEngine.Random.Range(typePar.AllowedItems.priceRange.x, typePar.AllowedItems.priceRange.y),
                            xp: UnityEngine.Random.Range((int)typePar.AllowedItems.XpRange.x, (int)typePar.AllowedItems.XpRange.y),
                            count: UnityEngine.Random.Range((int)typePar.AllowedItems.countRange.x, (int)typePar.AllowedItems.countRange.y)
                            );  // write back
                }
            }
        }

        return SetTaskDetails(mainDif, ChosenItems);
    }

    public List<a_f_types> GetUnlockedFood(List<a_f_types> requesting)
    {
        List<Plants> fplants = new List<Plants>() { Plants.Wheat, Plants.Corn, Plants.Carrot, Plants.Potato };
        List<a_f_types> foodnames = new List<a_f_types>() { a_f_types.Wheat, a_f_types.Corn, a_f_types.Carrot, a_f_types.Potato };
        List<a_f_types> unlockedfoods = new();
        List<a_f_types> afoods = new();
        for (int i = 0; i < fplants.Count; i++)
            if (StaticDatas.PlayerData.unlocked_items.u_plants.Contains(fplants[i])) unlockedfoods.Add(foodnames[i]);

        for (int i = 0; i < requesting.Count; i++)
            if (unlockedfoods.Contains(requesting[i])) afoods.Add(requesting[i]);
        return afoods;
    }

    private T GetProduct<T>(List<T> unlocked, List<T> items, Dictionary<object, (float priceM, int xp, int count)> chosenItems)
    {
        List<T> valid = new List<T>();

        for (int i = 0; i < items.Count; i++)
        {
            T item = items[i];
            if (unlocked.Contains(item) && !chosenItems.ContainsKey(item))
                valid.Add(item);
        }

        if (valid.Count == 0)
        {
            Debug.LogWarning($"GetProduct<{typeof(T).Name}>: No valid items available.");
            return default;
        }

        return valid[UnityEngine.Random.Range(0, valid.Count)];
    }

    private Task SetTaskDetails(TaskType mainDif, Dictionary<object, (float priceM, int xp, int count)> ChosenItems)
    {
        List<PlantCount> plants = new();
        List<FruitCount> fruits = new();
        List<afAmount> animalfoods = new();
        List<APCount> aProducts = new();
        List<ProductCount> products = new();
        List<ItemCount> items = new();

        int allXp = 0;
        int allMoney = 0;
        foreach (var item in ChosenItems)
        {
            int itemPrice = 0;
            allXp += item.Value.xp * item.Value.count;
            itemPrice += (int)Math.Ceiling(MyShop.instance.GetPriceOfItem(item.Key) * item.Value.priceM);
            if (item.Key is Plants)
                plants.Add(new PlantCount() { Plant = (Plants)item.Key, count = item.Value.count });
            else if (item.Key is Fruits)
                fruits.Add(new FruitCount() { Fruit = (Fruits)item.Key, count = item.Value.count });
            else if (item.Key is a_f_types)
                animalfoods.Add(new afAmount() { food = (a_f_types)item.Key, amount = item.Value.count });
            else if (item.Key is AProducts)
                aProducts.Add(new APCount() { animal_products = (AProducts)item.Key, count = item.Value.count });
            else if (item.Key is Products)
                products.Add(new ProductCount() { product = (Products)item.Key, count = item.Value.count });
            else if (item.Key is Items)
                items.Add(new ItemCount() { item = (Items)item.Key, count = item.Value.count });

            itemPrice *= item.Value.count;
            allMoney += itemPrice;
            Debug.Log($"item.Value.priceM = {item.Value.priceM}: allMoney = {allMoney}");
            Debug.Log($"item.Value.xp = {item.Value.xp}: allXp = {allXp}");
        }

        Task theTask = new Task()
        {
            difficulty = mainDif,
            Plants = plants,
            Fruits = fruits,
            AnimalFoods = animalfoods,
            AnimalProducts = aProducts,
            Products = products,
            Items = items,
            
            currency = Currency.Coin,
            Money = allMoney,
            Xp = allXp
        };

        return theTask;
    }

    #endregion

    #region Save System
    private static void SaveTasks(TaskSystem data)
    {
        filePath = Path.Combine(Application.persistentDataPath, "Tasks.json");
        string json = JsonUtility.ToJson(data, true);
        if(!string.IsNullOrEmpty(json))
            File.WriteAllText(filePath, json);
    }

    private static TaskSystem LoadTasks()
    {
        string path = Path.Combine(Application.persistentDataPath, "Tasks.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            TaskSystem ts = JsonUtility.FromJson<TaskSystem>(json);
            ts = SaveDataMerger.MergeWithDefaults(ts, default_taskList);
            return ts;
        }
        else
        {
            SaveTasks(default_taskList);
            Debug.LogWarning("Save file not found in " + path);
            return default_taskList; // Return a new instance with default values
        }
    }

    private TaskDetails GetFirstLevel()
    {
        TaskDetails dtask = new TaskDetails();
        dtask = default_taskList.Tasks[0];
        dtask.state = TaskState.HasTask;
        dtask.Task = SendRequest(TaskType.Easy, 30);

        return dtask;
    }

    public void LoadDatas()
    {
        TaskList = LoadTasks();
    }

    public void SaveDatas()
    {
        SaveTasks(TaskList);
    }
    #endregion
}