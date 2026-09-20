using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Console;

namespace AllOfPlugins_SCP_Verluer.DoorMnager
{
    public static class DoorIdLogger
    {
        public static void Enable()
        {
            DoorEvents.OnDoorAction += OnDoorAction;

            Logger.Info("[DoorIdLogger] Enabled.");
        }

        public static void Disable()
        {
            DoorEvents.OnDoorAction -= OnDoorAction;

            Logger.Info("[DoorIdLogger] Disabled.");
        }

        private static void OnDoorAction(
            DoorVariant door,
            DoorAction action,
            ReferenceHub user)
        {
            if (door == null)
                return;

            string name = "UNKNOWN";

            DoorNametagExtension nametag =
                door.GetComponent<DoorNametagExtension>();

            if (nametag != null)
                name = nametag.GetName;

            Logger.Info(
                $"[DoorIdLogger] " +
                $"DoorId={door.DoorId} | " +
                $"Name={name} | " +
                $"Action={action} | " +
                $"Type={door.GetType().Name} | " +
                $"TargetState={door.TargetState} | " +
                $"Position={door.transform.position}");
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

[2026-09-19 19:38:44.942 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=90 | Name=GATE_A | Action=Opened | Type=PryableDoor | TargetState=True | Position=(209.03, -100.03, 120.07)
[2026-09-19 19:40:30.370 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=95 | Name=GATE_B | Action=Opened | Type=PryableDoor | TargetState=True | Position=(173.72, -100.00, 115.13)
[2026-09-19 19:41:37.979 +03:00] [INFO] [AllOfPlugins_SCP_Verluer] [DoorIdLogger] DoorId=76 | Name=HID_CHAMBER | Action=Opened | Type=BreakableDoor | TargetState=True | Position=(88.66, -95.51, 59.28)
*/