using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using Scp049Doctor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer
{
    public static class Scp049DoctorPatch
    {
        private static float _lookingAtTargetDuration = 10f;

        private static readonly Dictionary<uint, bool> ZombieStates =
            new Dictionary<uint, bool>();

        private static readonly Dictionary<uint, float> TrueUntil =
            new Dictionary<uint, float>();

        public static void Enable(Harmony harmony)
        {
            if (harmony == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "Scp049DoctorPatch: Harmony instance is null."
                );

                return;
            }

            try
            {
                harmony.CreateClassProcessor(
                    typeof(RefreshChaseStatePatch)
                ).Patch();

                harmony.CreateClassProcessor(
                    typeof(DoctorOnDeathPatch)
                ).Patch();

                LabApi.Features.Console.Logger.Info(
                    "Scp049DoctorPatch enabled."
                );
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    "Scp049DoctorPatch Harmony ERROR: " + ex
                );
            }
        }

        public static void Disable()
        {
            ZombieStates.Clear();
            TrueUntil.Clear();

            LabApi.Features.Console.Logger.Info(
                "Scp049DoctorPatch disabled."
            );
        }

        #region SCP-049-2 State

        public static bool TryGetSavedState(
            uint networkId,
            out bool lookingAtTarget)
        {
            return ZombieStates.TryGetValue(
                networkId,
                out lookingAtTarget
            );
        }

        public static void Remove(uint networkId)
        {
            ZombieStates.Remove(networkId);
            TrueUntil.Remove(networkId);
        }

        #endregion

        #region LookingAtTarget Tracking

        [HarmonyPatch(
            typeof(ZombieBloodlustAbility),
            "RefreshChaseState"
        )]
        private static class RefreshChaseStatePatch
        {
            private static void Postfix(
                ZombieBloodlustAbility __instance)
            {
                if (__instance == null)
                    return;

                if (__instance.Role == null)
                    return;

                ReferenceHub hub;

                if (!__instance.Role.TryGetOwner(
                    out hub))
                {
                    return;
                }

                Player player = Player.Get(hub);

                if (player == null)
                    return;

                uint networkId = player.NetworkId;

                bool actualState =
                    __instance.LookingAtTarget;

                float currentTime = Time.time;

                //Продление таргета по таймеру
                if (actualState)
                {
                    TrueUntil[networkId] =
                        currentTime + _lookingAtTargetDuration;
                }

                bool newState = actualState;
                //Проверка на доп. время для изменения таргета 
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
                        $"Zombie {networkId}: initial state = {newState}"
                    );

                    return;
                }

                if (oldState == newState)
                    return;

                ZombieStates[networkId] = newState;

                LabApi.Features.Console.Logger.Info(
                    $"Zombie {networkId}: {oldState} -> {newState}"
                );
            }
        }

        #endregion

        #region Scp049Doctor - OnDeath

        [HarmonyPatch(
            typeof(DoctorHandlers),
            "OnDeath"
        )]
        private static class DoctorOnDeathPatch
        {
            private static bool Prefix(
                DoctorHandlers __instance,
                PlayerDeathEventArgs ev)
            {
                if (ev == null ||
                    ev.Player == null ||
                    ev.OldRole != RoleTypeId.Scp0492)
                {
                    return true;
                }

                uint zombieId =
                    ev.Player.NetworkId;

                LabApi.Features.Console.Logger.Info(
                    $"Scp049Doctor: processing death of zombie {zombieId}."
                );

                FieldInfo zombieToDoctorField =
                    typeof(DoctorHandlers).GetField(
                        "_zombieToDoctor",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic
                    );

                FieldInfo superZombiesField =
                    typeof(DoctorHandlers).GetField(
                        "_superZombies",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic
                    );

                if (zombieToDoctorField == null ||
                    superZombiesField == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp049Doctor: internal fields not found."
                    );
                    return true;
                }

                var zombieToDoctor =
                    zombieToDoctorField.GetValue(__instance)
                    as Dictionary<uint, uint>;

                var superZombies =
                    superZombiesField.GetValue(__instance)
                    as HashSet<uint>;

                if (zombieToDoctor == null ||
                    superZombies == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp049Doctor: could not access internal collections."
                    );

                    return true;
                }
                uint ownerDoctorId;

                if (!zombieToDoctor.TryGetValue(
                    zombieId,
                    out ownerDoctorId))
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"Zombie {zombieId}: owner doctor NOT FOUND."
                    );
                    return true;
                }
                bool super =
                    superZombies.Remove(zombieId);

                zombieToDoctor.Remove(zombieId);

                bool lookingAtTarget;

                if (!TryGetSavedState(
                    zombieId,
                    out lookingAtTarget))
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"Zombie {zombieId}: " +
                        "LookingAtTarget state NOT FOUND. " +
                        "Reward ALLOWED."
                    );

                    ForceReward(
                        __instance,
                        ownerDoctorId,
                        super
                    );

                    Remove(zombieId);

                    return false;
                }

                if (lookingAtTarget)
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"Zombie {zombieId}: " +
                        "LookingAtTarget = TRUE -> " +
                        "REWARD BLOCKED."
                    );

                    Remove(zombieId);

                    return false;
                }

                LabApi.Features.Console.Logger.Warn(
                    $"Zombie {zombieId}: " +
                    "LookingAtTarget = FALSE -> " +
                    "CALLING REWARD."
                );

                ForceReward(
                    __instance,
                    ownerDoctorId,
                    super
                );

                Remove(zombieId);

                return false;
            }

            private static void ForceReward(
                DoctorHandlers handlers,
                uint ownerDoctorId,
                bool super)
            {
                if (handlers == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp049Doctor: DoctorHandlers instance is null."
                    );

                    return;
                }

                MethodInfo rewardDoctors =
                    typeof(DoctorHandlers).GetMethod(
                        "RewardDoctors",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic
                    );

                if (rewardDoctors == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp049Doctor: RewardDoctors method NOT FOUND."
                    );

                    return;
                }

                try
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"Scp049Doctor: " +
                        $"!!! CALLING RewardDoctors !!! " +
                        $"doctor={ownerDoctorId}, super={super}"
                    );

                    rewardDoctors.Invoke(
                        handlers,
                        new object[]
                        {
                            ownerDoctorId,
                            super
                        }
                    );

                    LabApi.Features.Console.Logger.Info(
                        $"Scp049Doctor: RewardDoctors completed " +
                        $"for doctor {ownerDoctorId}."
                    );
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp049Doctor: " +
                        "Failed to call RewardDoctors: " +
                        ex
                    );
                }
            }
        }

        #endregion
    }
}