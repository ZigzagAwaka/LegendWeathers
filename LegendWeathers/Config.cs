using BepInEx.Configuration;

namespace LegendWeathers
{
    class Config
    {
        public bool WeatherRegistery = false;
        public readonly ConfigEntry<bool> majoraWeather;
        public readonly ConfigEntry<string> majoraMoonModel;
        public readonly ConfigEntry<float> majoraMoonMusicVolume;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            majoraWeather = cfg.Bind("Majora Moon", "Enabled", true, "Enable the Majora Moon weather.");
            majoraMoonModel = cfg.Bind("Majora Moon", "Model version", "3DS", new ConfigDescription("Choose the model version of the moon, if you want a more retro look try the N64 version.", new AcceptableValueList<string>("3DS", "N64")));
            majoraMoonMusicVolume = cfg.Bind("Majora Moon", "Music volume", 0.9f, new ConfigDescription("When the moon is about to crash, the Final Hours music starts to play. You can customize the music volume here.", new AcceptableValueRange<float>(0f, 1f)));
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
        }
    }
}
