
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.Features
{
    public class SwapScpRole
    {
        private static readonly Dictionary<ReferenceHub, bool> _playersPressedP = new Dictionary<ReferenceHub, bool>();


        public static void MethodSwapRole(ReferenceHub sender)
        {
            if (sender.roleManager.CurrentRole.Team != Team.SCPs)
            {
                return;
            }

            if (Time.time - EventModule.VanillaEventHandler.RoundStartTime > 45f)
            {
                Player.Get(sender).SendHint(
                   "Нельзя обменяться спустя 45 секунд после начала раунда",
                   5f
               );
                return;
            }
            _playersPressedP[sender] = true;


            foreach (ReferenceHub SCPhub in ReferenceHub.AllHubs)
            {
                if (SCPhub == sender)
                {
                    continue;
                }
                if (SCPhub.roleManager.CurrentRole.Team != Team.SCPs)
                    continue;

                if (!_playersPressedP.TryGetValue(SCPhub, out bool pressedP))
                    continue;

                if (!pressedP)
                    continue;

                RoleTypeId OneScpSwap = sender.GetRoleId();
                RoleTypeId TwoScpSwap = SCPhub.GetRoleId();

                sender.roleManager.ServerSetRole(
                    TwoScpSwap,
                    RoleChangeReason.RemoteAdmin,
                    RoleSpawnFlags.UseSpawnpoint);

                SCPhub.roleManager.ServerSetRole(
                    OneScpSwap,
                    RoleChangeReason.RemoteAdmin,
                    RoleSpawnFlags.UseSpawnpoint);

                _playersPressedP[sender] = false;
                _playersPressedP[SCPhub] = false;

                return;
            }
        }
    }
}
