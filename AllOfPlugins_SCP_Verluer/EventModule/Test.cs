using AllOfPlugins_SCP_Verluer.GamePatch;
using CentralAuth;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using RemoteAdmin.Communication;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AllOfPlugins_SCP_Verluer.EventModule
{
    public class Test
    {

        private static ReferenceHub SCP261Player;
        public static void Enable()
        {
            EventTest();
        }


        public static void Disable()
        {
        }

        public static void EventTest()
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

            SCP261Player = EventPlayerList[UnityEngine.Random.Range(0, EventPlayerList.Count)];

            SCP261Player.roleManager.ServerSetRole(RoleTypeId.ClassD, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
            Core.PlayerSchematicManager.Attach(Player.Get(SCP261Player), "NewSchematic", new Vector3(0f, 0.05f, 0.19f), new Vector3(0f, 0f, 0f), true);


            Timing.CallDelayed(0.5f, () =>
            {
                Core.PlayerSchematicManager.HideFor(Player.Get(SCP261Player));

            });
        }
      
    }
}
