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
        public GameObject? majoraSkyObject;

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

        private void RegisterWeather<T, T2>(Weathers.LegendWeathers.WeatherInfo info) where T : Component where T2 : Component
        {
            var weatherEffect = new ImprovedWeatherEffect(GetEffect<T2>(), GetEffect<T>());
            RegisterWeather(info, weatherEffect);
        }

        private void RegisterWeather<T>(Weathers.LegendWeathers.WeatherInfo info) where T : Component
        {
            var weatherEffect = new ImprovedWeatherEffect(null, GetEffect<T>());
            RegisterWeather(info, weatherEffect);
        }

        private void RegisterWeather(Weathers.LegendWeathers.WeatherInfo info, ImprovedWeatherEffect weatherEffect)
        {
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

            config = new Config(Config);
            config.SetupCustomConfigs();
            Effects.SetupNetwork();

            if (config.majoraWeather.Value)
            {
                majoraMoonObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraMoon.prefab");
                majoraSkyObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraSky.prefab");
                if (config.majoraMoonModel.Value == "N64")
                {
                    majoraMoonObject.transform.Find("Model1").gameObject.SetActive(true);
                    majoraMoonObject.transform.Find("Model2").gameObject.SetActive(false);
                }
                NetworkPrefabs.RegisterNetworkPrefab(majoraMoonObject);
                RegisterWeather<MajoraMoonWeather, MajoraSkyEffect>(MajoraMoonWeather.weatherInfo);
            }

            HarmonyPatchAll();
            logger.LogInfo(NAME + " is loaded !");
        }
    }
}
