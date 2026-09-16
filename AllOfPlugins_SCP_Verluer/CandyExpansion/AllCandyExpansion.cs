using CustomPlayerEffects;
using Exiled.API.Features.Roles;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using ProjectMER.Features.Extensions;
using Respawning.Objectives;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using UnityEngine;
using static PlayerList;

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
                Player.Get(hub).SendHint($"Вы <color=red>взорвались</color>. Вас убил <color=blue>Ferelur-Morron-Эдвард</color>", 5f);
            }
            return true;
        }

    }
}