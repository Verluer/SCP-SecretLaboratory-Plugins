using AllOfPlugins_SCP_Verluer.CustomModule.Role;
using AllOfPlugins_SCP_Verluer.CustomModule;
using HarmonyLib;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllOfPlugins_SCP_Verluer.CustomModule
{
    public class CustomModule
    {

        public static void Enable(Harmony harmony)
        {
            HumanSCP.Enable();
            ResearchSupervisor.Enable();
            ContainmentEngineer.Enable();
            FacilitySuperintendent.Enable();
            SCP049_2_Alpha.Enable(harmony);
            Test.Enable();
        }


        public static void Disable()
        {
            HumanSCP.Disable();
            ResearchSupervisor.Disable();
            ContainmentEngineer.Disable();
            FacilitySuperintendent.Disable();
            SCP049_2_Alpha.Disable();
            Test.Disable();
        }
     
    }
}
