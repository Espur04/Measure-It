using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Colossal.UI.Binding;
using Game.Input;
using Game.Tools;
using Game.UI;
using Unity.Mathematics;
using UnityEngine;
using GameMode = Game.GameMode;

namespace MeasureItCS2.Systems
{
    public partial class MeasureUISystem : UISystemBase
    {
        private const string Group = "measureItCS2";

        private static bool s_ResetPanelPositionRequested;

        public static void RequestResetPanelPosition()
        {
            s_ResetPanelPositionRequested = true;
        }

        private MeasureToolSystem m_MeasureToolSystem;
        private ToolSystem m_ToolSystem;
        private DefaultToolSystem m_DefaultToolSystem;
        private Game.Rendering.CameraUpdateSystem m_CameraUpdateSystem;

        private ProxyAction m_ToggleToolAction;
        private ProxyAction m_UndoLastPointAction;
        private ProxyAction m_AddPointAction;

        private ValueBinding<bool> m_ToolActive;
        private ValueBinding<int> m_UnitOfDistance;
        private ValueBinding<int> m_UnitOfHeight;
        private ValueBinding<int> m_UnitOfSlope;
        private ValueBinding<int> m_UnitOfDirection;
        private ValueBinding<int> m_UnitOfArea;
        private ValueBinding<string> m_SegmentsJson;
        private ValueBinding<string> m_SummaryJson;
        private ValueBinding<bool> m_PanelPlaced;
        private ValueBinding<float> m_PanelX;
        private ValueBinding<float> m_PanelY;
        private ValueBinding<string> m_PointLabelsJson;
        private ValueBinding<string> m_CursorJson;
        private ValueBinding<string> m_PointColorHex;
        private ValueBinding<bool> m_SnapToNodes;
        private ValueBinding<bool> m_ForceGroundHeight;
        private ValueBinding<bool> m_LockCurrentHeight;
        private ValueBinding<int> m_MeasureColor;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_MeasureToolSystem = World.GetOrCreateSystemManaged<MeasureToolSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_CameraUpdateSystem = World.GetOrCreateSystemManaged<Game.Rendering.CameraUpdateSystem>();

            m_ToggleToolAction = InputManager.instance.FindAction(Mod.Settings.id, nameof(Setting.ToggleToolBinding));
            m_UndoLastPointAction = InputManager.instance.FindAction(Mod.Settings.id, nameof(Setting.UndoLastPointBinding));
            m_AddPointAction = InputManager.instance.FindAction(Mod.Settings.id, nameof(Setting.AddPointBinding));
            m_ToggleToolAction.shouldBeEnabled = true;
            m_UndoLastPointAction.shouldBeEnabled = true;
            m_AddPointAction.shouldBeEnabled = true;

            AddBinding(m_ToolActive = new ValueBinding<bool>(Group, "toolActive", false));
            AddBinding(m_UnitOfDistance = new ValueBinding<int>(Group, "unitOfDistance", (int)Mod.Settings.UnitOfDistance));
            AddBinding(m_UnitOfHeight = new ValueBinding<int>(Group, "unitOfHeight", (int)Mod.Settings.UnitOfHeight));
            AddBinding(m_UnitOfSlope = new ValueBinding<int>(Group, "unitOfSlope", (int)Mod.Settings.UnitOfSlope));
            AddBinding(m_UnitOfDirection = new ValueBinding<int>(Group, "unitOfDirection", (int)Mod.Settings.UnitOfDirection));
            AddBinding(m_UnitOfArea = new ValueBinding<int>(Group, "unitOfArea", (int)Mod.Settings.UnitOfArea));
            AddBinding(m_SegmentsJson = new ValueBinding<string>(Group, "segmentsJson", "[]"));
            AddBinding(m_SummaryJson = new ValueBinding<string>(Group, "summaryJson", EmptySummaryJson()));
            AddBinding(m_PanelPlaced = new ValueBinding<bool>(Group, "panelPlaced", false));
            AddBinding(m_PanelX = new ValueBinding<float>(Group, "panelX", 0f));
            AddBinding(m_PanelY = new ValueBinding<float>(Group, "panelY", 0f));
            AddBinding(m_PointLabelsJson = new ValueBinding<string>(Group, "pointLabelsJson", "[]"));
            AddBinding(m_CursorJson = new ValueBinding<string>(Group, "cursorJson", "{\"visible\":false,\"x\":0,\"y\":0}"));
            AddBinding(m_PointColorHex = new ValueBinding<string>(Group, "pointColorHex", MeasureMath.ColorHex(Mod.Settings.MeasureColor)));
            AddBinding(m_SnapToNodes = new ValueBinding<bool>(Group, "snapToNodes", Mod.Settings.SnapToNodes));
            AddBinding(m_ForceGroundHeight = new ValueBinding<bool>(Group, "forceGroundHeight", Mod.Settings.ForceGroundHeight));
            AddBinding(m_LockCurrentHeight = new ValueBinding<bool>(Group, "lockCurrentHeight", Mod.Settings.LockCurrentHeight));
            AddBinding(m_MeasureColor = new ValueBinding<int>(Group, "measureColor", (int)Mod.Settings.MeasureColor));

            AddBinding(new TriggerBinding(Group, "toggleTool", ToggleTool));
            AddBinding(new TriggerBinding<string>(Group, "copyRaw", value =>
            {
                GUIUtility.systemCopyBuffer = value ?? string.Empty;
            }));
            AddBinding(new TriggerBinding(Group, "undoLastPoint", () => m_MeasureToolSystem.RemoveLastPoint()));
            AddBinding(new TriggerBinding(Group, "clearPoints", () => m_MeasureToolSystem.ClearPoints()));
            AddBinding(new TriggerBinding<int>(Group, "setUnitOfDistance", value =>
            {
                Mod.Settings.UnitOfDistance = (DistanceUnit)value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<int>(Group, "setUnitOfHeight", value =>
            {
                Mod.Settings.UnitOfHeight = (DistanceUnit)value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<int>(Group, "setUnitOfSlope", value =>
            {
                Mod.Settings.UnitOfSlope = (SlopeUnit)value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<int>(Group, "setUnitOfDirection", value =>
            {
                Mod.Settings.UnitOfDirection = (DirectionUnit)value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<int>(Group, "setUnitOfArea", value =>
            {
                Mod.Settings.UnitOfArea = (AreaUnit)value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<bool>(Group, "setSnapToNodes", value =>
            {
                Mod.Settings.SnapToNodes = value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<bool>(Group, "setForceGroundHeight", value =>
            {
                Mod.Settings.ForceGroundHeight = value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<bool>(Group, "setLockCurrentHeight", value =>
            {
                Mod.Settings.LockCurrentHeight = value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<int>(Group, "setMeasureColor", value =>
            {
                Mod.Settings.MeasureColor = (MeasureColorOption)value;
                Mod.Settings.ApplyAndSave();
            }));
            AddBinding(new TriggerBinding<float, float>(Group, "setPanelPosition", (x, y) =>
            {
                if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y))
                {
                    return;
                }

                x = math.clamp(x, 0f, 1f);
                y = math.clamp(y, 0f, 1f);

                if (IsEditorMode())
                {
                    Mod.Settings.EditorPanelPlaced = true;
                    Mod.Settings.EditorPanelX = x;
                    Mod.Settings.EditorPanelY = y;
                }
                else
                {
                    Mod.Settings.CityPanelPlaced = true;
                    Mod.Settings.CityPanelX = x;
                    Mod.Settings.CityPanelY = y;
                }

                Mod.Settings.ApplyAndSave();
            }));
        }

        private void ToggleTool()
        {
            if (m_ToolSystem.activeTool == m_MeasureToolSystem)
            {
                m_ToolSystem.activeTool = m_DefaultToolSystem;
            }
            else
            {
                m_ToolSystem.activeTool = m_MeasureToolSystem;
            }
        }

        private bool IsEditorMode()
        {
            return (m_ToolSystem.actionMode & GameMode.Editor) != 0;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            bool active = m_ToolSystem.activeTool == m_MeasureToolSystem;

            if (m_ToggleToolAction.WasPressedThisFrame())
            {
                ToggleTool();
                active = m_ToolSystem.activeTool == m_MeasureToolSystem;
            }

            if (active && m_UndoLastPointAction.WasPressedThisFrame())
            {
                m_MeasureToolSystem.RemoveLastPoint();
            }

            if (active && m_AddPointAction.WasPressedThisFrame() && m_MeasureToolSystem.HasCursorPosition)
            {
                m_MeasureToolSystem.AddPoint(m_MeasureToolSystem.CursorPosition);
            }

            m_ToolActive.Update(active);

            m_UnitOfDistance.Update((int)Mod.Settings.UnitOfDistance);
            m_UnitOfHeight.Update((int)Mod.Settings.UnitOfHeight);
            m_UnitOfSlope.Update((int)Mod.Settings.UnitOfSlope);
            m_UnitOfDirection.Update((int)Mod.Settings.UnitOfDirection);
            m_UnitOfArea.Update((int)Mod.Settings.UnitOfArea);
            m_PointColorHex.Update(MeasureMath.ColorHex(Mod.Settings.MeasureColor));
            m_SnapToNodes.Update(Mod.Settings.SnapToNodes);
            m_ForceGroundHeight.Update(Mod.Settings.ForceGroundHeight);
            m_LockCurrentHeight.Update(Mod.Settings.LockCurrentHeight);
            m_MeasureColor.Update((int)Mod.Settings.MeasureColor);

            if (s_ResetPanelPositionRequested)
            {
                Mod.Settings.CityPanelPlaced = false;
                Mod.Settings.EditorPanelPlaced = false;
                Mod.Settings.ApplyAndSave();
                s_ResetPanelPositionRequested = false;
            }

            bool editorMode = IsEditorMode();
            m_PanelPlaced.Update(editorMode ? Mod.Settings.EditorPanelPlaced : Mod.Settings.CityPanelPlaced);
            m_PanelX.Update(editorMode ? Mod.Settings.EditorPanelX : Mod.Settings.CityPanelX);
            m_PanelY.Update(editorMode ? Mod.Settings.EditorPanelY : Mod.Settings.CityPanelY);

            if (!active)
            {
                m_PointLabelsJson.Update("[]");
                m_CursorJson.Update("{\"visible\":false,\"x\":0,\"y\":0}");
                return;
            }

            var points = m_MeasureToolSystem.Points;

            var pointLabelsBuilder = new StringBuilder();
            pointLabelsBuilder.Append('[');

            Camera projectionCamera = m_CameraUpdateSystem.activeCamera;
            if (projectionCamera == null)
            {
                projectionCamera = Camera.main;
            }

            if (projectionCamera != null)
            {
                bool firstLabel = true;

                for (int i = 0; i < points.Count; i++)
                {
                    UnityEngine.Vector3 screenPoint = projectionCamera.WorldToScreenPoint(new UnityEngine.Vector3(points[i].x, points[i].y, points[i].z));

                    bool visible = screenPoint.z > 0f;
                    float screenX = screenPoint.x;
                    float screenY = Screen.height - screenPoint.y;

                    if (!firstLabel)
                    {
                        pointLabelsBuilder.Append(',');
                    }
                    firstLabel = false;

                    pointLabelsBuilder.Append('{');
                    pointLabelsBuilder.Append("\"index\":").Append(i).Append(',');
                    pointLabelsBuilder.Append("\"x\":").Append(screenX.ToString("0.#", CultureInfo.InvariantCulture)).Append(',');
                    pointLabelsBuilder.Append("\"y\":").Append(screenY.ToString("0.#", CultureInfo.InvariantCulture)).Append(',');
                    pointLabelsBuilder.Append("\"visible\":").Append(visible ? "true" : "false");
                    pointLabelsBuilder.Append('}');
                }
            }

            pointLabelsBuilder.Append(']');
            m_PointLabelsJson.Update(pointLabelsBuilder.ToString());

            if (projectionCamera != null && m_MeasureToolSystem.HasCursorPosition)
            {
                float3 cursor = m_MeasureToolSystem.CursorPosition;
                UnityEngine.Vector3 cursorScreen = projectionCamera.WorldToScreenPoint(new UnityEngine.Vector3(cursor.x, cursor.y, cursor.z));
                bool cursorVisible = cursorScreen.z > 0f;
                m_CursorJson.Update("{\"visible\":" + (cursorVisible ? "true" : "false") +
                    ",\"x\":" + cursorScreen.x.ToString("0.#", CultureInfo.InvariantCulture) +
                    ",\"y\":" + (Screen.height - cursorScreen.y).ToString("0.#", CultureInfo.InvariantCulture) + "}");
            }
            else
            {
                m_CursorJson.Update("{\"visible\":false,\"x\":0,\"y\":0}");
            }

            var segmentsBuilder = new StringBuilder();
            segmentsBuilder.Append('[');

            float totalDistance = 0f;
            float totalPlanDistance = 0f;
            float totalAscent = 0f;
            float totalDescent = 0f;
            bool firstRow = true;
            int segmentCount = points.Count > 1 ? points.Count - 1 : 0;

            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                float3 a = points[segmentIndex];
                float3 b = points[segmentIndex + 1];

                float relief = MeasureMath.Relief(a, b);
                float length = MeasureMath.LengthXZ(a, b);
                float distance = MeasureMath.Distance3D(a, b);
                float slope = MeasureMath.SlopeRatio(relief, length);
                float direction = MeasureMath.Direction(a, b);

                totalDistance += distance;
                totalPlanDistance += length;
                if (relief >= 0f)
                {
                    totalAscent += relief;
                }
                else
                {
                    totalDescent -= relief;
                }

                if (!firstRow)
                {
                    segmentsBuilder.Append(',');
                }
                firstRow = false;

                segmentsBuilder.Append('{');
                segmentsBuilder.Append("\"index\":").Append(segmentIndex + 1).Append(',');
                AppendJsonStringProperty(segmentsBuilder, "relief", MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, relief));
                segmentsBuilder.Append(',');
                AppendJsonStringProperty(segmentsBuilder, "distance", MeasureMath.DisplayDistance(Mod.Settings.UnitOfDistance, distance));
                segmentsBuilder.Append(',');
                AppendJsonStringProperty(segmentsBuilder, "slope", MeasureMath.DisplaySlope(Mod.Settings.UnitOfSlope, slope));
                segmentsBuilder.Append(',');
                AppendJsonStringProperty(segmentsBuilder, "direction", MeasureMath.DisplayDirection(Mod.Settings.UnitOfDirection, direction));
                segmentsBuilder.Append('}');
            }

            segmentsBuilder.Append(']');
            m_SegmentsJson.Update(segmentsBuilder.ToString());

            string elevation = points.Count > 0
                ? MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, points[0].y)
                : "0";

            float minElevationMeters = 0f;
            float maxElevationMeters = 0f;
            if (points.Count > 0)
            {
                minElevationMeters = points[0].y;
                maxElevationMeters = points[0].y;
                for (int i = 1; i < points.Count; i++)
                {
                    minElevationMeters = math.min(minElevationMeters, points[i].y);
                    maxElevationMeters = math.max(maxElevationMeters, points[i].y);
                }
            }

            float netElevationMeters = points.Count > 1
                ? points[points.Count - 1].y - points[0].y
                : 0f;
            string netElevation = MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, netElevationMeters);
            string ascent = MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, totalAscent);
            string descent = MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, totalDescent);
            string minElevation = MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, minElevationMeters);
            string maxElevation = MeasureMath.DisplayDistance(Mod.Settings.UnitOfHeight, maxElevationMeters);

            bool areaPreview = m_MeasureToolSystem.IsLastPointNearStart;
            bool areaAvailable = areaPreview;
            float areaSquareMeters = MeasureMath.PolygonAreaXZ(points);
            string area = areaAvailable
                ? MeasureMath.DisplayArea(Mod.Settings.UnitOfArea, areaSquareMeters)
                : "—";

            string totalDistanceStr = MeasureMath.DisplayDistance(Mod.Settings.UnitOfDistance, totalDistance);
            string totalPlanDistanceStr = MeasureMath.DisplayDistance(Mod.Settings.UnitOfDistance, totalPlanDistance);

            string straightLineStr = "0";
            if (points.Count > 1)
            {
                float straightLine = MeasureMath.Distance3D(points[0], points[points.Count - 1]);
                straightLineStr = MeasureMath.DisplayDistance(Mod.Settings.UnitOfDistance, straightLine);
            }

            var summaryBuilder = new StringBuilder();
            summaryBuilder.Append('{');
            summaryBuilder.Append("\"pointCount\":").Append(points.Count).Append(',');
            AppendJsonStringProperty(summaryBuilder, "elevation", elevation);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "netElevation", netElevation);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "totalAscent", ascent);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "totalDescent", descent);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "minElevation", minElevation);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "maxElevation", maxElevation);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "planDistance", totalPlanDistanceStr);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "area", area);
            summaryBuilder.Append(',');
            summaryBuilder.Append("\"areaAvailable\":").Append(areaAvailable ? "true" : "false").Append(',');
            summaryBuilder.Append("\"areaPreview\":").Append(areaPreview ? "true" : "false").Append(',');
            AppendJsonStringProperty(summaryBuilder, "heightUnitSymbol", MeasureMath.UnitSymbol(Mod.Settings.UnitOfHeight));
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "areaUnitSymbol", MeasureMath.UnitSymbol(Mod.Settings.UnitOfArea));
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "totalDistance", totalDistanceStr);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "straightLineDistance", straightLineStr);
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "distanceUnitSymbol", MeasureMath.UnitSymbol(Mod.Settings.UnitOfDistance));
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "slopeUnitSymbol", MeasureMath.UnitSymbol(Mod.Settings.UnitOfSlope));
            summaryBuilder.Append(',');
            AppendJsonStringProperty(summaryBuilder, "directionUnitSymbol", MeasureMath.UnitSymbol(Mod.Settings.UnitOfDirection));
            summaryBuilder.Append('}');

            m_SummaryJson.Update(summaryBuilder.ToString());
        }

        private static string EmptySummaryJson()
        {
            return "{\"pointCount\":0,\"elevation\":\"0\",\"netElevation\":\"0\",\"totalAscent\":\"0\",\"totalDescent\":\"0\",\"minElevation\":\"0\",\"maxElevation\":\"0\",\"planDistance\":\"0\",\"area\":\"—\",\"areaAvailable\":false,\"areaPreview\":false," +
                   "\"heightUnitSymbol\":\"m\",\"areaUnitSymbol\":\"m²\",\"totalDistance\":\"0\",\"straightLineDistance\":\"0\",\"distanceUnitSymbol\":\"m\",\"slopeUnitSymbol\":\"°\",\"directionUnitSymbol\":\"°\"}";
        }

        private static void AppendJsonStringProperty(StringBuilder sb, string key, string value)
        {
            sb.Append('"').Append(key).Append("\":\"");
            AppendJsonEscaped(sb, value ?? string.Empty);
            sb.Append('"');
        }

        private static void AppendJsonEscaped(StringBuilder sb, string value)
        {
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    default:
                        if (c < 0x20)
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
        }
    }
}
