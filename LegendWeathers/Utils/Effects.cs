﻿using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using WeatherRegistry;

namespace LegendWeathers.Utils
{
    internal class Effects
    {
        public enum DeathAnimation
        {
            Normal,  // classic death
            NoHead1,  // remove head from body
            Spring,  // remove head and replace it with spring
            Haunted,  // body moves a little after classic death
            Mask1,  // comedy mask attached to body
            Mask2,  // tragedy mask attached to body
            Fire,  // burned death
            CutInHalf,  // cut the body in half
            NoHead2  // same as NoHead but without sound
        }

        public class ItemNetworkReference
        {
            public NetworkObjectReference netObjectRef;
            public int value;
            public int save;

            public ItemNetworkReference(NetworkObjectReference netObjectRef, int value, int save = 0)
            {
                this.netObjectRef = netObjectRef;
                this.value = value;
                this.save = save;
            }
        }

        public static void SetupNetwork()
        {
            IEnumerable<System.Type> types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        public static List<PlayerControllerB> GetPlayers(bool includeDead = false, bool excludeOutsideFactory = false)
        {
            List<PlayerControllerB> rawList = StartOfRound.Instance.allPlayerScripts.ToList();
            List<PlayerControllerB> updatedList = new List<PlayerControllerB>(rawList);
            foreach (var p in rawList)
            {
                if (!p.IsSpawned || !p.isPlayerControlled || (!includeDead && p.isPlayerDead) || (excludeOutsideFactory && !p.isInsideFactory))
                {
                    updatedList.Remove(p);
                }
            }
            return updatedList;
        }

        public static List<PlayerControllerB> GetRandomPlayers(int numberToGet, bool shouldIgnoreOnePlayer = false, ulong playerIdToIgnore = default)
        {
            var rawList = StartOfRound.Instance.allPlayerScripts.ToList();
            rawList.RemoveAll(p => p == null || !p.IsSpawned || !p.isPlayerControlled || (shouldIgnoreOnePlayer && p.playerClientId == playerIdToIgnore));
            var players = new List<PlayerControllerB>();
            if (rawList.Count > numberToGet)
            {
                var rng = new System.Random();
                int n = rawList.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    (rawList[n], rawList[k]) = (rawList[k], rawList[n]);
                }
            }
            if (rawList.Count <= 1)
            {
                for (int i = 0; i < numberToGet; i++)
                {
                    players.Add(rawList.Count != 0 ? rawList[0] : StartOfRound.Instance.allPlayerScripts[0]);
                }
                return players;
            }
            for (int i = 0; i < numberToGet; i++)
            {
                var id = i < rawList.Count ? i : i - rawList.Count;
                players.Add(rawList[id]);
            }
            return players;
        }

        public static List<EnemyAI> GetEnemies(bool includeDead = false, bool includeCanDie = false, bool excludeDaytime = false)
        {
            List<EnemyAI> rawList = Object.FindObjectsOfType<EnemyAI>().ToList();
            List<EnemyAI> updatedList = new List<EnemyAI>(rawList);
            if (includeDead)
                return updatedList;
            foreach (var e in rawList)
            {
                if (!e.IsSpawned || e.isEnemyDead || (!includeCanDie && !e.enemyType.canDie) || (excludeDaytime && e.enemyType.isDaytimeEnemy))
                {
                    updatedList.Remove(e);
                }
            }
            return updatedList;
        }

        public static void Damage(PlayerControllerB player, int damageNb, CauseOfDeath cause = 0, int animation = 0, bool criticalBlood = true)
        {
            damageNb = player.health > 100 && damageNb == 100 ? 900 : damageNb;
            if (criticalBlood && player.health - damageNb <= 20)
                player.bleedingHeavily = true;
            player.DamagePlayer(damageNb, causeOfDeath: cause, deathAnimation: animation);
        }

        public static void Heal(ulong playerID, int health)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerID];
            player.health = player.health > 100 ? player.health : health;
            player.criticallyInjured = false;
            player.bleedingHeavily = false;
            player.playerBodyAnimator.SetBool("Limp", false);
        }

        public static void Teleportation(PlayerControllerB player, Vector3 position)
        {
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(position, true);
            player.beamOutParticle.Play();
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }

        public static void SetPosFlags(ulong playerID, bool ship = false, bool exterior = false, bool interior = false)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerID];
            if (ship)
            {
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
            }
            if (exterior)
            {
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = false;
            }
            if (interior)
            {
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = true;
            }
            foreach (var item in player.ItemSlots)
            {
                if (item != null)
                {
                    item.isInFactory = player.isInsideFactory;
                    item.isInElevator = player.isInElevator;
                    item.isInShipRoom = player.isInHangarShipRoom;
                }
            }
            if (GameNetworkManager.Instance.localPlayerController.playerClientId == player.playerClientId)
            {
                if (player.isInsideFactory)
                    TimeOfDay.Instance.DisableAllWeather();
                else
                    ActivateWeatherEffect();
            }
        }

        public static IEnumerator ShakeCameraAdvanced(ScreenShakeType shakeType, int repeat = 1, float inBetweenTimer = 0.5f)
        {
            if (shakeType == ScreenShakeType.Long || shakeType == ScreenShakeType.VeryStrong)
            {
                for (int i = 0; i < repeat; i++)
                {
                    HUDManager.Instance.playerScreenShakeAnimator.SetTrigger(shakeType == ScreenShakeType.Long ? "longShake" : "veryStrongShake");
                    if (repeat > 1)
                        yield return new WaitForSeconds(inBetweenTimer);
                }
            }
            else
                HUDManager.Instance.ShakeCamera(shakeType);
        }

        public static bool IsPlayerFacingObject<T>(PlayerControllerB player, out T obj, float distance)
        {
            if (Physics.Raycast(new Ray(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward), out var hitInfo, distance, 2816))
            {
                obj = hitInfo.transform.GetComponent<T>();
                if (obj != null)
                    return true;
            }
            obj = default;
            return false;
        }

        public static bool IsPlayerNearObject<T>(PlayerControllerB player, out T obj, float distance) where T : Component
        {
            T[] array = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            for (int i = 0; i < array.Length; i++)
            {
                if (Vector3.Distance(player.transform.position, array[i].transform.position) <= distance)
                {
                    obj = array[i];
                    return true;
                }
            }
            obj = default;
            return false;
        }

        public static Vector3 GetClosestAINodePosition(GameObject[] nodes, Vector3 position)
        {
            return nodes.OrderBy((GameObject x) => Vector3.Distance(position, x.transform.position)).ToArray()[0].transform.position;
        }

        public static void Knockback(Vector3 position, float range, int damage = 0, float physicsForce = 30)
        {
            Landmine.SpawnExplosion(position, false, 0, range, damage, physicsForce);
        }

        public static IEnumerator FadeOutAudio(AudioSource source, float time, bool specialStop = true)
        {
            yield return new WaitForEndOfFrame();
            var volume = source.volume;
            while (source.volume > 0)
            {
                source.volume -= volume * Time.deltaTime / time;
                if (specialStop && source.volume <= 0.04f)
                    break;
                yield return null;
            }
            source.Stop();
            source.volume = volume;
        }

        public static void SetCameraEndOfRound(PlayerControllerB player)
        {
            StartOfRound.Instance.SetSpectateCameraToGameOverMode(true);
            if (player.isPlayerDead)
                player.SetSpectatedPlayerEffects(true);
        }

        public static bool IsLocalPlayerInsideFacilityAbsolute()
        {
            if (GameNetworkManager.Instance == null)
                return false;
            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
                return false;
            if (!player.isPlayerDead)
                return player.isInsideFactory;
            if (player.spectatedPlayerScript == null)
                return false;
            return player.spectatedPlayerScript.isInsideFactory;
        }

        public static void ChangeWeather(LevelWeatherType weather)
        {
            var original = StartOfRound.Instance.currentLevel.currentWeather;
            StartOfRound.Instance.currentLevel.currentWeather = weather;
            if (Compatibility.WeatherRegisteryInstalled)
            {
                ChangeWeatherWR(weather);
                return;
            }
            RoundManager.Instance.SetToCurrentLevelWeather();
            TimeOfDay.Instance.SetWeatherBasedOnVariables();
            if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
                return;
            ActivateWeatherEffect(original);
        }

        public static void ActivateWeatherEffect(LevelWeatherType originalWeather = default)
        {
            for (var i = 0; i < TimeOfDay.Instance.effects.Length; i++)
            {
                var effect = TimeOfDay.Instance.effects[i];
                var enabled = (int)StartOfRound.Instance.currentLevel.currentWeather == i;
                effect.effectEnabled = enabled;
                if (effect.effectPermanentObject != null)
                    effect.effectPermanentObject.SetActive(enabled);
                if (effect.effectObject != null)
                    effect.effectObject.SetActive(enabled);
                if (TimeOfDay.Instance.sunAnimator != null)
                {
                    if (enabled && !string.IsNullOrEmpty(effect.sunAnimatorBool))
                        TimeOfDay.Instance.sunAnimator.SetBool(effect.sunAnimatorBool, true);
                    else
                    {
                        TimeOfDay.Instance.sunAnimator.Rebind();
                        TimeOfDay.Instance.sunAnimator.Update(0);
                    }
                }
            }
            if (originalWeather == LevelWeatherType.Flooded)
            {
                var player = GameNetworkManager.Instance.localPlayerController;
                player.isUnderwater = false;
                player.sourcesCausingSinking = Mathf.Clamp(player.sourcesCausingSinking - 1, 0, 100);
                player.isMovementHindered = Mathf.Clamp(player.isMovementHindered - 1, 0, 100);
                player.hinderedMultiplier = 1f;
            }
        }

        public static void ChangeWeatherWR(LevelWeatherType weather)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsHost)
                WeatherController.SetWeatherEffects(weather);
        }

        public static bool IsWeatherEffectPresent(string weatherNameResolvable)
        {
            return IsWeatherEffectPresent(new WeatherNameResolvable(weatherNameResolvable).WeatherType);
        }

        public static bool IsWeatherEffectPresent(LevelWeatherType weatherType)
        {
            for (int i = 0; i < WeatherManager.CurrentEffectTypes.Count; i++)
            {
                if (WeatherManager.CurrentEffectTypes[i] == weatherType)
                {
                    return true;
                }
            }
            return false;
        }

        public static void RemoveWeather(string weatherNameResolvable)
        {
            RemoveWeather(new WeatherNameResolvable(weatherNameResolvable).WeatherType);
        }

        public static void RemoveWeather(LevelWeatherType weatherType)
        {
            if (WeatherManager.CurrentWeathers.Contains(weatherType))
            {
                WeatherManager.GetWeather(weatherType).Effect.EffectEnabled = false;
            }
        }

        public static ImprovedWeatherEffect? GetWeatherEffect(string weatherNameResolvable)
        {
            return GetWeatherEffect(new WeatherNameResolvable(weatherNameResolvable).WeatherType);
        }

        public static ImprovedWeatherEffect? GetWeatherEffect(LevelWeatherType weatherType)
        {
            var weather = WeatherManager.GetWeather(weatherType);
            if (weather == null)
                return null;
            var weatherEffect = WeatherManager.GetWeather(weatherType).Effect;
            return weatherEffect;
        }

        public static void Message(string title, string bottom, bool warning = false)
        {
            HUDManager.Instance.DisplayTip(title, bottom, warning);
        }

        public static void MessageOneTime(string title, string bottom, bool warning = false, string saveKey = "LC_Tip1")
        {
            if (ES3.Load(saveKey, "LCGeneralSaveData", false))
            {
                return;
            }
            HUDManager.Instance.tipsPanelHeader.text = title;
            HUDManager.Instance.tipsPanelBody.text = bottom;
            if (warning)
            {
                HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerWarning");
                RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.warningSFX, randomize: false);
            }
            else
            {
                HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerHint");
                RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.tipsSFX, randomize: false);
            }
            ES3.Save(saveKey, true, "LCGeneralSaveData");
        }

        public static void MessageComputer(params string[] messages)
        {
            var dialogue = new DialogueSegment[messages.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                dialogue[i] = new DialogueSegment
                {
                    speakerText = "PILOT COMPUTER",
                    bodyText = messages[i]
                };
            }
            HUDManager.Instance.ReadDialogue(dialogue);
        }

        public static IEnumerator Status(string text)
        {
            while (true)
            {
                HUDManager.Instance.DisplayStatusEffect(text);
                yield return new WaitForSeconds(1);
            }
        }

        public static NetworkObjectReference Spawn(SpawnableEnemyWithRarity enemy, Vector3 position, float yRot = 0f)
        {
            GameObject gameObject = Object.Instantiate(enemy.enemyType.enemyPrefab, position, Quaternion.Euler(new Vector3(0f, yRot, 0f)));
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            return new NetworkObjectReference(gameObject);
        }

        public static void SpawnMaskedOfPlayer(ulong playerId, Vector3 position)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            bool flag = player.transform.position.y < -80f;
            var netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(position, player.transform.eulerAngles.y, -1, Utils.GetEnemies.Masked.enemyType);
            if (netObjectRef.TryGet(out var networkObject))
            {
                var component = networkObject.GetComponent<MaskedPlayerEnemy>();
                component.SetSuit(player.currentSuitID);
                component.mimickingPlayer = player;
                component.SetEnemyOutside(!flag);
                component.CreateMimicClientRpc(netObjectRef, flag, (int)playerId);
            }
        }

        public static void Spawn(SpawnableMapObject trap, Vector3 position, float yRot = 0f)
        {
            GameObject gameObject = Object.Instantiate(trap.prefabToSpawn, position, Quaternion.Euler(new Vector3(0f, yRot, 0f)), RoundManager.Instance.mapPropsContainer.transform);
            gameObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public static SpawnableItemWithRarity GetScrap(string scrapName)
        {
            return RoundManager.Instance.currentLevel.spawnableScrap.FirstOrDefault(i => i.spawnableItem.name.Equals(scrapName));
        }

        public static ItemNetworkReference Spawn(SpawnableItemWithRarity scrap, Vector3 position, bool inFactory = false, bool inShip = false, bool inElevator = false)
        {
            return Spawn(scrap.spawnableItem, position, inFactory, inShip, inElevator);
        }

        public static ItemNetworkReference Spawn(Item? item, Vector3 position, bool inFactory = false, bool inShip = false, bool inElevator = false)
        {
            if (item == null)
            {
                Plugin.logger.LogError("Spawning item error: Item is null.");
                return new ItemNetworkReference(default, 0);
            }
            var parent = RoundManager.Instance.spawnedScrapContainer ?? StartOfRound.Instance.elevatorTransform;
            GameObject gameObject = Object.Instantiate(item.spawnPrefab, position + Vector3.up * 0.25f, Quaternion.identity, parent);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.fallTime = 1f;
            component.hasHitGround = true;
            component.reachedFloorTarget = true;
            component.isInFactory = inFactory;
            component.isInElevator = inElevator;
            component.isInShipRoom = inShip;
            if (component.itemProperties.isScrap)
                component.scrapValue = (int)(Random.Range(item.minValue, item.maxValue) * RoundManager.Instance.scrapValueMultiplier);
            component.NetworkObject.Spawn();
            return new ItemNetworkReference(gameObject.GetComponent<NetworkObject>(), component.itemProperties.isScrap ? component.scrapValue : 0);
        }

        public static IEnumerator SyncItem(NetworkObjectReference itemRef, int value, int save)
        {
            NetworkObject? itemNetObject = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !itemRef.TryGet(out itemNetObject))  // wait for item to spawn
            {
                yield return new WaitForSeconds(0.03f);
            }
            if (itemNetObject == null)
            {
                Plugin.logger.LogError("Error while trying to sync the item.");
                yield break;
            }
            yield return new WaitForEndOfFrame();
            GrabbableObject component = itemNetObject.GetComponent<GrabbableObject>();
            component.fallTime = 0f;
            if (component.itemProperties.isScrap)
                component.SetScrapValue(value);
            if (component.itemProperties.saveItemVariable)
                component.LoadItemSaveData(save);
        }

        public static void SpawnQuicksand(int nb)
        {
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 2);
            var outsideAINodes = (from x in GameObject.FindGameObjectsWithTag("OutsideAINode")
                                  orderby Vector3.Distance(x.transform.position, Vector3.zero)
                                  select x).ToArray();
            NavMeshHit val = default;
            for (int i = 0; i < nb; i++)
            {
                Vector3 position = outsideAINodes[random.Next(0, outsideAINodes.Length)].transform.position;
                Vector3 position2 = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 30f, val, random) + Vector3.up;
                GameObject gameObject = Object.Instantiate(RoundManager.Instance.quicksandPrefab, position2, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
            }
        }

        public static void SpawnLightningBolt(Vector3 strikePosition, bool damage = true, bool redirectInside = true)
        {
            LightningBoltPrefabScript localLightningBoltPrefabScript;
            var random = new System.Random(StartOfRound.Instance.randomMapSeed);
            random.Next(-32, 32); random.Next(-32, 32);
            var vector = strikePosition + Vector3.up * 160f + new Vector3(random.Next(-32, 32), 0f, random.Next(-32, 32));
            if (redirectInside && Physics.Linecast(vector, strikePosition + Vector3.up * 0.5f, out _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                if (!Physics.Raycast(vector, strikePosition - vector, out var rayHit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    return;
                strikePosition = rayHit.point;
            }
            StormyWeather stormy = Object.FindObjectOfType<StormyWeather>(true);
            localLightningBoltPrefabScript = Object.Instantiate(stormy.targetedThunder);
            localLightningBoltPrefabScript.enabled = true;
            localLightningBoltPrefabScript.Camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
            localLightningBoltPrefabScript.AutomaticModeSeconds = 0.2f;
            localLightningBoltPrefabScript.Source.transform.position = vector;
            localLightningBoltPrefabScript.Destination.transform.position = strikePosition;
            localLightningBoltPrefabScript.CreateLightningBoltsNow();
            AudioSource audioSource = Object.Instantiate(stormy.targetedStrikeAudio);
            audioSource.transform.position = strikePosition + Vector3.up * 0.5f;
            audioSource.enabled = true;
            if (damage)
                Landmine.SpawnExplosion(strikePosition + Vector3.up * 0.25f, spawnExplosionEffect: false, 2.4f, 5f);
            stormy.PlayThunderEffects(strikePosition, audioSource);
        }
    }
}
