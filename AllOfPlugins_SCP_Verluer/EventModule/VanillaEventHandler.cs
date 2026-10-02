using AllOfPlugins_SCP_Verluer.Core;
using GameCore;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using MEC;
using PlayerRoles.RoleAssign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.EventModule
{
    public static class VanillaEventHandler
    {
        public static float RoundStartTime;
        public static void Enable()
        {
            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.SpawningRagdoll += OnSpawningRagdoll;
            RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;
        }

        public static void Disable()
        {
            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;
            PlayerEvents.SpawningRagdoll -= OnSpawningRagdoll;
            PlayerEvents.ChangedRole -= OnChangedRole;
        }

        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            HUD.ScpHpHood();
        }

        private static void OnPlayersSpawned()
        {
            RoundStartTime = Time.time;

            HUD.AllPlayerHud();
            HUD.ScpHpHood();
        }

        private static void OnSpawningRagdoll(PlayerSpawningRagdollEventArgs ev)
        {
            if (!PlayerSchematicManager.HasSchematic(ev.Player))
                return;

            ev.IsAllowed = false;

            PlayerSchematicManager.DetachAsCorpse(ev.Player);

            Timing.CallDelayed(0.1f, () =>
            {
                if (ev.Player == null)
                    return;

                PlayerSchematicManager.ShowFor(ev.Player);
            });
        }
    }
}
