using AdminToys;
using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;
using MEC;

namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public static class PlayerSchematicManager
    {
        private static readonly Dictionary<Player, SchematicObject> SpawnedSchematics = new();
        private static readonly Dictionary<Player, CoroutineHandle> FollowCoroutines = new();

        public static bool Attach(
    Player player,
    string schematicName,
    Vector3 positionOffset = default,
    Vector3 rotationOffset = default,
    bool useParent = false)
        {
            if (player == null)
                return false;

            try
            {
                Remove(player);

                GameObject playerObject = player.GameObject;

                Transform playerTransform =
                    playerObject != null
                        ? playerObject.transform
                        : null;

                if (playerTransform == null)
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"[PlayerSchematicManager] " +
                        $"У игрока {player.Nickname} отсутствует Transform."
                    );

                    return false;
                }

                Quaternion rotationOffsetQuaternion =
                    Quaternion.Euler(rotationOffset);

                Vector3 position =
                    player.Position +
                    playerTransform.TransformDirection(positionOffset);

                Quaternion rotation =
                    playerTransform.rotation *
                    rotationOffsetQuaternion;

                SchematicObject schematic =
                    ObjectSpawner.SpawnSchematic(
                        schematicName,
                        position,
                        rotation
                    );

                if (schematic == null)
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"[PlayerSchematicManager] " +
                        $"Не удалось создать schematic '{schematicName}' " +
                        $"для {player.Nickname}."
                    );

                    return false;
                }

                foreach (AdminToyBase adminToyBase in schematic.AdminToyBases)
                {
                    adminToyBase.syncInterval = 0f;
                }

                SpawnedSchematics[player] = schematic;

                if (useParent)
                {
                    // Обычный Parent.
                    // Position, Rotation и Scale наследуются от игрока.
                    schematic.transform.SetParent(
                        playerTransform,
                        true
                    );
                }
                else
                {
                    // Настоящий Parent для максимально плавного
                    // Position / Rotation.
                    //
                    // Scale игрока при этом будет компенсироваться.
                    schematic.transform.SetParent(
                        playerTransform,
                        true
                    );

                    // Запоминаем мировой размер schematic
                    // ДО того, как Scale игрока будет изменён.
                    Vector3 originalWorldScale =
                        schematic.transform.lossyScale;

                    CoroutineHandle coroutine =
                        Timing.RunCoroutine(
                            FollowParentWithoutScale(
                                player,
                                schematic,
                                originalWorldScale
                            )
                        );

                    FollowCoroutines[player] = coroutine;
                }

                LabApi.Features.Console.Logger.Info(
                    $"[PlayerSchematicManager] " +
                    $"Schematic '{schematicName}' прикреплён к " +
                    $"{player.Nickname}."
                );

                return true;
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[PlayerSchematicManager] " +
                    $"Ошибка при создании schematic '{schematicName}' " +
                    $"для {player.Nickname}:\n{ex}"
                );

                return false;
            }
        }

        private static IEnumerator<float> FollowParentWithoutScale(
            Player player,
            SchematicObject schematic,
            Vector3 originalWorldScale)
        {
            while (player != null &&
                   schematic != null &&
                   HasSchematic(player))
            {
                if (!player.IsAlive)
                {
                    ResetScale(player);
                    FollowCoroutines.Remove(player);

                    yield break;
                }

                Transform playerTransform =
                    player.GameObject?.transform;

                if (playerTransform != null)
                {
                    Vector3 parentScale =
                        playerTransform.lossyScale;

                    Transform schematicTransform =
                        schematic.transform;

                    schematicTransform.localScale =
                        new Vector3(
                            SafeDivide(
                                originalWorldScale.x,
                                parentScale.x
                            ),
                            SafeDivide(
                                originalWorldScale.y,
                                parentScale.y
                            ),
                            SafeDivide(
                                originalWorldScale.z,
                                parentScale.z
                            )
                        );
                }

                yield return Timing.WaitForOneFrame;
            }

            if (player != null)
                FollowCoroutines.Remove(player);
        }

        private static float SafeDivide(
            float value,
            float divisor)
        {
            if (Mathf.Abs(divisor) < 0.0001f)
                return 0f;

            return value / divisor;
        }
        public static void EnableFade(Player player)
        {
            if (player == null)
                return;

            player.EnableEffect<Fade>(
                byte.MaxValue,
                0f,
                false
            );
        }

        public static void DisableFade(Player player)
        {
            if (player == null)
                return;

            player.DisableEffect<Fade>();
        }

        /// <summary>
        /// Полностью удаляет schematic и сбрасывает состояние игрока.
        /// Используется при смене роли, повторном Attach или очистке.
        /// </summary>
        public static void Remove(Player player)
        {
            if (player == null)
                return;

            try
            {
                StopFollowing(player);

                if (SpawnedSchematics.TryGetValue(
                        player,
                        out SchematicObject schematic))
                {
                    if (schematic != null)
                    {
                        schematic.Destroy();
                    }

                    SpawnedSchematics.Remove(player);
                }

                DisableFade(player);
                ResetScale(player);
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[PlayerSchematicManager] " +
                    $"Ошибка удаления schematic у " +
                    $"{player.Nickname}:\n{ex}"
                );

                FollowCoroutines.Remove(player);
                SpawnedSchematics.Remove(player);

                DisableFade(player);
                ResetScale(player);
            }
        }

        /// <summary>
        /// Останавливает следование schematic за игроком,
        /// но не удаляет сам schematic.
        /// </summary>
        public static void StopFollowing(Player player)
        {
            if (player == null)
                return;

            if (FollowCoroutines.TryGetValue(
                    player,
                    out CoroutineHandle coroutine))
            {
                Timing.KillCoroutines(coroutine);
                FollowCoroutines.Remove(player);
            }
        }

        public static bool HasSchematic(Player player)
        {
            return player != null &&
                   SpawnedSchematics.TryGetValue(
                       player,
                       out SchematicObject schematic) &&
                   schematic != null;
        }

        public static SchematicObject Get(Player player)
        {
            if (player == null)
                return null;

            SpawnedSchematics.TryGetValue(
                player,
                out SchematicObject schematic);

            return schematic;
        }

        public static void ClearAll()
        {
            foreach (KeyValuePair<Player, SchematicObject> pair
                     in SpawnedSchematics)
            {
                try
                {
                    if (FollowCoroutines.TryGetValue(
                            pair.Key,
                            out CoroutineHandle coroutine))
                    {
                        Timing.KillCoroutines(coroutine);
                    }

                    if (pair.Value != null)
                    {
                        pair.Value.Destroy();
                    }

                    if (pair.Key != null)
                    {
                        pair.Key.DisableEffect<Fade>();
                        pair.Key.Scale = Vector3.one;
                    }
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error(
                        $"[PlayerSchematicManager] " +
                        $"Ошибка очистки schematic:\n{ex}"
                    );
                }
            }

            FollowCoroutines.Clear();
            SpawnedSchematics.Clear();
        }

        public static void SetScale(Player player, Vector3 scale)
        {
            if (player == null)
                return;

            player.Scale = scale;
        }

        public static void ResetScale(Player player)
        {
            if (player == null)
                return;

            player.Scale = Vector3.one;
        }
    }
}