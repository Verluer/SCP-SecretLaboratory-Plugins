using HarmonyLib;

using PlayerRoles.RoleAssign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public class CountSpawnSCP
    {
        public static void Enable(Harmony _harmony)
        {
            MethodInfo spawnScpsMethod = AccessTools.Method(typeof(ScpSpawner), nameof(ScpSpawner.SpawnScps));

            _harmony.Patch(spawnScpsMethod, prefix: new HarmonyMethod(typeof(CountSpawnSCP), nameof(SpawnScpsPrefix)));
        }
        public static void Disable()
        {

        }
        private static bool SpawnScpsPrefix(ref int targetScpNumber)
        {
            int players = ReferenceHub.AllHubs.Count(
                RoleAssigner.CheckPlayer);

            targetScpNumber = Mathf.Max(
                1,
                Mathf.FloorToInt(players / 5f));

            targetScpNumber = Mathf.Min(
                targetScpNumber,
                ScpSpawner.MaxSpawnableScps);

            return true;
        }
    }
}
