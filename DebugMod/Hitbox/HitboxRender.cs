using System;
using System.Collections.Generic;
using ExampleMod;
using UnityEngine;
using Logger = ModdingAPI.Logger;

namespace MoonscarsDebugMod.DebugMod.Hitbox;

public class HitboxRender : MonoBehaviour {
    private readonly struct HitboxType : IComparable<HitboxType> {
        public static readonly HitboxType Player = new(Color.yellow, 0); // yellow
        public static readonly HitboxType Enemy = new(new Color(0.8f, 0, 0), 1); // red      
        public static readonly HitboxType DamageTrigger = new(new Color(1.0f, 1.0f, 1.0f), 1); // white
        public static readonly HitboxType Attack = new(Color.cyan, 2); // cyan
        public static readonly HitboxType TilePlayerInteraction = new(new Color(0, 0.8f, 0), 3); // green
        public static readonly HitboxType Trigger = new(new Color(0.5f, 0.5f, 1f), 4); // blue
        public static readonly HitboxType Breakable = new(new Color(1f, 0.75f, 0.8f), 5); // pink
        public static readonly HitboxType Ladder = new(new Color(0.0f, 0.0f, 0.5f), 6); // dark blue
        public static readonly HitboxType CameraZone = new(new Color(0.5f, 0.0f, 0.1f), 7); // purple
        public static readonly HitboxType Terrain = new(Color.magenta, 7); // magenta
        public static readonly HitboxType Other = new(new Color(0.9f, 0.6f, 0.4f), 8); // orange

        public readonly Color Color;
        public readonly int Depth;

        private HitboxType(Color color, int depth) {
            Color = color;
            Depth = depth;
        }

        public int CompareTo(HitboxType other) => other.Depth.CompareTo(Depth);
    }


    private const float LineWidth = 1f;
    private Camera _camera;


    private readonly SortedDictionary<HitboxType, HashSet<Collider2D>> _colliders = new() {
        { HitboxType.Player, new HashSet<Collider2D>() },
        { HitboxType.Enemy, new HashSet<Collider2D>() },
        { HitboxType.Attack, new HashSet<Collider2D>() },
        { HitboxType.TilePlayerInteraction, new HashSet<Collider2D>() },
        { HitboxType.Trigger, new HashSet<Collider2D>() },
        { HitboxType.Breakable, new HashSet<Collider2D>() },
        { HitboxType.Ladder, new HashSet<Collider2D>() },
        { HitboxType.CameraZone, new HashSet<Collider2D>() },
        { HitboxType.Other, new HashSet<Collider2D>() }
    };


    public void SearchHitboxes() {
        foreach (var col in FindObjectsByType<Collider2D>(FindObjectsSortMode.None)) TryAddHitbox(col);
    }

    public void UpdateHitbox(GameObject go) {
        foreach (var col in go.GetComponentsInChildren<Collider2D>(true)) TryAddHitbox(col);
    }


    private void TryAddHitbox(Collider2D collider2D) {
        if (collider2D == null) return;


        if (!collider2D.isActiveAndEnabled) return;

        if (
            collider2D is BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D or CapsuleCollider2D
        ) {
            if (collider2D.GetComponent<PlayerPawn>())
                _colliders[HitboxType.Player].Add(collider2D);
            if (collider2D.GetComponent<UnitPawn>())
                _colliders[HitboxType.Enemy].Add(collider2D);
            if (collider2D.GetComponent<Ladder>())
                _colliders[HitboxType.Ladder].Add(collider2D);
            if (collider2D.GetComponent<TilePlayerInteraction>())
                _colliders[HitboxType.TilePlayerInteraction].Add(collider2D);
            if (collider2D.GetComponent<PlayerTrigger>())
                _colliders[HitboxType.Trigger].Add(collider2D);
            if (collider2D.GetComponent<CameraZone>())
                _colliders[HitboxType.CameraZone].Add(collider2D);
            if (collider2D.GetComponent<DamagingTrigger>())
                _colliders[HitboxType.DamageTrigger].Add(collider2D);
            else
                _colliders[HitboxType.Other].Add(collider2D);
        } else if (collider2D is CompositeCollider2D) {
            _colliders[HitboxType.Terrain].Add(collider2D);
        }
    }

    private bool IsColliderVisible(Collider2D collider) {
        if (collider is CompositeCollider2D) return true;

        var bounds = collider.bounds;
        var min = bounds.min;
        var max = bounds.max;
        var corners = new Vector2[4];
        corners[0] = new Vector2(min.x, min.y); // bottom-left
        corners[1] = new Vector2(min.x, max.y); // top-left
        corners[2] = new Vector2(max.x, max.y); // top-right
        corners[3] = new Vector2(max.x, min.y); // bottom-right

        for (var i = 0; i < 4; i++) {
            Vector2 viewportPoint = _camera.WorldToScreenPoint(corners[i]);
            var onScreen = viewportPoint.x >= 0 && viewportPoint.x <= Screen.width && viewportPoint.y >= 0 &&
                           viewportPoint.y <= Screen.height;
            if (onScreen) return true;
        }

        return false;
    }


    public void OnGUI() {
        if (ReferenceEquals(SceneController.Instance, null)) return;

        _camera = CameraController.Instance.Camera;


        try {
            GUI.depth = int.MaxValue;
            foreach (var pair in _colliders) {
                var toDelete = new List<Collider2D>();

                foreach (var collider2D in pair.Value) {
                    if (!collider2D) {
                        toDelete.Add(collider2D);
                        continue;
                    }

                    if (IsColliderVisible(collider2D)) DrawHitbox(_camera, collider2D, pair.Key, LineWidth);
                }

                foreach (var del in toDelete) pair.Value.Remove(del);
            }
        } catch (Exception e) {
            Logger.LogError(e.ToString());
        }
    }

    private void DrawHitbox(Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth) {
        var origDepth = GUI.depth;

        GUI.depth = hitboxType.Depth;
        switch (collider2D) {
            case BoxCollider2D boxCollider2D:
                var halfSize = boxCollider2D.size / 2f;
                Vector2 topLeft = new(-halfSize.x, halfSize.y);
                var topRight = halfSize;
                Vector2 bottomRight = new(halfSize.x, -halfSize.y);
                var bottomLeft = -halfSize;
                var boxPoints = new List<Vector2> {
                    topLeft, topRight, bottomRight, bottomLeft, topLeft
                };
                DrawPointSequence(boxPoints, camera, collider2D, hitboxType, lineWidth);
                break;
            case EdgeCollider2D edgeCollider2D:
                DrawPointSequence(new List<Vector2>(edgeCollider2D.points), camera, collider2D, hitboxType,
                    lineWidth);
                break;
            case PolygonCollider2D polygonCollider2D:
                for (var i = 0; i < polygonCollider2D.pathCount; i++) {
                    List<Vector2> polygonPoints = new(polygonCollider2D.GetPath(i));
                    if (polygonPoints.Count > 0) polygonPoints.Add(polygonPoints[0]);

                    DrawPointSequence(polygonPoints, camera, collider2D, hitboxType, lineWidth);
                }

                break;
            case CapsuleCollider2D capsuleCollider2D:
                if (capsuleCollider2D.direction == CapsuleDirection2D.Vertical) {
                    var size = capsuleCollider2D.size;
                    var radius = 0.5f * size.x;
                    var hs = size / 2f;

                    var tl = new Vector2(-hs.x, hs.y - radius);
                    var tr = new Vector2(hs.x, hs.y - radius);
                    var bl = new Vector2(-hs.x, -hs.y + radius);
                    var br = new Vector2(hs.x, -hs.y + radius);

                    Drawing.DrawLine(
                        LocalToScreenPoint(_camera, collider2D, tl),
                        LocalToScreenPoint(_camera, collider2D, bl),
                        hitboxType.Color, lineWidth, true
                    );
                    Drawing.DrawLine(
                        LocalToScreenPoint(_camera, collider2D, tr),
                        LocalToScreenPoint(_camera, collider2D, br),
                        hitboxType.Color, lineWidth, true
                    );

                    var screenSpaceRadius = (int)Math.Round(0.5f * (LocalToScreenPoint(_camera, collider2D, tr).x -
                                                                    LocalToScreenPoint(_camera, collider2D, tl).x));
                    var segments = Mathf.Clamp(screenSpaceRadius / 8, 4, 32);

                    Drawing.DrawUpperHalfCircle(
                        LocalToScreenPoint(camera, collider2D, new Vector2(0.0f, hs.y - radius)),
                        screenSpaceRadius,
                        hitboxType.Color, lineWidth, true, segments);
                    Drawing.DrawLowerHalfCircle(
                        LocalToScreenPoint(camera, collider2D, new Vector2(0.0f, -hs.y + radius)),
                        screenSpaceRadius,
                        hitboxType.Color, lineWidth, true, segments);
                } else {
                    var size = capsuleCollider2D.size;
                    var radius = 0.5f * size.y;
                    var hs = size / 2f;

                    var tl = new Vector2(-hs.x + radius, hs.y);
                    var tr = new Vector2(hs.x - radius, hs.y);
                    var bl = new Vector2(-hs.x + radius, -hs.y);
                    var br = new Vector2(hs.x - radius, -hs.y);

                    Drawing.DrawLine(
                        LocalToScreenPoint(_camera, collider2D, tl),
                        LocalToScreenPoint(_camera, collider2D, tr),
                        hitboxType.Color, lineWidth, true
                    );
                    Drawing.DrawLine(
                        LocalToScreenPoint(_camera, collider2D, bl),
                        LocalToScreenPoint(_camera, collider2D, br),
                        hitboxType.Color, lineWidth, true
                    );

                    var screenSpaceRadius = (int)Math.Round(0.5f * (LocalToScreenPoint(_camera, collider2D, br).y -
                                                                    LocalToScreenPoint(_camera, collider2D, tr).y));
                    var segments = Mathf.Clamp(screenSpaceRadius / 16, 4, 32);

                    Drawing.DrawRightHalfCircle(
                        LocalToScreenPoint(camera, collider2D, new Vector2(hs.x - radius, 0)),
                        screenSpaceRadius,
                        hitboxType.Color, lineWidth, true, segments);
                    Drawing.DrawLeftHalfCircle(
                        LocalToScreenPoint(camera, collider2D, new Vector2(-hs.x + radius, 0)),
                        screenSpaceRadius,
                        hitboxType.Color, lineWidth, true, segments);
                }

                break;
            case CircleCollider2D circleCollider2D: {
                var center = LocalToScreenPoint(camera, collider2D, Vector2.zero);
                var right = LocalToScreenPoint(camera, collider2D, Vector2.right * circleCollider2D.radius);
                var circleRradius = (int)Math.Round(Vector2.Distance(center, right));
                Drawing.DrawCircle(center, circleRradius, hitboxType.Color, lineWidth, true,
                    Mathf.Clamp(circleRradius / 8, 4, 32));
                break;
            }
            case CompositeCollider2D compositeCollider2D: {
                for (var i = 0; i < compositeCollider2D.pathCount; i++) {
                    var pathVerts = new Vector2[compositeCollider2D.GetPathPointCount(i)];
                    compositeCollider2D.GetPath(i, pathVerts);
                    DrawPointSequence(pathVerts, camera, collider2D, hitboxType, lineWidth);
                }

                break;
            }
        }

        GUI.depth = origDepth;
    }

    private void DrawPointSequence(IReadOnlyList<Vector2> points, Camera camera, Collider2D collider2D,
        HitboxType hitboxType,
        float lineWidth) {
        for (var i = 0; i < points.Count - 1; i++) {
            var pointA = LocalToScreenPoint(camera, collider2D, points[i]);
            var pointB = LocalToScreenPoint(camera, collider2D, points[i + 1]);
            Drawing.DrawLine(pointA, pointB, hitboxType.Color, lineWidth, true);
        }
    }

    private Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point) {
        Vector2 result =
            camera.WorldToScreenPoint((Vector2)collider2D.transform.TransformPoint(point + collider2D.offset));
        return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
    }
}