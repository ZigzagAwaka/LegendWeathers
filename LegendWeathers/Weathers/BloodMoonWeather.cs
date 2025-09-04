using LegendWeathers.WeatherSkyEffects;
using WeatherRegistry;

namespace LegendWeathers.Weathers
{
    public class BloodMoonWeather : LegendWeather
    {
        public static string weatherAlert = "";

        public static ImprovedWeatherEffect? BloodMoonEffectReference { get; private set; } = null;

        public BloodMoonWeather() : base(Plugin.instance.bloodMoonDefinition) { }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!WeatherManager.IsSetupFinished)
                return;
            BloodMoonEffectReference = ConfigHelper.ResolveStringToWeather("bloodmoon").Effect;
            EnableVanillaSun(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (!WeatherManager.IsSetupFinished)
                return;
            BloodMoonEffectReference?.EffectObject?.GetComponent<BloodSkyEffect>()?.ResetState();
            BloodMoonEffectReference = null;
            EnableVanillaSun(true);
        }
    }
}
