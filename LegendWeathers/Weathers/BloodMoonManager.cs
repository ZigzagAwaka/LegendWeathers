using LegendWeathers.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    internal class BloodMoonManager : NetworkBehaviour
    {
        public AudioSource introMusicAudio = null!;

        private bool isInitialized = false;
        private bool isPlayingIntroMusic = false;

        public void Update()
        {
            if (!isInitialized)
                return;
            Effects.StopVanillaMusic();
            if (isPlayingIntroMusic)
                CheckIntroMusicState();
        }

        private void CheckIntroMusicState()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null || player.isPlayerDead || player.isInsideFactory)
            {
                introMusicAudio.Stop();
                isPlayingIntroMusic = false;
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
            StartIntroductionMusic();
            isInitialized = true;
        }

        private void StartIntroductionMusic()
        {
            introMusicAudio.volume = Plugin.config.bloodMoonMusicVolume.Value;
            if (introMusicAudio.volume > 0)
            {
                introMusicAudio.Play();
                isPlayingIntroMusic = true;
            }
        }

        public override void OnDestroy()
        {
            if (isPlayingIntroMusic)
                introMusicAudio.Stop();
            base.OnDestroy();
        }
    }
}
