using LegendWeathers.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.BehaviourScripts
{
    public class BloodStoneItem : PhysicsProp
    {
        public AudioSource spawningAudio = null!;
        public ParticleSystem[] spawningParticles = null!;
        public BoxCollider grabCollider = null!;
        public BoxCollider scanCollider = null!;

        private IEnumerator Invocation(bool isFast)
        {
            if (!isFast)
            {
                SetVisibility(false);
                spawningParticles[0].Play();
                yield return new WaitForSeconds(0.5f);
                spawningAudio.Play();
                yield return new WaitForSeconds(0.6f);
                spawningParticles[0].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                yield return new WaitForSeconds(0.4f);
                SetVisibility(true);
                spawningParticles[1].Play();
                yield return new WaitForSeconds(0.4f);
            }
            yield return new WaitForSeconds(0.1f);
            spawningParticles[2].Play();
        }

        private void SetVisibility(bool visibleFlag)
        {
            EnableItemMeshes(visibleFlag);
            scanCollider.enabled = visibleFlag;
            grabCollider.enabled = visibleFlag;
            grabbable = visibleFlag;
            grabbableToEnemies = visibleFlag;
        }

        [ServerRpc]
        public void StartInvocationServerRpc(NetworkObjectReference itemRef, int value, bool isFast)
        {
            StartInvocationClientRpc(itemRef, value, isFast);
        }

        [ClientRpc]
        private void StartInvocationClientRpc(NetworkObjectReference itemRef, int value, bool isFast)
        {
            StartCoroutine(SyncAndStartInvocation(itemRef, value, isFast));
        }

        private IEnumerator SyncAndStartInvocation(NetworkObjectReference itemRef, int value, bool isFast)
        {
            yield return Effects.SyncItem(itemRef, value, 0);
            hasHitGround = true;
            reachedFloorTarget = true;
            yield return Invocation(isFast);
        }
    }
}
