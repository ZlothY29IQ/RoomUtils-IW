using HarmonyLib;
using UnityEngine;

namespace RoomUtils.Patches
{
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "ApplyKnockback")]
    public class KnockbackPatch
    {
        public static bool enabled;

        public static bool Prefix(Vector3 direction, float speed) =>
            !enabled;
    }
}