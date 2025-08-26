using BepInEx.Configuration;
using LegendWeathers.Utils;
using System.Linq;

namespace LegendWeathers
{
    class Config
    {
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

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;

            majoraWeather = cfg.Bind("Majora Moon", "Enabled", true, "Enable the Majora Moon weather.");
            majoraMoonModel = cfg.Bind("Majora Moon", "Model version", "3DS", new ConfigDescription("Choose the model version of the moon, if you want a more retro look try the N64 version.\nOther models are also available for fun !", new AcceptableValueList<string>("3DS", "N64", "Faceless", "Boomy", "Owl", "Abibabou", "Joy", "Dice", "Baldy")));
            majoraMoonModelAutomatic = cfg.Bind("Majora Moon", "Automatic model selection", false, "Allows the model to be automatically adjusted on moons based on certain conditions, this will vary depending on your installed mods.");
            majoraMoonMusicVolume = cfg.Bind("Majora Moon", "Music volume", 0.9f, new ConfigDescription("When the moon is about to crash, the Final Hours music starts to play.\nYou can customize the music volume here.", new AcceptableValueRange<float>(0f, 1f)));
            majoraOcarinaCompatible = cfg.Bind("Majora Moon", "Ocarina compatibility", true, "If you have the Ocarina item, playing Oath to Order while in altitude when the moon is about to crash will start a special animation.\nWill not work if ChillaxScraps is not installed.");
            majoraCompanyCompatible = cfg.Bind("Majora Moon", "Company Moon compatibility", false, "By default, the Majora Moon can't spawn on Company Moons but activating this config will make this a reality.\nYou may also need to remove the company moons in the MajoraMoon blacklist section in the WeatherRegistery config file.");
            majoraMaskValue = cfg.Bind("Majora Moon", "Majora Mask Item value", "200,400", "The min,max scrap value of the Majora's Mask item, supposed to be very high.\nThe final value will be randomized between these 2 numbers, but not divided by any external factors.");

            bloodMoonWeather = cfg.Bind("Blood Moon", "Enabled", true, "Enable the Blood Moon weather.");
            bloodMoonTexture = cfg.Bind("Blood Moon", "Moon texture", "Classic", new ConfigDescription("Choose the texture used for the Blood Moon material.", new AcceptableValueList<string>("Classic", "Bright")));

            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
            Compatibility.CheckInstalledPlugins();
            if (!Compatibility.WeatherRegisteryInstalled)
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
    }
}
