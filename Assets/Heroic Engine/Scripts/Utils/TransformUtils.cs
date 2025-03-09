using UnityEngine;

namespace HeroicEngine.Utils
{
    public static class TransformUtils
    {
        /// <summary>
        /// This method returns distance between two transforms.
        /// </summary>
        /// <param name="from">From transform</param>
        /// <param name="to">To transform</param>
        /// <returns>Distance between transforms</returns>
        public static float Distance(this Transform from, Transform to)
        {
            return (from.position - to.position).magnitude;
        }

        /// <summary>
        /// This method returns 2D distance between two given 3D transforms (in XZ plane, ignoring Y).
        /// </summary>
        /// <param name="from">From transform</param>
        /// <param name="to">To transform</param>
        /// <returns>XZ distance between transforms</returns>
        public static float DistanceXZ(this Transform from, Transform to)
        {
            Vector3 toPos = to.position;
            toPos.y = from.position.y;
            return (from.position - toPos).magnitude;
        }

        /// <summary>
        /// This method moves transform towards certain transform with given speed.
        /// </summary>
        /// <param name="t">From transform</param>
        /// <param name="target">Target transform</param>
        /// <param name="speed">Movement speed</param>
        public static void MoveTowards(this Transform t, Transform target, float speed)
        {
            t.position = Vector3.MoveTowards(t.position, target.position, speed * Time.deltaTime);
        }

        /// <summary>
        /// This method resets position, rotation and scale of given transform.
        /// </summary>
        /// <param name="t">Given transform</param>
        public static void ResetTransform(this Transform t)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// This method faces transform to the given transform, ignoring Y axis.
        /// It could be useful for top-down games.
        /// </summary>
        /// <param name="t">Given transform</param>
        /// <param name="target">Target transform</param>
        public static void LookAtIgnoreY(this Transform t, Transform target)
        {
            Vector3 direction = target.position - t.position;
            direction.y = 0;  // Ignore Y-axis
            t.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// This method searches a child Transform by its name even if this child is deeply nested.
        /// </summary>
        /// <param name="parent">Parent transform</param>
        /// <param name="name">Name of child</param>
        /// <returns>Found Transform</returns>
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = child.FindDeepChild(name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// This method sets the same scale for all axes.
        /// </summary>
        /// <param name="t">Given transform</param>
        /// <param name="scale">Given scale</param>
        public static void SetUniformScale(this Transform t, float scale)
        {
            t.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// This method checks if given transform visible for certain camera.
        /// </summary>
        /// <param name="t">Given transform</param>
        /// <param name="camera">Given camera</param>
        /// <returns>true, if this transform visible from given camera</returns>
        public static bool IsVisibleFrom(this Transform t, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Collider collider = t.GetComponent<Collider>();
            if (collider)
                return GeometryUtility.TestPlanesAABB(planes, collider.bounds);
            return false;
        }

        /// <summary>
        /// This method destroys all nested children of given transform.
        /// </summary>
        /// <param name="t">Given transform</param>
        public static void DestroyAllChildren(this Transform t)
        {
            foreach (Transform child in t)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }
}