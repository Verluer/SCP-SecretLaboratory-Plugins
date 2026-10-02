using AllOfPlugins_SCP_Verluer.CustomModule.Role;
using CentralAuth;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using Utils;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{

    public class BlackCandyExpansion
    {
        private static readonly HashSet<ReferenceHub> _punishmentBlockedPlayers =
        new HashSet<ReferenceHub>();
        private static readonly Dictionary<ReferenceHub, CoroutineHandle> _punishmentCoroutines =
        new Dictionary<ReferenceHub, CoroutineHandle>();
        private static readonly List<Action<ReferenceHub>> HlEffects =
        new List<Action<ReferenceHub>>
        {
                    WhiteCandyExpansion.ApplyWhiteCandyEffect,
                    BrownCandyExpansion.ApplyBrownCandyEffect,
                    GrayCandyExpansion.ApplyGrayCandyEffect,
                    OrangeCandyExpansion.ApplyOrangeCandyEffect,

        };
        private static string[] ScpRoles =
        {
            "Scp0492Alpha",
            "Scp3114",
            "HumanScp",
        };
        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(CandyBlack), "ServerApplyEffects");


            if (method == null) return;


            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(BlackCandyExpansion), nameof(ServerApplyEffectsPrefix)));

            PlayerEvents.InteractingScp330 += OnInteractingScp330;
        }

        public static void Disable()
        {

            PlayerEvents.InteractingScp330 -= OnInteractingScp330;

            foreach (CoroutineHandle coroutine in _punishmentCoroutines.Values)
            {
                if (coroutine.IsRunning)
                    Timing.KillCoroutines(coroutine);
            }

            _punishmentCoroutines.Clear();
            _punishmentBlockedPlayers.Clear();
        }

        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {

            int roll = UnityEngine.Random.Range(0, 7);
            switch (roll)
            {
                case 0:
                    string playerScpRole = ScpRoles[UnityEngine.Random.Range(0, ScpRoles.Length)];

                    HealthStat health = hub.playerStats.GetModule<HealthStat>();


                    if (playerScpRole == "Scp0492Alpha")
                    {
                        SCP049_2_Alpha.SpawnScp0492Alpha(hub);
                    }
                    if (playerScpRole == "Scp3114")
                    {
                        hub.roleManager.ServerSetRole(RoleTypeId.Scp3114, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
                        if (hub.roleManager.CurrentRole is IHumeShieldedRole shieldedRole)
                        {
                            var shield = shieldedRole.HumeShieldModule;

                            if (shield != null)
                                shield.HsCurrent = 750f;
                        }


                        health.MaxValue = 750f;
                        health.CurValue = 750f;
                    }
                    if (playerScpRole == "HumanScp")
                    {
                        HumanSCP.SpawnHumanScpFromExistingScp(hub);
                    }
                    foreach (ReferenceHub _hub in ReferenceHub.AllHubs)
                    {
                        if (_hub == null || _hub.Mode == ClientInstanceMode.DedicatedServer)
                            continue;

                        Player player = Player.Get(_hub);

                        if (player == null)
                            continue;

                        Timing.CallDelayed(5f, () =>
                        {
                            player.SendBroadcast("Обнаружен новый <color=red>SCP-объект</color> в комплексе", 10);
                        });

                    }

                    return false;
                case 1:
                    _punishmentBlockedPlayers.Add(hub);
                    if (_punishmentCoroutines.TryGetValue(hub, out CoroutineHandle oldCoroutine))
                    {
                        if (oldCoroutine.IsRunning)
                            Timing.KillCoroutines(oldCoroutine);
                    }

                    CoroutineHandle newCoroutine = Timing.RunCoroutine(UnAllowPunishment(hub));
                    _punishmentCoroutines[hub] = newCoroutine;

                    Player.Get(hub).SendHint($"Вы получили <color=green>благосклонность</color> <color=red>Scp330</color>. " +
                        $"В течении следующих 5-ти секунд сколько бы <color=green>конфет</color> вы бы не взяли наказания <color=blue>не последует</color>", 5f);

                    return false;
                case 2:
                    TeleportOutcome outcome = new TeleportOutcome();
                    outcome.ServerGrant(hub);
                    return false;
                case 3:
                    DuplicationOutcome duplicationOutcome = new DuplicationOutcome();
                    duplicationOutcome.ServerGrant(hub);
                    Player.Get(hub).SendHint($"Чёрных конфет стало больше!", 5f);
                    return false;
                case 4:
                    int rollboom = UnityEngine.Random.Range(0, 100);
                    if (rollboom < 50)
                    {
                        ExplosionUtils.ServerExplode(hub, ExplosionType.PinkCandy);
                    }
                    else
                    {
                        Player.Get(hub).SendHint($"Поздравляем! Вы избежали фееричной смерти! <color=blue>Идёт перерасчёт вероятностей...</color>", 5f);
                        Timing.CallDelayed(5f, () =>
                        {
                            ServerApplyEffectsPrefix(hub);
                        });
                        return false;
                    }
                    return false;
                case 5:
                    HlEffects[UnityEngine.Random.Range(0, HlEffects.Count)](hub);
                    Player.Get(hub).SendHint($"Вы получили случайный особый конфетный эффект", 5f);
                    return false;
                case 6:
                    int countCoin = UnityEngine.Random.Range(1, 11);
                    LabApi.Features.Console.Logger.Info(countCoin);
                    for (int i = 0; i < countCoin; i++)
                    {
                        LabApi.Features.Wrappers.Pickup.Create(ItemType.Coin, hub.transform.position);
                    }
                    Player.Get(hub).SendHint($"Вы <color=green>везунчик</color>! Посмотрите под ноги, аномалия подарила вам <color=yellow>деньги</color>!", 5f);
                    return false;
                default:
                    return true;

            }
        }
        private static IEnumerator<float> UnAllowPunishment(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(5f);

            _punishmentBlockedPlayers.Remove(hub);
            _punishmentCoroutines.Remove(hub);
        }
        private static void OnInteractingScp330(PlayerInteractingScp330EventArgs ev)
        {
            if (_punishmentBlockedPlayers.Contains(ev.Player.ReferenceHub))
            {
                ev.AllowPunishment = false;
            }
        }
        

    }
}
