using AdminToys;
using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.Core
{
    public static class PlayerSchematicManager
    {
        private static readonly Dictionary<Player, SchematicObject> SpawnedSchematics = new();
        private static readonly Dictionary<Player, CoroutineHandle> FollowCoroutines = new();
        private static readonly Dictionary<Player, CoroutineHandle> ParentDeathCoroutines = new();
        private static readonly Dictionary<Player, SchematicObject> CorpseSchematics = new();

        private static readonly System.Reflection.MethodInfo AddObserverMethod =
            typeof(NetworkIdentity).GetMethod(
                "AddObserver",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

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
                Transform playerTransform = playerObject != null
                    ? playerObject.transform
                    : null;

                if (playerTransform == null)
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"[PlayerSchematicManager] " +
                        $"У игрока {player.Nickname} отсутствует Transform.");

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
                        rotation);

                if (schematic == null)
                {
                    LabApi.Features.Console.Logger.Warn(
                        $"[PlayerSchematicManager] " +
                        $"Не удалось создать schematic '{schematicName}' " +
                        $"для {player.Nickname}.");

                    return false;
                }

                // Максимально частая синхронизация AdminToy.
                foreach (AdminToyBase adminToyBase in schematic.AdminToyBases)
                {
                    adminToyBase.syncInterval = 0f;
                }

                SpawnedSchematics[player] = schematic;

                if (useParent)
                {
                    schematic.transform.SetParent(
                        playerTransform,
                        true);
                }
                else
                {
                    CoroutineHandle coroutine =
                        Timing.RunCoroutine(
                            FollowPlayer(
                                player,
                                schematic,
                                positionOffset,
                                rotationOffsetQuaternion));

                    FollowCoroutines[player] = coroutine;
                }

                LabApi.Features.Console.Logger.Info(
                    $"[PlayerSchematicManager] " +
                    $"Schematic '{schematicName}' прикреплён к " +
                    $"{player.Nickname}.");

                return true;
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[PlayerSchematicManager] " +
                    $"Ошибка при создании schematic '{schematicName}' " +
                    $"для {player.Nickname}:\n{ex}");

                return false;
            }
        }

        /// <summary>
        /// Превращает schematic игрока в самостоятельный объект трупа.
        ///
        /// Вызывается в момент SpawningRagdoll.
        /// После этого schematic больше не связан с Player
        /// и не будет уничтожен через Remove(player).
        /// </summary>
        public static void DetachAsCorpse(Player player)
        {
            if (player == null)
                return;

            try
            {
                if (!SpawnedSchematics.TryGetValue(
                        player,
                        out SchematicObject schematic) ||
                    schematic == null)
                {
                    return;
                }

                StopFollowing(player);

                if (ParentDeathCoroutines.TryGetValue(
                        player,
                        out CoroutineHandle deathCoroutine))
                {
                    Timing.KillCoroutines(deathCoroutine);
                    ParentDeathCoroutines.Remove(player);
                }

                // Открепляем модель от игрока,
                // сохраняя её мировую позицию.
                schematic.transform.SetParent(null, true);

                // Убираем из обычных schematic игрока.
                SpawnedSchematics.Remove(player);

                // Но сохраняем связь:
                // Player -> его corpse schematic.
                CorpseSchematics[player] = schematic;

                ResetScale(player);

                LabApi.Features.Console.Logger.Info(
                    $"[PlayerSchematicManager] " +
                    $"Schematic игрока {player.Nickname} " +
                    $"отсоединён и оставлен как corpse.");
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[PlayerSchematicManager] " +
                    $"Ошибка DetachAsCorpse для {player.Nickname}:\n{ex}");
            }
        }

        private static IEnumerator<float> FollowPlayer(
            Player player,
            SchematicObject schematic,
            Vector3 positionOffset,
            Quaternion rotationOffset)
        {
            while (
                player != null &&
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
                    Vector3 worldOffset =
                        playerTransform.TransformDirection(
                            positionOffset);

                    schematic.Position =
                        player.Position + worldOffset;

                    schematic.Rotation =
                        playerTransform.rotation *
                        rotationOffset;
                }

                yield return Timing.WaitForOneFrame;
            }

            if (player != null)
                FollowCoroutines.Remove(player);
        }

        public static void EnableFade(Player player)
        {
            if (player == null)
                return;

            player.EnableEffect<Fade>(
                byte.MaxValue,
                0f,
                false);
        }

        public static void DisableFade(Player player)
        {
            if (player == null)
                return;

            player.DisableEffect<Fade>();
        }

        /// <summary>
        /// Полностью удаляет schematic игрока.
        /// </summary>
        public static void Remove(Player player)
        {
            if (player == null)
                return;

            try
            {
                StopFollowing(player);

                if (ParentDeathCoroutines.TryGetValue(
                        player,
                        out CoroutineHandle deathCoroutine))
                {
                    Timing.KillCoroutines(deathCoroutine);
                    ParentDeathCoroutines.Remove(player);
                }

                if (SpawnedSchematics.TryGetValue(
                        player,
                        out SchematicObject schematic))
                {
                    if (schematic != null)
                        schematic.Destroy();

                    SpawnedSchematics.Remove(player);
                }

                DisableFade(player);
                ResetScale(player);
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[PlayerSchematicManager] " +
                    $"Ошибка удаления schematic игрока " +
                    $"{player.Nickname}:\n{ex}");
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

        /// <summary>
        /// Полностью очищает все активные schematic'и
        /// и все оставшиеся corpse schematic'и.
        /// </summary>
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

                    if (ParentDeathCoroutines.TryGetValue(
                            pair.Key,
                            out CoroutineHandle deathCoroutine))
                    {
                        Timing.KillCoroutines(deathCoroutine);
                    }

                    if (pair.Value != null)
                        pair.Value.Destroy();

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
                        $"Ошибка очистки player schematic:\n{ex}");
                }
            }

            foreach (KeyValuePair<Player, SchematicObject> pair
                     in CorpseSchematics)
            {
                try
                {
                    if (pair.Value != null)
                        pair.Value.Destroy();
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error(
                        $"[PlayerSchematicManager] " +
                        $"Ошибка очистки corpse schematic:\n{ex}");
                }
            }

            FollowCoroutines.Clear();
            ParentDeathCoroutines.Clear();
            SpawnedSchematics.Clear();
            CorpseSchematics.Clear();
        }

        public static void SetScale(
            Player player,
            Vector3 scale)
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

        /// <summary>
        /// Скрывает schematic конкретного игрока
        /// только для указанного клиента.
        /// </summary>
        public static void HideFor(Player player)
        {
            if (player == null)
                return;

            if (!SpawnedSchematics.TryGetValue(
                    player,
                    out SchematicObject schematic) ||
                schematic == null)
            {
                return;
            }

            NetworkConnectionToClient connection =
                player.ReferenceHub.connectionToClient;

            if (connection == null)
                return;

            foreach (NetworkIdentity identity
                     in schematic.NetworkIdentities)
            {
                if (identity == null)
                    continue;

                if (!identity.observers.Remove(
                        connection.connectionId))
                {
                    continue;
                }

                connection.Send(
                    new ObjectDestroyMessage
                    {
                        netId = identity.netId
                    });
            }
        }

        public static void ShowFor(Player player)
        {
            if (player == null)
                return;

            SchematicObject schematic = null;

            if (SpawnedSchematics.TryGetValue(
                    player,
                    out SchematicObject activeSchematic))
            {
                schematic = activeSchematic;
            }
            else if (CorpseSchematics.TryGetValue(
                         player,
                         out SchematicObject corpseSchematic))
            {
                schematic = corpseSchematic;
            }

            if (schematic == null)
                return;

            NetworkConnectionToClient connection =
                player.ReferenceHub.connectionToClient;

            if (connection == null)
                return;

            if (AddObserverMethod == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[PlayerSchematicManager] " +
                    "Не найден internal NetworkIdentity.AddObserver().");

                return;
            }

            foreach (NetworkIdentity identity
                     in schematic.NetworkIdentities)
            {
                if (identity == null)
                    continue;

                if (identity.observers.ContainsKey(
                        connection.connectionId))
                {
                    continue;
                }

                try
                {
                    AddObserverMethod.Invoke(
                        identity,
                        new object[] { connection });
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error(
                        $"[PlayerSchematicManager] " +
                        $"Ошибка AddObserver для netId={identity.netId} " +
                        $"игрока {player.Nickname}:\n{ex}");
                }
            }
        }
    }
}