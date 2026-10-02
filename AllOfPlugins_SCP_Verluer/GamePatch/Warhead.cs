using AllOfPlugins_SCP_Verluer.Core;
using HarmonyLib;
using InventorySystem.Items;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles.RoleAssign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public class Warhead
    {
        private static bool _lockWarhead;
        public static void Enable(Harmony _harmony)
        {
            MethodInfo spawnScpsMethod = AccessTools.Method(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation),new[] { typeof(ReferenceHub) });

            _harmony.Patch(spawnScpsMethod, prefix: new HarmonyMethod(typeof(Warhead), nameof(CancelDetonationPrefix)));

            ServerEvents.GeneratorActivated += OnGeneratorActivated;
            ServerEvents.RoundEnded += OnRoundEnded;
        }
        public static void Disable()
        {
            ServerEvents.GeneratorActivated -= OnGeneratorActivated;
            ServerEvents.RoundEnded -= OnRoundEnded;
        }

        private static void OnGeneratorActivated(GeneratorActivatedEventArgs ev)
        {
            int activated = Generator.List.Count(x => x.Engaged);
            int total = Generator.List.Count();

            LabApi.Features.Console.Logger.Info($"Generators: {activated}/{total}");

            if (activated >= 3)
            {
                LabApi.Features.Console.Logger.Info("Все три генератора активированы!");

                _lockWarhead = true;

                LabApi.Features.Console.Logger.Info(_lockWarhead);
            }
        }
        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            _lockWarhead = false;
        }

        private static bool CancelDetonationPrefix(ReferenceHub disabler)
        {
            LabApi.Features.Console.Logger.Info($"CancelDetonation вызван! _lockWarhead = {_lockWarhead}");

            if (disabler == null)
                return true;

            var inventory = disabler.inventory;

            if (inventory == null)
                return true;

            var currentItem = inventory.CurInstance;

            if (currentItem == null)
            {
                return true;
            }


            if (_lockWarhead)
            {
                if (currentItem.ItemTypeId == ItemType.KeycardO5)
                {
                    LabApi.Features.Console.Logger.Info($"Отмена протокола: {currentItem.ItemTypeId}");
                    return true;
                }

                LabApi.Features.Console.Logger.Info(
                    "Отмена ядерки запрещена: все три генератора активированы.");

                return false;
            }

            return true;
        }
    }
}
