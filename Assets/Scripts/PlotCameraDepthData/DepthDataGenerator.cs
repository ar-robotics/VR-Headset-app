using System;
using System.Collections.Generic;

public class DepthDataGenerator
{
    private const int Width = 640; // Width of the depth camera resolution
    private const int Height = 480; // Height of the depth camera resolution
    private const float MaxDepth = 5.0f; // Maximum depth in meters
    private const float MinDepth = 0.5f; // Minimum depth in meters

    private MeshCreator meshCreator;
    // Method to generate test depth data
    public static CameraDepthData GenerateDepthData()
    {
        var depthData = new CameraDepthData
        {
            time = Environment.TickCount // Use system tick count as a timestamp
        };

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Normalize the x, y positions to simulate a real-world field of view
                float realX = (x - Width / 2) * (6.4f / Width);  // Center x around 0 and scale to real-world meters
                float realY = (y - Height / 2) * (4.8f / Height); // Center y around 0 and scale to real-world meters

                // Generate a random depth between MinDepth and MaxDepth
                float depth = MinDepth + (float)(new Random().NextDouble() * (MaxDepth - MinDepth));

                // Add the generated point to the dataset
                depthData.depthData.Add(new Vector3Data(realX, realY, depth));
            }
        }

        return depthData;
    }
}
