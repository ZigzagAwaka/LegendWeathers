using BepInEx.Configuration;

namespace LegendWeathers
{
    class Config
    {
        public bool WeatherRegistery = false;
        public readonly ConfigEntry<bool> majoraWeather;
        public readonly ConfigEntry<string> majoraMoonModel;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            majoraWeather = cfg.Bind("Majora Moon", "Enabled", true, "Enable the Majora Moon weather.");
            majoraMoonModel = cfg.Bind("Majora Moon", "Model version", "3DS", new ConfigDescription("Choose the model version of the moon, if you want a more retro look try the N64 version.", new AcceptableValueList<string>("3DS", "N64")));
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
