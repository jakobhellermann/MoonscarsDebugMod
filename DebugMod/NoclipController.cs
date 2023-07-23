using System.Reflection;
using ModdingAPI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonscarsDebugMod.DebugMod;

public class NoclipController : MonoBehaviour {
    public bool NoClip {
        get => enabled;
        set => enabled = value;
    }

    private Vector3 _beforeNoclipPosition;
    private float _beforeNoclipOriginalZoom;

    private Vector3 _noclipPos;

    public InputAction Movement;
    public InputAction Nyoom;

    private const float MoveSpeed = 5;
    private const float NyoomFactor = 8;
    private const float ZoomSensitivity = 0.01f;


    private void FixedUpdate() {
        if (SceneController.Instance is not { } sceneController) return;
        var rigidBody = sceneController.Player.PlayerPawn.UnitRigidBody;

        var movement = Movement.ReadValue<Vector2>();
        var nyoom = Nyoom.ReadValue<float>();

        var speed = MoveSpeed + nyoom * NyoomFactor;


        _noclipPos = _noclipPos + (Vector3)movement * (speed * Time.fixedDeltaTime);
        rigidBody.MovePosition(_noclipPos);
    }

    public void Zoom(float amount) {
        if (!NoClip) return;
        if (CameraController.Instance is not { } cameraController) return;

        var target = Mathf.Max(1.0f, GetOriginalZoom(cameraController) - amount * ZoomSensitivity);
        SetOriginalZoom(cameraController, target);
        SetCurrentZoom(cameraController, target);
    }

    public void ToggleNoclip() {
        if (SceneController.Instance is not { } sceneController) return;
        var playerPawn = sceneController.Player.PlayerPawn;
        var cameraController = CameraController.Instance;

        if (!NoClip) {
            UIUtils.ShowSystemMessage("Enabling noclip");

            playerPawn.enabled = false;
            playerPawn.UnitRigidBody.isKinematic = true;
            playerPawn.UnitRigidBody.velocity = Vector2.zero;
            playerPawn.HealthEntityPlayer.IgnoreDamageRetain();
            PlayerInputFacade.Instance.RetainLockControls();
            _beforeNoclipPosition = playerPawn.transform.position;
            _noclipPos = _beforeNoclipPosition;
            _beforeNoclipOriginalZoom = GetOriginalZoom(cameraController);
            NoClip = true;
        } else {
            UIUtils.ShowSystemMessage("Disabling noclip");

            playerPawn.enabled = true;
            playerPawn.UnitRigidBody.isKinematic = false;
            playerPawn.HealthEntityPlayer.Invincible = false;
            // playerPawn.transform.position = _beforeNoclipPosition;
            playerPawn.HealthEntityPlayer.IgnoreDamageRelease();
            PlayerInputFacade.Instance.ReleaseLockControls();
            SetOriginalZoom(cameraController, _beforeNoclipOriginalZoom);
            NoClip = false;
        }
    }


    private static FieldInfo? _cachedOriginalZoomField;
    private static FieldInfo? _cachedCurrentZoomField;

    private static float GetOriginalZoom(CameraController cameraController) {
        _cachedOriginalZoomField ??=
            typeof(CameraController).GetField("_originalZoom", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (float)_cachedOriginalZoomField.GetValue(cameraController);
    }

    private static void SetOriginalZoom(CameraController cameraController, float value) {
        _cachedOriginalZoomField ??=
            typeof(CameraController).GetField("_originalZoom", BindingFlags.NonPublic | BindingFlags.Instance)!;
        _cachedOriginalZoomField.SetValue(cameraController, value);
    }

    private static void SetCurrentZoom(CameraController cameraController, float value) {
        _cachedCurrentZoomField ??=
            typeof(CameraController).GetField("_currentZoom", BindingFlags.NonPublic | BindingFlags.Instance)!;
        _cachedCurrentZoomField.SetValue(cameraController, value);
    }
}