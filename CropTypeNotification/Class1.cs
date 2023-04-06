using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Wish;
using System.Reflection;
using System.Text.RegularExpressions;

[BepInPlugin("stevencheney.sunhaven.crop_type_notification", "Crop Type Notification", "0.0.1")]
public class CropTypeNotification : BaseUnityPlugin
{
    private Harmony m_harmony = new Harmony("stevencheney.sunhaven.crop_type_notification");
    public static ManualLogSource logger;

    private void Awake()
    {
        logger = this.Logger;
        logger.LogInfo((object)"stevencheney.sunhaven.crop_type_notification v0.0.1 loaded.");
        this.m_harmony.PatchAll();
    }

    [HarmonyPatch(typeof(Crop), "ReceiveDamage")]
    class HarmonyPatch_Crop_ReceiveDamage
    {
        public static bool Prefix(Crop __instance, DamageInfo damageInfo, DamageHit __result)
        {
            PropertyInfo fullyGrownProperty = typeof(Crop).GetProperty("FullyGrown", BindingFlags.NonPublic | BindingFlags.Instance);

            // Access the "FullyGrown" property from the "__instance" parameter using GetValue method
            bool fullyGrown = (bool) fullyGrownProperty.GetValue(__instance, null);

            if (!fullyGrown)
            {
                string timeLeft = (__instance.DaysLeft == 1) ? " and will finish tomorrow!" : " and will finish in " + __instance.DaysLeft + " days!";

                SingletonBehaviour<NotificationStack>.Instance.SendNotification("This crop is a: " + __instance._cropItem.FormattedName + timeLeft);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NotificationStack), "SendNotification")]
    public class HarmonyPatch_NotificationStack_SendNotification
    {
        public static bool Prefix(string text, int id = 0, int amount = 0, bool unique = false, bool error = false)
        {
            string pattern = @"This crop will finish in \d+ days!";

            if (Regex.IsMatch(text, pattern))
            {
                return false;
            }

            if (text == "This crop will finish tomorrow!")
            {
                return false;
            }

            return true;
        }
    }
}

