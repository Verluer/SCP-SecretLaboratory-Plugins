using Exiled.API.Features.Roles;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using PlayerStatsSystem;
using Respawning.Objectives;
using System;
using System.Collections.Generic;
using System.Reflection;

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
        private static readonly RoleTypeId[] ScpRoles =
        {
                RoleTypeId.Scp0492,
                RoleTypeId.Scp3114
            };
        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(CandyBlack), "ServerApplyEffects");

            MethodInfo methodZombie = AccessTools.Method(typeof(ZombieConsumeAbility), "ServerComplete");

            if (method == null) return;
            if (methodZombie == null) return;

            _harmony.Patch(methodZombie, prefix: new HarmonyMethod(typeof(BlackCandyExpansion), nameof(ServerCompletePrefix)));


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

            int roll = UnityEngine.Random.Range(0, 6);
            switch (roll)
            {
                case 0:
                    hub.roleManager.ServerSetRole(ScpRoles[UnityEngine.Random.Range(0, ScpRoles.Length)], RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
                    HealthStat health = hub.playerStats.GetModule<HealthStat>();

                    if (hub.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Scp0492)
                    {
                        if (hub.roleManager.CurrentRole is IHumeShieldedRole shieldedRole)
                        {
                            var shield = shieldedRole.HumeShieldModule;

                            if (shield != null)
                                shield.HsCurrent = 1000f;
                        }
                        health.MaxValue = 2000f;
                        health.CurValue = 1500f;
                        Timing.CallDelayed(15f, () =>
                        {
                            Player.Get(hub).SendHint(
                                $"Вы заразились <color=red>чумой. Как нулевой пациент вы распространяете <color=red>Чуму</color> при поедании других существ",
                                10f
                            );
                        });
                    }
                    if (hub.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Scp3114)
                    {
                        if (hub.roleManager.CurrentRole is IHumeShieldedRole shieldedRole)
                        {
                            var shield = shieldedRole.HumeShieldModule;

                            if (shield != null)
                                shield.HsCurrent = 750f;
                        }


                        health.MaxValue = 750f;
                        health.CurValue = 759f;
                    }
                    foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
                    {
                        Player.Get(allHub).SendHint($"Обнаружен новый <color=red> SCP-объект</color> в комплексе", 5f);
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
                    ExplosionOutcome explosionOutcome = new ExplosionOutcome();
                    explosionOutcome.ServerGrant(hub);
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
            yield return Timing.WaitForSeconds(15f);

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
        private static readonly PropertyInfo CurRagdollProperty =
    typeof(ZombieConsumeAbility)
        .BaseType
        .GetProperty(
            "CurRagdoll",
            BindingFlags.Instance |
            BindingFlags.NonPublic);

        private static bool ServerCompletePrefix(ZombieConsumeAbility __instance)
        {
            ReferenceHub zombie = __instance.Owner;
            BasicRagdoll corpse = CurRagdollProperty.GetValue(__instance) as BasicRagdoll;

            if (corpse == null)
                return true;

            ReferenceHub victim = corpse.Info.OwnerHub;

            if (victim == null)
                return true;


            HealthStat health = zombie.playerStats.GetModule<HealthStat>();
            if (health.MaxValue == 2000)
            {
                if (victim.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Spectator)
                {
                    victim.roleManager.ServerSetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
                }
            }

            return true;
        }

    }
}
