using GameNetcodeStuff;
using LegendWeathers.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    internal class BloodMoonManager : NetworkBehaviour
    {
        public AudioSource introMusicAudio = null!;
        public AudioSource sfxAudio = null!;
        public AudioClip[] sfx = null!;

        private bool isInitialized = false;
        private PlayerControllerB? localPlayer = null;
        private bool localPlayerIsInsideLastChecked = false;

        private float lastMoonSfxTime = 0;
        private float nextMoonSfxTime = 63;  // 63 = initial delay outside before first sfx
        private readonly float moonSfxTimeIntervalInside = 10.8f;
        private readonly float moonSfxTimeIntervalOutside = 81.8f;

        public void Update()
        {
            if (!isInitialized)
                return;
            Effects.StopVanillaMusic();
            if (introMusicAudio.isPlaying)
                CheckIntroMusicState();
            else
                TryAndPlayMoonSfx();
        }

        private void CheckIntroMusicState()
        {
            if (localPlayer == null || localPlayer.isPlayerDead || localPlayer.isInsideFactory)
            {
                introMusicAudio.Stop();
            }
        }

        private void TryAndPlayMoonSfx()
        {
            if (localPlayer == null)
                return;

            if (Effects.IsPlayerInsideFacilityAbsolute(localPlayer))
            {
                if (!localPlayerIsInsideLastChecked)
                {
                    lastMoonSfxTime = 0;
                    nextMoonSfxTime = moonSfxTimeIntervalInside;
                }
                localPlayerIsInsideLastChecked = true;
            }
            else
            {
                localPlayerIsInsideLastChecked = false;
            }

            if (localPlayerIsInsideLastChecked)
                return;

            lastMoonSfxTime += Time.deltaTime;
            if (lastMoonSfxTime >= nextMoonSfxTime)
            {
                lastMoonSfxTime = 0;
                nextMoonSfxTime = moonSfxTimeIntervalOutside;
                sfxAudio.PlayOneShot(sfx[Random.Range(0, sfx.Length)], 0.5f);
            }
        }

        [ServerRpc]
        public void InitializeManagerServerRpc(NetworkObjectReference managerReference)
        {
            InitializeManagerClientRpc(managerReference);
        }

        [ClientRpc]
        private void InitializeManagerClientRpc(NetworkObjectReference managerReference)
        {
            StartCoroutine(InitializeManager(managerReference));
        }

        private IEnumerator InitializeManager(NetworkObjectReference managerReference)
        {
            NetworkObject? netObj = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !managerReference.TryGet(out netObj))
            {
                yield return new WaitForSeconds(0.03f);
            }
            if (netObj == null)
            {
                Plugin.logger.LogError("Error while trying to sync Blood Moon manager.");
                yield break;
            }
            yield return new WaitForEndOfFrame();
            localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer != null && !localPlayer.isPlayerDead)
                localPlayerIsInsideLastChecked = localPlayer.isInsideFactory;
            if (localPlayerIsInsideLastChecked)
                nextMoonSfxTime = moonSfxTimeIntervalInside;
            StartIntroductionMusic();
            isInitialized = true;
        }

        private void StartIntroductionMusic()
        {
            introMusicAudio.volume = Plugin.config.bloodMoonMusicVolume.Value;
            introMusicAudio.Play();
        }

        public override void OnDestroy()
        {
            if (introMusicAudio.isPlaying)
                introMusicAudio.Stop();
            if (sfxAudio.isPlaying)
                sfxAudio.Stop();
            base.OnDestroy();
        }
    }
}
