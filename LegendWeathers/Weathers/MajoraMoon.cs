using LegendWeathers.Utils;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegendWeathers.Weathers
{
    public class MajoraMoon : NetworkBehaviour
    {
        public AudioSource finalHoursAudio = null!;
        public AudioSource sfxAudio = null!;
        public AudioSource crashAudio = null!;
        public AudioClip[] sfx = null!;
        public GameObject finalHoursTimer = null!;
        public ParticleSystem crashParticles1 = null!;
        public ParticleSystem crashParticles2 = null!;
        public GameObject impactObject = null!;

        private readonly int moonRadiusApprox = 18;
        private readonly float endSizeFactor = 7.3f;
        private Vector3 endPosition;
        private Vector3 endRotation;
        private Vector3 endScale;
        private float endTime;
        private bool isInitialized = false;

        private float smoothTime = 0;
        private Vector3 smoothPosVelocity;
        private Vector3 smoothRotVelocity;
        private Vector3 smoothScaVelocity;
        private float previousSpeedMultiplier = 1f;

        private float finalHoursTime;
        private bool finalHoursPlayingMusic = false;
        private bool finalHoursDisplayingTimer = false;
        private bool finalHoursPlayingParticles = false;
        private bool finalHoursFinishing = false;

        private VisualEnvironment? vanillaVisualEnvironment;
        private readonly int windSpeedFactor = 17;
        private float originalWindSpeed;

        private GameObject? timerUI;
        private TMP_Text? timerText;
        private bool lastMinute = false;
        private bool timerStarted = false;

        private GameObject? impact;
        private readonly float impactScaleFactor = 1.4f;
        private readonly int impactGroundPosOffset = 100;
        private bool impactStarted = false;
        private StartMatchLever? shipLever;

        private float lastRandomEventTime = 0;
        private float nextRandomEventTime = 40;
        private float lastBellSfxEvent = 0;

        public void Update()
        {
            if (!isInitialized)
                return;
            if (TimeOfDay.Instance.currentDayTimeStarted)
            {
                if (previousSpeedMultiplier != TimeOfDay.Instance.globalTimeSpeedMultiplier)
                {
                    previousSpeedMultiplier = TimeOfDay.Instance.globalTimeSpeedMultiplier;
                    smoothPosVelocity = default;
                    smoothRotVelocity = default;
                    smoothScaVelocity = default;
                }
                endTime = TimeOfDay.Instance.globalTimeAtEndOfDay / 1.8f;
                smoothTime = (endTime - TimeOfDay.Instance.currentDayTime) / TimeOfDay.Instance.globalTimeSpeedMultiplier;
                finalHoursTime = endTime - 280;
            }
            if (smoothTime != 0)
            {
                transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref smoothPosVelocity, smoothTime);
                transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, endRotation, ref smoothRotVelocity, smoothTime);
                transform.localScale = Vector3.SmoothDamp(transform.localScale, endScale, ref smoothScaVelocity, smoothTime);
            }
            PlayRandomEvents();
            FinalHours();
        }

        private void FinalHours()
        {
            if (!finalHoursPlayingMusic && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 30)
            {
                StartMusic();
                finalHoursPlayingMusic = true;
            }
            if (!finalHoursDisplayingTimer && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 50)
            {
                StartTimer();
                StartIncreasingWindSpeed();
                finalHoursDisplayingTimer = true;
            }
            if (!finalHoursPlayingParticles && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 205)
            {
                StartParticles();
                StartCrashAudio();
                finalHoursPlayingParticles = true;
            }
            if (!finalHoursFinishing && TimeOfDay.Instance.currentDayTime >= finalHoursTime + 235)
            {
                StartCoroutine(StartFinishing());
                finalHoursFinishing = true;
            }
            UpdateTimer(smoothTime - 20);
            UpdateImpact();
        }

        private void StartMusic()
        {
            finalHoursAudio.volume = Plugin.config.majoraMoonMusicVolume.Value;
            finalHoursAudio.Play();
        }

        private void StartTimer()
        {
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

        private void StartIncreasingWindSpeed()
        {
            if (vanillaVisualEnvironment != null)
            {
                originalWindSpeed = vanillaVisualEnvironment.windSpeed.value;
                vanillaVisualEnvironment.windSpeed.value *= windSpeedFactor;
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
            Effects.SetCameraEndOfRound(GameNetworkManager.Instance.localPlayerController);
            yield return new WaitForSeconds(4f);
            if (impact != null)
                Destroy(impact);
            impact = Instantiate(impactObject, endPosition - Vector3.up * impactGroundPosOffset, Quaternion.identity);
            impactStarted = true;
        }

        private void UpdateImpact()
        {
            if (impactStarted && impact != null)
            {
                impact.transform.localScale += Vector3.one * Time.deltaTime * impactScaleFactor;
                var player = GameNetworkManager.Instance.localPlayerController;
                if (!player.isPlayerDead && !player.isInHangarShipRoom && !player.isInElevator && Vector3.Distance(endPosition, player.transform.position) <= impact.transform.localScale.x * moonRadiusApprox * 1.5f)
                {
                    Effects.Damage(player, 99999, CauseOfDeath.Burning, (int)Effects.DeathAnimation.Fire, false);
                    Effects.SetCameraEndOfRound(player);
                }
            }
        }

        private void PlayRandomEvents()
        {
            if (IsServer)
            {
                lastRandomEventTime += Time.deltaTime;
                if (lastRandomEventTime >= nextRandomEventTime)
                {
                    int eventID = 0;
                    lastRandomEventTime = 0;
                    if (finalHoursDisplayingTimer)
                    {
                        if (!finalHoursPlayingParticles && Random.Range(0, 4) == 0)
                            eventID++;
                        nextRandomEventTime = Random.Range(20 - (eventID * 15), 45 - (eventID * 35));
                    }
                    else
                        nextRandomEventTime = Random.Range(60, 100);
                    PlayRandomEventsClientRpc(eventID);
                }
            }
            if (finalHoursPlayingMusic && SoundManager.Instance.musicSource.isPlaying)
                SoundManager.Instance.musicSource.Stop();
            if (finalHoursPlayingParticles)
            {
                lastBellSfxEvent += Time.deltaTime;
                if (lastBellSfxEvent >= 3.5f)
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
                    int sfxId = Effects.IsLocalPlayerInsideFacilityAbsolute() ? 1 : 0;
                    sfxAudio.PlayOneShot(sfx[sfxId]);
                    HUDManager.Instance.ShakeCamera(finalHoursFinishing ? ScreenShakeType.Big : ScreenShakeType.Long);
                    break;
                case 1:
                    sfxAudio.PlayOneShot(sfx[2]);
                    break;
                default:
                    break;
            }
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
            var moonRadiusOffset = moonRadiusApprox * endSizeFactor;
            endPosition = nodeEndPosition + Vector3.Normalize(transform.position - nodeEndPosition) * moonRadiusOffset;
            endRotation = new Vector3(90, transform.eulerAngles.y, transform.eulerAngles.z);
            endScale = transform.localScale * endSizeFactor;
            SetupComponents();
            isInitialized = true;
        }

        private void SetupComponents()
        {
            foreach (var volume in FindObjectsOfType<Volume>())
            {
                if (volume.name == "Sky and Fog Global Volume" || volume.priority == 1)
                {
                    foreach (var component in volume.profile.components)
                    {
                        if (component.active && component is VisualEnvironment environment && environment.windSpeed.overrideState)
                        {
                            vanillaVisualEnvironment = environment;
                            break;
                        }
                    }
                    if (vanillaVisualEnvironment != null)
                        break;
                }
            }
            shipLever = FindObjectOfType<StartMatchLever>();
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
                if (vanillaVisualEnvironment != null)
                    vanillaVisualEnvironment.windSpeed.value = originalWindSpeed;
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
            base.OnDestroy();
        }
    }
}
