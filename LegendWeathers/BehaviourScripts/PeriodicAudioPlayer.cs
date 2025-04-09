using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.BehaviourScripts
{
    public class PeriodicAudioPlayer : NetworkBehaviour
    {
        public GrabbableObject? attachedGrabbableObject;
        public AudioClip[] randomClips = null!;
        public AudioSource thisAudio = null!;
        public float audioMinInterval;
        public float audioMaxInterval;
        public float audioChancePercent;

        private float nextPlayTime = 0f;
        private float currentTime = 0f;
        private bool isReady = false;

        public void Start()
        {
            nextPlayTime = Random.Range(audioMinInterval, audioMaxInterval);
            isReady = true;
        }

        public void Update()
        {
            if (IsServer && GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null && isReady
                && randomClips.Length >= 1 && (attachedGrabbableObject == null || !attachedGrabbableObject.deactivated))
            {
                currentTime += Time.deltaTime;
                if (currentTime >= nextPlayTime)
                {
                    currentTime = 0f;
                    nextPlayTime = Random.Range(audioMinInterval, audioMaxInterval);
                    if (Random.Range(0f, 100f) < audioChancePercent)
                    {
                        PlayRandomAudioClientRpc(Random.Range(0, randomClips.Length));
                    }
                }
            }
        }

        [ClientRpc]
        private void PlayRandomAudioClientRpc(int clipIndex)
        {
            AudioClip clip = randomClips[clipIndex];
            thisAudio.PlayOneShot(clip, 1f);
            WalkieTalkie.TransmitOneShotAudio(thisAudio, clip);
            RoundManager.Instance.PlayAudibleNoise(thisAudio.transform.position, 7f);
        }
    }
}
