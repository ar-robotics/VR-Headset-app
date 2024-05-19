using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MeshCreator : MonoBehaviour
{
    // Start is called before the first frame update

    public Volume volume;
    public Gradient depthColorGradient; // Assign this gradient in the Unity editor

    void Start()
    {
        depthColorGradient = new Gradient();

        // Create gradient keys
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = Color.blue;
        colorKeys[0].time = 0.0f; // Start of the gradient
        colorKeys[1].color = Color.red;
        colorKeys[1].time = 1.0f; // End of the gradient

        // Create alpha keys
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;

        // Assign keys to the gradient
        depthColorGradient.SetKeys(colorKeys, alphaKeys);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 1000 == 0)
        {
            clearVolumeBoxes();
        }
    }

    public void CreateCubeGridTest(DepthData depthData)
    {
        clearVolumeBoxes();
        depthData = NormalizeDepthData(depthData); // Normalize the depth data  
        float localScale = 0.03578f;
        // Create a cube grid

        foreach (DepthDataPoint point in depthData.Points)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.parent = volume.transform;
            quad.transform.localPosition = new Vector3(point.X, point.Y, point.Z);
            quad.transform.localScale = new Vector3(localScale, localScale, 1);  // Note that the z-scale is irrelevant for quads.
            quad.GetComponent<Renderer>().material.color = depthColorGradient.Evaluate(point.Z);
        }
    }

    void clearVolumeBoxes()
    {
        foreach (Transform child in volume.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    void CreateMeshInBoxVolume(DepthData depthData)
    {
        depthData = NormalizeDepthData(depthData); // Normalize the depth data
        Mesh mesh = new Mesh();

        // Create vertices array
        Vector3[] vertices = new Vector3[depthData.Count];
        int[] triangles = new int[(depthData.Count - 2) * 3]; // Only valid if you have at least 3 points

        // Fill vertices array
        int i = 0;
        foreach (DepthDataPoint point in depthData.Points)
        {
            vertices[i++] = new Vector3(point.X, point.Y, point.Z);
        }

        // Assuming you want to create a mesh from vertices like a connected series of triangles
        if (depthData.Count >= 3)
        {
            for (int j = 0; j < depthData.Count - 2; j++)
            {
                triangles[j * 3] = 0;
                triangles[j * 3 + 1] = j + 1;
                triangles[j * 3 + 2] = j + 2;
            }
        }

        // Set the mesh vertices and triangles
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // To make sure the mesh is rendered correctly

        // Assign the mesh to a MeshFilter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    DepthData NormalizeDepthData(DepthData depthData)
    {
        // Optional: Define min and max depth for color normalization
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        // Find the minimum and maximum values for each axis
        foreach (DepthDataPoint point in depthData.Points)
        {
            minX = Mathf.Min(minX, point.X);
            maxX = Mathf.Max(maxX, point.X);
            minY = Mathf.Min(minY, point.Y);
            maxY = Mathf.Max(maxY, point.Y);
            minZ = Mathf.Min(minZ, point.Z);
            maxZ = Mathf.Max(maxZ, point.Z);
        }

        DepthData normalizedDepthData = new DepthData();

        // Volume scales
        float scaleX = 1f;
        float scaleY = 1f;
        float scaleZ = 1f;

        // Normalize the depth data to the volume's scales
        foreach (DepthDataPoint point in depthData.Points)
        {
            float normalizedX = Mathf.InverseLerp(minX, maxX, point.X) * scaleX - scaleX / 2;
            float normalizedY = Mathf.InverseLerp(minY, maxY, point.Y) * scaleY - scaleY / 2;
            float normalizedZ = Mathf.InverseLerp(minZ, maxZ, point.Z) * scaleZ - scaleZ / 2;
            normalizedDepthData.AddDepthDataPoint(normalizedX, normalizedY, normalizedZ);
        }

        return normalizedDepthData;
    }
}
