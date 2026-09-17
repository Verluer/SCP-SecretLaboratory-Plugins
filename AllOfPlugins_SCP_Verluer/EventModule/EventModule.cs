using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles.RoleAssign;
using System.Reflection;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.EventModule
{
    public class EventModule
    {
        public static bool isZombieEventActive = false;
        public static void Enable(Harmony harmony)
        {



            RoleAssigner.OnPlayersSpawned += () =>
            {
                int rollZombieEvent = UnityEngine.Random.Range(0, 100);
                if (rollZombieEvent < 15)
                {
                    isZombieEventActive = true;
                    ZombieApocalypse.Enable();
                }
                int rollSCP261Event = UnityEngine.Random.Range(0, 100);
                if (rollSCP261Event < 100)
                {
                    Scp261Event.Enable();
                }
            };

            ServerEvents.RoundEnded += (ev) =>
            {
                isZombieEventActive = false;
            };



        }


        public static void Disable()
        {
            RoleAssigner.OnPlayersSpawned -= () =>
            {
                ZombieApocalypse.Disable();
            };

                ServerEvents.RoundEnded += (ev) =>
            {
                isZombieEventActive = false;
            };

        }

    }
}
