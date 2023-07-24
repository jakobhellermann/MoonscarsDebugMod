using System.Reflection;
using MoonscarsUI;
using UnityEngine;

namespace MoonscarsDebugMod.DebugMod;

public static class MapTeleporter {
    private static FieldInfo _mapControllerScrollPositionActual =
        typeof(MapController).GetField("_scrollPositionActual", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static FieldInfo _mapControllerZoomedIn =
        typeof(MapController).GetField("_zoomedIn", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static FieldInfo _mapControllerScaleActual =
        typeof(MapController).GetField("_scaleActual", BindingFlags.Instance | BindingFlags.NonPublic)!;


    public static void AskTeleport(MapController mapController) {
        var scrollPositionActual = (Vector2)_mapControllerScrollPositionActual.GetValue(mapController);
        var zoomedIn = (bool)_mapControllerZoomedIn.GetValue(mapController);
        var scaleActual = (float)_mapControllerScaleActual.GetValue(mapController);

        var positionWorldSpace = -(scrollPositionActual * (zoomedIn ? 0.5f : 1f)) / scaleActual;

        ScreenUIController.Instance.ShowGenericPopupWindowYesNo("Teleport?", $"Teleport to {positionWorldSpace}?",
            () => {
                ScreenUIController.Instance.ShowCharacterScreen(false);
                CameraController.Instance.RecenterOnPlayer();
                SceneController.Instance.Player.PlayerPawn.Teleport(positionWorldSpace);
            },
            () => { }
        );
    }
}