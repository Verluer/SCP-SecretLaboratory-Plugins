using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using System.Collections.Generic;
using System.Reflection;
namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{

    public static class BrownCandyExpansion
    {
        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(HauntedCandyBrown), "ServerApplyEffects");

            if (method == null) return;

            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(BrownCandyExpansion), nameof(ServerApplyEffectsPrefix)));
        }
        public static void Disable()
        {
        }
        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            ApplyBrownCandyEffect(hub);
            return false;
        }
        public static void ApplyBrownCandyEffect(ReferenceHub hub)
        {
            hub.playerEffectsController.ChangeState<MovementBoost>(100, 10f);
            Timing.RunCoroutine(ApplySlowness(hub));
        }
        private static IEnumerator<float> ApplySlowness(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(10f);

            hub.playerEffectsController.ChangeState<Slowness>(75, 5f);
        }
    }
}
