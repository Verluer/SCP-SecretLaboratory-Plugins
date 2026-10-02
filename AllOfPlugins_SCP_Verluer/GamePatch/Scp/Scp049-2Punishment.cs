using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.GamePatch.Scp
{
    public static class Scp049_2Punishment
    {
        private static float _lookingAtTargetDuration = 10f;

        private static readonly Dictionary<uint, bool> ZombieStates =
            new Dictionary<uint, bool>();

        private static readonly Dictionary<uint, float> TrueUntil =
            new Dictionary<uint, float>();

        // SCP-049-2 NetworkId -> SCP-049 NetworkId
        private static readonly Dictionary<uint, uint> _zombieToDoctor =
            new Dictionary<uint, uint>();

        // NetworkId SCP-049-2
        private static readonly HashSet<uint> _superZombies =
            new HashSet<uint>();


        public static void Enable(Harmony harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(ZombieBloodlustAbility), "RefreshChaseState");

            if (method == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[Scp049_2Punishment] Could not find ZombieBloodlustAbility.RefreshChaseState.");
                return;
            }

            harmony.Patch(method, prefix: new HarmonyMethod(typeof(Scp049_2Punishment),nameof(RefreshChaseStatePrefix)));

            Scp049Events.ResurrectedBody += OnResurrectedBody;

            PlayerEvents.Death += OnDeath;

            LabApi.Features.Console.Logger.Info(
                "[Scp049_2Punishment] Enabled.");
        }


        public static void Disable()
        {
            Scp049Events.ResurrectedBody -= OnResurrectedBody;
            PlayerEvents.Death -= OnDeath;

            ZombieStates.Clear();
            TrueUntil.Clear();
            _zombieToDoctor.Clear();
            _superZombies.Clear();

            LabApi.Features.Console.Logger.Info(
                "[Scp049_2Punishment] Disabled.");
        }


        private static bool RefreshChaseStatePrefix(
            ZombieBloodlustAbility __instance)
        {
            if (__instance == null)
                return true;

            if (__instance.Role == null)
                return true;

            ReferenceHub hub;

            if (!__instance.Role.TryGetOwner(out hub))
                return true;

            Player player = Player.Get(hub);

            if (player == null)
                return true;

            uint networkId = player.NetworkId;

            bool actualState = __instance.LookingAtTarget;

            float currentTime = Time.time;

            if (actualState)
            {
                TrueUntil[networkId] =
                    currentTime + _lookingAtTargetDuration;
            }

            bool newState = actualState;

            if (!actualState)
            {
                float until;

                if (TrueUntil.TryGetValue(
                    networkId,
                    out until))
                {
                    if (currentTime < until)
                    {
                        newState = true;
                    }
                    else
                    {
                        TrueUntil.Remove(networkId);
                        newState = false;
                    }
                }
            }

            bool oldState;

            if (!ZombieStates.TryGetValue(
                networkId,
                out oldState))
            {
                ZombieStates[networkId] = newState;

                LabApi.Features.Console.Logger.Info(
                    $"[Scp049_2Punishment] Zombie {networkId}: initial state = {newState}");

                return true;
            }

            if (oldState == newState)
                return true;

            ZombieStates[networkId] = newState;

            LabApi.Features.Console.Logger.Info(
                $"[Scp049_2Punishment] Zombie {networkId}: {oldState} -> {newState}");

            return true;
        }


        private static void OnResurrectedBody(
            Scp049ResurrectedBodyEventArgs ev)
        {
            if (ev.Target == null || ev.Player == null)
                return;

            uint zombieId = ev.Target.NetworkId;
            uint doctorId = ev.Player.NetworkId;

            _zombieToDoctor[zombieId] = doctorId;

            bool isSuperZombie = false;

            Scp049SenseAbility sense;

            if (TryGetSense(ev.Player, out sense))
            {
                isSuperZombie =
                    sense.SpecialZombies.Contains(
                        ev.Target.ReferenceHub);
            }

            if (isSuperZombie)
            {
                _superZombies.Add(zombieId);

                LabApi.Features.Console.Logger.Info(
                    $"[Scp049_2Punishment] Zombie {zombieId} is SUPER. " +
                    $"Doctor: {doctorId}");
            }
            else
            {
                _superZombies.Remove(zombieId);

                LabApi.Features.Console.Logger.Info(
                    $"[Scp049_2Punishment] Zombie {zombieId} is NORMAL. " +
                    $"Doctor: {doctorId}");
            }
        }


        private static void OnDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player == null)
                return;

            if (ev.OldRole != RoleTypeId.Scp0492)
                return;

            uint zombieId = ev.Player.NetworkId;

            bool lookingAtTarget;

            if (!ZombieStates.TryGetValue(
                zombieId,
                out lookingAtTarget))
            {
                LabApi.Features.Console.Logger.Info(
                    $"[Scp049_2Punishment] Zombie {zombieId}: " +
                    "state not found. Punishment skipped.");

                Remove(zombieId);
                return;
            }

            // Если зомби смотрел на игрока — наказания нет.
            if (lookingAtTarget)
            {
                LabApi.Features.Console.Logger.Info(
                    $"[Scp049_2Punishment] Zombie {zombieId}: " +
                    "was looking at target. Punishment skipped.");

                Remove(zombieId);
                return;
            }

            // Если зомби НЕ смотрел на игрока,
            // проверяем, что его НЕ убил другой игрок.
            if (ev.Attacker == null || ev.Attacker == ev.Player)
            {
                LabApi.Features.Console.Logger.Info(
                    $"[Scp049_2Punishment] Zombie {zombieId}: " +
                    "was not looking at target and was not killed by another player. " +
                    "Punishment triggered.");

                ev.Player.SetRole(RoleTypeId.Overwatch);

                Remove(zombieId);
                return;
            }

            // Зомби не смотрел на игрока, но его убил другой игрок.
            LabApi.Features.Console.Logger.Info(
                $"[Scp049_2Punishment] Zombie {zombieId}: " +
                "was not looking at target but was killed by another player. " +
                "Punishment skipped.");

            Remove(zombieId);
        }


        private static bool TryGetSense(
            Player doctor,
            out Scp049SenseAbility sense)
        {
            sense = null;

            if (doctor == null)
                return false;

            Scp049Role scp049Role =
                doctor.RoleBase as Scp049Role;

            if (scp049Role == null)
                return false;

            return scp049Role.SubroutineModule
                .TryGetSubroutine<Scp049SenseAbility>(
                    out sense);
        }


        public static bool TryGetSavedState(
            uint networkId,
            out bool lookingAtTarget)
        {
            return ZombieStates.TryGetValue(
                networkId,
                out lookingAtTarget);
        }


        public static bool IsSuperZombie(
            uint networkId)
        {
            return _superZombies.Contains(networkId);
        }


        public static bool TryGetDoctor(
            uint zombieNetworkId,
            out uint doctorNetworkId)
        {
            return _zombieToDoctor.TryGetValue(
                zombieNetworkId,
                out doctorNetworkId);
        }


        public static void Remove(uint networkId)
        {
            ZombieStates.Remove(networkId);
            TrueUntil.Remove(networkId);
            _zombieToDoctor.Remove(networkId);
            _superZombies.Remove(networkId);
        }
    }
}