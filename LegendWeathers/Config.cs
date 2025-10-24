using BepInEx.Configuration;
using LegendWeathers.Utils;
using System.Collections.Generic;
using System.Linq;

namespace LegendWeathers
{
    class Config
    {
        public readonly ConfigEntry<bool> generalWeatherAlertsSaved;

        public readonly ConfigEntry<bool> majoraWeather;
        public readonly ConfigEntry<string> majoraMoonModel;
        public readonly ConfigEntry<bool> majoraMoonModelAutomatic;
        public readonly ConfigEntry<float> majoraMoonMusicVolume;
        public readonly ConfigEntry<bool> majoraOcarinaCompatible;
        public readonly ConfigEntry<bool> majoraCompanyCompatible;
        public readonly ConfigEntry<string> majoraMaskValue;
        public (int, int) majoraMaskValueParsed;

        public readonly ConfigEntry<bool> bloodMoonWeather;
        public readonly ConfigEntry<string> bloodMoonTexture;
        public readonly ConfigEntry<float> bloodMoonSizeFactor;
        public readonly ConfigEntry<int> bloodMoonEffectsAbundance;
        public readonly ConfigEntry<float> bloodMoonEffectsVolume;
        public readonly ConfigEntry<float> bloodMoonIntroMusicVolume;
        public readonly ConfigEntry<float> bloodMoonAmbientMusicVolume;
        public readonly ConfigEntry<string> bloodMoonAmbientMusicType;
        public readonly ConfigEntry<float> bloodMoonResurrectWaitTime;
        public readonly ConfigEntry<bool> bloodMoonStoneItemSpawning;
        public readonly ConfigEntry<string> bloodMoonStoneSpawnBlacklistStr;
        public readonly ConfigEntry<string> bloodMoonDifficultyFactor;
        public readonly List<string> bloodMoonStoneSpawnBlacklist = new List<string>();

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;

            generalWeatherAlertsSaved = cfg.Bind("_General", "Weather alerts saved", true, "By default, weather alerts are shown once then saved so you'll never get them again just like the vanilla system.\nDisable this config if you prefer to see them during every ship landing.");

            majoraWeather = cfg.Bind("Majora Moon", "Enabled", true, "Enable the Majora Moon weather.");
            majoraMoonModel = cfg.Bind("Majora Moon", "Model version", "3DS", new ConfigDescription("Choose the model version of the moon, if you want a more retro look try the N64 version.\nOther models are also available for fun !", new AcceptableValueList<string>("3DS", "N64", "Faceless", "Boomy", "Owl", "Abibabou", "Joy", "Dice", "Baldy")));
            majoraMoonModelAutomatic = cfg.Bind("Majora Moon", "Automatic model selection", false, "Allows the model to be automatically adjusted on moons based on certain conditions, this will vary depending on your installed mods.");
            majoraMoonMusicVolume = cfg.Bind("Majora Moon", "Music volume", 0.9f, new ConfigDescription("When the moon is about to crash, the Final Hours music starts to play.\nYou can customize the music volume here.", new AcceptableValueRange<float>(0f, 1f)));
            majoraOcarinaCompatible = cfg.Bind("Majora Moon", "Ocarina compatibility", true, "If you have the Ocarina item, playing Oath to Order while in altitude when the moon is about to crash will start a special animation.\nWill not work if ChillaxScraps is not installed.");
            majoraCompanyCompatible = cfg.Bind("Majora Moon", "Company Moon compatibility", false, "By default, the Majora Moon can't spawn on Company Moons but activating this config will make this a reality.\nYou may also need to remove the company moons in the MajoraMoon blacklist section in the WeatherRegistery config file.");
            majoraMaskValue = cfg.Bind("Majora Moon", "Majora Mask Item value", "200,400", "The min,max scrap value of the Majora's Mask item, supposed to be very high.\nThe final value will be randomized between these 2 numbers, but not divided by any external factors.");

            bloodMoonWeather = cfg.Bind("Blood Moon", "Enabled", true, "Enable the Blood Moon weather.");
            bloodMoonTexture = cfg.Bind("Blood Moon", "Moon texture", "Classic", new ConfigDescription("Choose the texture used for the Blood Moon material.", new AcceptableValueList<string>("Classic", "Bright")));
            bloodMoonSizeFactor = cfg.Bind("Blood Moon", "Moon size factor", 5.5f, new ConfigDescription("The size factor of the Blood Moon material compared to the size of the vanilla sun.", new AcceptableValueRange<float>(1f, 10f)));
            bloodMoonEffectsAbundance = cfg.Bind("Blood Moon", "Terrain effects abundance", 30, new ConfigDescription("The abundance of terrain effects during the Blood Moon weather.\nHigher values will spawn more effects but may impact performance.", new AcceptableValueRange<int>(0, 100)));
            bloodMoonEffectsVolume = cfg.Bind("Blood Moon", "Terrain effects volume", 1f, new ConfigDescription("The Blood Moon's terrain effects all have small sfx coming from them. Those sfx volumes are not very high but if you really want to reduce it then this config is for you.", new AcceptableValueRange<float>(0f, 1f)));
            bloodMoonIntroMusicVolume = cfg.Bind("Blood Moon", "Intro music volume", 0.8f, new ConfigDescription("An introduction music is played when the Blood Moon is spawned on the planet, this will last 30s.\nYou can customize the music volume here.", new AcceptableValueRange<float>(0f, 1f)));
            bloodMoonAmbientMusicVolume = cfg.Bind("Blood Moon", "Ambient music volume", 1f, new ConfigDescription("The volume for the Blood Moon ambient music when you are outside (similar to the vanilla Eclipsed ambient music).", new AcceptableValueRange<float>(0f, 1f)));
            bloodMoonAmbientMusicType = cfg.Bind("Blood Moon", "Ambient music type", "Blood Moon", new ConfigDescription("If you prefer to have the vanilla ambient Eclipsed music instead of the Blood Moon ambient custom music, then you can change it here.\nPlease note that the 'Ambient music volume' config will not apply if you select the Eclipsed music.", new AcceptableValueList<string>("Blood Moon", "Eclipsed")));
            bloodMoonResurrectWaitTime = cfg.Bind("Blood Moon", "Enemy resurrection wait time", 10f, new ConfigDescription("When an enemy is resurrected by the Blood Moon effect, this is the time in seconds it will take before it actually respawns.", new AcceptableValueRange<float>(0f, 30f)));
            bloodMoonStoneItemSpawning = cfg.Bind("Blood Moon", "Blood Stone spawning", true, "Allow the spawning of the Blood Stone item during specific conditions.");
            bloodMoonStoneSpawnBlacklistStr = cfg.Bind("Blood Moon", "Blood Stone spawning blacklist", "", "Comma separated list of enemy names that will never spawn Blood Stones when they are killed.");
            bloodMoonDifficultyFactor = cfg.Bind("Blood Moon", "Difficulty", "Normal", new ConfigDescription("The difficulty factor of the Blood Moon effect, this will impact the number of spawned enemies during the weather.", new AcceptableValueList<string>("Easy", "Normal", "Hard")));

            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
            Compatibility.CheckInstalledPlugins();
            if (!Compatibility.WeatherRegisteryInstalled)
                Plugin.logger.LogError("WeatherRegistery is not installed! Please install WeatherRegistery before using this mod.");
            ParseValues();
            PopulateList();
        }

        private void ParseValues()
        {
            if (majoraMaskValue.Value == "")
            { majoraMaskValueParsed = (-1, -1); return; }
            var valueTab = majoraMaskValue.Value.Split(',').Select(s => s.Trim()).ToArray();
            if (valueTab.Count() != 2)
            { majoraMaskValueParsed = (-1, -1); return; }
            if (!int.TryParse(valueTab[0], out var minV) || !int.TryParse(valueTab[1], out var maxV))
            { majoraMaskValueParsed = (-1, -1); return; }
            if (minV > maxV)
            { majoraMaskValueParsed = (-1, -1); return; }
            majoraMaskValueParsed = (minV, maxV);
        }

        private void PopulateList()
        {
            if (string.IsNullOrWhiteSpace(bloodMoonStoneSpawnBlacklistStr.Value))
                return;
            foreach (string value in bloodMoonStoneSpawnBlacklistStr.Value.Split(',').Select(s => s.Trim()))
            {
                bloodMoonStoneSpawnBlacklist.Add(value.ToLower());
            }
        }
    }
}
