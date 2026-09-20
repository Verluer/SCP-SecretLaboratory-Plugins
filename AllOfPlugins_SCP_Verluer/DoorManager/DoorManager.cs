using AllOfPlugins_SCP_Verluer.DoorMnager;
using CentralAuth;
using HarmonyLib;
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
    public class DoorManager
    {
        public static void Enable()
        {
            ScpDoorBlocker.Enable();
            ZombieDoorController.Enable();
            if (false)
            {
                DoorIdLogger.Enable();
            }
        }
        public static void Disable()
        {
            ScpDoorBlocker.Disable();
            ZombieDoorController.Disable();
            if (false)
            {
                DoorIdLogger.Disable();
            }
        }
    }
}