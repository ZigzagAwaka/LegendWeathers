﻿using LegendWeathers.Utils;
using System.Collections;
using UnityEngine;

namespace LegendWeathers.WeatherSkyEffects
{
    public class MajoraSkyEffect : SkyEffect
    {
        private readonly float minWeight = 0.05f;
        private readonly float maxWeight = 0.9f;
        private bool isReversed = false;
        private bool isCoroutineRunning = false;
        private bool isInstant = false;

        public MajoraSkyEffect() : base(Plugin.instance.majoraSkyObject)
        {
            overrideFogVolume = true;
        }

        public override void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                if (RoundManager.Instance.currentLevel.planetHasTime)
                    base.OnEnable();
                else if (Compatibility.IsMoonCompanyCompatible(RoundManager.Instance.currentLevel))
                {
                    isInstant = true;
                    base.OnEnable();
                }
            }
        }

        public override void OnDisable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                    isReversed = false;
                isCoroutineRunning = false;
                isInstant = false;
                base.OnDisable();
            }
        }

        public override void Update()
        {
            if (IsEffectActive && spawnedSkyVolume != null)
            {
                if (isInstant)
                {
                    if (!isReversed && spawnedSkyVolume.weight < maxWeight)
                    {
                        spawnedSkyVolume.weight = maxWeight;
                    }
                    else if (isReversed && !isCoroutineRunning)
                    {
                        spawnedSkyVolume.weight = minWeight;
                    }
                }
                else if (TimeOfDay.Instance.currentDayTimeStarted)
                {
                    if (!isReversed && spawnedSkyVolume.weight < maxWeight)
                    {
                        var endTime = TimeOfDay.Instance.globalTimeAtEndOfDay / 2.1f;
                        spawnedSkyVolume.weight = TimeOfDay.Instance.currentDayTime * maxWeight / endTime;
                    }
                    else if (isReversed && !isCoroutineRunning)
                    {
                        spawnedSkyVolume.weight = minWeight;
                    }
                }
            }
        }

        public void ReverseEffect()
        {
            if (IsEffectActive && spawnedSkyVolume != null && (isInstant || TimeOfDay.Instance.currentDayTimeStarted))
            {
                isCoroutineRunning = true;
                StartCoroutine(ReverseEffectCoroutine());
            }
            isReversed = true;
        }

        private IEnumerator ReverseEffectCoroutine()
        {
            if (spawnedSkyVolume != null)
            {
                var weight = spawnedSkyVolume.weight;
                while (spawnedSkyVolume.weight > minWeight)
                {
                    if (isCoroutineRunning == false)
                        yield break;
                    spawnedSkyVolume.weight -= weight * Time.deltaTime / 5f;  // 5 seconds
                    yield return null;
                }
                spawnedSkyVolume.weight = minWeight;
            }
            isCoroutineRunning = false;
        }

        public void ResetState()
        {
            isReversed = false;
            isCoroutineRunning = false;
        }
    }
}