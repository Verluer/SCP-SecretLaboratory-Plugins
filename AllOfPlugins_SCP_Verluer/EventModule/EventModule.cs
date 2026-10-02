using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using PlayerRoles.RoleAssign;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.EventModule
{
    public static class EventModule
    {
        public static bool IsZombieEventActive { get; private set; }

        public static void Enable()
        {
            VanillaEventHandler.Enable();

            RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;
            ServerEvents.RoundEnded += OnRoundEnded;
        }

        public static void Disable()
        {
            ServerEvents.RoundEnded -= OnRoundEnded;
            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;

            VanillaEventHandler.Disable();

            ZombieApocalypse.Disable();
            Scp261Event.Disable();

            IsZombieEventActive = false;
        }

        private static void OnPlayersSpawned()
        {
            StartRandomEvents();
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            IsZombieEventActive = false;

            ZombieApocalypse.Disable();
            Scp261Event.Disable();
        }

        private static void StartRandomEvents()
        {
            if (UnityEngine.Random.Range(0, 100) < 15)
            {
                IsZombieEventActive = true;
                ZombieApocalypse.Enable();
            }

            if (UnityEngine.Random.Range(0, 100) < 5)
            {
                Scp261Event.Enable();
            }

            if (false)
            {
                Test.Enable();
            }
        }
    }
}