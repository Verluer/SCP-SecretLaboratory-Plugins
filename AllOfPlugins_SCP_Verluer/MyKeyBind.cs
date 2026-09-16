using AllOfPlugins_SCP_Verluer.GamePatch;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;

public class MyKeybind : CustomEventsHandler
{
    private static readonly int[] KeybindId = { 1, 2, 3 };
    private static readonly HashSet<ReferenceHub> PressedKeys =
    new HashSet<ReferenceHub>();

    public override void OnServerWaitingForPlayers()
    {
        ServerSpecificSettingsSync.DefinedSettings = new ServerSpecificSettingBase[]
        {
            new SSGroupHeader("Мои настройки"),

            new SSKeybindSetting(
                KeybindId[0],
                "Кнопка обмена SCP-ролями",
                KeyCode.Y,
                hint: "Нажмите Y"
            ),
            new SSKeybindSetting(
                KeybindId[1],
                "Говорить как обычный человек",
                KeyCode.V,
                hint: "Удерживайте V"
            )

        };

        ServerSpecificSettingsSync.SendToAll();

        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }
    public void Dispose()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -=
            OnSettingValueReceived;
    }
    private void OnSettingValueReceived(
        ReferenceHub sender,
        ServerSpecificSettingBase setting)
    {
        if (!(setting is SSKeybindSetting keybind))
            return;

        if (setting.SettingId == KeybindId[0])
        {
            if (keybind.SyncIsPressed)
            {
                SwapScpRole.MethodSwapRole(sender);
            }

            return;
        }

        if (setting.SettingId == KeybindId[1])
        {
            ScpProximityVoice.HandleKey(
                sender,
                keybind.SyncIsPressed);

            return;
        }
    }
}
