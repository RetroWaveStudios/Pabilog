using System;
using System.Collections.Generic;
using UnityEngine;

#region enums
public enum LandState
{
    Dry,
    Empty,
    Plow,
    Planted,
    Stumped,
    Blocked
}

public enum PlantState
{
    None,
    Growing,
    ReadyToHarvest,
}

public enum Plants
{
    Wheat,
    Corn,
    Carrot,
    SugarCane,
    Tomato,
    Onion,
    Potato,
    Herbs,
    Spice,
    Cotton,
    None
}

public enum Fruits
{
    Apple,
    Cherry,
    Berry,
    Banana,
    Orange,
    Cacao,
    None
}

public enum Animals
{
    Chicken,
    Cow,
    Sheep,
    Pig,
    None
}

public enum a_f_types //animal food types
{
    Wheat,
    Corn,
    Carrot,
    Potato,
    None
}

public enum AState
{
    None,
    Fertilizing,
    ReadyToCollect,
}

public enum AProducts
{
    Egg,
    ChickenMeat,
    Milk,
    Beef,
    Wool,
    SheepMeat,
    Bacon,
    None
}

public enum ASpotState
{
    Empty,
    HasAnimal,
    Broken
}

public enum Currency
{
    Coin,
    Crystal,
    RealMoney
}

public enum Products
{
    //  Dairy Churn
    Cream,          //  Milk 1
    Butter,         //  Milk 2
    Cheese,         //  Milk 3

    //  Oven / Bakery
    Bread,          //  Wheat 1 + Egg 1
    CornBread,     //  Corn  1 + Egg 1
    Cookie,         //  Wheat 2 + Sugar 1 + Egg 1
    Pizza,          //  Bread 1 + Cheese 1 + Tomato 1
    ChickenPizza,    //  Bread 1 + Cheese 1 + Tomato 1 + any Meat 1
    SpicyPizza,    //  Bread 1 + Cheese 1 + Tomato 1 + Spice 2

    //  Popcorn Machine
    Popcorn,        //  Corn 2
    ButterPopcorn, //  Corn 2 + Butter 1
    SpicyPopcorn,  //  Corn 2 + Spice 1

    //  Pie Oven
    ApplePie,      //  Apple 2 + Wheat 2 + Sugar 1 + Egg 1
    CarrotPie,     //  Carrot 2 + Wheat 2 + Sugar 1 + Egg 1
    BaconPie,      //  Bacon 2 + Wheat 2 + Sugar 1 + Egg 2

    //  Cake House
    CarrotCake,    //  Sugar 1 + Egg 2 + Carrot 2
    CreamCake,     //  Sugar 1 + Egg 2 + Cream 2
    BerryCake,     //  Sugar 1 + Egg 2 + Berries 2
    CherryCake,     //  Sugar 1 + Egg 2 + Cherries 2

    //  Press / Juicer
    Sugar,              //  SugarCane 1
    AppleJuice,         //  Apple 1
    BerryJuice,         //  Berries 1
    MultiFruitJuice,    //  Apple 1, Cherry 1, Berry 1, Sugar 1

    // Stove
    Soup,           //  Potato 1 + Onion 1 + Herbs 1 + Water 2
    ChickenSoup,        //  Potato 1 + Onion 1 + Herbs 1 + Water 2 + Chmeat 1
    Stew,           //  Cowmeat 1 + Carrot 2 + Onion 1
    Steak,          //  Cowmeat 1
    RoastedCord,   //  Corn 3
    Gratin,         //  Potato 2 + Cheese 2

    //  Color Pigment Machine
    PigmentRed,    //  Berries
    PigmentBlue,   //  Berries
    PigmentYellow, //  Corn
    PigmentGreen,  //  Herbs
    PigmentBrown,  //  Tree barks
    PigmentPurple, //  Grape
    PigmentOrange, //  Carrot

    //  Color Producer Machine
    Red,            //  PigmentRed 1 + Water 1
    Blue,           //  PigmentBlue 1 + Water 1
    Yellow,         //  PigmentYellow 1 + Water 1
    Green,          //  PigmentGreen 1 + Water 1
    Brown,          //  PigmentBrown 1 + Water 1
    Purple,         //  PigmentPurple 1 + Water 1
    Orange,         //  PigmentOrange 1 + Water 1

    //  Cosmetic Station

    //  Icecream Stand
    VanillaIcecream,    // Cream 2 + Milk 1 + Sugar 1
    CherryIcecream,     // Cream 2 + Milk 1 + Sugar 1 + Cherry 2
    OilyIcecream,       // Cream 2 + Milk 1 + Sugar 1 + Butter 1
    ChocolatteIcecream, // Cream 2 + Milk 1 + Sugar 1 + Cacao 2

    //  Simple Sewing Machine
    SimpleTShirt,   //  Wool 3
    Hat,            //  Wool 2
    Sweater,        //  Wool 3

    None,
}

public enum Items
{
    Rake,   // for plow farm land
    Axe,    // for remove stump

    Bolt,   // to fix broken machine
    Screw,  // to fix broken machine
    Hammer, // to fix broken machine

    Tape,   // to upgrage Storage
    Drill,  // to upgrade Storage
    Pliers, // to upgrade Storage

    ToolSet,    // to upgrade Storage
    None
}

public enum BoxTypes
{
    ProductBox,
    Wooden,
    Copper,
    Iron,
    Golden,
    Master,
    Ancient
}

public enum Machines
{
    Oven,
    DairyChurn,
    PopcornMachine,
    PieOven,
    CakeHouse,
    Presser,
    Stove,
    ColorPigmentMachine,
    ColorProducerMachine,
    IcecreamStand,
    SewingMachine,
    None
}

public enum Category
{
    Plants,
    Fruits,
    AnimalFood,
    AProducts,
    Products,
    Items
}

public enum ItemCat
{
    List,
    Class,
    Else
}

public enum PushType
{
    Alert,
    Notice,
    Achivement
}

public enum TaskType
{
    Easy,
    Medium,
    Hard,
    Legendary,
    None
}

public enum TaskState
{
    Empty,
    HasTask,
}
#endregion

#region Plants
[System.Serializable]
public class PGStages
{
    public string name;
    public Plants plant;
    public List<Sprite> stages;
}

[System.Serializable]
public class PD
{
    public string name;
    public Plants plant;
    public PlantState state;
    public double GrowthTime; // by minutes
    public double pauseTime; // by minutes

    public double wTimer; // by minutes
    public string waterTime;

    public bool hasWater;

    public int harvestAmount;
    public int xp;
    public int price;

    public PD Clone()
    {
        return new PD
        {
            plant = this.plant,
            state = this.state,
            GrowthTime = this.GrowthTime,
            pauseTime = this.pauseTime,
            wTimer = this.wTimer,
            waterTime = this.waterTime,
            hasWater = this.hasWater,
            harvestAmount = this.harvestAmount,
            xp = this.xp,
            price = this.price,
        };
    }
}

[System.Serializable]
public class PlantSprites
{
    public string name;
    public Plants plant;
    public Sprite sprite;
}
#endregion

#region Fruits

[System.Serializable]
public class TreeD
{
    public string name;
    public Fruits fruit;
    public PlantState state;
    public int usage;
    public int stage;

    public List<double> GrowthTimeByStage; // by minutes
    public double pauseTime; // by minutes

    public double wTimer; // by minutes
    public List<double> wTimerByStages; // by minutes
    public string waterTime;

    public bool hasWater;
    public List<int> WaterAmoutByStage;

    public int harvestAmount;
    public List<int> hFrutis;
    public int xp;

    public Currency currency;
    public int price;

    public TreeD Clone()
    {
        return new TreeD
        {
            fruit = this.fruit,
            state = this.state,
            usage = this.usage,
            GrowthTimeByStage = this.GrowthTimeByStage,
            pauseTime = this.pauseTime,
            wTimer = this.wTimer,
            wTimerByStages = this.wTimerByStages,
            waterTime = this.waterTime,
            hasWater = this.hasWater,
            WaterAmoutByStage = this.WaterAmoutByStage,
            harvestAmount = this.harvestAmount,
            hFrutis = this.hFrutis,
            xp = this.xp,
            currency = this.currency,
            price = this.price
        };
    }
}

[System.Serializable]
public class FruitSprites
{
    public string name;
    public Fruits fruit;
    public Sprite sprite;
}

[System.Serializable]
public class TreeStages
{
    public string name;
    public Fruits tree;
    public List<Sprite> stages;
}

#endregion

#region Animals
[System.Serializable]
public class APSprites
{
    public string name;
    public AProducts a_product;
    public Sprite sprite;
}
[System.Serializable]
public class AnimalSprites
{
    public string name;
    public Animals animal;
    public Sprite sprite;
}

[System.Serializable]
public class APD
{
    public string name;
    public Animals animal;
    public AState state;

    public int a_price;
    public int a_Xp;

    public int xp;
    public int amount;

    public List<double> prTimes;

    public double productTime; // by minutes
    public double pauseTime; // by minutes

    public double fTimer; // by minutes
    public string feedTime;

    public bool hasFood;

    public List<AProducts> products;
    public List<int> prAmounts;
    public List<int> prXp;
    public AProducts theProduct;

    public APD Clone()
    {
        return new APD()
        {
            animal = this.animal,
            state = this.state,
            a_price = this.a_price,
            a_Xp = this.a_Xp,
            prTimes = this.prTimes,
            productTime = this.productTime,
            pauseTime = this.pauseTime,
            fTimer = this.fTimer,
            feedTime = this.feedTime,
            hasFood = this.hasFood,
            products = this.products,
            theProduct = this.theProduct,
            xp = this.xp,
            amount = this.amount,
            prAmounts = this.prAmounts,
            prXp = this.prXp
        };
    }
}
#endregion

#region Production

[System.Serializable]
public class PrD    //  Product Details
{
    public string name;
    public AState state;
    public Products product;
    public List<PlantCount> p_Used;
    public List<FruitCount> f_used;
    public List<APCount> ap_Used;
    public List<ProductCount> pr_Used;

    public double prTimer;
    public string Time;

    public int amount;
    public int xp;

    public PrD Clone()
    {
        return new PrD()
        {
            state = this.state,
            product = this.product,
            p_Used = this.p_Used,
            f_used = this.f_used,
            ap_Used = this.ap_Used,
            pr_Used = this.pr_Used,
            prTimer = this.prTimer,
            Time = this.Time,
            amount = this.amount,
            xp = this.xp
        };
    }
}

[System.Serializable]
public class MachineProduct
{
    public string MachineName;
    public List<PrD> products;

    public MachineProduct Clone()
    {
        return new MachineProduct()
        {
            MachineName = this.MachineName,
            products = this.products
        };
    }
}

[System.Serializable]
public class PrSprites
{
    public string name;
    public Products product;
    public Sprite sprite;
}

[System.Serializable]
public class MachineSprites
{
    public string name;
    public Machines machine;
    public Sprite sprite;
}

#endregion

#region Boxes

#endregion

#region PlayerInfos

[System.Serializable]
public class Player
{
    public string name;
    public int IDnumber;
    public int ppIndex;

    public int XP;
    public int Coin;
    public int Crystal;
    public int FoodLevel;
    public FoodSystem Food;
    public int WellLevel;
    public WaterSystem Water;
    public float currentChanceOfLB;
}

[System.Serializable]
public class TheFood
{
    public a_f_types Food;
    public Plants material;
    public PlantState PrState;
    public int reqAmount;
    public float pTimer;

    public int collectAmount;
    public float foodTimer;
    public string fillTime;
    public TheFood Clone()
    {
        return new TheFood()
        {
            Food = this.Food,
            material = this.material,
            PrState = this.PrState,
            reqAmount = this.reqAmount,
            pTimer = this.pTimer,
            collectAmount = this.collectAmount,
            foodTimer = this.foodTimer,
            fillTime = this.fillTime
        };
    }
}

[System.Serializable]
public class FoodSystem
{
	public LandState MachState;
    public List<TheFood> materials;

    public List<TheFood> InQueue;
    public int qLimit;

    public List<afAmount> Amounts;
    public int sumAmount;
    public int xp;
}

[System.Serializable]
public class FoodLevelSystem
{
    public Currency currency;
    public int price;

    public int progTimerDec; // by %
    public int foodTimerInc; // by %

    public int productionX;

    public int xp;
}

[System.Serializable]
public class afSprites
{
    public string name;
    public a_f_types food;
    public Sprite sprite;
}

[System.Serializable]
public class afAmount : IStorageItem
{
    public Category category;
    public a_f_types food;
    public int amount;
    public string name;


    public int Count => amount;
    public string Name => name;
    public object Item => food;
}

[System.Serializable]
public class Level
{
    public int LevelNumber;
    public int reqXP;
}
[System.Serializable]
public class LevelRewards
{
    public List<Plants> Plant;
    public List<Fruits> Trees;
    public List<Animals> Animal;
    public List<AProducts> AnimalProduct;
    public List<Products> Product;
    public List<Machines> Machine;
    public int Coin;
    public int Crystal;
}
[System.Serializable]

public class WaterSystem
{
    public string fillTime;
    public double fTimer; // by minute

    public double WateringTimer; // by minute
    public int MaxAmount;
    public int amount;

    public Currency currency;
    public int price;
}

[System.Serializable]
public class LevelSystem
{
    public List<int> xpReqs = new();
    public List<Level> levels = new();

    public LevelSystem(int maxLevel)
    {
        xpReqs.Clear();
        levels.Clear();
        xpReqs.Add(0);

        for (int level = 2; level <= maxLevel; level++)
        {
            double xp = 50 * Math.Pow(level - 1, 2.7);
            xpReqs.Add((int)Math.Round(xp));
        }

        for (int i = 0; i < xpReqs.Count; i++)
        {
            levels.Add(new Level { LevelNumber = i + 1, reqXP = xpReqs[i] });
        }
    }

    public Level GetLevelByXP(int XP)
    {
        for (int i = 1; i < levels.Count; i++)
        {
            if (XP < levels[i].reqXP)
                return levels[i - 1];
        }
        return levels[levels.Count - 1]; // max level
    }

    public Level GetLevelByNumber(int levelNumber)
    {
        if (levelNumber < 1) return levels[0];
        if (levelNumber > levels.Count) return levels[levels.Count - 1];
        return levels[levelNumber - 1];
    }

    public float GetProgressToNextLevel(int XP)
    {
        Level current = GetLevelByXP(XP);
        int index = current.LevelNumber - 1;

        if (index + 1 >= levels.Count) return 1f; // max level

        int currentXP = current.reqXP;
        int nextXP = levels[index + 1].reqXP;

        return (float)(XP - currentXP) / (nextXP - currentXP);
    }
}

#endregion

#region PlayerDatas
    #region stats details
    [System.Serializable]
    public class FarmSlotStats
    {
        public LandState state;
        public int usage;
        public int plowed;
        public int dried;
        public PD PlantDetails;
    }

    [System.Serializable]
    public class TreeSpotStats
    {
        public LandState state;
        public TreeD TreeDetails;
    }

    [System.Serializable]
    public class AnimalsSpotStats
    {
        public ASpotState state;
        public APD AnimalProductDetails;
    }

    [System.Serializable]
    public class MachineStats
    {
        public string MachineName;
        public ASpotState state;
        public int usage;
        public int Fixed;
        public int qLimit;
        public List<PrD> queue;
    }

    [System.Serializable]
    public class TaskSystem
    {
        public List<TaskDetails> Tasks;
    }

#endregion


    [System.Serializable]
    public class Unlocked_Machines
    {
        public Machines machEnum;
        public string MachineName;
        public bool bought;
    }

    [System.Serializable]
    public class u_items
    {
        public List<Plants> u_plants;
        public List<Fruits> u_fruits;
        public List<Animals> u_animals;
        public List<AProducts> u_a_products;
        public List<Unlocked_Machines> u_machines;
        public List<Products> u_Products;
    }

#region Storage Items
    [System.Serializable]
    public class ThingCount : IStorageItem
    {
        public object item;
        public int count;
        public string name;

        public int Count => count;
        public string Name => name;
        public object Item => item;
    }

    public interface IStorageItem
    {
        int Count { get; }
        object Item { get; }
        string Name { get; }
    }

    [System.Serializable]
    public class PlantCount : IStorageItem
    {
        public Plants Plant;
        public int count;
        public string name;

        public int Count => count;
        public string Name => name;
        public object Item => Plant;
    }
    [System.Serializable]

    public class FruitCount : IStorageItem
    {
        public Fruits Fruit;
        public int count;
        public string name;
        public int Count => count;
        public string Name => name;
        public object Item => Fruit;
    }

    [System.Serializable]
    public class APCount : IStorageItem
    {
        public AProducts animal_products;
        public int count;
        public string name;

        public int Count => count;
        public string Name => name;
        public object Item => animal_products;
    }

    [System.Serializable]
    public class ProductCount : IStorageItem
    {
        public Products product;
        public int count;
        public string name;

        public int Count => count;
        public string Name => name;
        public object Item => product;
    }

    [System.Serializable]
    public class ItemCount : IStorageItem
    {
        public Items item;
        public int count;
        public string name;

        public int Count => count;
        public string Name => name;
        public object Item => item;
    }

[System.Serializable]
    public class StorageItems
    {
        public List<PlantCount> PlantsInStorage;
        public List<FruitCount> FruitInStorage;
        public List<APCount> a_p_inStorage;
        public List<ProductCount> ProductsInStorage;
        public List<ItemCount> ItemsInStorage;
    }
    #endregion

[System.Serializable]
public class PlayerDatas
{
    public int saveVersion;

    public Player PlayerInfos;

    public u_items unlocked_items;

    public int land_slot_count;
    public List<FarmSlotStats> FarmSlots;

    public List<TreeSpotStats> TreeSpots;

    public int animal_slot_count;
    public List<AnimalsSpotStats> AnimalSpots;

    public List<MachineStats> MachineStats;

    public StorageItems Storage;
    public int StorageLevel;

    public int ShopSlotCount;

    public PlayerDatas Clone()
    {
        return new PlayerDatas()
        {
            saveVersion = this.saveVersion,
            PlayerInfos = this.PlayerInfos,
            land_slot_count = this.land_slot_count,
            FarmSlots = this.FarmSlots,
            TreeSpots = this.TreeSpots,
            animal_slot_count = this.animal_slot_count,
            AnimalSpots = this.AnimalSpots,
            MachineStats = this.MachineStats,
            Storage = this.Storage,
            StorageLevel = this.StorageLevel,
            ShopSlotCount = this.ShopSlotCount
        };

    }
}
#endregion

[System.Serializable]
public class AStorageLevel
{
    public int LevelNumber;
    public int ItemCount;

    public int ToolSet;

    public int Capacity;

    public AStorageLevel Clone()
    {
        return new AStorageLevel()
        {
            LevelNumber = this.LevelNumber,
            ItemCount = this.ItemCount,
            ToolSet = this.ToolSet,
            Capacity = this.Capacity,
        };
    }
}

[System.Serializable]
public class StorageSprites
{
    public List<PlantSprites> plants;
    public List<PGStages> StageSprites;
    public List<PlantSprites> readySprites;

    public List<FruitSprites> trees;
    public List<TreeStages> TreeStageSprites;
    public List<FruitSprites> fruits;

    public List<AnimalSprites> animals;
    public List<afSprites> AnimalFoodSprites;

    public List<APSprites> a_products;

    public List<PrSprites> products;

    public List<MachineSprites> machines;

    public List<ItemSprites> items;

    public List<Curr_Sprites> currencies;
}

[System.Serializable]
public class Curr_Sprites
{
    public string name;
    public Currency Currency;
    public Sprite sprite;
}

[System.Serializable]
public class ItemSprites
{
    public string name;
    public Items item;
    public Sprite sprite;
}