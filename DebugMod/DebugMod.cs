using System;
using System.Reflection;
using ExampleMod;
using JetBrains.Annotations;
using ModdingAPI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Logger = ModdingAPI.Logger;
using Object = UnityEngine.Object;

namespace MoonscarsDebugMod.DebugMod;

[UsedImplicitly]
internal class DebugMod : Mod {
    public override string GetName() => Assembly.GetExecutingAssembly().GetName().Name;

    public override string Version() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    private GameObject _debugmodGameObject = null!;
    private InputActionMap _keybindings = null!;
    private NoclipController _noclipController = null!;
    private HitboxRender _hitboxRender = null!;
    private DebugInfo _debugInfo = null!;


    public override void Load() {
        Logger.Log("Loaded DebugMod");

        _debugmodGameObject = new GameObject();
        Object.DontDestroyOnLoad(_debugmodGameObject);

        _noclipController = _debugmodGameObject.AddComponent<NoclipController>();
        _noclipController.enabled = false;
        _hitboxRender = _debugmodGameObject.AddComponent<HitboxRender>();
        _hitboxRender.enabled = false;
        _debugInfo = _debugmodGameObject.AddComponent<DebugInfo>();
        _debugInfo.enabled = true;

        _keybindings = GetKeybindings();

        _keybindings.Enable();
    }

    public override void Unload() {
        Logger.Log("Unloaded DebugMod");

        _keybindings.Dispose();
        Object.Destroy(_debugmodGameObject);
    }


    private void ToggleHitboxes() {
        _hitboxRender.enabled = !_hitboxRender.enabled;
        if (_hitboxRender.enabled) _hitboxRender.SearchHitboxes();
    }

    private void ExitToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    private void ToggleNoclip() {
        _noclipController.ToggleNoclip();
    }

    private void ToggleDebugInfo() {
        _debugInfo.enabled = !_debugInfo.enabled;
    }

    private InputActionMap GetKeybindings() {
        var map = new InputActionMap("DebugMod");
        AddButtonAction(map, "FastExit", "o", ExitToMainMenu);
        AddAltModifierButtonAction(map, "ToggleHitboxes", "b", ToggleHitboxes);
        AddAltModifierButtonAction(map, "ToggleNoclip", "period", ToggleNoclip);
        AddAltModifierButtonAction(map, "ToggleDebugInfo", "comma", ToggleDebugInfo);

        var noclipMovement = map.AddAction("NoclipMovement");
        noclipMovement.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        var noclipZoom = map.AddAction("NoclipZoom", binding: "<Mouse>/scroll");
        noclipZoom.performed += ctx => _noclipController.Zoom(ctx.ReadValue<Vector2>().y);
        var noclipNyoom = map.AddAction("NoclipNyoom", binding: "<Keyboard>/leftShift");

        _noclipController.Movement = noclipMovement;
        _noclipController.Nyoom = noclipNyoom;

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