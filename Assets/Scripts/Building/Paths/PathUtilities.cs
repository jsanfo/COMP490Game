using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class PathUtilities
{
    public static Vector3[] CalculateSpacedPoints((Vector3, Vector3, Vector3) pathPoints, bool isCurved, float spacing, float resolution = 1)
    {
        /*
        if (pathPoints.Item1 != Vector3.zero)
        {
            pathPoints.Item1 = new Vector3(pathPoints.Item1.x, meshOffset, pathPoints.Item1.z);
        }

        if (pathPoints.Item2 != Vector3.zero)
        {
            pathPoints.Item2 = new Vector3(pathPoints.Item2.x, meshOffset, pathPoints.Item2.z);
        }

        if (pathPoints.Item3 != Vector3.zero)
        {
            pathPoints.Item3 = new Vector3(pathPoints.Item3.x, meshOffset, pathPoints.Item3.z);
        }
        */

        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(pathPoints.Item1);
        Vector3 previousPoint = pathPoints.Item1;

        float distSinceLastEvenPoint = 0;

        float controlNetLength = Vector3.Distance(pathPoints.Item1, pathPoints.Item2);
        float estimatedLength = Vector3.Distance(pathPoints.Item1, pathPoints.Item2) + controlNetLength / 2f;
        int divisions = Mathf.CeilToInt(estimatedLength * resolution * 10);
        float t = 0;
        while (t <= 1)
        {
            t += 0.1f / divisions;
            Vector3 pointOnCurve;
            if (isCurved)
            {
                pointOnCurve = Bezier.EvaluateQuadratic(pathPoints.Item1, pathPoints.Item3, pathPoints.Item2, t);
            }
            else
            {
                pointOnCurve = Bezier.EvaluateLinear(pathPoints.Item1, pathPoints.Item2, t);
            }
            distSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

            // Calculate overshoot
            while (distSinceLastEvenPoint >= spacing)
            {
                float overshootDist = distSinceLastEvenPoint - spacing;
                Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDist;
                evenlySpacedPoints.Add(newEvenlySpacedPoint);
                distSinceLastEvenPoint = overshootDist;
                previousPoint = newEvenlySpacedPoint;
            }

            previousPoint = pointOnCurve;
        }

        // Adds the last point (clicked endpoint) to the array
        evenlySpacedPoints.Add(pathPoints.Item2);

        return evenlySpacedPoints.ToArray();
    }

    public static Mesh CreateMesh(Vector3[] spacedPoints, float pathWidth)
    {
        // Vertice and UV arrays
        Vector3[] vertices = new Vector3[spacedPoints.Length * 2];
        Vector2[] uvs = new Vector2[vertices.Length];

        // Triangle array
        int[] triangles = new int[2 * (spacedPoints.Length - 1) * 3];

        // Calculate triangles
        #region CalculateTriangles
        int vertIndex = 0, triIndex = 0;
        for (int i = 0; i < spacedPoints.Length; i++)
        {
            // Get forward direction
            #region CalculateForward
            Vector3 forward = Vector3.zero;

            // Handle points before last point
            if (i < spacedPoints.Length - 1)
            {
                forward += spacedPoints[i + 1] - spacedPoints[i];
            }

            // Handle points after first point
            if (i > 0)
            {
                forward += spacedPoints[i] - spacedPoints[i - 1];
            }

            forward.Normalize();
            #endregion

            // Get left direction
            #region CalculateLeft
            // Swap x, y | multiply x by -1 for left vector
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);
            #endregion

            // Adds 2 vertices to the left and right of the spaced point
            vertices[vertIndex] = spacedPoints[i] + left * pathWidth * 0.5f;        // Left vertex
            vertices[vertIndex + 1] = spacedPoints[i] - left * pathWidth * 0.5f;    // Right vertex

            // Calculate UVs
            float completePercent = i / (float)spacedPoints.Length - 1;
            uvs[vertIndex] = new Vector2(0, completePercent);
            uvs[vertIndex + 1] = new Vector2(1, completePercent);

            // Check if point is not the last point
            if (i < spacedPoints.Length - 1)
            {
                // Create triangles CW
                // Get first triangle vertex indices
                triangles[triIndex + 2] = vertIndex + 2;
                triangles[triIndex + 1] = vertIndex + 1;
                triangles[triIndex] = vertIndex;

                // Get second triangle vertex indices
                triangles[triIndex + 5] = vertIndex + 1;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 3] = vertIndex + 3;
                
                /*
                // Create triangles CCW
                // Get first triangle vertex indices
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 1] = vertIndex + 1;
                triangles[triIndex + 2] = vertIndex + 2;

                // Get second triangle vertex indices
                triangles[triIndex + 3] = vertIndex + 3;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 5] = vertIndex + 1;
                */
            }

            vertIndex += 2;
            triIndex += 6;
        }
        #endregion

        // Construct mesh
        #region ConstructMesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        #endregion

        return mesh;
    }

    public static bool CheckForCollision(GameObject builderGameObject, string objectWithCollisions, int skipTo = 0, int skipFromEnd = 0)
    {
        Transform collisionsTransform = builderGameObject.transform.Find(objectWithCollisions);
        if (collisionsTransform == null) return false;

        if (skipTo > collisionsTransform.childCount - skipFromEnd) return false;
        if (skipFromEnd > collisionsTransform.childCount - skipFromEnd) return false;

        for (int i = skipTo; i < collisionsTransform.childCount - skipFromEnd; i++)
        {
            if (collisionsTransform.GetChild(i).GetComponent<PathColliderTrigger>().isPathCollision == true) return true;
        }

        return false;
    }

    public static void CreateCollider(string colliderName, Transform collisionHolder, Vector3 position, bool isTrigger)
    {
        // Create collider objects
        GameObject collider = new GameObject(colliderName);
        collider.transform.SetParent(collisionHolder.transform);
        collider.transform.position = position;

        // Add collision components
        collider.layer = LayerMask.NameToLayer("Ignore Raycast");
        SphereCollider sphereCollider = collider.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = isTrigger;
        Rigidbody rigidBody = collider.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        collider.AddComponent<PathColliderTrigger>();
    }
}
