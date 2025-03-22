using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LegendWeathers.Utils;
using LegendWeathers.Weathers;
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
        const string VERSION = "0.1.0";

        public static Plugin instance;
        public static ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;

        public GameObject? majoraMoonObject;

        void HarmonyPatchAll()
        {
            harmony.PatchAll();
        }

        private GameObject GetEffect(GameObject objectPrefab)
        {
            var effect = Instantiate(objectPrefab);
            effect.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(effect);
            return effect;
        }

        private void RegisterWeather(GameObject objectPrefab, Weathers.LegendWeathers.WeatherInfo info)
        {
            var weatherEffect = new ImprovedWeatherEffect(null, GetEffect(objectPrefab));
            var weather = new Weather(info.name, weatherEffect);
            weather.Config.DefaultWeight.DefaultValue = info.weight;
            weather.Config.ScrapAmountMultiplier.DefaultValue = info.scrapAmount;
            weather.Config.ScrapValueMultiplier.DefaultValue = info.scrapValue;
            weather.Config.FilteringOption.DefaultValue = false;
            weather.Config.LevelFilters.DefaultValue = "Gordion;Galetry;";
            weather.Color = info.color;
            WeatherManager.RegisterWeather(weather);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        void Awake()
        {
            instance = this;
            logger = Logger;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "legendweathers");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            string directory = "Assets/Data/_Misc/LegendWeathers/";
            majoraMoonObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraMoon.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(majoraMoonObject);

            config = new Config(Config);
            config.SetupCustomConfigs();
            Effects.SetupNetwork();

            RegisterWeather(majoraMoonObject, MajoraMoonWeather.weatherInfo);

            HarmonyPatchAll();
            logger.LogInfo("LegendWeathers is loaded !");
        }
    }
}
