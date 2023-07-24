using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoonscarsDebugMod.DebugMod;

public class DebugInfo : MonoBehaviour {
    private readonly GUIStyle _style = new(GUIStyle.none);

    private const TextAnchor AnchorMainMenu = TextAnchor.UpperRight;
    private const TextAnchor AnchorInGame = TextAnchor.LowerRight;

    private void Start() {
        _style.normal.textColor = Color.white;
        // ReSharper disable once Unity.UnknownResource
        _style.font = Resources.Load<Font>("fonts & materials/Carnas W03 Light");
        _style.padding = new RectOffset(5, 5, 5, 5);
    }

    private Vector3 _positionLastFrame;
    private string _infoText = "";
    private int _fixedUpdateCounter;

    private void FixedUpdate() {
        _fixedUpdateCounter++;
    }


    private void LateUpdate() {
        var sceneController = SceneController.Instance;

        _style.alignment = sceneController is null ? AnchorMainMenu : AnchorInGame;

        if (sceneController is null) {
            _infoText = "Main menu";
            return;
        }

        var player = sceneController.Player;
        var playerPawn = player.PlayerPawn;
        var playerRigidbody = playerPawn.UnitRigidBody;
        var playerModifierController = playerPawn.PlayerModifierController;
        var playerAnimatorController = playerPawn.UnitAnimatorController;
        var playerHealthEntity = player.HealthEntity;

        var position = player.gameObject.transform.position;
        var velocity = playerRigidbody.velocity;

        var moved = position - _positionLastFrame;

        var flags = new List<string>();
        if (!PlayerInputFacade.Instance.GameInputEnabled) flags.Add("NoControl");
        flags.Add(playerPawn.InAir ? "InAir" : "Grounded");
        if (playerPawn.IsAttachedToLadder) flags.Add("Ladder");
        if (playerModifierController.DashShouldDamage) flags.Add("LadderAvailable");
        if (playerPawn.PlayerModifierController.IsDashChainActive) flags.Add("DashChain");
        if (playerAnimatorController.IsStunned) flags.Add("Stunned");

        var animationPhases = Enum.GetValues(typeof(EAnimationPhase)).Cast<EAnimationPhase>()
            .Where(phase => playerAnimatorController.GetPhaseState(phase))
            .Select(phase => phase.ToString());

        var currentScene = SceneStreamerController.Instance.GetSceneAtPoint(position);
        _infoText = $@"
Pos: {position.x:N2}, {position.y:N2}
Speed: {velocity.x:N2}, {velocity.y:N2}
Moved: {moved.x:N2}, {moved.y:N2}
Mana: {PlayerInventory.Mana} ({PlayerInventory.ManaDebtAvailable})/{PlayerInventory.ManaMaxAvailable}
{(playerHealthEntity.IsIgnoringDamage ? "IgnoringDamage" : "")} HP: {playerHealthEntity.CurrentHP}/{playerHealthEntity.MaxHP}
{string.Join(" ", flags)}
{string.Join(" ", animationPhases)}
[{currentScene.FunctionalScene.name}] TimeScale {Time.timeScale} FPS {1 / Time.deltaTime:N1}
";
        _positionLastFrame = position;
        _fixedUpdateCounter = 0;
    }

    public void OnGUI() {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), _infoText, _style);
    }
}