using AllOfPlugins_SCP_Verluer.GamePatch;
using CentralAuth;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerStatsSystem;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AllOfPlugins_SCP_Verluer.EventModule
{
    public class Scp261Event
    {
        public static void Enable()
        {
            LabApi.Features.Console.Logger.Info("Scp261Event START");
            EventScp261();
        }


        public static void Disable()
        {
        }

        public static void EventScp261()
        {
            List<ReferenceHub> EventPlayerList = new List<ReferenceHub>();


            LabApi.Features.Console.Logger.Info("Event START");
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;

                if (hub.roleManager.CurrentRole.Team != Team.SCPs)
                {
                    EventPlayerList.Add(hub);
                }         
            }

            LabApi.Features.Console.Logger.Info(
                $"Candidates: {EventPlayerList.Count}"
            );

            ReferenceHub SCP261Player = EventPlayerList[UnityEngine.Random.Range(0, EventPlayerList.Count)];

            Player.Get(SCP261Player).SendBroadcast("Поздравляем! Вы выбраны великим Scp261 как его апостол. Привнесите в этот мир больше лудомании!.", 5);

            SCP261Player.roleManager.ServerSetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
            PlayerSchematicManager.Attach(Player.Get(SCP261Player), "Scp261", new Vector3(0f, -0.7f, 0f), Vector3.zero, true);

            Timing.CallDelayed(0.5f, () =>
            {
                PlayerSchematicManager.EnableFade(Player.Get(SCP261Player));
                PlayerSchematicManager.HideFor(Player.Get(SCP261Player));
            });


        }
    }
}
