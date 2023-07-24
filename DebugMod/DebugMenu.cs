using System;
using System.Linq;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonscarsDebugMod.DebugMod;

public class DebugMenu : MonoBehaviour {
    private GUIStyle? _buttonStyle;
    private Rect _area = new(100, 100, 0, 0);

    private bool _cursorVisible;
    private CursorLockMode _cursorLockState;

    private void OnEnable() {
        _cursorVisible = Cursor.visible;
        _cursorLockState = Cursor.lockState;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable() {
        Cursor.lockState = _cursorLockState;
        Cursor.visible = _cursorVisible;
    }


    private void DebugPanel() {
        var sceneController = SceneController.Instance;
        AddEnabled(sceneController is not null, () => {
            AddButton("Respawn", () => sceneController!.RespawnPlayer(false, false));
            AddButton("Kill", () => sceneController!.KillPlayer());
        });

        AddButton("Load dev save", LoadDevSave);
        AddButton("Unlock map", DiscoverAllMapAreas);
        AddButton("Forget map", ForgetAllMapAreas);

        AddButton("Pause units", PauseAllUnits);
        AddButton("Resume units", StartAllUnits);

        GUILayout.Space(32);
        AddButton("Close", () => enabled = false);
    }

    private void PauseAllUnits() {
        UnitsHandler.Instance.StopAllBehaviours();
    }

    private void StartAllUnits() {
        UnitsHandler.Instance.StartAllBehaviours();
    }

    private void LoadDevSave() {
        SavesHandler.SaveName = "DevSave";
        SceneManager.LoadScene("RootScene");
    }

    private void DiscoverAllMapAreas() {
        SavesHandler.ActiveSave.WorldData.DiscoveredMapAreas = Enumerable.Range(0, 159).ToList();
    }

    private void ForgetAllMapAreas() {
        SavesHandler.ActiveSave.WorldData.DiscoveredMapAreas = Enumerable.Range(0, 159).ToList();
    }

    private void Update() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGUI() {
        _buttonStyle ??= ButtonStyle();

        _area = GUILayout.Window(0, _area, _ => DebugPanel(), "Debug Window");
    }

    private void AddButton(string label, Action performed) {
        if (GUILayout.Button(label, _buttonStyle)) performed();
    }

    private GUIStyle ButtonStyle() {
        var style = new GUIStyle(GUI.skin.button);
        var backgroundNormal = MakeBackgroundTexture(10, 10, new Color(0.2f, 0.2f, 0.2f));
        var backgroundHover = MakeBackgroundTexture(10, 10, new Color(0.1f, 0.5f, 0.6f));
        var backgroundActive = MakeBackgroundTexture(10, 10, new Color(0.1f, 0.4f, 0.6f));
        var backgroundFocused = MakeBackgroundTexture(10, 10, new Color(0.6f, 0.4f, 0.6f));
        style.normal.background = backgroundNormal;
        style.hover.background = backgroundHover;
        style.active.background = backgroundActive;
        style.focused.background = backgroundFocused;
        return style;
    }

    private Texture2D MakeBackgroundTexture(int width, int height, Color color) {
        var pixels = new Color[width * height];

        for (var i = 0; i < pixels.Length; i++) pixels[i] = color;

        var backgroundTexture = new Texture2D(width, height);
        backgroundTexture.hideFlags = HideFlags.DontUnloadUnusedAsset;

        backgroundTexture.SetPixels(pixels);
        backgroundTexture.Apply();

        return backgroundTexture;
    }

    private void AddEnabled(bool enable, Action action) {
        var prev = GUI.enabled;
        GUI.enabled = enable;
        action();
        GUI.enabled = prev;
    }
}