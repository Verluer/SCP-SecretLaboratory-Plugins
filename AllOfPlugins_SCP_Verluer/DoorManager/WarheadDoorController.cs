using CentralAuth;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using Respawning;
using System;
using System.Collections.Generic;

namespace AllOfPlugins_SCP_Verluer.DoorManager
{
    public static class WarheadDoorController
    {
        private static bool WarheadFirstActive = false;
        private static readonly string[] AlphaProtocolDoors =
            {
                "CHECKPOINT_EZ_HCZ_A",
            };
        private static readonly string[] DeathEndProtocolDoors =
            {
                "GATE_A",
                "GATE_B"
            };
        private static CoroutineHandle _doorTracker;

        public static void Enable()
        {
            WarheadEvents.Started += OnWarheadStarted;
            WarheadEvents.Stopped += OnWarheadStopped;
            ServerEvents.RoundEnded += OnRoundEnded;

            Logger.Info("[WarheadDoorController] Enabled.");
        }

        public static void Disable()
        {
            WarheadEvents.Started -= OnWarheadStarted;
            WarheadEvents.Stopped -= OnWarheadStopped;
            ServerEvents.RoundEnded -= OnRoundEnded;
            if (_doorTracker.IsRunning)
                Timing.KillCoroutines(_doorTracker);

            WarheadFirstActive = false;

            Logger.Info("[WarheadDoorController] Disabled.");
        }

        private static void OnWarheadStarted(WarheadStartedEventArgs ev)
        {
            if (ev.IsAutomatic)
            {
                return;
            }

            if (AlphaWarheadController.Singleton.Info.ScenarioType == WarheadScenarioType.DeadmanSwitch)
            {
                string selectedDoor = DeathEnd();

                if (selectedDoor == null)
                {
                    return;
                }

                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                        continue;


                    Player player = Player.Get(hub);

                    if (player == null)
                        continue;

                    player.SendBroadcast($"Начался процесс активации Мёртвой руки. В целях безопапности {selectedDoor} заблокирован", 5);
                }

                if (DoorManager._doorSurfaceTracker.IsRunning)
                {
                    Timing.KillCoroutines(DoorManager._doorSurfaceTracker);
                    DoorManager._doorSurfaceTracker = default;
                }
                return;
            }
            if (WarheadFirstActive == false)
            {
                Timing.CallDelayed(10f, () =>
                {
                    SetAlphaProtocolDoorsLock(true);

                    if (_doorTracker.IsRunning)
                        Timing.KillCoroutines(_doorTracker);

                    _doorTracker = Timing.RunCoroutine(TrackDoors(AlphaProtocolDoors));

                    WarheadFirstActive = true;
                });
                if (DoorManager._doorSurfaceTracker.IsRunning)
                {
                    Timing.KillCoroutines(DoorManager._doorSurfaceTracker);
                    DoorManager._doorSurfaceTracker = default;
                }
                return;
            }
            else if (WarheadFirstActive)
            {
                Timing.CallDelayed(5f, () =>
                {
                    SetAlphaProtocolDoorsLock(true);

                    if (_doorTracker.IsRunning)
                        Timing.KillCoroutines(_doorTracker);

                    _doorTracker = Timing.RunCoroutine(TrackDoors(AlphaProtocolDoors));
                });

                if (DoorManager._doorSurfaceTracker.IsRunning)
                {
                    Timing.KillCoroutines(DoorManager._doorSurfaceTracker);
                    DoorManager._doorSurfaceTracker = default;
                }
                return;
            }
        }
        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (AlphaWarheadController.Singleton.Info.ScenarioType == WarheadScenarioType.DeadmanSwitch)
                return;

            if (DoorManager._doorSurfaceTracker.IsRunning)
                Timing.KillCoroutines(DoorManager._doorSurfaceTracker);

            DoorManager._doorSurfaceTracker = Timing.RunCoroutine(WarheadDoorController.TrackDoors(DoorManager.DoorRoundStart));

            if (_doorTracker.IsRunning)
            {
                Timing.KillCoroutines(_doorTracker);
                _doorTracker = default;
            }
            SetAlphaProtocolDoorsLock(false);
        }
        private static void OnWarheadStopped(WarheadStoppedEventArgs ev)
        {
            if (AlphaWarheadController.Singleton.Info.ScenarioType == WarheadScenarioType.DeadmanSwitch)
                return;

            if (DoorManager._doorSurfaceTracker.IsRunning)
                Timing.KillCoroutines(DoorManager._doorSurfaceTracker);

            DoorManager._doorSurfaceTracker = Timing.RunCoroutine(WarheadDoorController.TrackDoors(DoorManager.DoorRoundStart));

            if (_doorTracker.IsRunning)
            {
                Timing.KillCoroutines(_doorTracker);
                _doorTracker = default;
            }
            SetAlphaProtocolDoorsLock(false);
        }
        private static string DeathEnd()
        {
            if (DeathEndProtocolDoors.Length == 0)
            {
                Logger.Warn(
                    "[WarheadDoorController] DeathEndProtocolDoors пуст.");

                return null;
            }

            int index = new Random().Next(DeathEndProtocolDoors.Length);
            string selectedDoorName = DeathEndProtocolDoors[index];

            Logger.Info(
                $"[WarheadDoorController] DeathEnd выбрал дверь: {selectedDoorName}");

            foreach (DoorVariant door in DoorVariant.AllDoors)
            {
                if (door == null)
                    continue;

                if (door.DoorName != selectedDoorName)
                    continue;


                Logger.Info(
                    $"[WarheadDoorController] DeathEnd закрывает дверь: {door.DoorName}");

                door.NetworkTargetState = false;

                door.ServerChangeLock(
                    DoorLockReason.SpecialDoorFeature,
                    true);

                Logger.Info(
                    $"[WarheadDoorController] DeathEnd заблокировал дверь: {door.DoorName}");

                break;
            }

            return selectedDoorName;
        }
        private static void SetAlphaProtocolDoorsLock(bool locked)
        {
            foreach (DoorVariant door in DoorVariant.AllDoors)
            {
                if (door == null)
                    continue;

                if (Array.IndexOf(AlphaProtocolDoors, door.DoorName) < 0)
                    continue;

                if (locked)
                {
                    door.NetworkTargetState = false;

                    door.ServerChangeLock(
                        DoorLockReason.SpecialDoorFeature,
                        true);
                }
                else
                {
                    door.ServerChangeLock(
                        DoorLockReason.SpecialDoorFeature,
                        false);
                }
            }
        }

        public static IEnumerator<float> TrackDoors(string[] list)
        {
            while (true)
            {
                foreach (DoorVariant door in DoorVariant.AllDoors)
                {
                    if (door == null)
                        continue;

                    if (Array.IndexOf(list, door.DoorName) < 0)
                        continue;

                    if (door.NetworkTargetState)
                    {
                        Logger.Warn(
                            $"[WarheadDoorController] Дверь {door.DoorName} была открыта. " +
                            "Принудительно закрываем.");

                        door.NetworkTargetState = false;
                    }
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }
    }
}