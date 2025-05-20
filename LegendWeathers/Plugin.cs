using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LegendWeathers.BehaviourScripts;
using LegendWeathers.Utils;
using LegendWeathers.Weathers;
using LegendWeathers.WeatherSkyEffects;
using LethalLib.Modules;
using System.IO;
using System.Reflection;
using UnityEngine;
using WeatherRegistry;

namespace LegendWeathers
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.legendweathers";
        const string NAME = "LegendWeathers";
        const string VERSION = "1.1.3";

        public static Plugin instance;
        public static ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;

        public GameObject? majoraMoonObject;
        public GameObject? majoraSkyObject;
        public Item? majoraMaskItem;
        public Item? majoraMoonTearItem;

        void HarmonyPatchAll()
        {
            harmony.PatchAll();
        }

        private GameObject GetEffect<T>() where T : Component
        {
            var effect = Instantiate(new GameObject(typeof(T).Name));
            effect.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(effect);
            effect.AddComponent<T>();
            return effect;
        }

        private void RegisterWeather<T, T2>(LegendWeather.WeatherInfo info) where T : LegendWeather where T2 : SkyEffect
        {
            var weatherEffect = new ImprovedWeatherEffect(GetEffect<T2>(), GetEffect<T>());
            RegisterWeather(info, weatherEffect);
        }

        private void RegisterWeather<T>(LegendWeather.WeatherInfo info) where T : LegendWeather
        {
            var weatherEffect = new ImprovedWeatherEffect(null, GetEffect<T>());
            RegisterWeather(info, weatherEffect);
        }

        private void RegisterWeather(LegendWeather.WeatherInfo info, ImprovedWeatherEffect weatherEffect)
        {
            var weather = new Weather(info.name, weatherEffect);
            weather.Config.DefaultWeight.DefaultValue = info.weight;
            weather.Config.ScrapAmountMultiplier.DefaultValue = info.scrapAmount;
            weather.Config.ScrapValueMultiplier.DefaultValue = info.scrapValue;
            weather.Config.FilteringOption.DefaultValue = false;
            weather.Config.LevelFilters.DefaultValue = "Gordion;Galetry;Cosmocos;Black Mesa;";
            weather.Color = info.color;
            WeatherManager.RegisterWeather(weather);
        }

        private void RegisterMajora(AssetBundle bundle, string directory)
        {
            majoraMoonObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraMoon.prefab");
            majoraSkyObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraSky.prefab");
            majoraMaskItem = bundle.LoadAsset<Item>(directory + "MajoraMoon/Items/MajoraMask/MajoraMaskItem.asset");
            majoraMoonTearItem = bundle.LoadAsset<Item>(directory + "MajoraMoon/Items/MoonTear/MoonTearItem.asset");
            if (!config.majoraMoonModel.Value.Equals(config.majoraMoonModel.DefaultValue))
            {
                MajoraMoon.CheckAndReplaceModel();
                if (config.majoraMoonModel.Value == "N64")
                {
                    majoraMaskItem.spawnPrefab.transform.Find("Model/Model1").gameObject.SetActive(true);
                    majoraMaskItem.spawnPrefab.transform.Find("Model/Model2").gameObject.SetActive(false);
                    majoraMaskItem.spawnPrefab.GetComponent<MajoraMaskItem>().headMaskPrefab.transform.Find("Model/Model1").gameObject.SetActive(true);
                    majoraMaskItem.spawnPrefab.GetComponent<MajoraMaskItem>().headMaskPrefab.transform.Find("Model/Model2").gameObject.SetActive(false);
                }
            }
            NetworkPrefabs.RegisterNetworkPrefab(majoraMoonObject);
            NetworkPrefabs.RegisterNetworkPrefab(majoraMaskItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(majoraMoonTearItem.spawnPrefab);
            Utilities.FixMixerGroups(majoraMaskItem.spawnPrefab);
            Utilities.FixMixerGroups(majoraMoonTearItem.spawnPrefab);
            if (config.majoraMaskValueParsed.Item1 != -1)
            {
                majoraMaskItem.minValue = (int)(config.majoraMaskValueParsed.Item1 * 2.5f);
                majoraMaskItem.maxValue = (int)(config.majoraMaskValueParsed.Item2 * 2.5f);
            }
            Items.RegisterScrap(majoraMaskItem, 0, Levels.LevelTypes.None);
            Items.RegisterScrap(majoraMoonTearItem, 0, Levels.LevelTypes.None);
            RegisterWeather<MajoraMoonWeather, MajoraSkyEffect>(MajoraMoonWeather.weatherInfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        void Awake()
        {
            instance = this;
            logger = Logger;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "legendweathers");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            string directory = "Assets/Data/_Misc/LegendWeathers/";

            config = new Config(Config);
            config.SetupCustomConfigs();
            Effects.SetupNetwork();

            if (config.majoraWeather.Value)
            {
                RegisterMajora(bundle, directory);
            }

            HarmonyPatchAll();
            logger.LogInfo(NAME + " is loaded !");
        }
    }
}
