using BepInEx.Bootstrap;
using LethalLib.Modules;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LegendWeathers.Utils
{
    internal class Compatibility
    {
        ////// PLUGIN //////

        public static bool WeatherRegisteryInstalled = false;
        public static bool WeatherTweaksInstalled = false;
        public static bool LethalElementsInstalled = false;
        public static bool BiodiversityInstalled = false;
        public static bool SurfacedInstalled = false;
        public static bool PremiumScrapsInstalled = false;
        public static bool EmergencyDiceInstalled = false;
        public static bool CodeRebirthInstalled = false;
        public static bool MrovWeathersInstalled = false;

        public static void CheckInstalledPlugins()
        {
            WeatherRegisteryInstalled = IsPluginInstalled("mrov.WeatherRegistry");
            WeatherTweaksInstalled = IsPluginInstalled("WeatherTweaks");
            LethalElementsInstalled = IsPluginInstalled("voxx.LethalElementsPlugin", "1.3.0");
            BiodiversityInstalled = IsPluginInstalled("com.github.biodiversitylc.Biodiversity");
            SurfacedInstalled = IsPluginInstalled("Surfaced");
            PremiumScrapsInstalled = IsPluginInstalled("zigzag.premiumscraps");
            EmergencyDiceInstalled = IsPluginInstalled("Theronguard.EmergencyDice");
            CodeRebirthInstalled = IsPluginInstalled("CodeRebirth");
            MrovWeathersInstalled = IsPluginInstalled("MrovWeathers");
        }

        private static bool IsPluginInstalled(string pluginGUID, string? pluginVersion = null)
        {
            return Chainloader.PluginInfos.ContainsKey(pluginGUID) &&
                (pluginVersion == null || new System.Version(pluginVersion).CompareTo(Chainloader.PluginInfos[pluginGUID].Metadata.Version) <= 0);
        }

        ////// MAJORA MOON //////

        internal static bool IsMajoraActiveOnCompany { get; private set; } = false;
        internal static float MajoraCompanyTimer { get; private set; } = 105;  // in seconds

        public static void SetMajoraCompanyCompatible(bool isCompatible)
        {
            IsMajoraActiveOnCompany = isCompatible;
            MajoraCompanyTimer = 105;
        }

        public static float GetCompanySmoothTime(float deltaTime)
        {
            MajoraCompanyTimer -= deltaTime;
            return MajoraCompanyTimer;
        }

        public static bool IsMoonCompanyCompatible(string planetName)
        {
            if (!Plugin.config.majoraCompanyCompatible.Value)
                return false;
            var moonName = Regex.Replace(planetName, "^[0-9]+", string.Empty);
            if (moonName[0] == ' ')
                moonName = moonName[1..];
            if (moonName == "Gordion" || moonName == "Galetry" || moonName == "-Oxyde")
                return true;
            return false;
        }

        public static bool IsMajoraBiodiversityCompatible()
        {
            if (!BiodiversityInstalled || Biodiversity.Creatures.MicBird.MicBirdHandler.Instance == null || RoundManager.Instance == null)
            {
                return false;
            }
            var (levelRarities, customLevelRarities) = Biodiversity.Creatures.BiodiverseAIHandler<Biodiversity.Creatures.MicBird.MicBirdHandler>.ConfigParsing(Biodiversity.Creatures.MicBird.MicBirdHandler.Instance.Config.BoomBirdRarity);
            var moonName = Regex.Replace(RoundManager.Instance.currentLevel.PlanetName, "^[0-9]+", string.Empty);
            if (moonName[0] == ' ')
                moonName = moonName[1..];
            if ((System.Enum.TryParse(moonName, true, out Levels.LevelTypes levelType) && levelRarities.ContainsKey(levelType) && levelRarities[levelType] >= 30)
                || (System.Enum.TryParse(moonName + "Level", true, out Levels.LevelTypes modifLevelType) && levelRarities.ContainsKey(modifLevelType) && levelRarities[modifLevelType] >= 30)
                || (customLevelRarities.ContainsKey(moonName) && customLevelRarities[moonName] >= 30))
                return true;
            return false;
        }

        public static bool IsMajoraSurfacedCompatible()
        {
            if (!SurfacedInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            foreach (var scrap in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (scrap.itemProperties.itemName == "Rodriguez" && scrap.isInFactory)
                    return true;
            }
            return false;
        }

        public static bool IsMajoraPremiumScrapsCompatible()
        {
            if (!PremiumScrapsInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            if (Effects.GetPlayers(includeDead: true).Find(p => p.playerSteamId == 76561198198881967) != default)
                return true;
            foreach (var scrap in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (scrap.itemProperties.itemName == "The talking orb" && scrap.isInFactory)
                    return true;
            }
            return false;
        }

        public static bool IsMajoraEmergencyDiceCompatible()
        {
            if (!EmergencyDiceInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            foreach (var scrap in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (scrap.itemProperties.itemName == "Rusty" && scrap.isInFactory)
                    return true;
            }
            return false;
        }

        public static bool IsMajoraCodeRebirthCompatible()
        {
            if (!CodeRebirthInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            if (Effects.GetPlayers(includeDead: true).Find(p => p.playerSteamId == 76561198984467725) != default)
                return true;
            return false;
        }
    }
}
