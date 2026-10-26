using System;
using Unity.Mathematics;

namespace MeasureItCS2
{
    public static class MeasureMath
    {
        private const float MetersPerFoot = 0.3048f;
        private const float MetersPerMile = 1609.344f;
        private const float MetersPerKilometer = 1000f;
        private const float SquareMetersPerSquareFoot = 0.09290304f;
        private const float SquareMetersPerAcre = 4046.8564224f;

        private static readonly string[] Cardinals =
        {
            "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N"
        };

        public static float ConvertDistance(DistanceUnit unit, float meters)
        {
            switch (unit)
            {
                case DistanceUnit.Meters:
                    return meters;
                case DistanceUnit.Kilometers:
                    return meters / MetersPerKilometer;
                case DistanceUnit.Feet:
                    return meters / MetersPerFoot;
                case DistanceUnit.Miles:
                    return meters / MetersPerMile;
                default:
                    return meters;
            }
        }

        public static string UnitSymbol(DistanceUnit unit)
        {
            switch (unit)
            {
                case DistanceUnit.Meters: return "m";
                case DistanceUnit.Kilometers: return "km";
                case DistanceUnit.Feet: return "ft";
                case DistanceUnit.Miles: return "mi";
                default: return "?";
            }
        }

        public static string UnitSymbol(SlopeUnit unit)
        {
            return unit == SlopeUnit.Degree ? "°" : "%";
        }

        public static string UnitSymbol(DirectionUnit unit)
        {
            return unit == DirectionUnit.Degree ? "°" : "cardinal";
        }

        public static float ConvertArea(AreaUnit unit, float squareMeters)
        {
            switch (unit)
            {
                case AreaUnit.Hectares: return squareMeters / 10000f;
                case AreaUnit.SquareKilometers: return squareMeters / 1000000f;
                case AreaUnit.SquareFeet: return squareMeters / SquareMetersPerSquareFoot;
                case AreaUnit.Acres: return squareMeters / SquareMetersPerAcre;
                default: return squareMeters;
            }
        }

        public static string UnitSymbol(AreaUnit unit)
        {
            switch (unit)
            {
                case AreaUnit.Hectares: return "ha";
                case AreaUnit.SquareKilometers: return "km²";
                case AreaUnit.SquareFeet: return "ft²";
                case AreaUnit.Acres: return "ac";
                default: return "m²";
            }
        }

        public static string DisplayDistance(DistanceUnit unit, float meters)
        {
            return Round(ConvertDistance(unit, meters)).ToString("0.##");
        }

        public static string DisplayArea(AreaUnit unit, float squareMeters)
        {
            return Round(ConvertArea(unit, squareMeters)).ToString("0.##");
        }

        public static float PolygonAreaXZ(System.Collections.Generic.IReadOnlyList<float3> points)
        {
            if (points == null || points.Count < 3)
            {
                return 0f;
            }

            double twiceArea = 0d;
            for (int i = 0; i < points.Count; i++)
            {
                float3 current = points[i];
                float3 next = points[(i + 1) % points.Count];
                twiceArea += (double)current.x * next.z - (double)next.x * current.z;
            }

            return (float)(Math.Abs(twiceArea) * 0.5d);
        }

        public static float ConvertSlope(SlopeUnit unit, float slopeRatio)
        {
            return unit == SlopeUnit.Degree
                ? math.degrees(math.atan(slopeRatio))
                : slopeRatio * 100f;
        }

        public static string DisplaySlope(SlopeUnit unit, float slopeRatio)
        {
            return Round(ConvertSlope(unit, slopeRatio)).ToString("0.##");
        }

        public static string DisplayDirection(DirectionUnit unit, float directionDegrees)
        {
            if (unit == DirectionUnit.Point)
            {
                int index = (int)math.round((directionDegrees % 360f) / 45f);
                return Cardinals[index];
            }

            return Round(directionDegrees).ToString("0.##");
        }

        public static float LengthXZ(float3 a, float3 b)
        {
            float2 d = new float2(b.x - a.x, b.z - a.z);
            return math.length(d);
        }

        public static float Distance3D(float3 a, float3 b)
        {
            return math.distance(a, b);
        }

        public static float Relief(float3 a, float3 b)
        {
            return b.y - a.y;
        }

        public static float SlopeRatio(float relief, float lengthXZ)
        {
            return lengthXZ > 0f ? relief / lengthXZ : 0f;
        }

        public static float Direction(float3 from, float3 to)
        {
            float2 a = new float2(0f, -1f);
            float2 b = new float2(to.x - from.x, to.z - from.z);

            if (math.lengthsq(b) < 1e-8f)
            {
                return 0f;
            }

            float angle = math.degrees(math.acos(math.clamp(math.dot(math.normalize(a), math.normalize(b)), -1f, 1f)));
            float cross = a.x * b.y - a.y * b.x;

            if (cross > 0f)
            {
                angle = 360f - angle;
            }

            return angle;
        }

        private static float Round(float value)
        {
            return (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public static string ColorHex(MeasureColorOption option)
        {
            switch (option)
            {
                case MeasureColorOption.Yellow: return "#FFD900";
                case MeasureColorOption.Magenta: return "#FF00FF";
                case MeasureColorOption.Red: return "#FF2626";
                case MeasureColorOption.Green: return "#33FF33";
                case MeasureColorOption.Cyan: return "#00E5FF";
                case MeasureColorOption.White: return "#FFFFFF";
                case MeasureColorOption.Orange: return "#FF8000";
                default: return "#FFD900";
            }
        }
    }
}
