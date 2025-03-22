using BepInEx.Configuration;

namespace LegendWeathers
{
    class Config
    {
        public bool WeatherRegistery = false;
        public readonly ConfigEntry<bool> majoraWeather;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            majoraWeather = cfg.Bind("Majora", "Enabled", true, "Enable the Majora weather.");
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
                Plugin.logger.LogError("WeatherRegistery is not installed! Please install WeatherRegistery before playing the game.");
        }
    }
}
