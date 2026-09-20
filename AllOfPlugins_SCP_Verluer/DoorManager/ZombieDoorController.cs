using CentralAuth;
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
    public class ZombieDoorController
    {
        private static readonly string[] Easy_Door_Name = new string[]
        {
            "LCZ_ARMORY",
            "330",
            "HCZ_ARMORY",
            "INTERCOM",
            "914",
            "049_ARMORY",
        };

        private static readonly string[] Medium_Door_Name = new string[]
        {
            "CHECKPOINT_EZ_HCZ_A",
            "106_SECONDARY",
            "106_PRIMARY",
        };

        private static readonly string[] Hard_Door_Name = new string[]
        {
            "GATE_A",
            "GATE_B",
            "HID_CHAMBER",
        };

        private static readonly List<ReferenceHub> PlayerNoScp =
            new List<ReferenceHub>();

        private const float MaxDistance = 5f;

        public static void Enable()
        {
            PlayerEvents.InteractingDoor += OnInteractingDoor;
            RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;

            LabApi.Features.Console.Logger.Info(
                "[ZombieDoorController] Enabled."
            );
        }

        public static void Disable()
        {
            PlayerEvents.InteractingDoor -= OnInteractingDoor;
            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;

            PlayerNoScp.Clear();

            LabApi.Features.Console.Logger.Info(
                "[ZombieDoorController] Disabled."
            );
        }

        private static void OnPlayersSpawned()
        {
            PlayerNoScp.Clear();

            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null)
                    continue;

                if (hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;

                if (hub.roleManager.CurrentRole.Team != Team.SCPs)
                {
                    PlayerNoScp.Add(hub);
                }
            }

            LabApi.Features.Console.Logger.Info(
                $"[ZombieDoorController] " +
                $"PlayerNoScp updated: {PlayerNoScp.Count}"
            );
        }

        private static void OnInteractingDoor(
            PlayerInteractingDoorEventArgs ev)
        {
            if (ev.Player == null || ev.Door == null)
                return;

            DoorVariant door = ev.Door.Base;

            if (door == null)
                return;

            if (ev.Player.Role != RoleTypeId.Scp0492)
                return;

            float distance = Vector3.Distance(
                ev.Player.Position,
                door.transform.position
            );

            if (distance > MaxDistance)
                return;

            if (!IsSpecialZombieDoor(door))
                return;

            if (door.TargetState)
                return;

            int zombieCount = GetNearbyZombieCount(door);

            if (!IsZombieSpecialDoor(door, zombieCount))
            {
                ev.IsAllowed = false;

                SendZombieDoorRequirementMessage(
                    ev.Player,
                    door,
                    zombieCount
                );

                LabApi.Features.Console.Logger.Info(
                    $"[ZombieDoorController] " +
                    $"SCP-049-2 {ev.Player.Nickname} " +
                    $"FAILED to open {door.DoorName} | " +
                    $"Zombies={zombieCount}/{PlayerNoScp.Count} | " +
                    $"Distance={distance:F2}m"
                );

                return;
            }

            ev.IsAllowed = false;

            door.NetworkTargetState = true;

            LabApi.Features.Console.Logger.Info(
                $"[ZombieDoorController] " +
                $"SCP-049-2 {ev.Player.Nickname} opened " +
                $"{door.DoorName} | " +
                $"Zombies={zombieCount}/{PlayerNoScp.Count} | " +
                $"Distance={distance:F2}m"
            );
        }

        private static int GetNearbyZombieCount(
            DoorVariant door)
        {
            if (door == null)
                return 0;

            int zombieCount = 0;

            foreach (Player player in Player.List)
            {
                if (player == null)
                    continue;

                if (player.Role != RoleTypeId.Scp0492)
                    continue;

                float distance = Vector3.Distance(
                    player.Position,
                    door.transform.position
                );

                if (distance <= MaxDistance)
                    zombieCount++;
            }

            return zombieCount;
        }

        private static bool IsSpecialZombieDoor(
            DoorVariant door)
        {
            if (door == null)
                return false;

            string doorName = door.DoorName;

            return
                IsDoorInArray(
                    Easy_Door_Name,
                    doorName
                )
                ||
                IsDoorInArray(
                    Medium_Door_Name,
                    doorName
                )
                ||
                IsDoorInArray(
                    Hard_Door_Name,
                    doorName
                );
        }

        private static bool IsZombieSpecialDoor(
            DoorVariant door,
            int zombieCount)
        {
            if (door == null)
                return false;

            string doorName = door.DoorName;

            int hardRequired =
                GetRequiredZombieCount(0.80f);

            if (zombieCount >= hardRequired)
            {
                if (IsDoorInArray(
                    Hard_Door_Name,
                    doorName))
                {
                    return true;
                }
            }

            int mediumRequired =
                GetRequiredZombieCount(0.60f);

            if (zombieCount >= mediumRequired)
            {
                if (IsDoorInArray(
                    Medium_Door_Name,
                    doorName))
                {
                    return true;
                }
            }

            int easyRequired =
                GetRequiredZombieCount(0.40f);

            if (zombieCount >= easyRequired)
            {
                if (IsDoorInArray(
                    Easy_Door_Name,
                    doorName))
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetRequiredZombieCount(
            float percentage)
        {
            if (PlayerNoScp.Count <= 0)
                return int.MaxValue;

            return (int)Math.Round(
                PlayerNoScp.Count * percentage,
                MidpointRounding.AwayFromZero
            );
        }

        private static void SendZombieDoorRequirementMessage(
            Player player,
            DoorVariant door,
            int zombieCount)
        {
            if (player == null || door == null)
                return;

            float percentage;
            string category;

            if (IsDoorInArray(
                Hard_Door_Name,
                door.DoorName))
            {
                percentage = 0.80f;
                category = "HARD";
            }
            else if (IsDoorInArray(
                Medium_Door_Name,
                door.DoorName))
            {
                percentage = 0.60f;
                category = "MEDIUM";
            }
            else if (IsDoorInArray(
                Easy_Door_Name,
                door.DoorName))
            {
                percentage = 0.40f;
                category = "EASY";
            }
            else
            {
                return;
            }

            int requiredZombieCount =
                GetRequiredZombieCount(percentage);

            int missingZombieCount =
                requiredZombieCount - zombieCount;

            if (missingZombieCount < 0)
                missingZombieCount = 0;

            player.SendBroadcast(
                $"<color=#FF4444>Дверь {category}</color>\n" +
                $"Для открытия необходимо зомби: " +
                $"<color=#FFAA00>{requiredZombieCount}</color>\n" +
                $"Сейчас рядом: " +
                $"<color=#FFAA00>{zombieCount}</color>\n" +
                $"Не хватает: " +
                $"<color=#FFAA00>{missingZombieCount}</color>",
                5
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