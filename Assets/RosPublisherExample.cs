using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

/// <summary>
///
/// </summary>
public class RosPublisherExample : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "pos_rot";
    public string testingTopicName = "testing";

    // The game object
    public GameObject cube;
    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.5f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    int count = 0;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROS Connection is not set.");
            return;
        }
        ros.RegisterPublisher<PosRotMsg>(topicName);
        ros.RegisterPublisher<TestingMsg>(testingTopicName);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (count > 77797)
        {
            count = 0;
        }
        else
        {
            count++;
        }

        if (timeElapsed > publishMessageFrequency)
        {
            cube.transform.rotation = Random.rotation;

            PosRotMsg cubePos = new PosRotMsg(
                cube.transform.position.x,
                cube.transform.position.y,
                cube.transform.position.z,
                cube.transform.rotation.x,
                cube.transform.rotation.y,
                cube.transform.rotation.z,
                cube.transform.rotation.w
            );

            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cubePos);
            ros.Publish(testingTopicName, new TestingMsg(count));
            timeElapsed = 0;
            Debug.Log("Count: " + count);
        }
    }
}