using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Features.Wrappers;
using System;
using System.Reflection;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{
    public class AllCandyExpansion
    {

        public static void Enable(Harmony _harmony)
        {
            Type[] CandyListMethod =
{
                typeof(CandyRainbow),
                typeof(CandyYellow),
                typeof(CandyPurple),
                typeof(CandyBlue),
                typeof(CandyGreen),
                typeof(CandyRed),

                typeof(CandyBlack),
                typeof(CandyPink),

                typeof(HauntedCandyOrange),
                typeof(HauntedCandyGray),
                typeof(HauntedCandyWhite),
                typeof(HauntedCandyBrown),

                typeof(HauntedCandyEvil),
            };


            for (int i = 0; i < CandyListMethod.Length; i++)
            {
                MethodInfo method = AccessTools.Method(CandyListMethod[i], "ServerApplyEffects");

                _harmony.Patch(method, prefix: new HarmonyMethod(typeof(AllCandyExpansion), nameof(ServerApplyEffectsPrefix)));
            }
        }

        public static void Disable()
        {

        }

        [HarmonyPriority(Priority.First)]
        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < 1)
            {

                ExplosionOutcome explosionOutcome = new ExplosionOutcome();
                explosionOutcome.ServerGrant(hub);
                Player.Get(hub).SendHint($"Вы <color=red>взорвались</color>. Вас убил <color=blue>Ferelur-Morron-Эдвард</color>", 10f);
            }
            return true;
        }

    }
}