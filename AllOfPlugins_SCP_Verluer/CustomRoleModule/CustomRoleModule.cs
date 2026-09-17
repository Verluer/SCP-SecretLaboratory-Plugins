using AllOfPlugins_SCP_Verluer.GamePatch;
using HarmonyLib;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllOfPlugins_SCP_Verluer.CustomRoleModule
{
    public class CustomRoleModule
    {
  
        public static void Enable(Harmony harmony)
        {
                HumanSCP.Enable();
        }


        public static void Disable()
        {
            HumanSCP.Disable();

        }
     
    }
}
