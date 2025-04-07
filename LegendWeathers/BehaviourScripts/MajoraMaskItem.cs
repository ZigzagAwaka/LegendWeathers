using LegendWeathers.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using WeatherRegistry;

namespace LegendWeathers.BehaviourScripts
{
    public class MajoraMaskItem : HauntedMaskItem
    {
        public System.Action? grabbableObjectUpdateMethod;
        public int numberOfMaskedEnemies = 4;
        public bool hasBeenFound = false;

        public override void Start()
        {
            base.Start();
            mimicEnemy = GetEnemies.Masked.enemyType;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            lastIntervalCheck = Time.realtimeSinceStartup + 5f;
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (!hasBeenFound)
            {
                hasBeenFound = true;
                bool isMajoraMoonWeatherEnabled = false;
                for (int i = 0; i < WeatherManager.CurrentEffectTypes.Count; i++)
                {
                    if (WeatherManager.CurrentEffectTypes[i] == new WeatherNameResolvable("majoramoon").WeatherType)
                    {
                        isMajoraMoonWeatherEnabled = true;
                        break;
                    }
                }
            }
        }

        private void RunOriginalUpdate()
        {
            if (grabbableObjectUpdateMethod == null)
            {
                var methodPointer = typeof(GrabbableObject).GetMethod("Update").MethodHandle.GetFunctionPointer();
                grabbableObjectUpdateMethod = (System.Action)System.Activator.CreateInstance(typeof(System.Action), this, methodPointer);
            }
            grabbableObjectUpdateMethod.Invoke();
        }

        public override void Update()
        {
            RunOriginalUpdate();
            if (!maskIsHaunted || !IsOwner || previousPlayerHeldBy == null || !maskOn || !holdingLastFrame || finishedAttaching)
            {
                return;
            }
            if (!attaching)
            {
                if (!StartOfRound.Instance.shipIsLeaving && (!StartOfRound.Instance.inShipPhase || StartOfRound.Instance.testRoom != null) && Time.realtimeSinceStartup > lastIntervalCheck)
                {
                    lastIntervalCheck = Time.realtimeSinceStartup + 5f;
                    if (Random.Range(0, 100) < 65)
                    {
                        BeginAttachment();
                    }
                }
            }
            else
            {
                attachTimer -= Time.deltaTime;
                if (previousPlayerHeldBy.isPlayerDead)
                {
                    CancelAttachToPlayerOnLocalClient();
                }
                else if (attachTimer <= 0f)
                {
                    FinishAttachingMajoraMask();
                }
            }
        }

        private void FinishAttachingMajoraMask()
        {
            if (IsOwner && !finishedAttaching)
            {
                finishedAttaching = true;
                if (!previousPlayerHeldBy.AllowPlayerDeath())
                {
                    CancelAttachToPlayerOnLocalClient();
                    return;
                }
                bool isInsideFactory = previousPlayerHeldBy.isInsideFactory;
                Vector3 position = previousPlayerHeldBy.transform.position;
                previousPlayerHeldBy.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Suffocation, (int)Effects.DeathAnimation.Haunted);
                CreateMaskedServerRpc(isInsideFactory, position);
            }
        }

        [ServerRpc]
        private void CreateMaskedServerRpc(bool inFactory, Vector3 playerPositionAtDeath)
        {
            if (previousPlayerHeldBy == null)
            {
                return;
            }
            Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(playerPositionAtDeath, default, 10f);
            if (!RoundManager.Instance.GotNavMeshPositionResult)
                navMeshPosition = Effects.GetClosestAINodePosition(inFactory ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes, playerPositionAtDeath);
            for (int i = 0; i < numberOfMaskedEnemies; i++)
            {
                NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(navMeshPosition, previousPlayerHeldBy.transform.eulerAngles.y, -1, GetEnemies.Masked.enemyType);
                if (netObjectRef.TryGet(out var networkObject))
                {
                    MaskedPlayerEnemy component = networkObject.GetComponent<MaskedPlayerEnemy>();
                    component.SetSuit(previousPlayerHeldBy.currentSuitID);
                    component.mimickingPlayer = previousPlayerHeldBy;
                    component.SetEnemyOutside(!inFactory);
                    component.SetVisibilityOfMaskedEnemy();
                    previousPlayerHeldBy.redirectToEnemy = component;
                    previousPlayerHeldBy.deadBody?.DeactivateBody(setActive: false);
                }
                CreateMimicClientRpc(netObjectRef, inFactory);
            }
        }

        [ServerRpc]
        public void SyncMaskServerRpc(NetworkObjectReference maskRef, int value)
        {
            SyncMaskClientRpc(maskRef, value);
        }

        [ClientRpc]
        private void SyncMaskClientRpc(NetworkObjectReference maskRef, int value)
        {
            StartCoroutine(SyncMask(maskRef, value));
        }

        private IEnumerator SyncMask(NetworkObjectReference maskRef, int value)
        {
            yield return Effects.SyncItem(maskRef, value, 0);
        }
    }
}
