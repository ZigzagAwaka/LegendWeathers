using LegendWeathers.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.BehaviourScripts
{
    public class MoonTearItem : PhysicsProp
    {
        public AudioSource fallingAudio = null!;
        public AudioClip[] fallingSfx = null!;
        public ParticleSystem[] fallingParticles = null!;
        public GameObject blastEffect = null!;
        public ParticleSystem landedParticle = null!;

        private bool isFalling = false;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private readonly float smoothTime = 2.6f;
        private Coroutine? fallingCoroutine = null;

        public override void GrabItem()
        {
            base.GrabItem();
            StopFalling();
            if (landedParticle.isPlaying)
                landedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void StartFalling(Vector3 crashPosition)
        {
            if (isHeld)
                return;
            endPosition = crashPosition;
            EnableFallingEffects(true);
            isFalling = true;
            if (fallingCoroutine != null)
                StopCoroutine(fallingCoroutine);
            fallingCoroutine = StartCoroutine(Falling());
        }

        private IEnumerator Falling()
        {
            var time = 0f;
            while (time < smoothTime && isFalling && !isHeld)
            {
                float lerpFactor = Mathf.SmoothStep(0f, 1f, time / smoothTime);
                time += Time.deltaTime;
                targetFloorPosition = Vector3.Lerp(startPosition, endPosition, lerpFactor);
                yield return null;
            }
            if (isFalling && !isHeld)
            {
                StopFalling();
                targetFloorPosition = endPosition;
                if (!landedParticle.isPlaying)
                    landedParticle.Play();
            }
        }

        private void StopFalling()
        {
            if (isFalling)
            {
                isFalling = false;
                EnableFallingEffects(false);
                if (isHeld && fallingCoroutine != null)
                    StopCoroutine(fallingCoroutine);
            }
        }

        private void EnableFallingEffects(bool enable)
        {
            foreach (var particle in fallingParticles)
            {
                if (enable)
                    particle.Play();
                else
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (enable)
                fallingAudio.PlayOneShot(fallingSfx[0], 1f);
            else
            {
                fallingAudio.Stop();
                if (!isHeld)
                {
                    fallingAudio.PlayOneShot(fallingSfx[1], 1.5f);
                    Instantiate(blastEffect, targetFloorPosition, Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform);
                    Landmine.SpawnExplosion(targetFloorPosition, false, 4, 7, 30, 25);
                }
            }
        }

        [ServerRpc]
        public void StartFallingServerRpc(NetworkObjectReference tearRef, int value, Vector3 crashPosition)
        {
            StartFallingClientRpc(tearRef, value, crashPosition);
        }

        [ClientRpc]
        private void StartFallingClientRpc(NetworkObjectReference tearRef, int value, Vector3 crashPosition)
        {
            StartCoroutine(SyncAndStartFall(tearRef, value, crashPosition));
        }

        private IEnumerator SyncAndStartFall(NetworkObjectReference tearRef, int value, Vector3 crashPosition)
        {
            yield return Effects.SyncItem(tearRef, value, 0);
            hasHitGround = true;
            reachedFloorTarget = true;
            yield return new WaitForSeconds(0.5f);
            startPosition = targetFloorPosition;
            StartFalling(crashPosition);
        }
    }
}
