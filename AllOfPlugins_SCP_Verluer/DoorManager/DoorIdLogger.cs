using Interactables.Interobjects.DoorButtons;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using System;
using System.Reflection;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.DoorManager
{
    public static class DoorIdLogger
    {
        public static void Enable()
        {
            PlayerEvents.InteractingDoor += OnInteractingDoor;

            LabApi.Features.Console.Logger.Info(
                "[DoorIdLogger] Enabled."
            );
        }

        public static void Disable()
        {
            PlayerEvents.InteractingDoor -= OnInteractingDoor;

            LabApi.Features.Console.Logger.Info(
                "[DoorIdLogger] Disabled."
            );
        }

        private static void OnInteractingDoor(
            PlayerInteractingDoorEventArgs ev)
        {
            if (ev.Door == null)
                return;

            DoorVariant door = ev.Door.Base;

            if (door == null)
                return;

            LabApi.Features.Console.Logger.Info(
                "========================================"
            );

            LabApi.Features.Console.Logger.Info(
                "[DoorIdLogger] INTERACTING DOOR"
            );

            LabApi.Features.Console.Logger.Info(
                "---------- LABAPI WRAPPER ----------"
            );

            LabApi.Features.Console.Logger.Info(
                $"Wrapper Type: {ev.Door.GetType().FullName}"
            );

            LabApi.Features.Console.Logger.Info(
                $"Wrapper Name: {ev.Door.NameTag}"
            );

            LabApi.Features.Console.Logger.Info(
                "---------- DOOR VARIANT ----------"
            );

            LabApi.Features.Console.Logger.Info(
                $"Type: {door.GetType().FullName}"
            );

            LabApi.Features.Console.Logger.Info(
                $"GameObject: {door.gameObject.name}"
            );

            LabApi.Features.Console.Logger.Info(
                $"DoorName: {door.DoorName}"
            );

            LabApi.Features.Console.Logger.Info(
                $"TargetState: {door.TargetState}"
            );

            LabApi.Features.Console.Logger.Info(
                $"IsMoving: {door.IsMoving}"
            );

            LabApi.Features.Console.Logger.Info(
                $"ActiveLocks: {door.ActiveLocks}"
            );

            LabApi.Features.Console.Logger.Info(
                $"RequiredPermissions: {door.RequiredPermissions.RequiredPermissions}"
            );

            LabApi.Features.Console.Logger.Info(
                $"RequireAll: {door.RequiredPermissions.RequireAll}"
            );

            LabApi.Features.Console.Logger.Info(
                $"Position: {door.transform.position}"
            );

            LabApi.Features.Console.Logger.Info(
                $"Rotation: {door.transform.rotation.eulerAngles}"
            );

            LabApi.Features.Console.Logger.Info(
                $"Hierarchy: {GetHierarchy(door.transform)}"
            );

            LabApi.Features.Console.Logger.Info(
                "========================================"
            );
        }

        private static string GetHierarchy(Transform transform)
        {
            if (transform == null)
                return "NULL";

            string result = transform.name;

            Transform current = transform.parent;

            while (current != null)
            {
                result = current.name + " <- " + result;
                current = current.parent;
            }

            return result;
        }
       
    }
}
/*
[2026-09-19 19:26:45.528 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=33 | Name=LCZ_ARMORY | Action=Opened | Type=BreakableDoor | TargetState=True | Position=(31.44, 100.00, 104.99)
[2026-09-19 19:28:38.844 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=41 | Name=914 | Action=Opened | Type=PryableDoor | TargetState=True | Position=(105.00, 100.00, 117.23)
[2026-09-19 19:29:51.079 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=42 | Name=330 | Action=Opened | Type=BreakableDoor | TargetState=True | Position=(132.74, 99.96, 61.99)
[2026-09-19 19:31:18.586 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=63 | Name=HCZ_ARMORY | Action=Opened | Type=BreakableDoor | TargetState=True | Position=(90.00, -100.01, 119.75)
[2026-09-19 19:32:18.484 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=81 | Name=106_PRIMARY | Action=Opened | Type=CheckpointDoor | TargetState=True | Position=(26.58, -100.00, 134.98)
[2026-09-19 19:33:08.476 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=55 | Name=CHECKPOINT_EZ_HCZ_A | Action=Opened | Type=CheckpointDoor | TargetState=True | Position=(120.35, -100.00, 150.19)
[2026-09-19 19:39:06.710 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=97 | Name=INTERCOM | Action=Opened | Type=BreakableDoor | TargetState=True | Position=(197.26, -100.02, 135.02)

[2026-09-23 03:18:22.268 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=105 | Name=SURFACE_GATE | Action=Opened | Type=PryableDoor | TargetState=True | Position=(37.60, 290.71, -42.82)

[2026-09-19 19:38:44.942 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=90 | Name=GATE_A | Action=Opened | Type=PryableDoor | TargetState=True | Position=(209.03, -100.03, 120.07)
[2026-09-19 19:40:30.370 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=95 | Name=GATE_B | Action=Opened | Type=PryableDoor | TargetState=True | Position=(173.72, -100.00, 115.13)
[2026-09-19 19:41:37.979 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=76 | Name=HID_CHAMBER | Action=Opened | Type=BreakableDoor | TargetState=True | Position=(88.66, -95.51, 59.28)
*/