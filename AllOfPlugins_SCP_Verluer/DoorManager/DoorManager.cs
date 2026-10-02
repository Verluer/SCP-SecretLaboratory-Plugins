using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Handlers;
using MEC;

namespace AllOfPlugins_SCP_Verluer.DoorManager
{
    public class DoorManager
    {
        public static readonly string[] DoorRoundStart =
    {
                "SURFACE_GATE",
            };
        public static CoroutineHandle _doorSurfaceTracker;
        public static void Enable()
        {
            ScpDoorBlocker.Enable();
            ZombieDoorController.Enable();
            WarheadDoorController.Enable();
            if (false)
            {
                DoorIdLogger.Enable();
            }
            ServerEvents.RoundStarted += OnRoundStarted;
        }
        public static void Disable()
        {
            ScpDoorBlocker.Disable();
            ZombieDoorController.Disable();
            WarheadDoorController.Disable();
            if (false)
            {
                DoorIdLogger.Disable();
            }
            ServerEvents.RoundStarted -= OnRoundStarted;
        }
        private static void OnRoundStarted()
        {
            DoorManagerStartRound(true);
        }
        private static void DoorManagerStartRound(bool customAccess)
        {
            LabApi.Features.Console.Logger.Info("=== ROUND STARTED ===");

            foreach (DoorVariant door in DoorVariant.AllDoors)
            {
                if (door == null)
                    continue;

                if (door.DoorName != "SURFACE_GATE")
                    continue;

                LabApi.Features.Console.Logger.Info(
                    $"SURFACE_GATE RequiredPermissions BEFORE = {door.RequiredPermissions.RequiredPermissions}"
                );

                door.RequiredPermissions = new DoorPermissionsPolicy(
                    DoorPermissionFlags.AlphaWarhead
                );

                LabApi.Features.Console.Logger.Info(
                    $"SURFACE_GATE RequiredPermissions AFTER = {door.RequiredPermissions.RequiredPermissions}"
                );
            }
            if (DoorManager._doorSurfaceTracker.IsRunning)
                Timing.KillCoroutines(DoorManager._doorSurfaceTracker);

            DoorManager._doorSurfaceTracker = Timing.RunCoroutine(WarheadDoorController.TrackDoors(DoorManager.DoorRoundStart));
        }
       

    }
}