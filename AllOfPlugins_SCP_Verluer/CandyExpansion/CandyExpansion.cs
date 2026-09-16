using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Features.Wrappers;
using System.Reflection;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{
    public class CandyExpansion
    {
        private const int CandyCount = 4;

        private static readonly CandyKindID[] NormalCandies =
        {
                CandyKindID.Rainbow,
                CandyKindID.Yellow,
                CandyKindID.Purple,
                CandyKindID.Red,
                CandyKindID.Green,
                CandyKindID.Blue,
            };
        public static readonly CandyKindID[] SpecialCandies =
{
               CandyKindID.Orange,
               CandyKindID.White,
               CandyKindID.Gray,
               CandyKindID.Brown,
            };
        private static readonly CandyKindID[] RareCandies =
        {
                CandyKindID.Black,
                CandyKindID.Pink
            };
        private static readonly CandyKindID[] UltraRareCandies =
        {
             CandyKindID.Evil
        };

        public static void Enable(Harmony harmony)
        {
            BlackCandyExpansion.Enable(harmony);

            Scp330CandiesPatch.Enable(harmony);
            CandiesPatch.Enable(harmony);

            AllCandyExpansion.Enable(harmony);

            WhiteCandyExpansion.Enable(harmony);
            BrownCandyExpansion.Enable(harmony);
            GrayCandyExpansion.Enable(harmony);
            EvilCandyExpansion.Enable(harmony);
            OrangeCandyExpansion.Enable(harmony);
        }


        public static void Disable()
        {
            BlackCandyExpansion.Disable();

            Scp330CandiesPatch.Disable();
            CandiesPatch.Disable();

            AllCandyExpansion.Disable();

            WhiteCandyExpansion.Disable();
            BrownCandyExpansion.Disable();
            GrayCandyExpansion.Disable();
            EvilCandyExpansion.Disable();
            OrangeCandyExpansion.Disable();
        }

        public static void GiveRandomCandy(Player player)
        {
            if (player == null)
                return;

            for (int i = 0; i < CandyCount; i++)
            {
                CandyKindID candy = GetRandomCandy();

                if (candy == CandyKindID.None)
                    continue;

                Scp330Bag.TryAddCandy(
                    player.ReferenceHub,
                    candy);
            }
        }


        private static CandyKindID GetRandomCandy()
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll == 99)
                return UltraRareCandies[0];
            if (roll >= 85)
                return RareCandies[UnityEngine.Random.Range(0, RareCandies.Length)];

            if (roll >= 65)
                return SpecialCandies[UnityEngine.Random.Range(0, SpecialCandies.Length)];

            return NormalCandies[UnityEngine.Random.Range(0, NormalCandies.Length)];
        }
        private static class Scp330CandiesPatch
        {
            public static void Enable(Harmony _harmony)
            {
                MethodInfo method = AccessTools.Method(typeof(Scp330Candies), "GetRandom");
                if (method == null)
                { return; }

                _harmony.Patch(method, prefix: new HarmonyMethod(typeof(Scp330CandiesPatch), nameof(NewGetRandom)));
            }
            public static void Disable()
            {

            }
            public static bool NewGetRandom(ref CandyKindID __result)
            {
                __result = GetRandomCandy();
                return false;
            }
        }
        private static class CandiesPatch
        {
            public static void Enable(Harmony _harmony)
            {
                MethodInfo method = AccessTools.PropertyGetter(typeof(Scp330Candies), "Candies");
                if (method == null)
                { return; }

                _harmony.Patch(method, prefix: new HarmonyMethod(typeof(CandiesPatch), nameof(EnableHwCandies)));
            }
            public static void Disable()
            {

            }
            public static bool EnableHwCandies(ref ICandy[] __result)
            {
                __result = NewCandies;
                return false;
            }
            public static readonly ICandy[] NewCandies = new ICandy[]
            {
                new CandyGreen(),
                new CandyPurple(),
                new CandyRainbow(),
                new CandyRed(),
                new CandyYellow(),
                new CandyBlue(),
                new CandyPink(),
                new CandyBlack(),
                new HauntedCandyBrown(),
                new HauntedCandyOrange(),
                new HauntedCandyWhite(),
                new HauntedCandyGray(),
                new HauntedCandyEvil()
            };
        }
    }
}
