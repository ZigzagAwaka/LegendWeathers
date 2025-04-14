namespace LegendWeathers.WeatherSkyEffects
{
    public class MajoraSkyEffect : SkyEffect
    {
        private readonly float maxWeight = 0.9f;

        public MajoraSkyEffect() : base(Plugin.instance.majoraSkyObject) { }

        public override void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished && RoundManager.Instance.currentLevel.planetHasTime)
                base.OnEnable();
        }

        public override void Update()
        {
            if (isEffectReady && spawnedSkyVolume != null)
            {
                if (TimeOfDay.Instance.currentDayTimeStarted && spawnedSkyVolume.weight < maxWeight)
                {
                    var endTime = TimeOfDay.Instance.globalTimeAtEndOfDay / 2.1f;
                    spawnedSkyVolume.weight = TimeOfDay.Instance.currentDayTime * maxWeight / endTime;
                }
            }
        }
    }
}
