﻿using BepInEx.Configuration;
using System.Linq;

namespace LegendWeathers
{
    class Config
    {
        public bool WeatherRegistery = false;
        public readonly ConfigEntry<bool> majoraWeather;
        public readonly ConfigEntry<string> majoraMoonModel;
        public readonly ConfigEntry<float> majoraMoonMusicVolume;
        public readonly ConfigEntry<string> majoraMaskValue;
        public (int, int) majoraMaskValueParsed;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            majoraWeather = cfg.Bind("Majora Moon", "Enabled", true, "Enable the Majora Moon weather.");
            majoraMoonModel = cfg.Bind("Majora Moon", "Model version", "3DS", new ConfigDescription("Choose the model version of the moon, if you want a more retro look try the N64 version.", new AcceptableValueList<string>("3DS", "N64")));
            majoraMoonMusicVolume = cfg.Bind("Majora Moon", "Music volume", 0.9f, new ConfigDescription("When the moon is about to crash, the Final Hours music starts to play. You can customize the music volume here.", new AcceptableValueRange<float>(0f, 1f)));
            majoraMaskValue = cfg.Bind("Majora Moon", "Majora Mask Item value", "200,400", "The min,max scrap value of the Majora's Mask item, supposed to be very high. The final value will be randomized between these 2 numbers, but not divided by any external factors.");
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrov.WeatherRegistry"))
            {
                WeatherRegistery = true;
            }
            if (!WeatherRegistery)
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
