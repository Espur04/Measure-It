using System.Collections.Generic;
using Colossal.Logging;
using Colossal.Mathematics;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using GameMode = Game.GameMode;

namespace MeasureItCS2.Systems
{
    public partial class MeasureToolSystem : ToolBaseSystem
    {
        private static Color PointColor
        {
            get
            {
                Color color = Color.red;
                ColorUtility.TryParseHtmlString(MeasureMath.ColorHex(Mod.Settings.MeasureColor), out color);
                color.a = 0.85f;
                return color;
            }
        }
        private const float PointRadius = 0.8f;

        private const float NodeSnapRadius = 15f;

        private const float NodeVisibilityRadius = 80f;
        private const float NodeMarkerRadius = 1.5f;
        private const float ShapeClosureTolerance = 6f;

        private readonly List<float3> m_NearbyNodePositions = new List<float3>();

        private OverlayRenderSystem m_OverlayRenderSystem;
        private TerrainSystem m_TerrainSystem;
        private EntityQuery m_NodeQuery;
        private EntityQuery m_EdgeQuery;

        private float3 m_CursorPosition;
        private bool m_HasCursorPosition;
        private Entity m_CursorNodeEntity;
        private bool m_HasLockedHeight;
        private float m_LockedHeight;

        private readonly List<float3> m_PreviewPathScratch = new List<float3>();

        public float3 CursorPosition => m_CursorPosition;
        public bool HasCursorPosition => m_HasCursorPosition;

        public IReadOnlyList<float3> Points => m_Points;
        private readonly List<float3> m_Points = new List<float3>();
        public bool IsLastPointNearStart => m_Points.Count >= 3 &&
                                            MeasureMath.LengthXZ(m_Points[m_Points.Count - 1], m_Points[0]) <= ShapeClosureTolerance;

        private readonly List<Entity> m_PointNodeEntities = new List<Entity>();

        private readonly List<List<float3>> m_SegmentPaths = new List<List<float3>>();

        public override string toolID => "MeasureItCS2.MeasureTool";

        protected override void OnCreate()
        {
            base.OnCreate();

            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_NodeQuery = GetEntityQuery(ComponentType.ReadOnly<Node>());
            m_EdgeQuery = GetEntityQuery(ComponentType.ReadOnly<Edge>(), ComponentType.ReadOnly<Curve>());

            allowUnderground = true;

            Enabled = false;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            applyAction.shouldBeEnabled = true;
            secondaryApplyAction.shouldBeEnabled = true;

            requireUnderground = (m_ToolSystem.actionMode & GameMode.Game) != 0;
        }

        protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);

            ClearPoints();
            m_HasCursorPosition = false;
            m_CursorNodeEntity = Entity.Null;
            m_NearbyNodePositions.Clear();
            m_PreviewPathScratch.Clear();
        }

        protected override void OnStopRunning()
        {
            applyAction.shouldBeEnabled = false;
            secondaryApplyAction.shouldBeEnabled = false;

            base.OnStopRunning();
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();

            m_ToolRaycastSystem.typeMask = TypeMask.Terrain | TypeMask.Net | TypeMask.StaticObjects;
            m_ToolRaycastSystem.raycastFlags = RaycastFlags.Outside;

            m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground | CollisionMask.Underground;

            m_ToolRaycastSystem.netLayerMask = Layer.All;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_ToolRaycastSystem.typeMask = Mod.Settings.ForceGroundHeight
                ? TypeMask.Terrain
                : TypeMask.Terrain | TypeMask.Net | TypeMask.StaticObjects;

            if (!Mod.Settings.LockCurrentHeight)
            {
                m_HasLockedHeight = false;
            }
            else if (!m_HasLockedHeight && m_Points.Count > 0)
            {
                m_LockedHeight = m_Points[m_Points.Count - 1].y;
                m_HasLockedHeight = true;
            }

            if (GetRaycastResult(out float3 hitPosition))
            {
                if (Mod.Settings.SnapToNodes)
                {
                    if (TryFindNearestNode(hitPosition, out float3 nodePosition, out Entity nodeEntity))
                    {
                        m_CursorPosition = nodePosition;
                        m_CursorNodeEntity = nodeEntity;
                    }
                    else
                    {
                        m_CursorPosition = hitPosition;
                        m_CursorNodeEntity = Entity.Null;
                    }
                }
                else
                {
                    m_CursorPosition = hitPosition;
                    m_CursorNodeEntity = Entity.Null;
                    m_NearbyNodePositions.Clear();
                }

                if (Mod.Settings.ForceGroundHeight)
                {
                    TerrainHeightData terrainData = m_TerrainSystem.GetHeightData(waitForPending: true);
                    m_CursorPosition.y = terrainData.isCreated
                        ? TerrainUtils.SampleHeight(ref terrainData, m_CursorPosition)
                        : hitPosition.y;
                    m_CursorNodeEntity = Entity.Null;
                }
                else if (Mod.Settings.LockCurrentHeight && m_HasLockedHeight)
                {
                    m_CursorPosition.y = m_LockedHeight;
                    m_CursorNodeEntity = Entity.Null;
                }

                m_HasCursorPosition = true;
            }
            else
            {
                m_HasCursorPosition = false;
                m_CursorNodeEntity = Entity.Null;
                m_NearbyNodePositions.Clear();
            }

            bool mouseOverUI = InputManager.instance.mouseOverUI;

            if (applyAction.WasPressedThisFrame() && m_HasCursorPosition && !mouseOverUI)
            {
                AddPoint(m_CursorPosition);
            }
            else if (secondaryApplyAction.WasPressedThisFrame())
            {
                RemoveLastPoint();
            }

            DrawOverlay();

            return inputDeps;
        }

        private bool GetRaycastResult(out float3 position)
        {
            if (GetRaycastResult(out _, out RaycastHit hit))
            {
                position = hit.m_HitPosition;
                return true;
            }

            position = default;
            return false;
        }

        private bool TryFindNearestNode(float3 searchPosition, out float3 nodePosition, out Entity nodeEntity)
        {
            m_NearbyNodePositions.Clear();

            NativeArray<Entity> nodeEntities = m_NodeQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Node> nodes = m_NodeQuery.ToComponentDataArray<Node>(Allocator.Temp);

            bool found = false;
            float nearestDistanceSq = NodeSnapRadius * NodeSnapRadius;
            float visibilityDistanceSq = NodeVisibilityRadius * NodeVisibilityRadius;
            nodePosition = default;
            nodeEntity = Entity.Null;

            for (int i = 0; i < nodes.Length; i++)
            {
                float3 candidate = nodes[i].m_Position;
                float dx = candidate.x - searchPosition.x;
                float dz = candidate.z - searchPosition.z;
                float distanceSq = dx * dx + dz * dz;

                if (distanceSq <= visibilityDistanceSq)
                {
                    m_NearbyNodePositions.Add(candidate);
                }

                if (distanceSq <= nearestDistanceSq)
                {
                    nearestDistanceSq = distanceSq;
                    nodePosition = candidate;
                    nodeEntity = nodeEntities[i];
                    found = true;
                }
            }

            nodeEntities.Dispose();
            nodes.Dispose();
            return found;
        }

        private const int MaxPathfindNodesVisited = 3000;

        private const int PathEdgeSampleCount = 10;

        private bool TryFindNetworkPath(Entity startNode, Entity endNode, float3 startPosition, float3 endPosition, List<float3> result)
        {
            result.Clear();

            if (startNode == Entity.Null || endNode == Entity.Null || startNode == endNode)
            {
                return false;
            }

            NativeArray<Edge> edges = m_EdgeQuery.ToComponentDataArray<Edge>(Allocator.Temp);
            NativeArray<Curve> curves = m_EdgeQuery.ToComponentDataArray<Curve>(Allocator.Temp);

            var touchingEdges = new Dictionary<Entity, List<int>>();
            for (int i = 0; i < edges.Length; i++)
            {
                AddTouchingEdge(touchingEdges, edges[i].m_Start, i);
                AddTouchingEdge(touchingEdges, edges[i].m_End, i);
            }

            var cameFromEdge = new Dictionary<Entity, int>();
            var cameFromNode = new Dictionary<Entity, Entity>();
            var visited = new HashSet<Entity> { startNode };

            var queue = new List<Entity> { startNode };
            int queueHead = 0;

            bool found = false;
            int visitedCount = 1;

            while (queueHead < queue.Count && visitedCount <= MaxPathfindNodesVisited)
            {
                Entity current = queue[queueHead];
                queueHead++;

                if (current == endNode)
                {
                    found = true;
                    break;
                }

                if (!touchingEdges.TryGetValue(current, out List<int> edgeIndices))
                {
                    continue;
                }

                for (int e = 0; e < edgeIndices.Count; e++)
                {
                    int edgeIndex = edgeIndices[e];
                    Edge edge = edges[edgeIndex];
                    Entity other = edge.m_Start == current ? edge.m_End : edge.m_Start;

                    if (visited.Contains(other))
                    {
                        continue;
                    }

                    visited.Add(other);
                    cameFromEdge[other] = edgeIndex;
                    cameFromNode[other] = current;
                    queue.Add(other);
                    visitedCount++;
                }
            }

            if (found)
            {
                var edgeSequence = new List<(int edgeIndex, bool forward)>();
                Entity walk = endNode;

                while (walk != startNode)
                {
                    int edgeIndex = cameFromEdge[walk];
                    Entity previous = cameFromNode[walk];
                    bool forward = edges[edgeIndex].m_Start == previous;
                    edgeSequence.Add((edgeIndex, forward));
                    walk = previous;
                }

                edgeSequence.Reverse();

                for (int s = 0; s < edgeSequence.Count; s++)
                {
                    (int edgeIndex, bool forward) = edgeSequence[s];
                    Bezier4x3 bezier = curves[edgeIndex].m_Bezier;

                    for (int step = 0; step <= PathEdgeSampleCount; step++)
                    {
                        float t = step / (float)PathEdgeSampleCount;
                        if (!forward)
                        {
                            t = 1f - t;
                        }

                        if (step == 0 && s > 0)
                        {
                            continue;
                        }

                        result.Add(MathUtils.Position(bezier, t));
                    }
                }

                if (result.Count > 0)
                {
                    result[0] = startPosition;
                    result[result.Count - 1] = endPosition;
                }
            }

            edges.Dispose();
            curves.Dispose();

            return found && result.Count > 1;
        }

        private static void AddTouchingEdge(Dictionary<Entity, List<int>> touchingEdges, Entity node, int edgeIndex)
        {
            if (!touchingEdges.TryGetValue(node, out List<int> list))
            {
                list = new List<int>();
                touchingEdges[node] = list;
            }
            list.Add(edgeIndex);
        }

        public void AddPoint(float3 position)
        {
            m_Points.Add(position);
            m_PointNodeEntities.Add(m_CursorNodeEntity);

            if (m_Points.Count >= 2)
            {
                Entity fromNode = m_PointNodeEntities[m_PointNodeEntities.Count - 2];
                Entity toNode = m_PointNodeEntities[m_PointNodeEntities.Count - 1];
                float3 from = m_Points[m_Points.Count - 2];
                float3 to = m_Points[m_Points.Count - 1];

                var path = new List<float3>();
                TryFindNetworkPath(fromNode, toNode, from, to, path);
                m_SegmentPaths.Add(path);
            }

            if (Mod.Settings.LockCurrentHeight && !m_HasLockedHeight)
            {
                m_LockedHeight = position.y;
                m_HasLockedHeight = true;
            }

        }

        public void RemoveLastPoint()
        {
            if (m_Points.Count > 0)
            {
                m_Points.RemoveAt(m_Points.Count - 1);
                m_PointNodeEntities.RemoveAt(m_PointNodeEntities.Count - 1);

                if (m_SegmentPaths.Count > 0)
                {
                    m_SegmentPaths.RemoveAt(m_SegmentPaths.Count - 1);
                }
            }

            if (m_Points.Count == 0)
            {
                m_HasLockedHeight = false;
            }
        }

        public void ClearPoints()
        {
            m_Points.Clear();
            m_PointNodeEntities.Clear();
            m_SegmentPaths.Clear();
            m_HasLockedHeight = false;
        }

        public override void GetAvailableSnapMask(out Snap onMask, out Snap offMask)
        {
            onMask = Snap.NetSide | Snap.NetNode | Snap.ObjectSide;
            offMask = Snap.None;
        }

        private void DrawOverlay()
        {
            OverlayRenderSystem.Buffer buffer = m_OverlayRenderSystem.GetBuffer(out JobHandle overlayDeps);
            overlayDeps.Complete();

            if (Mod.Settings.SnapToNodes)
            {
                Color nodeMarkerColor = PointColor;
                nodeMarkerColor.a *= 0.6f;

                for (int i = 0; i < m_NearbyNodePositions.Count; i++)
                {
                    buffer.DrawCircle(nodeMarkerColor, m_NearbyNodePositions[i], NodeMarkerRadius);
                }
            }

            for (int i = 0; i < m_Points.Count; i++)
            {
                buffer.DrawCircle(PointColor, m_Points[i], PointRadius);

                if (i > 0)
                {
                    DrawSegmentOrPath(buffer, m_Points[i - 1], m_Points[i], m_SegmentPaths[i - 1]);
                }
            }

            if (m_Points.Count >= 3 && IsLastPointNearStart)
            {
                Color closureHintColor = PointColor;
                closureHintColor.a *= 0.5f;
                float3 lastPoint = m_Points[m_Points.Count - 1];
                buffer.DrawLine(closureHintColor, new Line3.Segment(lastPoint, m_Points[0]), PointRadius * 0.3f);
            }

            if (m_HasCursorPosition && m_Points.Count > 0)
            {
                float3 lastPoint = m_Points[m_Points.Count - 1];
                Entity lastPointNode = m_PointNodeEntities[m_PointNodeEntities.Count - 1];
                TryFindNetworkPath(lastPointNode, m_CursorNodeEntity, lastPoint, m_CursorPosition, m_PreviewPathScratch);
                DrawSegmentOrPath(buffer, lastPoint, m_CursorPosition, m_PreviewPathScratch);
            }
        }

        private void DrawSegmentOrPath(OverlayRenderSystem.Buffer buffer, float3 from, float3 to, List<float3> path)
        {
            if (path != null && path.Count > 1)
            {
                for (int p = 1; p < path.Count; p++)
                {
                    buffer.DrawLine(PointColor, new Line3.Segment(path[p - 1], path[p]), PointRadius * 0.25f);
                }
            }
            else
            {
                buffer.DrawLine(PointColor, new Line3.Segment(from, to), PointRadius * 0.25f);
            }
        }

        public override PrefabBase GetPrefab() => null;

        public override bool TrySetPrefab(PrefabBase prefab) => false;
    }
}
