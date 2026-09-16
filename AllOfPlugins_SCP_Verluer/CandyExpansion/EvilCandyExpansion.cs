using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using Respawning.Objectives;
using System.Collections.Generic;
using System.Reflection;
namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{

    public static class EvilCandyExpansion
    {
        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(HauntedCandyEvil), "ServerApplyEffects");

            if (method == null) return;

            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(EvilCandyExpansion), nameof(ServerApplyEffectsPrefix)));
        }
        public static void Disable()
        {
        }
        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            List<ReferenceHub> SCPlist = new List<ReferenceHub>();
            foreach (ReferenceHub SCPhub in ReferenceHub.AllHubs)
            {
                if (SCPhub.roleManager.CurrentRole.Team == Team.SCPs && SCPhub.roleManager.CurrentRole.RoleTypeId != RoleTypeId.Scp0492)
                {
                    SCPlist.Add(SCPhub);
                    var role = SCPhub.roleManager.CurrentRole;

                    LabApi.Features.Console.Logger.Info(
                        $"[{SCPhub.nicknameSync.MyNick}] " +
                        $"Type={role.GetType().FullName}, " +
                        $"RoleId={role.RoleTypeId}, " +
                        $"Team={role.Team}, " +
                        $"Fpc={role is FpcStandardRoleBase}"
                    );
                }
            }
            if (SCPlist.Count == 0)
            {
                AhpStat ahp = hub.playerStats.GetModule<AhpStat>();
                ahp.ServerKillAllProcesses();
                ahp.ServerAddProcess(450f, 450f, 0f, 1f, 0f, true);

            }
            else
            {
                int roll = UnityEngine.Random.Range(0, SCPlist.Count);
                hub.inventory.ServerDropEverything();
                RoleTypeId HumanRole = hub.GetRoleId();
                RoleTypeId SCPRole = SCPlist[roll].GetRoleId();

                hub.roleManager.ServerSetRole(SCPRole, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
                SCPlist[roll].roleManager.ServerSetRole(HumanRole, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.AssignInventory);

                Timing.CallDelayed(10f, () =>
                {
                    Player.Get(hub).SendHint(
                        $"Вы заменили <color=red>{SCPRole}</color> <color=green>{SCPlist[roll].GetNickname()}</color>, поздравляем вас!",
                        10f
                    );

                    Player.Get(SCPlist[roll]).SendHint(
                        $"Вас заменил <color=blue>{HumanRole}</color> <color=green>{hub.GetNickname()}</color>, теперь вы самый обычный человек",
                        10f
                    );
                });

            }
            return false;
        }
    }
}
