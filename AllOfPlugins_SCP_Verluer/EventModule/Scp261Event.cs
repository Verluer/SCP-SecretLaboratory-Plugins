using AllOfPlugins_SCP_Verluer.GamePatch;
using CentralAuth;
using CustomPlayerEffects;
using InventorySystem.Items.Usables.Scp330;
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
        private static CoroutineHandle _coroutine;
        private static ReferenceHub SCP261Player;
        public static void Enable()
        {
            LabApi.Features.Console.Logger.Info("Scp261Event START");
            EventScp261();
            _coroutine = Timing.RunCoroutine(CheckPlayers());
        }


        public static void Disable()
        {
            if (_coroutine.IsRunning)
                Timing.KillCoroutines(_coroutine);
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

            SCP261Player = EventPlayerList[UnityEngine.Random.Range(0, EventPlayerList.Count)];

            Player.Get(SCP261Player).SendBroadcast("Поздравляем! Вы выбраны великим Scp261 как его апостол. Привнесите в этот мир больше лудомании!.", 5);

            SCP261Player.roleManager.ServerSetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
            Core.PlayerSchematicManager.Attach(Player.Get(SCP261Player), "Scp261", new Vector3(0f, -0.65f, 0f), Vector3.zero, true);

            Timing.CallDelayed(0.5f, () =>
            {
                Core.PlayerSchematicManager.EnableFade(Player.Get(SCP261Player));
                Core.PlayerSchematicManager.HideFor(Player.Get(SCP261Player));
                Player.Get(SCP261Player).ClearInventory();

            });
            TeleportOutcome outcome = new TeleportOutcome();
            outcome.ServerGrant(SCP261Player);
        }
        private static IEnumerator<float> CheckPlayers()
        {
            while (true)
            {
                if (SCP261Player != null)
                {
                    Player player = Player.Get(SCP261Player);

                    if (player == null || !player.IsAlive)
                    {
                        SCP261Player.playerEffectsController
                            .DisableEffect<Slowness>();

                        SCP261Player = null;

                        yield break;
                    }

                    bool nearbyPlayer = false;

                    foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                    {
                        if (hub == null ||
                            hub == SCP261Player ||
                            hub.Mode == ClientInstanceMode.DedicatedServer)
                        {
                            continue;
                        }

                        if (hub.playerStats.GetModule<HealthStat>().CurValue <= 0)
                            continue;

                        float distance = Vector3.Distance(
                            SCP261Player.transform.position,
                            hub.transform.position);

                        if (distance <= 3f)
                        {
                            nearbyPlayer = true;
                            break;
                        }
                    }

                    if (nearbyPlayer)
                    {
                        SCP261Player.playerEffectsController
                            .ChangeState<Slowness>(100, 99999f);
                    }
                    else
                    {
                        SCP261Player.playerEffectsController
                            .DisableEffect<Slowness>();
                    }
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }
    }
}
