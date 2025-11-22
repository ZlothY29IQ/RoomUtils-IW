using HarmonyLib;
using UnityEngine;
using static RoomUtils.Plugin;

namespace RoomUtils.Patches
{
    [HarmonyPatch(typeof(ForceVolume))]
    [HarmonyPatch("SliceUpdate")]
    internal class WindPatch
    {
        private static bool Prefix(ForceVolume __instance)
        {
            if (WindState.WindEnabled)
            {
                if (__instance.audioSource != null)
                    __instance.audioSource.enabled = false;

                Collider volume = Traverse.Create(__instance).Field<Collider>("volume").Value;
                if (volume != null)
                    volume.enabled = false;

                return false;
            }

            Collider volume2 = Traverse.Create(__instance).Field<Collider>("volume").Value;
            if (volume2 != null)
                volume2.enabled = true;

            if (__instance.audioSource != null)
                __instance.audioSource.enabled = true;

            return true;
        }
    }
}