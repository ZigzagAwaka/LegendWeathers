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
using WeatherRegistry.Editor;

namespace LegendWeathers
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("WeatherTweaks", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("voxx.LethalElementsPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.biodiversitylc.Biodiversity", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Surfaced", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("zigzag.premiumscraps", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Theronguard.EmergencyDice", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("CodeRebirth", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("MrovWeathers", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("giosuel.Imperium", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.legendweathers";
        const string NAME = "LegendWeathers";
        const string VERSION = "1.1.11";

        public static Plugin instance;
        public static ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;

        public WeatherDefinition? majoraMoonDefinition;
        public GameObject? majoraMoonObject;
        public GameObject? majoraSkyObject;
        public Item? majoraMaskItem;
        public Item? majoraMoonTearItem;
        public Sprite? vanillaItemIcon;

        public WeatherDefinition? bloodMoonDefinition;
        public GameObject? bloodMoonManagerObject;
        public GameObject? bloodSkyObject;
        public GameObject? bloodSunObject;
        public GameObject? bloodParticlesObject;
        public GameObject? bloodTerrainEffectObject;

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

        private void RegisterWeather<T, T2>(WeatherDefinition definition) where T : LegendWeather where T2 : SkyEffect
        {
            var weatherEffect = new ImprovedWeatherEffect(GetEffect<T2>(), GetEffect<T>());
            RegisterWeather(definition, weatherEffect);
        }

        private void RegisterWeather<T>(WeatherDefinition definition) where T : LegendWeather
        {
            var weatherEffect = new ImprovedWeatherEffect(null, GetEffect<T>());
            RegisterWeather(definition, weatherEffect);
        }

        private void RegisterWeather(WeatherDefinition definition, ImprovedWeatherEffect weatherEffect)
        {
            var weather = new Weather(definition.Name, weatherEffect)
            {
                Config = definition.Config.CreateFullConfig(),
                Color = definition.Color
            };
            WeatherManager.RegisterWeather(weather);
        }

        private void RegisterMajora(AssetBundle bundle, string directory)
        {
            majoraMoonDefinition = bundle.LoadAsset<WeatherDefinition>(directory + "MajoraMoon/MajoraMoonDefinition.asset");
            majoraMoonObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraMoon.prefab");
            majoraSkyObject = bundle.LoadAsset<GameObject>(directory + "MajoraMoon/MajoraSky.prefab");
            majoraMaskItem = bundle.LoadAsset<Item>(directory + "MajoraMoon/Items/MajoraMask/MajoraMaskItem.asset");
            majoraMoonTearItem = bundle.LoadAsset<Item>(directory + "MajoraMoon/Items/MoonTear/MoonTearItem.asset");
            vanillaItemIcon = bundle.LoadAsset<Sprite>(directory + "MajoraMoon/Items/MajoraMask/ScrapItemIcon2.png");
            if (!config.majoraMoonModel.Value.Equals(config.majoraMoonModel.DefaultValue))
            {
                MajoraMoon.CheckAndReplaceModel();
                MajoraMaskItem.CheckAndReplaceModel();
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
            RegisterWeather<MajoraMoonWeather, MajoraSkyEffect>(majoraMoonDefinition);
        }

        private void RegisterBloodMoon(AssetBundle bundle, string directory)
        {
            bloodMoonDefinition = bundle.LoadAsset<WeatherDefinition>(directory + "BloodMoon/BloodMoonDefinition.asset");
            bloodMoonManagerObject = bundle.LoadAsset<GameObject>(directory + "BloodMoon/BloodMoonManager.prefab");
            bloodSkyObject = bundle.LoadAsset<GameObject>(directory + "BloodMoon/BloodSky.prefab");
            bloodSunObject = bundle.LoadAsset<GameObject>(directory + "BloodMoon/SunBloodTexture.prefab");
            bloodParticlesObject = bundle.LoadAsset<GameObject>(directory + "BloodMoon/BloodRainParticles.prefab");
            bloodTerrainEffectObject = bundle.LoadAsset<GameObject>(directory + "BloodMoon/BloodTerrainEffect.prefab");
            if (!config.bloodMoonTexture.Value.Equals(config.bloodMoonTexture.DefaultValue))
            {
                BloodSkyEffect.CheckAndReplaceTexture();
            }
            NetworkPrefabs.RegisterNetworkPrefab(bloodMoonManagerObject);
            RegisterWeather<BloodMoonWeather, BloodSkyEffect>(bloodMoonDefinition);
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

            if (config.bloodMoonWeather.Value)
            {
                RegisterBloodMoon(bundle, directory);
            }

            if (Compatibility.WeatherTweaksInstalled)
            {
                TweaksIntegration.Setup();
            }

            HarmonyPatchAll();
            logger.LogInfo(NAME + " is loaded !");
        }
    }
}
