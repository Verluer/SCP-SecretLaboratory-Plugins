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
        public static void Enable(Harmony harmony)
        {



            RoleAssigner.OnPlayersSpawned += () =>
            {
                int roll = UnityEngine.Random.Range(0, 100);
                if (roll < 15)
                {
                    ActionEvent.isEventActive = true;
                    OnRolesInitialized();
                }
            };

            ServerEvents.RoundEnded += (ev) =>
            {
                ActionEvent.isEventActive = false;
            };



        }


        public static void Disable()
        {
            RoleAssigner.OnPlayersSpawned -= OnRolesInitialized;

            ServerEvents.RoundEnded += (ev) =>
            {
                ActionEvent.isEventActive = false;
            };

        }
        private static void OnRolesInitialized()
        {
            ZombieApocalypse.Enable();

        }
    }
}
