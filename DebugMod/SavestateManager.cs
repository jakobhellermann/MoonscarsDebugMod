using ModdingAPI;
using UnityEngine;
using Logger = ModdingAPI.Logger;

namespace MoonscarsDebugMod.DebugMod;

public record Savestate {
    public Vector3 Position;
    public Vector2 RigidbodyPosition;
    public Vector2 Velocity;


    public static Savestate? Create() {
        if (SceneController.Instance is not { } sceneController) return null;

        var rigidBody = sceneController.Player.PlayerPawn.UnitRigidBody;
        return new Savestate {
            Position = sceneController.Player.transform.position,
            RigidbodyPosition = rigidBody.position,
            Velocity = rigidBody.velocity
        };
    }

    public bool Restore() {
        if (SceneController.Instance is not { } sceneController) return false;

        CameraController.Instance.RecenterOnPlayer();
        var playerPawn = sceneController.Player.PlayerPawn;
        playerPawn.transform.position = Position;
        playerPawn.UnitRigidBody.position = RigidbodyPosition;
        playerPawn.UnitRigidBody.velocity = Velocity;

        return true;
    }
}

public class SavestateManager {
    private Savestate? _savestate;

    public void Save() {
        UIUtils.ShowSystemMessage("Creating savestate...");

        _savestate = Savestate.Create();
        if (_savestate is null) {
            UIUtils.ShowSystemMessage("Failed to create savestate");
            return;
        }

        Logger.Log(_savestate.ToString());
    }

    public void Load() {
        if (_savestate is null) {
            UIUtils.ShowSystemMessage("No savestate currently set!");
            return;
        }

        if (!_savestate.Restore()) {
            UIUtils.ShowSystemMessage("Failed to load savestate!");
            return;
        }

        UIUtils.ShowSystemMessage("Loaded savestate!");
    }
}