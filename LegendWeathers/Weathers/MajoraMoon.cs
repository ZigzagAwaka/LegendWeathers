using LegendWeathers.BehaviourScripts;
using LegendWeathers.Utils;
using LegendWeathers.WeatherSkyEffects;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

namespace LegendWeathers.Weathers
{
    public class MajoraMoon : NetworkBehaviour
    {
        public AudioSource finalHoursAudio = null!;
        public AudioSource sfxAudio = null!;
        public AudioSource crashAudio = null!;
        public AudioClip[] sfx = null!;
        public AudioClip callOfTheGiantsMusic = null!;
        public AudioClip stopMoonSfx = null!;
        public GameObject finalHoursTimer = null!;
        public ParticleSystem crashParticles1 = null!;
        public ParticleSystem crashParticles2 = null!;
        public GameObject impactObject = null!;
        public Transform tearPosition = null!;
        public ParticleSystem burstParticles1 = null!;
        public ParticleSystem burstParticles2 = null!;
        public VisualEffect burstVFX = null!;
        public Animator burstAnimator = null!;
        public static string modelName = "3DS";

        private readonly int moonRadiusApprox = 19;
        private readonly float endSizeFactor = 7.3f;
        private Vector3 startPosition;
        private Vector3 startRotation;
        private Vector3 startScale;
        private Vector3 endPosition;
        private Vector3 outsideNodeEndPosition;
        private Vector3 endRotation;
        private Vector3 endScale;
        private float startTime;
        private float endTime;
        private readonly float moonFallTimerOffset = 45;
        private bool isInitialized = false;

        private float endTimeFactor = 1.8f;
        private float smoothTime = 0;
        private Vector3 smoothPosVelocity;
        private Vector3 smoothRotVelocity;
        private Vector3 smoothScaVelocity;
        private float previousSpeedMultiplier = 1f;

        private float finalHoursTime;
        public bool finalHoursPlayingMusic = false;
        public bool finalHoursDisplayingTimer = false;
        public bool finalHoursPlayingParticles = false;
        public bool finalHoursFinishing = false;
        public bool oathToOrderStopingMoon = false;

        private List<VisualEnvironment> visualEnvironments = new List<VisualEnvironment>();
        private List<float> originalWindSpeeds = new List<float>();
        private readonly int windSpeedFactor = 17;

        private GameObject? timerUI;
        private TMP_Text? timerText;
        private bool lastMinute = false;
        private bool timerStarted = false;

        private GameObject? impact;
        private readonly float impactScaleFactor = 1.4f;
        private readonly int impactGroundPosOffset = 100;
        private bool impactStarted = false;
        private StartMatchLever? shipLever;

        private float lastRumbleEventTime = 0;
        private float nextRumbleEventTime = 35;
        private float lastTearEventTime = 0;
        private float nextTearEventTime = 60;
        private float lastBellSfxEvent = 0;
        private bool isRareTearEventDay = false;
        private int rareTearEventDayNB = 0;

        private Coroutine? stopMoonCoroutine = null;
        private bool collidersDisabled = false;

        public bool AccelerateEndTimeFactor()
        {
            if (finalHoursPlayingParticles || oathToOrderStopingMoon)
                return false;
            endTimeFactor += finalHoursPlayingMusic ? 0.3f : 0.6f;
            return true;
        }

        public float GetMoonFallAccelerateFactor(float timeFactor)
        {
            return endTimeFactor switch
            {
                2.1f => 0.05f,
                2.4f => 0.09f,
                _ => 0
            } + timeFactor;
        }

        public void StopMoonCrash()
        {
            if (finalHoursFinishing || !Plugin.config.majoraOcarinaCompatible.Value)
                return;
            oathToOrderStopingMoon = true;
            if (stopMoonCoroutine != null)
                StopCoroutine(stopMoonCoroutine);
            stopMoonCoroutine = StartCoroutine(StopMoonAnimation());
        }

        public void Update()
        {
            if (!isInitialized)
                return;
            if (finalHoursPlayingMusic)
                Effects.StopVanillaMusic();
            if (oathToOrderStopingMoon)
                return;
            if (Compatibility.IsMajoraActiveOnCompany)
                smoothTime = Compatibility.GetCompanySmoothTime(Time.deltaTime);
            else if (TimeOfDay.Instance.currentDayTimeStarted)
            {
                if (previousSpeedMultiplier != TimeOfDay.Instance.globalTimeSpeedMultiplier)
                {
                    previousSpeedMultiplier = TimeOfDay.Instance.globalTimeSpeedMultiplier;
                    smoothPosVelocity = default;
                    smoothRotVelocity = default;
                    smoothScaVelocity = default;
                }
                if (startTime == default)
                    startTime = TimeOfDay.Instance.currentDayTime - moonFallTimerOffset;
                endTime = TimeOfDay.Instance.globalTimeAtEndOfDay / endTimeFactor;
                smoothTime = (endTime - TimeOfDay.Instance.currentDayTime) / TimeOfDay.Instance.globalTimeSpeedMultiplier;
                finalHoursTime = endTime - 280;
            }
            if (smoothTime != 0 && !Effects.IsTimePaused())
            {
                if (Compatibility.IsMajoraActiveOnCompany)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref smoothPosVelocity, smoothTime);
                    transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, endRotation, ref smoothRotVelocity, smoothTime);
                    transform.localScale = Vector3.SmoothDamp(transform.localScale, endScale, ref smoothScaVelocity, smoothTime);
                }
                else
                {
                    var timeFactor = (TimeOfDay.Instance.currentDayTime - moonFallTimerOffset - startTime) / (endTime - startTime);
                    transform.position = Vector3.Lerp(startPosition, endPosition - (Vector3.up * 2), GetMoonFallAccelerateFactor(timeFactor));
                    transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, GetMoonFallAccelerateFactor(timeFactor));
                    transform.localScale = Vector3.Lerp(startScale, endScale, GetMoonFallAccelerateFactor(timeFactor));
                }
            }
            PlayRandomEvents();
            FinalHours();
        }

        private void FinalHours()
        {
            if (!finalHoursPlayingMusic && (Compatibility.IsMajoraActiveOnCompany || TimeOfDay.Instance.currentDayTime >= finalHoursTime + 30))
            {
                StartMusic();
                finalHoursPlayingMusic = true;
            }
            if (!finalHoursDisplayingTimer && ((!Compatibility.IsMajoraActiveOnCompany && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 50)
                                                || (Compatibility.IsMajoraActiveOnCompany && Compatibility.MajoraCompanyTimer <= 91)))
            {
                StartTimer();
                Effects.IncreaseWindSpeed(visualEnvironments, windSpeedFactor);
                finalHoursDisplayingTimer = true;
            }
            if (!finalHoursPlayingParticles && ((!Compatibility.IsMajoraActiveOnCompany && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 205)
                                                || (Compatibility.IsMajoraActiveOnCompany && Compatibility.MajoraCompanyTimer <= 33)))
            {
                StartParticles();
                StartCrashAudio();
                finalHoursPlayingParticles = true;
            }
            if (!finalHoursFinishing && ((!Compatibility.IsMajoraActiveOnCompany && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 235)
                                            || (Compatibility.IsMajoraActiveOnCompany && Compatibility.MajoraCompanyTimer <= 18)))
            {
                StartCoroutine(StartFinishing());
                finalHoursFinishing = true;
            }
            if (!finalHoursFinishing && StartOfRound.Instance.shipIsLeaving)
                DisableColliders(false);
            UpdateTimer(smoothTime + (!Compatibility.IsMajoraActiveOnCompany ? -21 : -8));
            UpdateImpact();
        }

        private void StartMusic()
        {
            finalHoursAudio.volume = Plugin.config.majoraMoonMusicVolume.Value;
            if (finalHoursAudio.volume > 0)
                finalHoursAudio.Play();
        }

        private void StartTimer()
        {
            if (Plugin.config.majoraMoonRemoveTimerDisplay.Value)
                return;
            if (timerUI != null)
                Destroy(timerUI);
            timerUI = Instantiate(finalHoursTimer);
            timerText = timerUI.GetComponentInChildren<TMP_Text>();
            timerStarted = true;
        }

        private void UpdateTimer(float timeBeforeEnd)
        {
            if (timerStarted && timerText != null && timeBeforeEnd >= 0)
            {
                var duration = System.TimeSpan.FromSeconds(timeBeforeEnd);
                timerText.text = duration.ToString(@"mm\:ss\:ff");
                if (!lastMinute && duration.Minutes <= 0)
                {
                    lastMinute = true;
                    timerText.color = Color.red;
                }
            }
        }

        private void StartParticles()
        {
            crashParticles1.Play();
            crashParticles2.Play();
        }

        private void StartCrashAudio()
        {
            crashAudio.Play();
        }

        private IEnumerator StartFinishing()
        {
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                yield break;
            StartOfRound.Instance.shipLeftAutomatically = false;
            StartOfRound.Instance.shipIsLeaving = true;
            Effects.MessageComputer("The autopilot emergency code has been activated.", "You've met with a terrible fate, haven't you?");
            yield return new WaitForSeconds(5f);
            HUDManager.Instance.shipLeavingEarlyIcon.enabled = false;
            if (shipLever != null)
            {
                shipLever.triggerScript.animationString = "SA_PushLeverBack";
                shipLever.leverHasBeenPulled = false;
                shipLever.triggerScript.interactable = false;
                shipLever.leverAnimatorObject.SetBool("pullLever", false);
            }
            StartOfRound.Instance.ShipLeave();
            yield return new WaitForSeconds(1f);
            //Effects.SetCameraEndOfRound(GameNetworkManager.Instance.localPlayerController);
            yield return new WaitForSeconds(4f);
            if (impact != null)
                Destroy(impact);
            impact = Instantiate(impactObject, endPosition - Vector3.up * impactGroundPosOffset, Quaternion.identity);
            DisableColliders();
            impactStarted = true;
            yield return Effects.ShakeCameraAdvanced(ScreenShakeType.VeryStrong, 2);
        }

        private void DisableColliders(bool disable = true)
        {
            if (collidersDisabled == disable)
                return;
            GameObject? model = transform.Find("Models/" + modelName).gameObject;
            if (model == null)
                return;
            foreach (var collider in model.GetComponents<MeshCollider>())
            {
                collider.enabled = !disable;
            }
            collidersDisabled = disable;
        }

        private void UpdateImpact()
        {
            if (impactStarted && impact != null)
            {
                impact.transform.localScale += Vector3.one * Time.deltaTime * impactScaleFactor;
                var player = GameNetworkManager.Instance.localPlayerController;
                if (!player.isPlayerDead && !player.isInHangarShipRoom && !player.isInElevator && Vector3.Distance(outsideNodeEndPosition, player.transform.position) <= impact.transform.localScale.x * moonRadiusApprox * 0.9f)
                {
                    Effects.Damage(player, 99999, CauseOfDeath.Burning, (int)Effects.DeathAnimation.Fire, false);
                    //Effects.SetCameraEndOfRound(player);
                }
            }
        }

        private void PlayRandomEvents()
        {
            if (IsServer)
            {
                lastRumbleEventTime += Time.deltaTime;
                lastTearEventTime += Time.deltaTime;
                if (lastRumbleEventTime >= nextRumbleEventTime)
                {
                    int eventID = 0;
                    lastRumbleEventTime = 0;
                    if (finalHoursDisplayingTimer)
                    {
                        if (!finalHoursPlayingParticles && Random.Range(0, 5) == 0)
                            eventID++;
                        nextRumbleEventTime = Random.Range(20 - (eventID * 14), 35 - (eventID * 30));
                    }
                    else
                        nextRumbleEventTime = Random.Range(35, 60);
                    PlayRandomEventsClientRpc(eventID);
                }
                if (!Compatibility.IsMajoraActiveOnCompany && lastTearEventTime >= nextTearEventTime && !finalHoursPlayingParticles)
                {
                    lastTearEventTime = 0;
                    if (isRareTearEventDay)
                    {
                        nextTearEventTime = Random.Range(3, 6);
                        rareTearEventDayNB++;
                        if (rareTearEventDayNB >= 8)
                            isRareTearEventDay = false;
                    }
                    else
                        nextTearEventTime = Random.Range(50, 60);
                    if (Random.Range(0, 10) <= (!isRareTearEventDay ? (finalHoursDisplayingTimer ? 1 : 3) : 7))
                        PlayMoonTearEvent();
                }
            }
            if (finalHoursPlayingParticles)
            {
                lastBellSfxEvent += Time.deltaTime;
                if (lastBellSfxEvent >= 3f)
                {
                    lastBellSfxEvent = 0;
                    sfxAudio.PlayOneShot(sfx[2]);
                }
            }
        }

        [ClientRpc]
        private void PlayRandomEventsClientRpc(int eventID)
        {
            switch (eventID)
            {
                case 0:
                    int sfxId = Effects.IsLocalPlayerInsideFacilityAbsolute() ? 0 : 1;
                    sfxAudio.PlayOneShot(sfx[sfxId]);
                    StartCoroutine(Effects.ShakeCameraAdvanced(ScreenShakeType.Long, finalHoursPlayingParticles ? 4 : 2));
                    break;
                case 1:
                    sfxAudio.PlayOneShot(sfx[2]);
                    break;
                default:
                    break;
            }
        }

        private void PlayMoonTearEvent()
        {
            if (Plugin.instance.majoraMoonTearItem == null)
                return;
            var crashPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(outsideNodeEndPosition, 20);
            var moonTear = Instantiate(Plugin.instance.majoraMoonTearItem.spawnPrefab, tearPosition.position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
            if (moonTear != null)
            {
                var maskComponent = moonTear.GetComponent<MoonTearItem>();
                maskComponent.transform.rotation = Quaternion.Euler(maskComponent.itemProperties.restingRotation);
                maskComponent.fallTime = 1f;
                maskComponent.hasHitGround = true;
                maskComponent.reachedFloorTarget = true;
                maskComponent.isInFactory = true;
                maskComponent.scrapValue = (int)(Random.Range(Plugin.instance.majoraMoonTearItem.minValue, Plugin.instance.majoraMoonTearItem.maxValue) * RoundManager.Instance.scrapValueMultiplier);
                maskComponent.NetworkObject.Spawn();
                maskComponent.StartFallingServerRpc(maskComponent.NetworkObject, maskComponent.scrapValue, crashPosition);
            }
            else
            {
                Plugin.logger.LogError("Failed to spawn the Moon Tear item on the server.");
            }
        }

        private IEnumerator StopMoonAnimation()
        {
            yield return new WaitForEndOfFrame();
            if (finalHoursPlayingMusic)
                yield return Effects.FadeOutAudio(finalHoursAudio, 1f);
            if (finalHoursAudio.volume > 0)
                finalHoursAudio.PlayOneShot(callOfTheGiantsMusic);
            if (finalHoursPlayingParticles)
            {
                crashParticles1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                crashParticles2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                crashAudio.Stop();
            }
            burstParticles2.Play();
            yield return new WaitForSeconds(19f);
            burstParticles1.Play();
            yield return new WaitForSeconds(3f);
            burstParticles2.Stop();
            DisableColliders();
            burstVFX.Play();
            burstAnimator.SetTrigger("Burst");
            yield return new WaitForSeconds(8f);
            crashAudio.PlayOneShot(stopMoonSfx);
            yield return new WaitForSeconds(15f);
            burstParticles1.Stop();
            var majoraEffect = Effects.GetWeatherEffect("majoramoon");
            majoraEffect?.EffectObject?.GetComponent<MajoraSkyEffect>()?.ReverseEffect();
            yield return new WaitForSeconds(6f);
            majoraEffect?.EffectObject?.GetComponent<MajoraSkyEffect>()?.ResetState();
            Effects.RemoveWeather("majoramoon");
        }

        public static void CheckAndReplaceModel(GameObject? moonInstanceObject = null)
        {
            var newModelName = Plugin.config.majoraMoonModel.Value;
            var moonObject = moonInstanceObject ?? Plugin.instance.majoraMoonObject;
            if (moonObject == null)
            {
                Plugin.logger.LogError("Failed to replace the Majora Moon model, moon object is null.");
                return;
            }
            if (moonInstanceObject != null)
            {
                if (Compatibility.CodeRebirthInstalled && Compatibility.IsMajoraCodeRebirthCompatible())
                    newModelName = "Baldy";
                else if (Compatibility.PremiumScrapsInstalled && Compatibility.IsMajoraPremiumScrapsCompatible())
                    newModelName = "Abibabou";
                else if (Compatibility.SurfacedInstalled && Compatibility.IsMajoraSurfacedCompatible())
                    newModelName = "Owl";
                else if (Compatibility.EmergencyDiceInstalled && Compatibility.IsMajoraEmergencyDiceCompatible())
                    newModelName = "Dice";
                else if (Compatibility.BiodiversityInstalled && Compatibility.IsMajoraBiodiversityCompatible())
                    newModelName = "Boomy";
            }
            if (modelName.Equals(newModelName))
                return;
            moonObject.transform.Find("Models/" + newModelName).gameObject.SetActive(true);
            moonObject.transform.Find("Models/" + modelName).gameObject.SetActive(false);
            modelName = newModelName;
        }

        [ServerRpc]
        public void InitializeMoonServerRpc(NetworkObjectReference moonRef, Vector3 nodeEndPosition)
        {
            InitializeMoonClientRpc(moonRef, nodeEndPosition);
        }

        [ClientRpc]
        private void InitializeMoonClientRpc(NetworkObjectReference moonRef, Vector3 nodeEndPosition)
        {
            StartCoroutine(InitializeMoon(moonRef, nodeEndPosition));
        }

        private IEnumerator InitializeMoon(NetworkObjectReference moonRef, Vector3 nodeEndPosition)
        {
            NetworkObject? moonNetObj = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !moonRef.TryGet(out moonNetObj))
            {
                yield return new WaitForSeconds(0.03f);
            }
            if (moonNetObj == null)
            {
                Plugin.logger.LogError("Error while trying to sync Majora Moon.");
                yield break;
            }
            yield return new WaitForEndOfFrame();
            if (IsServer)
                isRareTearEventDay = Random.Range(0, 100) == 0;
            startPosition = transform.position;
            startRotation = transform.eulerAngles;
            startScale = transform.localScale;
            var moonRadiusOffset = moonRadiusApprox * endSizeFactor;
            outsideNodeEndPosition = nodeEndPosition;
            endPosition = nodeEndPosition + Vector3.Normalize(transform.position - nodeEndPosition) * moonRadiusOffset;
            endRotation = new Vector3(90, transform.eulerAngles.y, transform.eulerAngles.z);
            endScale = transform.localScale * endSizeFactor;
            if (Plugin.config.majoraMoonModelAutomatic.Value)
                CheckAndReplaceModel(gameObject);
            Effects.SetupWindSpeedComponents(ref visualEnvironments, ref originalWindSpeeds);
            shipLever = FindObjectOfType<StartMatchLever>();
            isInitialized = true;
        }

        public override void OnDestroy()
        {
            if (finalHoursPlayingMusic)
                finalHoursAudio.Stop();
            if (finalHoursDisplayingTimer)
            {
                if (timerUI != null)
                {
                    Destroy(timerUI);
                    timerUI = null;
                    timerText = null;
                }
                for (int i = 0; i < visualEnvironments.Count; i++)
                {
                    if (visualEnvironments[i] != null)
                        visualEnvironments[i].windSpeed.value = originalWindSpeeds[i];
                }
                visualEnvironments.Clear();
                originalWindSpeeds.Clear();
            }
            if (finalHoursPlayingParticles)
            {
                crashParticles1.Stop();
                crashParticles2.Stop();
                crashAudio.Stop();
            }
            if (finalHoursFinishing)
            {
                if (impact != null)
                {
                    Destroy(impact);
                    impact = null;
                }
            }
            if (sfxAudio.isPlaying)
                sfxAudio.Stop();
            if (oathToOrderStopingMoon)
            {
                burstParticles1.Stop();
                burstParticles2.Stop();
                burstVFX.Stop();
            }
            if (stopMoonCoroutine != null)
                StopCoroutine(stopMoonCoroutine);
            modelName = Plugin.config.majoraMoonModel.Value;
            base.OnDestroy();
        }
    }
}
