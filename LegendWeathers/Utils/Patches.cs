namespace LegendWeathers.Utils
{
    /*[HarmonyPatch(typeof(ItemCharger))]
    internal class BombItemChargerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePatch(ref ItemCharger __instance)
        {
            if ((float)Traverse.Create(__instance).Field("updateInterval").GetValue() == 0f)
            {
                if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    __instance.triggerScript.interactable = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null && (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.requiresBattery ||
                        (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.name == "BombItem" && GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer is CustomEffects.Bomb bomb && !bomb.activated));
                    return;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ChargeItem")]
        public static bool ChargeItemPatch()
        {
            return CustomEffects.Bomb.ChargeItemUnstable();
        }
    }*/
}
