using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using System.Reflection;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{
    public static class OrangeCandyExpansion
    {
        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(HauntedCandyOrange), "ServerApplyEffects");

            if (method == null) return;

            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(OrangeCandyExpansion), nameof(ServerApplyEffectsPrefix)));
        }
        public static void Disable()
        {
        }
        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            ApplyOrangeCandyEffect(hub);

            return false;
        }
        public static void ApplyOrangeCandyEffect(ReferenceHub hub)
        {
            foreach (ReferenceHub target in ReferenceHub.AllHubs)
            {
                if (target == null)
                    continue;

                if (target == hub)
                    continue;

                float distance = Vector3.Distance(
                    hub.transform.position,
                    target.transform.position
                );
                if (distance > 10f)
                    continue;

                target.playerEffectsController.EnableEffect<Flashed>(15f);

            }
        }

    }
}
