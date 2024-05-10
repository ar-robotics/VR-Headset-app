

using UnityEngine;

//  {
//                 "rotation": 170,  Y: (0: 180) Link_1
//                 "shoulder": 90,   X: (Link_2) 0 = 75.047  |  90 = -13 | 180 = -103.146
//                 "elbow": 80,    Y: Link_3  0 = 239.297    |  90 = 150.859 | 180 = 62.422
//                 "tilt": 70,    Y_ Link_4  0 = -88.106   |  90 = -177.285 | 180 = 88.106
//                 "wrist": 60,   X: Link_5 100 = 0        0 = 90  270 = -170
//                 "pinch": 50,  slip on x axis: PinchLeft&right  45(unpintch) - 180(pintch)
//             }
public class DigitalTwinController : MonoBehaviour
{
    private int frameCounter = 0;  // Frame counter to count up to 10
    private bool rotateTo170 = true;

    NetworkManager networkManager;
    public ArticulationBody Link_1;
    public ArticulationBody Link_2;
    public ArticulationBody Link_3;
    // public GameObject Link_3;
    public ArticulationBody Link_4;
    public ArticulationBody Link_5;
    public GameObject Pinch_left_x;
    public GameObject Pinch_right_y;
    // Start is called before the first frame update
    void Start()
    {
        networkManager = NetworkManager.Instance;
        networkManager.onArmLengthDataReceived += HandleReceivedRobotInfoData;

        // Link_1.transform.localRotation = Quaternion.Euler(0, 0, -90);
    }


    void HandleReceivedRobotInfoData(JsonArmLengthInfo armInfo)
    {
        SetTargetRotation(Link_1, armInfo.Link_1);
        SetTargetRotation(Link_2, armInfo.Link_2);
        SetTargetRotation(Link_3, armInfo.Link_3);
        SetTargetRotation(Link_4, armInfo.Link_4);
        SetTargetRotationY(Link_5, armInfo.Link_5);
        // SetTargetRotationY(Link_5, armInfo.Link_5);
    }
    // Update is called once per frame
    void Update()
    {
        // frameCounter++;  // Increment frame counter each frame

        // if (frameCounter >= 500)  // Check if 10 frames have passed
        // {
        //     if (rotateTo170)
        //     {
        //         SetTargetRotation(Link_2, 180);
        //         // SetTargetRotation(Link_1, 180);
        //         // Rotate to 170 degrees on the Y-axis
        //         // SetTargetRotation(Link_1, 170);
        //         // SetTargetRotation(Link_2, 53);
        //         for (int i = 90; i >= 0; i -= 5)
        //         {
        //             SetTargetRotation(Link_3, i);
        //         }
        //         SetTargetRotation(Link_4, 90);
        //         // SetTargetRotation(Link_3, 90);
        //         // SetTargetRotation(Link_3, 90);
        //         // Link_2.transform.localRotation = Quaternion.Euler(8, -90, -90);
        //         // Link_3.transform.localRotation = Quaternion.Euler(0, 240, 0);
        //         // Link_4.transform.localRotation = Quaternion.Euler(0, -140, 0);
        //         SetTargetRotationY(Link_5, 180);
        //         rotateTo170 = false;
        //     }
        //     else
        //     {
        //         // Rotate back to 0 degrees on the Y-axis
        //         // SetTargetRotation(Link_1, 0);
        //         // SetTargetRotation(Link_2, -55);
        //         // SetTargetRotation(Link_2, 0);
        //         // // SetTargetRotation(Link_1, 0);
        //         // for (int i = 0; i <= 180; i += 5)
        //         // {
        //         //     SetTargetRotation(Link_3, i);
        //         // }
        //         // SetTargetRotation(Link_4, 180);
        //         // // Link_2.transform.localRotation = Quaternion.Euler(-25, -90, -90);
        //         // // Link_3.transform.localRotation = Quaternion.Euler(0, 180, 0);
        //         // // Link_4.transform.localRotation = Quaternion.Euler(0, -199, 0);
        //         SetTargetRotationY(Link_5, 0);
        //         rotateTo170 = true;
        //     }

        //     frameCounter = 0;  // Reset frame counter
        // }
    }

    void SetTargetRotation(ArticulationBody body, float targetAngle)
    {
        // Get the current xDrive from the Articulation Body
        ArticulationDrive drive = body.xDrive;

        // Set the new target position (degrees)
        drive.target = targetAngle;

        // Apply the modified drive back to the Articulation Body
        body.xDrive = drive;
    }
    void SetTargetRotationY(ArticulationBody body, float targetAngle)
    {
        // Get the current xDrive from the Articulation Body
        ArticulationDrive drive = body.yDrive;

        // Set the new target position (degrees)
        drive.target = targetAngle;

        // Apply the modified drive back to the Articulation Body
        body.yDrive = drive;
    }
}
