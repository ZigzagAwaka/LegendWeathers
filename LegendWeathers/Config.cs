using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System.Linq;

namespace LegendWeathers
{
    class Config
    {
        public bool WeatherRegisteryInstalled = false;
        public bool WeatherTweaksInstalled = false;
        public bool LethalElementsInstalled = false;
        public bool BiodiversityInstalled = false;
        public bool SurfacedInstalled = false;
        public bool PremiumScrapsInstalled = false;
        public bool EmergencyDiceInstalled = false;
        public bool CodeRebirthInstalled = false;
        public bool MrovWeathersInstalled = false;

        public readonly ConfigEntry<bool> majoraWeather;
        public readonly ConfigEntry<string> majoraMoonModel;
        public readonly ConfigEntry<bool> majoraMoonModelAutomatic;
        public readonly ConfigEntry<float> majoraMoonMusicVolume;
        public readonly ConfigEntry<bool> majoraOcarinaCompatible;
        public readonly ConfigEntry<string> majoraMaskValue;
        public (int, int) majoraMaskValueParsed;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            majoraWeather = cfg.Bind("Majora Moon", "Enabled", true, "Enable the Majora Moon weather.");
            majoraMoonModel = cfg.Bind("Majora Moon", "Model version", "3DS", new ConfigDescription("Choose the model version of the moon, if you want a more retro look try the N64 version.\nOther models are also available for fun !", new AcceptableValueList<string>("3DS", "N64", "Faceless", "Boomy", "Owl", "Abibabou", "Joy", "Dice", "Baldy")));
            majoraMoonModelAutomatic = cfg.Bind("Majora Moon", "Automatic model selection", false, "Allows the model to be automatically adjusted on moons based on certain conditions, this will vary depending on your installed mods.");
            majoraMoonMusicVolume = cfg.Bind("Majora Moon", "Music volume", 0.9f, new ConfigDescription("When the moon is about to crash, the Final Hours music starts to play. You can customize the music volume here.", new AcceptableValueRange<float>(0f, 1f)));
            majoraOcarinaCompatible = cfg.Bind("Majora Moon", "Ocarina compatibility", true, "If you have the Ocarina item, playing Oath to Order while in altitude when the moon is about to crash will start a special animation.\nWill not work if ChillaxScraps is not installed.");
            majoraMaskValue = cfg.Bind("Majora Moon", "Majora Mask Item value", "200,400", "The min,max scrap value of the Majora's Mask item, supposed to be very high. The final value will be randomized between these 2 numbers, but not divided by any external factors.");
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
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
            if (!WeatherRegisteryInstalled)
                Plugin.logger.LogError("WeatherRegistery is not installed! Please install WeatherRegistery before using this mod.");
            ParseValues();
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

        private bool IsPluginInstalled(string pluginGUID, string? pluginVersion = null)
        {
            return Chainloader.PluginInfos.ContainsKey(pluginGUID) &&
                (pluginVersion == null || new System.Version(pluginVersion).CompareTo(Chainloader.PluginInfos[pluginGUID].Metadata.Version) <= 0);
        }
    }
}
