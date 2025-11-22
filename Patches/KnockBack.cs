using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace RoomUtils.Patches
{
    [HarmonyPatch(typeof(GTPlayer), "ApplyKnockback")]
    public class KnockbackPatch
    {
        public static bool Prefix(Vector3 direction, float speed)
        {
            bool disabled = Plugin.Knockback.Value || Plugin.KnockbackState.KnockbackEnabled;

            return !disabled;
        }
    }
}
