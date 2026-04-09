using BepInEx.Bootstrap;
using GameNetcodeStuff;
using LegendWeathers.Weathers;
using LethalLib.Modules;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Utils
{
    internal class Compatibility
    {
        #region PLUGIN

        public static bool WeatherRegisteryInstalled = false;
        public static bool BiodiversityInstalled = false;
        public static bool SurfacedInstalled = false;
        public static bool PremiumScrapsInstalled = false;
        public static bool EmergencyDiceInstalled = false;
        public static bool CodeRebirthInstalled = false;
        public static bool HauntedHarpistInstalled = false;
        public static bool ImperiumInstalled = false;

        public static void CheckInstalledPlugins()
        {
            WeatherRegisteryInstalled = IsPluginInstalled("mrov.WeatherRegistry");
            BiodiversityInstalled = IsPluginInstalled("com.github.biodiversitylc.Biodiversity");
            SurfacedInstalled = IsPluginInstalled("Surfaced");
            PremiumScrapsInstalled = IsPluginInstalled("zigzag.premiumscraps");
            EmergencyDiceInstalled = IsPluginInstalled("Theronguard.EmergencyDice");
            CodeRebirthInstalled = IsPluginInstalled("CodeRebirth");
            HauntedHarpistInstalled = IsPluginInstalled("LethalCompanyHarpGhost");
            ImperiumInstalled = IsPluginInstalled("giosuel.Imperium");
        }

        private static bool IsPluginInstalled(string pluginGUID, string? pluginVersion = null)
        {
            return Chainloader.PluginInfos.ContainsKey(pluginGUID) &&
                (pluginVersion == null || new System.Version(pluginVersion).CompareTo(Chainloader.PluginInfos[pluginGUID].Metadata.Version) <= 0);
        }

        #endregion

        #region MAJORA MOON

        internal static bool IsMajoraActiveOnCompany { get; private set; } = false;
        internal static float MajoraCompanyTimer { get; private set; } = 105;  // in seconds

        public static void SetMajoraCompanyCompatible(bool isCompatible)
        {
            IsMajoraActiveOnCompany = isCompatible;
            MajoraCompanyTimer = 105;
        }

        public static float GetCompanySmoothTime(float deltaTime)
        {
            MajoraCompanyTimer -= deltaTime;
            return MajoraCompanyTimer;
        }

        public static bool IsMoonCompanyCompatible(SelectableLevel level)
        {
            if (!Plugin.config.majoraCompanyCompatible.Value)
                return false;
            if (!level.planetHasTime && !level.spawnEnemiesAndScrap)
                return true;
            return false;
        }

        public static bool IsMajoraBiodiversityCompatible()
        {
            if (!BiodiversityInstalled || Biodiversity.Creatures.MicBird.MicBirdHandler.Instance == null || RoundManager.Instance == null)
            {
                return false;
            }
            var (levelRarities, customLevelRarities) = Biodiversity.Util.LethalLibUtils.ConfigParsing(Biodiversity.Creatures.MicBird.MicBirdHandler.Instance.Config.BoomBirdRarity);
            var moonName = Regex.Replace(RoundManager.Instance.currentLevel.PlanetName, "^[0-9]+", string.Empty);
            if (moonName[0] == ' ')
                moonName = moonName[1..];
            if ((System.Enum.TryParse(moonName, true, out Levels.LevelTypes levelType) && levelRarities.ContainsKey(levelType) && levelRarities[levelType] >= 30)
                || (System.Enum.TryParse(moonName + "Level", true, out Levels.LevelTypes modifLevelType) && levelRarities.ContainsKey(modifLevelType) && levelRarities[modifLevelType] >= 30)
                || (customLevelRarities.ContainsKey(moonName) && customLevelRarities[moonName] >= 30))
                return true;
            return false;
        }

        public static bool IsMajoraSurfacedCompatible()
        {
            if (!SurfacedInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            foreach (var scrap in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (scrap.itemProperties.itemName == "Rodriguez" && scrap.isInFactory)
                    return true;
            }
            return false;
        }

        public static bool IsMajoraPremiumScrapsCompatible()
        {
            if (!PremiumScrapsInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            if (Effects.GetPlayers(includeDead: true).Find(p => p.playerSteamId == 76561198198881967) != default)
                return true;
            foreach (var scrap in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (scrap.itemProperties.itemName == "The talking orb" && scrap.isInFactory)
                    return true;
            }
            return false;
        }

        public static bool IsMajoraEmergencyDiceCompatible()
        {
            if (!EmergencyDiceInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            foreach (var scrap in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (scrap.itemProperties.itemName == "Rusty" && scrap.isInFactory)
                    return true;
            }
            return false;
        }

        public static bool IsMajoraCodeRebirthCompatible()
        {
            if (!CodeRebirthInstalled || RoundManager.Instance == null)
            {
                return false;
            }
            if (Effects.GetPlayers(includeDead: true).Find(p => p.playerSteamId == 76561198984467725) != default)
                return true;
            return false;
        }

        public static bool IsMajoraImperiumPausedTime()
        {
            if (!ImperiumInstalled || RoundManager.Instance == null || Imperium.Imperium.MoonManager == null)
            {
                return false;
            }
            return Imperium.Imperium.MoonManager.TimeIsPaused.Value;
        }

        #endregion

        #region BLOOD MOON

        public static bool BloodMoonSpawnEnemySpecific(EnemyAI enemy, Vector3 spawnPosition, out bool withEnemyRef, out NetworkObjectReference enemyRef, out bool withPlayer, out ulong playerId)
        {
            withEnemyRef = false;
            withPlayer = false;
            enemyRef = new NetworkObjectReference();
            playerId = 0;
            if (enemy.gameObject == null)
            {
                return false;
            }
            var masked = enemy.gameObject.GetComponent<MaskedPlayerEnemy>();
            var cadaverBloom = enemy.gameObject.GetComponent<CadaverBloomAI>();
            if (masked != null && masked.mimickingPlayer != null)
            {
                Effects.SpawnMaskedOfPlayer(masked.mimickingPlayer.playerClientId, spawnPosition);
            }
            else if (cadaverBloom != null && cadaverBloom.burstPlayer != null)
            {
                withEnemyRef = true;
                withPlayer = true;
                playerId = cadaverBloom.burstPlayer.playerClientId;
                enemyRef = RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, Random.Range(-90f, 90f), -1, enemy.enemyType);
            }
            else
            {
                return false;
            }
            return true;
        }

        public static void SyncBloodMoonSpawnEnemySpecific(NetworkObjectReference enemyRef, Vector3 spawnPosition, bool playerIdIsValid, ulong playerId)
        {
            GameObject enemyObject = (GameObject)enemyRef;
            CadaverBloomAI cadaverBloom = enemyObject.GetComponent<CadaverBloomAI>();
            if (cadaverBloom != null && playerIdIsValid)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
                cadaverBloom.BurstForth(player, false, spawnPosition, new Vector3(0f, Random.Range(-90f, 90f), 0f));
            }
        }

        public static bool ManageSpecialItemForBloodMoon(string enemyNameHeldBy, GrabbableObject? specialItem)  // used by BloodMoonPatches
        {
            if (Plugin.config.bloodMoonSpecificEnemiesItemSpawningMode.Value == "Chance")
            {
                return ManageSpecialItemForBloodMoonByChance(specialItem);
            }
            if (!BloodMoonManager.UpdateFirstEnemyRespawned(enemyNameHeldBy))
            {
                if (specialItem != null && specialItem is ShotgunItem gunItem)
                {
                    gunItem.shellsLoaded = 2;
                }
                BloodMoonManager.UpdateFirstEnemyRespawned(enemyNameHeldBy, true);
                return true;
            }
            else
            {
                if (specialItem != null && specialItem.IsServer)
                {
                    var itemNetwork = specialItem.gameObject.GetComponent<NetworkObject>();
                    if (itemNetwork != null && itemNetwork.IsSpawned)
                    {
                        itemNetwork.Despawn();
                    }
                }
                return false;
            }
        }

        public static bool ManageSpecialItemForBloodMoonByChance(GrabbableObject? specialItem)  // used by BloodMoonPatches
        {
            if (BloodMoonManager.nextRandomSpecificEnemyItemSpawnChance < Plugin.config.bloodMoonSpecificEnemiesItemSpawningChance.Value)
            {
                return true;
            }
            else
            {
                if (specialItem != null && specialItem.IsServer)
                {
                    var itemNetwork = specialItem.gameObject.GetComponent<NetworkObject>();
                    if (itemNetwork != null && itemNetwork.IsSpawned)
                    {
                        itemNetwork.Despawn();
                    }
                }
                return false;
            }
        }

        #endregion
    }
}
