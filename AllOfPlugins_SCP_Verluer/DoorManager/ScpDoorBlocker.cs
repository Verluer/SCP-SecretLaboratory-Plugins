using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.DoorManager
{
    public class ScpDoorBlocker
    {
        private static readonly string[] BlockedSCP_Door_Name =
        new string[]
        {
            "CHECKPOINT_EZ_HCZ_A",
        };

        private static bool IsDecontaminationActive = false;

        private const float MaxDistance = 5f;

        private static readonly Dictionary<DoorVariant, Action>
            DoorStateHandlers =
            new Dictionary<DoorVariant, Action>();

        public static void Enable()
        {
            PlayerEvents.InteractingDoor += OnInteractingDoor;

            RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;

            ServerEvents.LczDecontaminationStarted +=
                OnLczDecontaminationStarted;

            foreach (DoorVariant door in DoorVariant.AllDoors)
            {
                if (door == null)
                    continue;

                if (!IsBlockedSCPDoor(door))
                    continue;

                DoorVariant capturedDoor = door;

                Action handler = () =>
                    OnBlockedDoorStateChanged(capturedDoor);

                DoorStateHandlers[capturedDoor] = handler;

                capturedDoor.OnStateChanged += handler;
            }

            LabApi.Features.Console.Logger.Info(
                "[ScpDoorBlocker] Enabled."
            );
        }

        public static void Disable()
        {
            PlayerEvents.InteractingDoor -= OnInteractingDoor;

            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;

            ServerEvents.LczDecontaminationStarted -=
                OnLczDecontaminationStarted;

            foreach (KeyValuePair<DoorVariant, Action> pair
                in DoorStateHandlers)
            {
                if (pair.Key == null)
                    continue;

                pair.Key.OnStateChanged -= pair.Value;
            }

            DoorStateHandlers.Clear();

            IsDecontaminationActive = false;

            LabApi.Features.Console.Logger.Info(
                "[ScpDoorBlocker] Disabled."
            );
        }

        private static void OnPlayersSpawned()
        {
            IsDecontaminationActive = false;
        }

        private static void OnLczDecontaminationStarted()
        {
            IsDecontaminationActive = true;

            LabApi.Features.Console.Logger.Info(
                "[ScpDoorBlocker] LCZ Decontamination started."
            );
        }

        private static void OnInteractingDoor(
            PlayerInteractingDoorEventArgs ev)
        {
            if (ev.Player == null || ev.Door == null)
                return;

            if (IsDecontaminationActive)
                return;

            if (ev.Player.Team != Team.SCPs)
                return;

            if (CustomModule.Role.HumanSCP.IsHumanScp(ev.Player))
                return;

            DoorVariant door = ev.Door.Base;

            if (door == null)
                return;

            if (!IsBlockedSCPDoor(door))
                return;

            float distance = Vector3.Distance(
                ev.Player.Position,
                door.transform.position
            );

            if (distance > MaxDistance)
                return;

            if (door.TargetState)
                return;

            if (ev.Player.Role == RoleTypeId.Scp0492)
            {
                ev.IsAllowed = false;
                return;
            }
            ev.Player.SendBroadcast(
                "Повышенные меры безопасности. " +
                "Двери заблокированы до обеззараживания Light Zone",
                5
            );

            ev.IsAllowed = false;

            LabApi.Features.Console.Logger.Info(
                $"[ScpDoorBlocker] " +
                $"BLOCKED: SCP {ev.Player.Nickname} tried to open " +
                $"{door.DoorName} | Distance={distance:F2}m"
            );
        }

        private static void OnBlockedDoorStateChanged(
            DoorVariant door)
        {
            if (door == null)
                return;

            if (!door.TargetState)
                return;

            foreach (Player player in Player.List)
            {
                if (player == null)
                    continue;

                if (player.Role == RoleTypeId.Scp0492)
                    continue;

                if (player.Team != Team.SCPs)
                    continue;

                float distance = Vector3.Distance(
                    player.Position,
                    door.transform.position
                );

                if (distance > MaxDistance)
                    continue;

                door.NetworkTargetState = false;

                LabApi.Features.Console.Logger.Info(
                    $"[ScpDoorBlocker] " +
                    $"FORCE CLOSED: {door.DoorName} | " +
                    $"SCP={player.Nickname} | " +
                    $"Distance={distance:F2}m"
                );

                return;
            }
        }

        private static bool IsBlockedSCPDoor(
            DoorVariant door)
        {
            if (door == null)
                return false;

            return IsDoorInArray(
                BlockedSCP_Door_Name,
                door.DoorName
            );
        }

        private static bool IsDoorInArray(
            string[] doors,
            string doorName)
        {
            if (doors == null || doorName == null)
                return false;

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == doorName)
                    return true;
            }

            return false;
        }
    }
}