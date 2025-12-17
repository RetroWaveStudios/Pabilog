using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");

    private static readonly PlayerDatas default_datas = new PlayerDatas()
    {
        saveVersion = 0,
        PlayerInfos = new Player()
        {
            name = null,
            IDnumber = 0,
            ppIndex = 0,

            XP = 0,
            Coin = 1000,
            Crystal = 10,
            FoodLevel = 1,
            Food = new FoodSystem()
            {
                materials = new List<TheFood>(),
                MachState = LandState.Empty,

                InQueue = new(),

                qLimit = 1,
                Amounts = new List<afAmount>()
                {
                    new afAmount()
                    {
                        food = a_f_types.Wheat,
                        amount = 5
                    },
                    new afAmount()
                    {
                        food = a_f_types.Corn,
                        amount = 0
                    },
                    new afAmount()
                    {
                        food = a_f_types.Carrot,
                        amount = 0
                    },
                    new afAmount()
                    {
                        food = a_f_types.Potato,
                        amount = 0
                    }
                },
                sumAmount = 0,
                xp = 20
            },
            WellLevel = 1,
            Water = new WaterSystem()
            {
                fillTime = "",
                fTimer = 0.5, // by minutes
                WateringTimer = 3,
                MaxAmount = 20,
                amount = 20,

                currency = Currency.Coin,
                price = 350
            },
            currentChanceOfLB = 5f
        },
        unlocked_items = new u_items()
        {
            u_plants = new List<u_Plants>()
            {
                new() { plant = Plants.Wheat,      owned = true },
                new() { plant = Plants.Corn,       owned = true },
                new() { plant = Plants.Carrot,     owned = true },
                new() { plant = Plants.SugarCane,  owned = true },
                new() { plant = Plants.Potato,     owned = true },
                new() { plant = Plants.Cotton,     owned = false },
                new() { plant = Plants.Herbs,      owned = false },
                new() { plant = Plants.Tomato,     owned = true },
                new() { plant = Plants.Onion,      owned = false },

            },
            u_fruits = new List<u_Fruits>()
            {
                new() { fruit = Fruits.Apple,  owned = true },
                new() { fruit = Fruits.Cherry, owned = true },
                new() { fruit = Fruits.Berry,  owned = false },
                new() { fruit = Fruits.Banana, owned = false },
                new() { fruit = Fruits.Orange, owned = false },
                new() { fruit = Fruits.Cacao,  owned = false },
            },
            u_animals = new List<u_Animals>()
            {
                new() { animal = Animals.Chicken, owned = true },
                new() { animal = Animals.Cow,     owned = true },
                new() { animal = Animals.Sheep,   owned = false },
                new() { animal = Animals.Pig,     owned = true }
            },
            u_a_products = new List<u_AProducts>()
            {
                new() { animalProduct = AProducts.Egg,      owned = true },
                new() { animalProduct = AProducts.Ch_Meat,  owned = true },
                new() { animalProduct = AProducts.Milk,     owned = true },
                new() { animalProduct = AProducts.Cow_Meat, owned = true },
                new() { animalProduct = AProducts.Wool,     owned = false },
                new() { animalProduct = AProducts.Bacon,    owned = true },
            },
            u_Products = new List<u_Products>()
            {
                new() { Product = Products.Cream,            owned = true},
                new() { Product = Products.Butter,           owned = true},
                new() { Product = Products.Cheese,           owned = true},

                new() { Product = Products.Bread,            owned = true},
                new() { Product = Products.CornBread,        owned = true},
                new() { Product = Products.Cookie,           owned = true},
                new() { Product = Products.Pizza,            owned = true},
                new() { Product = Products.ChickenPizza,     owned = true},

                new() { Product = Products.Sugar,            owned = true},
                new() { Product = Products.AppleJuice,       owned = true},
                new() { Product = Products.BerryJuice,       owned = false},

                new() { Product = Products.ApplePie,         owned = false},
                new() { Product = Products.CarrotPie,        owned = false},
                new() { Product = Products.BaconPie,         owned = false},

                new() { Product = Products.Popcorn,          owned = true},
                new() { Product = Products.ButterPopcorn,    owned = false},
                new() { Product = Products.SpicyPopcorn,     owned = false},

                new() { Product = Products.CarrotCake,       owned = false},
                new() { Product = Products.CreamCake,        owned = false},
                new() { Product = Products.BerryCake,        owned = false},

                new() { Product = Products.Soup,             owned = false},
                new() { Product = Products.ChickenSoup,      owned = false},
                new() { Product = Products.Stew,             owned = false},
                new() { Product = Products.Steak,            owned = false},
                new() { Product = Products.RoastedCord,      owned = false},
                new() { Product = Products.Gratin,           owned = false},

                new() { Product = Products.PigmentRed,       owned = false},
                new() { Product = Products.PigmentBlue,      owned = false},
                new() { Product = Products.PigmentYellow,    owned = false},
                new() { Product = Products.PigmentGreen,     owned = false},
                new() { Product = Products.PigmentBrown,     owned = false},
                new() { Product = Products.PigmentPurple,    owned = false},
                new() { Product = Products.PigmentOrange,    owned = false},

                new() { Product = Products.Red,              owned = false},
                new() { Product = Products.Blue,             owned = false},
                new() { Product = Products.Yellow,           owned = false},
                new() { Product = Products.Green,            owned = false},
                new() { Product = Products.Brown,            owned = false},
                new() { Product = Products.Purple,           owned = false},
                new() { Product = Products.Orange,           owned = false},

                new() { Product = Products.VanillaIcecream,  owned = false},
                new() { Product = Products.CherryIcecream,   owned = false},
                new() { Product = Products.OilyIcecream,     owned = false},
                new() { Product = Products.CherryIcecream,   owned = false},

                new() { Product = Products.SimpleTShirt,     owned = false},
                new() { Product = Products.Hat,              owned = false},
                new() { Product = Products.Sweater,          owned = false},
            },
            u_machines = new List<u_Machines>()
            {
                new() { MachineName = "Oven",                   owned = true},
                new() { MachineName = "Dairy Churn",            owned = true},
                new() { MachineName = "Presser",                owned = true},
                new() { MachineName = "Popcorn Machine",        owned = true},
                new() { MachineName = "Pie Oven",               owned = false},
                new() { MachineName = "Cake House",             owned = false},
                new() { MachineName = "Stove",                  owned = false},
                new() { MachineName = "Color Pigment Machine",  owned = false},
                new() { MachineName = "Color Producer Machine", owned = false},
                new() { MachineName = "Icecream Stand",         owned = false},
            }
        },
        land_slot_count = 3,
        FarmSlots = new List<FarmSlotStats>()
        {
            new()
            {
                state = LandState.Empty,
                PlantDetails = new PD()
                {
                    plant = Plants.None
                }
            }
        },

        TreeSpots = new List<TreeSpotStats>(),
        animal_slot_count = 1,
        AnimalSpots = new List<AnimalsSpotStats>()
        {
            new()
            {
                state = ASpotState.Empty,
                AnimalProductDetails = new APD()
                {
                    animal = Animals.None,
                    theProduct = AProducts.None
                }
            }
        },

        Storage = new StorageItems()
        {
            PlantsInStorage = new List<PlantCount>()
            {
                new()
                {
                    Plant = Plants.Wheat,
                    count = 3
                }
            }
        },

        StorageLevel = 1,
        ShopSlotCount = 3
    };
    private const int CURRENT_VERSION = 1;

    public static void SaveGameData(PlayerDatas data)
    {
        data.saveVersion = CURRENT_VERSION;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        StaticDatas.save_queue = 0;
    }

    public static PlayerDatas LoadGameData()
    {
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerDatas pd = JsonUtility.FromJson<PlayerDatas>(json);
            pd = SaveDataMerger.MergeWithDefaults(pd, default_datas);
            return pd;
        }
        else
        {
            SaveGameData(default_datas);
            Debug.LogWarning("Save file not found in " + path);
            return default_datas; // Return a new instance with default values
        }
    }
}
/*
    "Storage": {
        "PlantsInStorage": [
            {
                "Plant": 0,
                "count": 13
            },
            {
                "Plant": 1,
                "count": 10
            }
        ],
        "a_p_inStorage": [
            {
                "animal_products": 0,
                "count": 1
            }
        ],
        "ProductsInStorage": []
    },
*/
