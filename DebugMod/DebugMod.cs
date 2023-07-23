using System;
using System.Reflection;
using JetBrains.Annotations;
using ModdingAPI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MoonscarsDebugMod.DebugMod;

[UsedImplicitly]
internal class DebugMod : Mod {
    public override string GetName() => Assembly.GetExecutingAssembly().GetName().Name;

    public override string Version() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    private InputActionMap _keybindings = null!;

    public override void Load() {
        Logger.Log("Loaded DebugMod");

        _keybindings = GetKeybindings();
        _keybindings.Enable();
    }

    public override void Unload() {
        Logger.Log("Unloaded DebugMod");

        _keybindings.Dispose();
    }


    private void ToggleHitboxes() {
    }

    private void ExitToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    private InputActionMap GetKeybindings() {
        var map = new InputActionMap("DebugMod");
        AddButtonAction(map, "FastExit", "o", ExitToMainMenu);
        AddAltModifierButtonAction(map, "ToggleHitboxes", "b", ToggleHitboxes);

        return map;
    }

    private static void AddButtonAction(InputActionMap map, string name, string key, Action performed) {
        var action = map.AddAction(name, InputActionType.Button, $"<Keyboard>/{key}");
        action.performed += _ => performed();
    }

    private static void AddAltModifierButtonAction(InputActionMap map, string name, string key, Action performed) {
        var action = map.AddAction(name, InputActionType.Button);
        action.AddCompositeBinding("ButtonWithOneModifier")
            .With("Button", $"<Keyboard>/{key}")
            .With("Modifier", "<Keyboard>/leftAlt")
            .With("Modifier", "<Keyboard>/rightAlt");
        action.performed += _ => performed();
    }
}