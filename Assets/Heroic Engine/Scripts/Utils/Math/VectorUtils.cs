using UnityEngine;

namespace HeroicEngine.Utils.Math
{
    public static class VectorUtils
    {
        /// <summary>
        /// This method returns 2D distance between two given 3D positions (in XZ plane, ignoring Y).
        /// </summary>
        /// <param name="from">From v</param>
        /// <param name="to">To position</param>
        /// <returns>XZ distance between positions</returns>
        public static float DistanceZX(Vector3 from, Vector3 to)
        {
            from.y = to.y;
            return Vector3.Distance(from, to);
        }

        /// <summary>
        /// This method returns distance between two positions.
        /// </summary>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <returns>Distance between positions</returns>
        public static float Distance(this Vector3 from, Vector3 to)
        {
            return (from - to).magnitude;
        }

        /// <summary>
        /// This method returns 2D distance between two given 3D positions (in XZ plane, ignoring Y).
        /// </summary>
        /// <param name="from">From v</param>
        /// <param name="to">To position</param>
        /// <returns>XZ distance between positions</returns>
        public static float DistanceXZ(this Vector3 from, Vector3 to)
        {
            to.y = from.y;
            return (from - to).magnitude;
        }

        /// <summary>
        /// This method generates random position inside of cube represented by two given positions (min and max).
        /// </summary>
        /// <param name="minPosition">Min position</param>
        /// <param name="maxPosition">max position</param>
        /// <returns>Random point inside of cube represented by min and max positions</returns>
        public static Vector3 GetRandomPosition(Vector3 minPosition, Vector3 maxPosition)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minPosition.x, maxPosition.x),
                Random.Range(minPosition.y, maxPosition.y),
                Random.Range(minPosition.z, maxPosition.z)
            );

            return randomPosition;
        }

        /// <summary>
        /// This method returns the closest point on given mesh, from certain world point.
        /// </summary>
        /// <param name="meshFilter">Given mesh</param>
        /// <param name="worldPoint">World point</param>
        /// <returns>Closest point on given mesh</returns>
        public static Vector3 ClosestPointOnMeshOBB(MeshFilter meshFilter, Vector3 worldPoint)
        {
            // First, we transform the point into the local coordinate space of the mesh.
            var localPoint = meshFilter.transform.InverseTransformPoint(worldPoint);

            // Next, we compare it against the mesh's axis-aligned bounds in its local space.
            var localClosest = meshFilter.sharedMesh.bounds.ClosestPoint(localPoint);

            // Finally, we transform the local point back into world space.
            return meshFilter.transform.TransformPoint(localClosest);
        }

        /// <summary>
        /// This method helps to detect if certain transform situated inside the given cone.
        /// It could be useful for enemies detection by AI bots.
        /// </summary>
        /// <param name="targetObj">Target object</param>
        /// <param name="lookFromTransform">From what transform we are looking</param>
        /// <param name="coneAngle">Cone angle in degrees</param>
        /// <returns>true, if target object is in cone</returns>
        public static bool IsObjectInCone(Transform targetObj, Transform lookFromTransform, float coneAngle)
        {
            Vector3 dir = (targetObj.position - lookFromTransform.position).normalized;

            Vector3 localDir = lookFromTransform.InverseTransformDirection(dir);

            if (localDir.z > 0f && Mathf.Atan2(localDir.z, localDir.x) < Mathf.Deg2Rad * coneAngle)
            {
                return true;
            }

            return false;
        }
    }
}