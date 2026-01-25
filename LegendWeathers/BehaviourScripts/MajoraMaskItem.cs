using LegendWeathers.Utils;
using LegendWeathers.Weathers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.BehaviourScripts
{
    public class MajoraMaskItem : HauntedMaskItem
    {
        public static MajoraMaskItem? Instance;
        public System.Action? grabbableObjectUpdateMethod;
        public List<MaskedPlayerEnemy> spawnedMaskedEnemies = new List<MaskedPlayerEnemy>();
        public MaskedPlayerEnemy? spawnedMajoraEnemy;

        public AudioSource screamAudio = null!;
        public int numberOfMaskedEnemies = 4;
        public bool hasBeenFound = false;
        public GameObject maskedEnemyInvocationObject = null!;
        public readonly Vector3 majoraOnMaskedPosOverride = new Vector3(-0.01f, 0.2f, 0.1f);

        private bool canCheckForSpawnedEnemies = false;
        private int currentInventorySlot = -1;
        private readonly int hauntedEventChance = 80;
        private (float, float) hauntedPocketTimer = (0, 20);
        private (float, float) hauntedActivateTimer = (0, 10);

        private readonly NetworkVariable<bool> hauntedActivateAttaching = new NetworkVariable<bool>(false);

        public override void Start()
        {
            base.Start();
            mimicEnemy = GetEnemies.Masked.enemyType;
            attachTimer = 8f;
        }

        public override int GetItemDataToSave()
        {
            return hasBeenFound ? 1 : 0;
        }

        public override void LoadItemSaveData(int saveData)
        {
            hasBeenFound = saveData == 1;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            currentInventorySlot = previousPlayerHeldBy.currentItemSlot;
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (!hasBeenFound)
            {
                AccelerateMoonServerRpc();
            }
            hauntedPocketTimer.Item1 = 0f;
            hauntedActivateTimer.Item1 = 0f;
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            hauntedPocketTimer.Item1 = 0f;
            hauntedActivateTimer.Item1 = 0f;
            hauntedActivateAttaching.Value = false;
        }

        public override void PocketItem()
        {
            base.PocketItem();
            hauntedActivateAttaching.Value = false;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!attaching && !finishedAttaching && playerHeldBy != null && IsOwner && !buttonDown)
                hauntedActivateAttaching.Value = false;
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
            CheckConditionForHauntedEvent();
            CheckConditionForSpawnedEnemies();
            if (!maskIsHaunted || !IsOwner || previousPlayerHeldBy == null || !maskOn || !holdingLastFrame || finishedAttaching)
            {
                return;
            }
            if (!attaching)
            {
                if (!StartOfRound.Instance.shipIsLeaving && (!StartOfRound.Instance.inShipPhase || StartOfRound.Instance.testRoom != null) && Time.realtimeSinceStartup > lastIntervalCheck)
                {
                    lastIntervalCheck = Time.realtimeSinceStartup + 5f;
                    if (Random.Range(0, 100) < (hauntedActivateAttaching.Value ? 20 : 65))
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
                    hauntedActivateAttaching.Value = false;
                }
                else if (attachTimer <= 0f)
                {
                    if (IsOwner && !finishedAttaching && !previousPlayerHeldBy.AllowPlayerDeath())
                    {
                        hauntedActivateAttaching.Value = false;
                    }
                    FinishAttachingMajoraMask();
                }
            }
        }

        private void CheckConditionForHauntedEvent()
        {
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null
                && !StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.shipIsLeaving && isHeld &&
                playerHeldBy.playerClientId == GameNetworkManager.Instance.localPlayerController.playerClientId
                && !playerHeldBy.isGrabbingObjectAnimation && playerHeldBy.timeSinceSwitchingSlots >= 0.3f
                && !playerHeldBy.inSpecialInteractAnimation && !playerHeldBy.throwingObject && !playerHeldBy.twoHanded
                && !playerHeldBy.jetpackControls && !playerHeldBy.disablingJetpackControls && currentInventorySlot != -1
                && !attaching && !finishedAttaching && !hauntedActivateAttaching.Value)
            {
                if (isPocketed)  // try to force equip the mask
                {
                    hauntedActivateTimer.Item1 = 0f;
                    hauntedPocketTimer.Item1 += Time.deltaTime;
                    if (hauntedPocketTimer.Item1 >= hauntedPocketTimer.Item2)
                    {
                        hauntedPocketTimer.Item1 = 0f;
                        if (Random.Range(0, 100) < hauntedEventChance + 1)
                        {
                            playerHeldBy.playerBodyAnimator.SetBool("GrabValidated", value: false);
                            playerHeldBy.SwitchToItemSlot(currentInventorySlot);
                            ForceSwitchSlotServerRpc(currentInventorySlot);
                            playerHeldBy.currentlyHeldObjectServer?.gameObject.GetComponent<AudioSource>().PlayOneShot(playerHeldBy.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
                            playerHeldBy.timeSinceSwitchingSlots = 0f;
                        }
                    }
                }
                else  // try to force wear the mask
                {
                    hauntedPocketTimer.Item1 = 0f;
                    hauntedActivateTimer.Item1 += Time.deltaTime;
                    if (hauntedActivateTimer.Item1 >= hauntedActivateTimer.Item2)
                    {
                        hauntedActivateTimer.Item1 = 0f;
                        if (Random.Range(0, 100) < hauntedEventChance + 1)
                        {
                            hauntedActivateAttaching.Value = true;
                            ItemActivate(true, true);
                        }
                    }
                }
            }
        }

        private void CheckConditionForSpawnedEnemies()
        {
            if (IsServer && canCheckForSpawnedEnemies)
            {
                if (spawnedMajoraEnemy == null)
                    canCheckForSpawnedEnemies = false;
                else
                {
                    if (spawnedMajoraEnemy.isEnemyDead && Plugin.instance.majoraMaskItem != null)  // respawn mask item
                    {
                        RemoveSpecialAttributesForMajoraEnemyClientRpc();
                        var spawnedMask = Instantiate(Plugin.instance.majoraMaskItem.spawnPrefab, spawnedMajoraEnemy.transform.position + Vector3.up * 0.25f, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                        var maskComponent = spawnedMask.GetComponent<MajoraMaskItem>();
                        maskComponent.transform.rotation = Quaternion.Euler(maskComponent.itemProperties.restingRotation);
                        maskComponent.fallTime = 1f;
                        maskComponent.hasHitGround = true;
                        maskComponent.reachedFloorTarget = true;
                        maskComponent.isInFactory = true;
                        maskComponent.scrapValue = scrapValue;
                        maskComponent.NetworkObject.Spawn();
                        maskComponent.SyncMaskServerRpc(maskComponent.NetworkObject, maskComponent.scrapValue, hasBeenFound ? 1 : 0);
                        canCheckForSpawnedEnemies = false;
                    }
                    else if (!spawnedMajoraEnemy.isEnemyDead && spawnedMaskedEnemies != null && spawnedMaskedEnemies.Count >= 1)  // respawn masked enemy
                    {
                        for (int i = 0; i < spawnedMaskedEnemies.Count; i++)
                        {
                            var masked = spawnedMaskedEnemies[i];
                            if (masked != null && masked.isEnemyDead)
                            {
                                spawnedMaskedEnemies.RemoveAt(i);
                                StartCoroutine(SpawnMaskedEnemy(masked.transform.position.y < -80f, masked.transform.position, 5));
                            }
                        }
                    }
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
                CreateMajoraMaskEnemiesServerRpc(isInsideFactory, position);
            }
        }

        [ServerRpc]
        private void CreateMajoraMaskEnemiesServerRpc(bool inFactory, Vector3 playerPositionAtDeath)
        {
            if (previousPlayerHeldBy == null)
                return;
            Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(playerPositionAtDeath, default, 10f);
            if (!RoundManager.Instance.GotNavMeshPositionResult)
                navMeshPosition = Effects.GetClosestAINodePosition(inFactory ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes, playerPositionAtDeath);
            SpawnMajoraEnemy(inFactory, navMeshPosition);
            StartCoroutine(SpawnMaskedEnemies(inFactory, navMeshPosition, previousPlayerHeldBy.playerClientId));
        }

        private void SpawnMajoraEnemy(bool inFactory, Vector3 navMeshPosition)
        {
            var netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(navMeshPosition, previousPlayerHeldBy.transform.eulerAngles.y, -1, mimicEnemy);
            CreateEnemyClientRpc(netObjectRef, inFactory, isSpecialMajoraEnemy: true);
        }

        private IEnumerator SpawnMaskedEnemies(bool inFactory, Vector3 originalPosition, ulong originalPlayerId)
        {
            for (int i = 0; i < numberOfMaskedEnemies; i++)
            {
                StartCoroutine(SpawnMaskedEnemy(inFactory, originalPosition, 30));
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator SpawnMaskedEnemy(bool inFactory, Vector3 originalPosition, int spawnRadius)
        {
            var spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(originalPosition, spawnRadius);
            CreateInvocationObjectClientRpc(spawnPosition);
            yield return new WaitForSeconds(2.5f);
            if (!StartOfRound.Instance.inShipPhase)
            {
                var netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, Random.Range(-90f, 90f), -1, mimicEnemy);
                if (netObjectRef.TryGet(out var networkObject))
                {
                    var component = networkObject.GetComponent<MaskedPlayerEnemy>();
                    component.SetSuit(previousPlayerHeldBy.currentSuitID);
                    component.mimickingPlayer = previousPlayerHeldBy;
                    component.SetEnemyOutside(!inFactory);
                    CreateEnemyClientRpc(netObjectRef, inFactory);
                }
            }
        }

        [ClientRpc]
        private void CreateInvocationObjectClientRpc(Vector3 position)
        {
            if (maskedEnemyInvocationObject != null)
                Instantiate(maskedEnemyInvocationObject, position, Quaternion.identity);
        }

        [ClientRpc]
        private void CreateEnemyClientRpc(NetworkObjectReference netObjectRef, bool inFactory, bool isSpecialMajoraEnemy = false)
        {
            StartCoroutine(WaitForEnemyToSpawnThenSync(netObjectRef, inFactory, isSpecialMajoraEnemy));
        }

        private IEnumerator WaitForEnemyToSpawnThenSync(NetworkObjectReference netObjectRef, bool inFactory, bool isSpecialMajoraEnemy)
        {
            var playerToMimic = previousPlayerHeldBy;
            NetworkObject? netObject = null;
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime > 20f || netObjectRef.TryGet(out netObject));
            if (isSpecialMajoraEnemy && playerToMimic.deadBody == null)
            {
                startTime = Time.realtimeSinceStartup;
                yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime > 20f || playerToMimic.deadBody != null);
            }
            if (isSpecialMajoraEnemy && playerToMimic.deadBody != null)
            {
                playerToMimic.deadBody.DeactivateBody(setActive: false);
            }
            if (netObject != null)
            {
                var component = netObject.GetComponent<MaskedPlayerEnemy>();
                component.mimickingPlayer = playerToMimic;
                component.SetSuit(playerToMimic.currentSuitID);
                component.SetEnemyOutside(!inFactory);
                if (isSpecialMajoraEnemy)
                {
                    component.SetVisibilityOfMaskedEnemy();
                    playerToMimic.redirectToEnemy = component;
                    SetSpecialAttributesForMajoraEnemy(component);
                    spawnedMajoraEnemy = component;
                    Instance = this;
                    canCheckForSpawnedEnemies = true;
                }
                else
                    spawnedMaskedEnemies.Add(component);
            }
        }

        private void SetSpecialAttributesForMajoraEnemy(MaskedPlayerEnemy component)
        {
            var maskPrefab = Instantiate(headMaskPrefab, component.maskTypes[0].transform.position, component.maskTypes[0].transform.rotation, component.maskTypes[0].transform.parent);
            maskPrefab.transform.localPosition = majoraOnMaskedPosOverride;
            maskPrefab.GetComponent<Animator>().enabled = false;
            var voice = transform.GetComponents<PeriodicAudioPlayer>().ToList().Find(x => x != null && !x.enabled);
            voice.thisAudio = component.creatureVoice;
            voice.enabled = true;
            component.maskTypes[0].SetActive(false);
            component.maskTypes[1].SetActive(false);
            component.enemyHP *= 5;
        }

        [ClientRpc]
        private void RemoveSpecialAttributesForMajoraEnemyClientRpc()
        {
            spawnedMajoraEnemy?.maskTypes[0].transform.parent.Find("MajoraHead(Clone)").gameObject.SetActive(false);
            var voice = transform.GetComponents<PeriodicAudioPlayer>().ToList().Find(x => x != null && x.enabled && x.attachedGrabbableObject == null);
            voice.enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void AccelerateMoonServerRpc()
        {
            AccelerateMoonClientRpc(Effects.IsWeatherEffectPresent("majoramoon"));
        }

        [ClientRpc]
        private void AccelerateMoonClientRpc(bool isMajoraMoonWeatherActive)
        {
            hasBeenFound = true;
            if (isMajoraMoonWeatherActive)
            {
                var majoraMoon = FindObjectOfType<MajoraMoon>();
                if (majoraMoon == null)
                {
                    Plugin.logger.LogError("Failed to find Majora Moon object.");
                    return;
                }
                var isSuccess = majoraMoon.AccelerateEndTimeFactor();
                if (isSuccess && GameNetworkManager.Instance.localPlayerController != null)
                {
                    screamAudio.Play();
                    if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                    {
                        GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1.5f);
                        GameNetworkManager.Instance.localPlayerController.playersManager.fearLevelIncreasing = false;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ForceSwitchSlotServerRpc(int slot)
        {
            ForceSwitchSlotClientRpc(slot);
        }

        [ClientRpc]
        private void ForceSwitchSlotClientRpc(int slot)
        {
            if (playerHeldBy != null && !playerHeldBy.IsOwner)
            {
                playerHeldBy.SwitchToItemSlot(slot);
                playerHeldBy.currentlyHeldObjectServer?.gameObject.GetComponent<AudioSource>().PlayOneShot(playerHeldBy.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
            }
        }

        public static void CheckAndReplaceModel()
        {
            var moonModelName = Plugin.config.majoraMoonModel.Value;
            if (moonModelName == "N64" || moonModelName == "Faceless" || moonModelName == "Vanilla")
            {
                var maskItem = Plugin.instance.majoraMaskItem;
                if (maskItem == null)
                {
                    Plugin.logger.LogError("Failed to replace the Majora Mask model, mask item is null.");
                    return;
                }
                string maskModelName = moonModelName == "N64" ? "Model1" : "Model3";
                SetModelActive(maskItem, "Model2", false);
                SetModelActive(maskItem, maskModelName, true);
                if (maskModelName == "Model3")
                {
                    TweakMaskDataModel(maskItem);
                }
            }
        }

        private static void SetModelActive(Item maskItem, string modelName, bool active)
        {
            maskItem.spawnPrefab.transform.Find("Model/" + modelName).gameObject.SetActive(active);
            maskItem.spawnPrefab.GetComponent<MajoraMaskItem>().headMaskPrefab.transform.Find("Model/" + modelName).gameObject.SetActive(active);
        }

        private static void TweakMaskDataModel(Item maskItem)
        {
            maskItem.spawnPrefab.transform.Find("ScanNode").GetComponent<ScanNodeProperties>().headerText = "Unknown Mask";
            var itemProperties = maskItem.spawnPrefab.GetComponent<MajoraMaskItem>().itemProperties;
            itemProperties.itemName = "Unknown Mask";
            if (Plugin.instance.vanillaItemIcon != null)
                itemProperties.itemIcon = Plugin.instance.vanillaItemIcon;
        }

        [ServerRpc]
        public void SyncMaskServerRpc(NetworkObjectReference maskRef, int value, int save = 0)
        {
            SyncMaskClientRpc(maskRef, value, save);
        }

        [ClientRpc]
        private void SyncMaskClientRpc(NetworkObjectReference maskRef, int value, int save)
        {
            StartCoroutine(SyncMask(maskRef, value, save));
        }

        private IEnumerator SyncMask(NetworkObjectReference maskRef, int value, int save)
        {
            yield return Effects.SyncItem(maskRef, value, save);
        }
    }
}
