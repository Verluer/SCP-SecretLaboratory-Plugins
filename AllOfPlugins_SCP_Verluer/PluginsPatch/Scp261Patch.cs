using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using Scp261;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AllOfPlugins_SCP_Verluer.PluginsPatch
{
    public static class Scp261Patch
    {
        private const double CandyChance = 40.0;

        private static Type _eventHandlerType;

        public static void Enable(Harmony harmony)
        {
            try
            {
                _eventHandlerType = AccessTools.TypeByName("Scp261.EventHandler");

                if (_eventHandlerType == null)
                {
                    LabApi.Features.Console.Logger.Error("Scp261Patch: Scp261.EventHandler не найден.");
                    return;
                }

                MethodInfo interacted = AccessTools.Method(_eventHandlerType, "Interacted");

                if (interacted == null)
                {
                    LabApi.Features.Console.Logger.Error("Scp261Patch: метод Interacted не найден.");
                    return;
                }

                harmony.Patch(interacted, transpiler: new HarmonyMethod(typeof(Scp261Patch), nameof(Transpiler)));

                LabApi.Features.Console.Logger.Info($"Scp261Patch: Candy добавлена в SCP-261 с весом {CandyChance}.");
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error($"Scp261Patch: ошибка Enable:\n{ex}");
            }
        }

        public static void Disable()
        {
            _eventHandlerType = null;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            MethodInfo addCandy = AccessTools.Method(typeof(Scp261Patch), nameof(AddCandy));
            MethodInfo getPlayer = AccessTools.Method(typeof(Scp261Patch), nameof(GetPlayer));

            if (addCandy == null || getPlayer == null)
            {
                LabApi.Features.Console.Logger.Error("Scp261Patch: не удалось найти методы AddCandy/GetPlayer.");
                return codes;
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Newobj)
                    continue;

                if (!(codes[i].operand is ConstructorInfo constructor))
                    continue;

                if (constructor.DeclaringType != typeof(WeightedChanceExecutor))
                    continue;

                codes.Insert(i++, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(i++, new CodeInstruction(OpCodes.Call, getPlayer));
                codes.Insert(i, new CodeInstruction(OpCodes.Call, addCandy));

                break;
            }

            return codes;
        }

        private static Player GetPlayer(PlayerInteractedToyEventArgs ev)
        {
            return Player.Get(ev.Player.ReferenceHub);
        }

        private static WeightedChanceParam[] AddCandy(WeightedChanceParam[] parameters, Player player)
        {
            if (parameters == null)
                return null;

            WeightedChanceParam[] result = new WeightedChanceParam[parameters.Length + 1];

            Array.Copy(parameters, result, parameters.Length);

            result[parameters.Length] = new WeightedChanceParam(
                () => CandyExpansion.CandyExpansion.GiveRandomCandy(player),
                CandyChance);

            return result;
        }
    }
}