using System;
using System.Collections.Generic;
using System.Reflection;
using ExampleMod;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Logger = ModdingAPI.Logger;

namespace MoonscarsDebugMod.DebugMod;

internal readonly struct DebugLine {
    public readonly Vector2 Start;
    public readonly Vector2 End;
    public readonly Color Color;
    public readonly float ExpireTime;
    public readonly bool DepthTest;

    public DebugLine(Vector2 start, Vector2 end, Color color, float expireTime, bool depthTest) {
        Start = start;
        End = end;
        Color = color;
        ExpireTime = expireTime;
        DepthTest = depthTest;
    }
}

public class UnityDebugPolyfill : MonoBehaviour {
    private readonly List<DebugLine> _lines = new();

    private static UnityDebugPolyfill _instance = null!;

    private void Awake() {
        _instance = this;
    }

    private static void DrawLineHook(Vector3 start, Vector3 end, Color color, float duration, bool depthTest) {
        try {
            _instance.DrawLine(start, end, color, duration, depthTest);
        } catch (Exception e) {
            Logger.LogError($"Failed to draw line: {e}");
        }
    }

    public void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest) {
        if (!enabled) return;


        var expireTime = Time.time + duration;
        _lines.Add(new DebugLine(start, end, color, expireTime, depthTest));
    }


    private void LateUpdate() {
        var now = Time.time;
        _lines.RemoveAll(line => line.ExpireTime < now);
    }

    private void OnGUI() {
        if (Event.current.type != EventType.Repaint) return;

        if (CameraController.Instance is not { } cameraController) return;

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _lines.Count; i++) {
            var line = _lines[i];

            var camera = cameraController.Camera;
            var startScreenspace = camera.WorldToScreenPoint(line.Start);
            var endScreenspace = camera.WorldToScreenPoint(line.End);
            startScreenspace.y = Screen.height - startScreenspace.y;
            endScreenspace.y = Screen.height - endScreenspace.y;

            Drawing.DrawLine(startScreenspace, endScreenspace, line.Color, 1, true);
        }
    }


    private Detour _drawLineDetour = null!;

    public void Hook() {
        var drawLineMethod = typeof(Debug).GetMethod(
            "DrawLine",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Vector3), typeof(Vector3), typeof(Color), typeof(float), typeof(bool) },
            null);
        _drawLineDetour = new Detour(
            drawLineMethod,
            typeof(UnityDebugPolyfill).GetMethod(nameof(DrawLineHook), BindingFlags.NonPublic | BindingFlags.Static)
        );
    }

    public void Unhook() {
        _drawLineDetour.Dispose();
    }
}