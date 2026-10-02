using AllOfPlugins_SCP_Verluer.Core;
using AllOfPlugins_SCP_Verluer.CustomModule.Role;
using CentralAuth;
using CustomPlayerEffects;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerStatsSystem;
using System.Collections.Generic;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.EventModule
{

    public class ZombieApocalypse
    {
        public static Vector3 ZombiePosition;
        public static void Enable()
        {
            LabApi.Features.Console.Logger.Info("ZombieApocalypse START");
            EventZombie();
        }


        public static void Disable()
        {
        }

        public static void EventZombie()
        {
            List<ReferenceHub> EventPlayerList = new List<ReferenceHub>();


            LabApi.Features.Console.Logger.Info("Event START");
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;

                if (hub.roleManager.CurrentRole.Team == Team.SCPs)
                {
                    hub.roleManager.ServerSetRole(RoleTypeId.ClassD, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
                }

                if (hub.roleManager.CurrentRole.Team == Team.ClassD || hub.roleManager.CurrentRole.Team == Team.Scientists)
                {
                    LabApi.Features.Console.Logger.Info($"Nick: {hub.nicknameSync.MyNick}");
                    EventPlayerList.Add(hub);
                }
            }

            LabApi.Features.Console.Logger.Info(
                $"Candidates: {EventPlayerList.Count}"
            );

            ReferenceHub ZombiePlayer = EventPlayerList[UnityEngine.Random.Range(0, EventPlayerList.Count)];

            Player.Get(ZombiePlayer).SendBroadcast("Вы чувствуете лёгкое <color=green>недомогание</color>...", 5);

            Timing.CallDelayed(10f, () =>
            {
                Player.Get(ZombiePlayer).SendBroadcast("По телу пробегает неприятный <color=blue>холод</color>. Вам становится всё <color=red>хуже</color>.", 5);
            });
            Timing.CallDelayed(25f, () =>
            {
                Player.Get(ZombiePlayer).SendBroadcast("Вас начинает <color=red>трясти</color>. Дыхание становится тяжёлым.", 5);
            });
            Timing.CallDelayed(40f, () =>
            {
                Player.Get(ZombiePlayer).SendBroadcast("Вы чувствуете, как <color=red>болезнь</color> стремительно распространяется по вашему телу.", 5);
            });
            Timing.CallDelayed(55f, () =>
            {
                Player.Get(ZombiePlayer).SendBroadcast("Последние силы покидают вас. <color=red>Чума</color> поглощает ваше тело...", 5);
                ZombiePosition = Player.Get(ZombiePlayer).Position; 
                Player.Get(ZombiePlayer).Kill();
            });
            Timing.CallDelayed(60f, () =>
            {
                SCP049_2_Alpha.SpawnScp0492Alpha(ZombiePlayer, ZombiePosition);

                Player.Get(ZombiePlayer).SendBroadcast($"Ваше тело поглотила <color=red>чума</color>. Как нулевой пациент вы распространяете <color=red>Чуму</color> при поедании других существ",15);
            });
        }
    }
}
