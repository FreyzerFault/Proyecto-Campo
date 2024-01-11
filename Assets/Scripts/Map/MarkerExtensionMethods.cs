using UnityEngine;

namespace Map
{
    public static class MarkerExtensionMethods
    {
        // Compara 2 markers, si estan a menos de maxDeltaRadius de distancia, devuelve true
        public static bool DistanceTo(this MapMarkerData marker, MapMarkerData other, float maxDeltaRadius = 0.1f)
        {
            return Vector2.Distance(marker.normalizedPosition, other.normalizedPosition) < maxDeltaRadius;
        }

        public static bool IsAtPoint(this MapMarkerData marker, Vector2 normalizedPos, float maxDeltaRadius = 0.1f)
        {
            return Vector2.Distance(marker.normalizedPosition, normalizedPos) < maxDeltaRadius;
        }

        public static float DistanceTo(this MapMarkerData marker, Vector2 normalizedPos)
        {
            return Vector2.Distance(marker.normalizedPosition, normalizedPos);
        }

        public static float DistanceTo(this MapMarkerData marker, Vector3 globalPos)
        {
            return Vector3.Distance(marker.worldPosition, globalPos);
        }
    }
}