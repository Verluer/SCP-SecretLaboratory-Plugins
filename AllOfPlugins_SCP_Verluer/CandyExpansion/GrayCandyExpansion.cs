using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{
    public static class GrayCandyExpansion
    {
        private static readonly HashSet<ReferenceHub> ActivePlayers =
            new HashSet<ReferenceHub>();

        private static readonly Dictionary<ReferenceHub, CoroutineHandle> ActiveCoroutines =
            new Dictionary<ReferenceHub, CoroutineHandle>();

        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(
                typeof(HauntedCandyGray),
                "ServerApplyEffects"
            );

            if (method == null)
                return;

            _harmony.Patch(
                method,
                prefix: new HarmonyMethod(
                    typeof(GrayCandyExpansion),
                    nameof(ServerApplyEffectsPrefix)
                )
            );

            PlayerStats.OnAnyPlayerDamaged += OnAnyPlayerDamaged;
        }

        public static void Disable()
        {
            PlayerStats.OnAnyPlayerDamaged -= OnAnyPlayerDamaged;

            foreach (CoroutineHandle coroutine in ActiveCoroutines.Values)
            {
                if (coroutine.IsRunning)
                    Timing.KillCoroutines(coroutine);
            }

            ActiveCoroutines.Clear();
            ActivePlayers.Clear();
        }

        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            ApplyGrayCandyEffect(hub);

            return false;
        }

        public static void ApplyGrayCandyEffect(ReferenceHub hub)
        {
            if (hub == null)
                return;

            hub.playerEffectsController.ChangeState<DamageReduction>(
                160,
                10f
            );

            hub.playerEffectsController.ChangeState<Slowness>(
                50,
                10f
            );

            ActivePlayers.Add(hub);

            if (ActiveCoroutines.TryGetValue(hub, out CoroutineHandle oldCoroutine))
            {
                if (oldCoroutine.IsRunning)
                    Timing.KillCoroutines(oldCoroutine);
            }

            CoroutineHandle coroutine = Timing.RunCoroutine(
                RemoveGrayCandyEffect(hub)
            );

            ActiveCoroutines[hub] = coroutine;
        }

        private static IEnumerator<float> RemoveGrayCandyEffect(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(10f);

            ActivePlayers.Remove(hub);
            ActiveCoroutines.Remove(hub);
        }

        private static void OnAnyPlayerDamaged(
            ReferenceHub victim,
            DamageHandlerBase handler)
        {
            if (victim == null)
                return;

            if (!ActivePlayers.Contains(victim))
                return;

            UniversalDamageHandler universal =
                handler as UniversalDamageHandler;

            if (universal == null)
                return;

            if (universal.TranslationId != DeathTranslations.Falldown.Id)
                return;

            float damage = universal.Damage * 7f;

            LabApi.Features.Console.Logger.Info(
                "Gray Candy: " +
                victim.nicknameSync.MyNick +
                " получил урон от падения: " +
                universal.Damage +
                ", перенаправляется: " +
                damage
            );

            foreach (ReferenceHub target in ReferenceHub.AllHubs)
            {
                if (target == null)
                    continue;

                if (target == victim)
                    continue;

                float distance = Vector3.Distance(
                    victim.transform.position,
                    target.transform.position
                );

                if (distance > 5f)
                    continue;

                target.playerStats.DealDamage(
                    new GrayCandyDamageHandler(
                        victim,
                        damage
                    )
                );
            }
        }
    }
}