namespace LegendWeathers.Weathers
{
    public class BloodMoonWeather : LegendWeather
    {
        public static string weatherAlert = "";

        public BloodMoonWeather() : base(Plugin.instance.bloodMoonDefinition) { }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            EnableVanillaSun(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            EnableVanillaSun(true);
        }
    }
}
