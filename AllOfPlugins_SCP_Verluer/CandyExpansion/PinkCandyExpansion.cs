using Footprinting;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{
    public class PinkCandyExpansion
    {
        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(CandyPink), "ServerApplyEffects");

            if (method == null) return;

            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(PinkCandyExpansion), nameof(ServerApplyEffectsPrefix)));
        }
        public static void Disable()
        {
        }
        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            try
            {
                int pinkCandies = GetPinkCandyCount(hub);
                RemoveAllPinkCandies(hub);


                Vector3 explosionPosition = hub.transform.position;
                Footprint explosionFootprint = new Footprint(hub);
                LabApi.Features.Console.Logger.Info($"Pink candies: {pinkCandies}");

                for (int i = 0; i < pinkCandies; i++)
                {
                    ExplosionUtils.ServerExplode(explosionPosition, explosionFootprint, ExplosionType.PinkCandy);

                }
                return false;
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"PinkCandyExpansion exception: {ex}");
                return true;
            }
        }
        private static int GetPinkCandyCount(ReferenceHub hub)
        {
            if (!Scp330Bag.TryGetBag(hub, out Scp330Bag bag))
                return 0;

            return bag.Candies.Count(x => x == CandyKindID.Pink);
        }
        private static int RemoveAllPinkCandies(ReferenceHub hub)
        {
            if (!Scp330Bag.TryGetBag(hub, out Scp330Bag bag))
                return 0;

            int removed = 0;

            for (int i = bag.Candies.Count - 1; i >= 0; i--)
            {
                if (bag.Candies[i] == CandyKindID.Pink)
                {
                    bag.Candies.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
                bag.ServerRefreshBag();

            return removed;
        }
    }
}
