using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeroicEngine.Utils.Math;
using System;

namespace HeroicEngine.Utils
{
    public struct VertexData
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Normal;
        public bool Side;
    }

    public enum FractureMode
    {
        VerticalOnly = 0,
        HorizontalOnly = 1,
        VerticalAndHorizontal = 2,
        RandomAngles = 3
    }

    public static class MeshUtils
    {
        public static bool GetPlaneIntersectionPoint(Vector3 from, Vector3 to, Vector3 planeOrigin, Vector3 normal, out Vector3 result)
        {
            Vector3 translation = to - from;
            float dot = Vector3.Dot(normal, translation);

            if (Mathf.Abs(dot) > float.Epsilon)
            {
                Vector3 fromOrigin = from - planeOrigin;
                float fac = -Vector3.Dot(normal, fromOrigin) / dot;
                translation = translation * fac;
                result = from + translation;
                return true;
            }

            result = Vector3.zero;
            return false;
        }

        /// <summary>
        /// This method fractures given object into certain count of fragments, with certain fracture mode.
        /// </summary>
        /// <param name="origin">Original gameobject with mesh</param>
        /// <param name="fragmentsCount">Needed fragments count</param>
        /// <param name="fractureMode">Fracture mode</param>
        /// <returns>Fragments</returns>
        public static GameObject[] Fracture(GameObject origin, int fragmentsCount, FractureMode fractureMode)
        {
            if (fragmentsCount < 2)
            {
                return new GameObject[1] { origin };
            }

            Material mat = origin.GetComponent<MeshRenderer>().material;

            List<Mesh> allFragments = new List<Mesh>();

            Vector3 sliceNormal = UnityEngine.Random.onUnitSphere;

            switch (fractureMode)
            {
                case FractureMode.VerticalOnly:
                    sliceNormal = Vector3.right;
                    break;
                case FractureMode.HorizontalOnly:
                    sliceNormal = Vector3.up;
                    break;
            }

            Mesh sliceObject = origin.GetComponent<MeshFilter>().mesh;
            Vector3 originPos = origin.transform.position;
            Quaternion originRot = origin.transform.rotation;

            for (int i = 0; i < fragmentsCount - 1; i++)
            {
                switch (fractureMode)
                {
                    case FractureMode.VerticalAndHorizontal:
                        sliceNormal = UnityEngine.Random.value > 0.5f ? Vector3.right : Vector3.up;
                        break;
                    case FractureMode.RandomAngles:
                        sliceNormal = UnityEngine.Random.onUnitSphere;
                        break;
                }

                if (allFragments.Count > 0)
                {
                    sliceObject = allFragments.GetRandomElement();
                }

                allFragments.Remove(sliceObject);
                allFragments.AddRange(Slice(sliceObject, sliceObject.bounds.center, sliceNormal));
            }

            List<GameObject> resultObjects = new List<GameObject>();

            foreach (Mesh frag in allFragments)
            {
                resultObjects.Add(CreateGameObjectFromMesh(frag, mat, originPos, originRot, $"{origin.name} Part", origin));
            }

            UnityEngine.Object.Destroy(origin);

            return resultObjects.ToArray();
        }

        private static GameObject CreateGameObjectFromMesh(Mesh mesh, Material material, Vector3 worldPos, Quaternion worldRot, string name, GameObject originGO = null)
        {
            GameObject newGO = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            newGO.transform.SetPositionAndRotation(worldPos, worldRot);
            newGO.GetComponent<MeshFilter>().sharedMesh = mesh;
            newGO.name = name;

            MeshRenderer meshRenderer = newGO.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            Bounds partBounds = newGO.GetComponent<MeshRenderer>().bounds;
            float partVolume = partBounds.size.x * partBounds.size.y * partBounds.size.z;

            if (originGO != null)
            {
                Bounds originBounds = originGO.GetComponent<MeshRenderer>().bounds;
                float originVolume = originBounds.size.x * originBounds.size.y * originBounds.size.z;

                if (originGO.TryGetComponent(out Rigidbody rb))
                {
                    Rigidbody body = newGO.AddComponent<Rigidbody>();
                    body.GetCopyOf(rb);
                    body.mass = rb.mass * partVolume / originVolume;
                }

                if (originGO.TryGetComponent(out Collider collider))
                {
                    MeshCollider col = newGO.AddComponent<MeshCollider>();
                    col.isTrigger = collider.isTrigger;
                    col.convex = true;
                }
            }

            return newGO;
        }

        private static Mesh[] Slice(Mesh mesh, Vector3 cutOrigin, Vector3 cutNormal)
        {
            Plane plane = new Plane(cutNormal, cutOrigin);

            MeshContructionHelper positiveHelper = new MeshContructionHelper();
            MeshContructionHelper negativeHelper = new MeshContructionHelper();

            List<VertexData> pointsAlongPlane = new List<VertexData>();

            int[] meshTriangles = mesh.triangles;
            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                VertexData vertexA = GetVertexData(mesh, plane, meshTriangles[i]);
                VertexData vertexB = GetVertexData(mesh, plane, meshTriangles[i + 1]);
                VertexData vertexC = GetVertexData(mesh, plane, meshTriangles[i + 2]);

                bool isABSameSide = vertexA.Side == vertexB.Side;
                bool isBCSameSide = vertexB.Side == vertexC.Side;

                if (isABSameSide && isBCSameSide)
                {
                    MeshContructionHelper helper = vertexA.Side ? positiveHelper : negativeHelper;
                    helper.AddMeshSection(vertexA, vertexB, vertexC);
                }
                else
                {
                    VertexData intersectionD;
                    VertexData intersectionE;

                    MeshContructionHelper helperA = vertexA.Side ? positiveHelper : negativeHelper;
                    MeshContructionHelper helperB = vertexB.Side ? positiveHelper : negativeHelper;
                    MeshContructionHelper helperC = vertexC.Side ? positiveHelper : negativeHelper;

                    if (isABSameSide)
                    {
                        intersectionD = GetIntersectionVertex(vertexA, vertexC, cutOrigin, cutNormal);
                        intersectionE = GetIntersectionVertex(vertexB, vertexC, cutOrigin, cutNormal);

                        helperA.AddMeshSection(vertexA, vertexB, intersectionE);
                        helperA.AddMeshSection(vertexA, intersectionE, intersectionD);
                        helperC.AddMeshSection(intersectionE, vertexC, intersectionD);
                    }
                    else if (isBCSameSide)
                    {
                        intersectionD = GetIntersectionVertex(vertexB, vertexA, cutOrigin, cutNormal);
                        intersectionE = GetIntersectionVertex(vertexC, vertexA, cutOrigin, cutNormal);

                        helperB.AddMeshSection(vertexB, vertexC, intersectionE);
                        helperB.AddMeshSection(vertexB, intersectionE, intersectionD);
                        helperA.AddMeshSection(intersectionE, vertexA, intersectionD);
                    }
                    else
                    {
                        intersectionD = GetIntersectionVertex(vertexA, vertexB, cutOrigin, cutNormal);
                        intersectionE = GetIntersectionVertex(vertexC, vertexB, cutOrigin, cutNormal);

                        helperA.AddMeshSection(vertexA, intersectionE, vertexC);
                        helperA.AddMeshSection(intersectionD, intersectionE, vertexA);
                        helperB.AddMeshSection(vertexB, intersectionE, intersectionD);
                    }

                    pointsAlongPlane.Add(intersectionD);
                    pointsAlongPlane.Add(intersectionE);
                }
            }

            JoinPointsAlongPlane(ref positiveHelper, ref negativeHelper, cutNormal, pointsAlongPlane);

            return new[] { positiveHelper.ConstructMesh(), negativeHelper.ConstructMesh() };
        }

        /// <summary>
        /// This method slices given mesh into 2 parts, saving all physical characteristics and collider (if was presented on original gameobject)
        /// </summary>
        /// <param name="origin">Original gameobject with mesh</param>
        /// <param name="cutOrigin">Point which slicing plane should pass (in local object's space)</param>
        /// <param name="cutNormal">Normal of cutting</param>
        /// <returns>Separate parts</returns>
        public static GameObject[] Slice(GameObject origin, Vector3 cutOrigin, Vector3 cutNormal)
        {
            if (!origin.TryGetComponent(out MeshFilter meshFilter))
            {
                return null;
            }

            if (!origin.TryGetComponent(out MeshRenderer meshRenderer))
            {
                return null;
            }

            var submeshes = Slice(meshFilter.sharedMesh, cutOrigin, cutNormal);

            var submesh1 = CreateGameObjectFromMesh(submeshes[0], meshRenderer.material, origin.transform.position, origin.transform.rotation, $"{origin.name} Part", origin);
            var submesh2 = CreateGameObjectFromMesh(submeshes[1], meshRenderer.material, origin.transform.position, origin.transform.rotation, $"{origin.name} Part", origin);

            UnityEngine.Object.Destroy(origin);

            return new[] { submesh1, submesh2 };
        }

        private static VertexData GetVertexData(Mesh mesh, Plane plane, int index)
        {
            Vector3 position = mesh.vertices[index];
            VertexData vertexData = new VertexData()
            {
                Position = position,
                Side = plane.GetSide(position),
                UV = mesh.uv[index],
                Normal = mesh.normals[index]
            };
            return vertexData;
        }

        private static VertexData GetIntersectionVertex(VertexData vertexA, VertexData vertexB, Vector3 planeOrigin, Vector3 normal)
        {
            GetPlaneIntersectionPoint(vertexA.Position, vertexB.Position, planeOrigin, normal, out Vector3 result);
            float distanceA = Vector3.Distance(vertexA.Position, result);
            float distanceB = Vector3.Distance(vertexB.Position, result);
            float t = distanceA / (distanceA + distanceB);

            return new VertexData()
            {
                Position = result,
                Normal = normal,
                UV = Vector2.Lerp(vertexA.UV, vertexB.UV, t)
            };
        }

        private static Vector3 ComputeNormal(VertexData vertexA, VertexData vertexB, VertexData vertexC)
        {
            Vector3 sideL = vertexB.Position - vertexA.Position;
            Vector3 sideR = vertexC.Position - vertexA.Position;

            Vector3 normal = Vector3.Cross(sideL, sideR);

            return normal.normalized;
        }

        private static Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
        {
            if (pointsAlongPlane.Count > 0)
            {
                Vector3 firstPoint = pointsAlongPlane[0].Position;
                Vector3 furthestPoint = Vector3.zero;
                float distance = 0f;

                for (int index = 0; index < pointsAlongPlane.Count; index++)
                {
                    Vector3 point = pointsAlongPlane[index].Position;
                    float currentDistance = Vector3.Distance(firstPoint, point);

                    if (currentDistance > distance)
                    {
                        distance = currentDistance;
                        furthestPoint = point;
                    }
                }

                return Vector3.Lerp(firstPoint, furthestPoint, 0.5f);
            }
            else
            {
                return Vector3.zero;
            }
        }

        private static void JoinPointsAlongPlane(ref MeshContructionHelper positive, ref MeshContructionHelper negative, Vector3 cutNormal, List<VertexData> pointsAlongPlane)
        {
            VertexData halfway = new VertexData()
            {
                Position = GetHalfwayPoint(pointsAlongPlane)
            };

            for (int i = 0; i < pointsAlongPlane.Count; i += 2)
            {
                VertexData firstVertex = pointsAlongPlane[i];
                VertexData secondVertex = pointsAlongPlane[i + 1];

                Vector3 normal = ComputeNormal(halfway, secondVertex, firstVertex);
                halfway.Normal = Vector3.forward;

                float dot = Vector3.Dot(normal, cutNormal);

                if (dot > 0)
                {
                    positive.AddMeshSection(firstVertex, secondVertex, halfway);
                    negative.AddMeshSection(secondVertex, firstVertex, halfway);
                }
                else
                {
                    negative.AddMeshSection(firstVertex, secondVertex, halfway);
                    positive.AddMeshSection(secondVertex, firstVertex, halfway);
                }
            }
        }
    }

    class MeshContructionHelper
    {
        private List<Vector3> _vertices;
        private List<int> _triangles;
        private List<Vector2> _uvs;
        private List<Vector3> _normals;

        public MeshContructionHelper()
        {
            _triangles = new List<int>();
            _vertices = new List<Vector3>();
            _uvs = new List<Vector2>();
            _normals = new List<Vector3>();
        }

        public Mesh ConstructMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = _vertices.ToArray();
            mesh.triangles = _triangles.ToArray();
            mesh.normals = _normals.ToArray();
            mesh.uv = _uvs.ToArray();
            return mesh;
        }

        public void AddMeshSection(VertexData vertexA, VertexData vertexB, VertexData vertexC)
        {
            int indexA = TryAddVertex(vertexA);
            int indexB = TryAddVertex(vertexB);
            int indexC = TryAddVertex(vertexC);

            AddTriangle(indexA, indexB, indexC);
        }


        private void AddTriangle(int indexA, int indexB, int indexC)
        {
            _triangles.Add(indexA);
            _triangles.Add(indexB);
            _triangles.Add(indexC);
        }

        private int TryAddVertex(VertexData vertex)
        {
            _vertices.Add(vertex.Position);
            _uvs.Add(vertex.UV);
            _normals.Add(vertex.Normal);
            return _vertices.Count - 1;
        }
    }
}