using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using CustomPlayerEffects;
using Scp261;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AllOfPlugins_SCP_Verluer
{
    public static class Scp261Patch
    {
        private const double CandyChance = 40.0;

        private static Type _eventHandlerType;
        private static MethodInfo _giveRandomItemMethod;

        private static PropertyInfo _singletonProperty;
        private static PropertyInfo _configProperty;

        public static void Enable(Harmony harmony)
        {
            try
            {
                _eventHandlerType =
                    AccessTools.TypeByName("Scp261.EventHandler");

                if (_eventHandlerType == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: Scp261.EventHandler не найден.");

                    return;
                }


                MethodInfo interacted =
                    AccessTools.Method(
                        _eventHandlerType,
                        "Interacted");

                if (interacted == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: метод Interacted не найден.");

                    return;
                }

                _giveRandomItemMethod =
                    AccessTools.Method(
                        _eventHandlerType,
                        "GiveRandomItem");

                if (_giveRandomItemMethod == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: метод GiveRandomItem не найден.");

                    return;
                }


                Type scp261Type =
                    AccessTools.TypeByName("Scp261.Scp261");

                if (scp261Type == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: тип Scp261.Scp261 не найден.");

                    return;
                }

                _singletonProperty =
                    AccessTools.Property(
                        scp261Type,
                        "Singleton");

                if (_singletonProperty == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: свойство Scp261.Singleton не найдено.");

                    return;
                }

                object singleton =
                    _singletonProperty.GetValue(null);

                if (singleton != null)
                {
                    _configProperty =
                        AccessTools.Property(
                            singleton.GetType(),
                            "Config");
                }

                if (_configProperty == null)
                {
                    LabApi.Features.Console.Logger.Warn(
                        "Scp261Patch: свойство Config пока не найдено. " +
                        "Оно будет получаться через reflection при необходимости.");
                }

                // ---------------------------------------------------------
                // Harmony Transpiler
                // ---------------------------------------------------------

                harmony.Patch(
                    interacted,
                    transpiler: new HarmonyMethod(
                        typeof(Scp261Patch),
                        nameof(Transpiler)));

                LabApi.Features.Console.Logger.Info(
                    $"Scp261Patch: Candy добавлена в SCP-261 " +
                    $"с весом {CandyChance}.");

            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"Scp261Patch: ошибка Enable:\n{ex}");
            }
        }

        public static void Disable()
        {

            _eventHandlerType = null;
            _giveRandomItemMethod = null;
            _singletonProperty = null;
            _configProperty = null;
        }
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new List<CodeInstruction>(instructions);

            MethodInfo addCandy =
                AccessTools.Method(
                    typeof(Scp261Patch),
                    nameof(AddCandy));

            MethodInfo getPlayer =
                AccessTools.Method(
                    typeof(Scp261Patch),
                    nameof(GetPlayer));

            if (addCandy == null || getPlayer == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "Scp261Patch: не удалось найти методы AddCandy/GetPlayer.");

                return codes;
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Newobj ||
                    !(codes[i].operand is ConstructorInfo constructor) ||
                    constructor.DeclaringType != typeof(WeightedChanceExecutor))
                {
                    continue;
                }

                codes.Insert(
                    i++,
                    new CodeInstruction(
                        OpCodes.Ldarg_0));

                codes.Insert(
                    i++,
                    new CodeInstruction(
                        OpCodes.Call,
                        getPlayer));


                codes.Insert(
                    i,
                    new CodeInstruction(
                        OpCodes.Call,
                        addCandy));

                break;
            }

            return codes;
        }


        private static Player GetPlayer(
            PlayerInteractedToyEventArgs ev)
        {
            return Player.Get(
                ev.Player.ReferenceHub);
        }

        private static WeightedChanceParam[] AddCandy(
            WeightedChanceParam[] parameters,
            Player player)
        {
            if (parameters == null)
            {
                return parameters;
            }


            if (parameters.Length > 1)
            {
                double effectChance =
                    GetConfigDouble(
                        "EffectChance");

                parameters[1] =
                    new WeightedChanceParam(
                        () => GiveRandomEffect(player),
                        effectChance);
            }

            WeightedChanceParam[] result =
                new WeightedChanceParam[
                    parameters.Length + 1];

            Array.Copy(
                parameters,
                result,
                parameters.Length);

            result[parameters.Length] =
                new WeightedChanceParam(
                    () => CandyExpansion.CandyExpansion.GiveRandomCandy(player),
                    CandyChance);

            return result;
        }
        private static void GiveRandomEffect(Player player)
        {
            try
            {
                object config =
                    GetScp261Config();

                if (config == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: не удалось получить Config.");

                    return;
                }

                bool enableEffects =
                    GetConfigBool(
                        config,
                        "EnableEffects");

                if (!enableEffects)
                {
                    InvokeOriginalGiveRandomItem(player);
                    return;
                }
                IList effectList =
                    GetConfigValue<IList>(
                        config,
                        "EffectList");

                if (effectList == null ||
                    effectList.Count == 0)
                {
                    LabApi.Features.Console.Logger.Warn(
                        "Scp261Patch: EffectList пуст.");

                    return;
                }
                int index =
                    UnityEngine.Random.Range(
                        0,
                        effectList.Count);

                string effectName =
                    effectList[index] as string;

                if (string.IsNullOrEmpty(effectName))
                {
                    LabApi.Features.Console.Logger.Warn(
                        "Scp261Patch: выбранное имя эффекта пустое.");

                    return;
                }

                if (player.TryGetEffect(
                    effectName,
                    out StatusEffectBase effect))
                {
                    float minDuration =
                        GetConfigFloat(
                            config,
                            "MinEffectDuration");

                    float maxDuration =
                        GetConfigFloat(
                            config,
                            "MaxEffectDuration");

                    float duration =
                        UnityEngine.Random.Range(
                            minDuration,
                            maxDuration);

                    player.EnableEffect(
                        effect,
                        1,
                        duration);

                    string successMessage =
                        GetConfigString(
                            config,
                            "InteractionSuccessfulEffects");

                    player.SendHint(
                        successMessage,
                        5f);

                    LabApi.Features.Console.Logger.Info(
                        $"Scp261Patch: игроку {player.Nickname} " +
                        $"выдан эффект '{effectName}' " +
                        $"на {duration:F1} сек.");
                }
                else
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"Scp261Patch: не удалось найти эффект " +
                        $"'{effectName}' для игрока {player.Nickname}.");
                }
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"Scp261Patch: ошибка при выдаче эффекта:\n{ex}");
            }
        }

        // ================================================================
        // ВЫЗОВ ОРИГИНАЛЬНОГО GiveRandomItem
        // ================================================================

        private static void InvokeOriginalGiveRandomItem(
            Player player)
        {
            try
            {
                if (_giveRandomItemMethod == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        "Scp261Patch: GiveRandomItem не найден.");

                    return;
                }

                _giveRandomItemMethod.Invoke(
                    null,
                    new object[]
                    {
                        player
                    });
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"Scp261Patch: ошибка вызова оригинального " +
                    $"GiveRandomItem:\n{ex}");
            }
        }

        // ================================================================
        // ПОЛУЧЕНИЕ CONFIG
        // ================================================================

        private static object GetScp261Config()
        {
            try
            {
                if (_singletonProperty == null)
                {
                    Type scp261Type =
                        AccessTools.TypeByName(
                            "Scp261.Scp261");

                    if (scp261Type == null)
                    {
                        return null;
                    }

                    _singletonProperty =
                        AccessTools.Property(
                            scp261Type,
                            "Singleton");
                }

                object singleton =
                    _singletonProperty?.GetValue(null);

                if (singleton == null)
                {
                    return null;
                }

                if (_configProperty == null ||
                    _configProperty.DeclaringType != singleton.GetType())
                {
                    _configProperty =
                        AccessTools.Property(
                            singleton.GetType(),
                            "Config");
                }

                if (_configProperty == null)
                {
                    return null;
                }

                return _configProperty.GetValue(
                    singleton);
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"Scp261Patch: ошибка получения Config:\n{ex}");

                return null;
            }
        }

        // ================================================================
        // ПОЛУЧЕНИЕ ЗНАЧЕНИЙ CONFIG
        // ================================================================

        private static T GetConfigValue<T>(
            object config,
            string propertyName)
        {
            if (config == null)
            {
                return default;
            }

            try
            {
                PropertyInfo property =
                    AccessTools.Property(
                        config.GetType(),
                        propertyName);

                if (property == null)
                {
                    LabApi.Features.Console.Logger.Error(
                        $"Scp261Patch: свойство Config.{propertyName} " +
                        $"не найдено.");

                    return default;
                }

                object value =
                    property.GetValue(config);

                if (value == null)
                {
                    return default;
                }

                if (value is T typedValue)
                {
                    return typedValue;
                }

                return (T)Convert.ChangeType(
                    value,
                    typeof(T));
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"Scp261Patch: ошибка получения " +
                    $"Config.{propertyName}:\n{ex}");

                return default;
            }
        }

        private static bool GetConfigBool(
            object config,
            string propertyName)
        {
            object value =
                GetConfigValue<object>(
                    config,
                    propertyName);

            if (value == null)
            {
                return false;
            }

            return Convert.ToBoolean(value);
        }

        private static double GetConfigDouble(
            string propertyName)
        {
            object config =
                GetScp261Config();

            if (config == null)
            {
                return 0.0;
            }

            object value =
                GetConfigValue<object>(
                    config,
                    propertyName);

            if (value == null)
            {
                return 0.0;
            }

            return Convert.ToDouble(value);
        }

        private static float GetConfigFloat(
            object config,
            string propertyName)
        {
            object value =
                GetConfigValue<object>(
                    config,
                    propertyName);

            if (value == null)
            {
                return 0f;
            }

            return Convert.ToSingle(value);
        }

        private static string GetConfigString(
            object config,
            string propertyName)
        {
            return GetConfigValue<string>(
                config,
                propertyName);
        }
    }
}