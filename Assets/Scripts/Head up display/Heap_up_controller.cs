using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// This class is used to control the head up display of the robot.
/// It allows the user to see the robot info on the head up display.
/// </summary>
public class Heap_up_controller : MonoBehaviour
{

    /// <summary>
    /// The heap up canvas game object that hostes the heap up display
    /// </summary>
    public Canvas head_up_canvas;

    /// <summary>
    /// Robot view plane(Robot camera monitor)
    public Plane robot_view_plane;
    /// <summary>
    /// The OVR camera rig that is used to get the position of the camera
    /// </summary>
    public OVRCameraRig ovr_camera_rig;

    // Distance from the camera to the head up canvas
    private float head_up_canvas_distance = 4f;

    // Offset of the head up canvas from the camera
    private Vector3 offset_ovr_camera_rig = new Vector3(0, 0, 0);

    private NetworkManager networkManager;

    // Diaplay update
    public TMP_Text speed_text;
    public TMP_Text voltage_text;
    public TMP_Text battery_precentage_text;
    public TMP_Text mode_text;
    public TMP_Text latecy_text;

    public TMP_Text conenctionStatus_text;
    private Color connectedColor;
    private Color disconnectedColor;

    // Robot info
    private List<float> accelerometer;
    private List<float> gyroscope;
    private List<float> magnetometer;
    private List<float> motion;
    private float speed = 0;
    private float voltage;
    private int battery_precentage;
    private string mode;
    private float cms_speed;

    /// <summary>
    /// Updates the position and rotation of the head up canvas to match the position and rotation of the camera
    /// This allows the user to allways see the head up display in front of them
    /// </summary>
    private void update_head_up_canvas_position_and_rotation()
    {
        // Move the head up canvas to the position of the camera
        Vector3 newPosition = ovr_camera_rig.centerEyeAnchor.position + ovr_camera_rig.centerEyeAnchor.forward * head_up_canvas_distance + offset_ovr_camera_rig;


        newPosition.y = head_up_canvas.transform.position.y;
        newPosition.z = head_up_canvas.transform.position.z;

        head_up_canvas.transform.position = newPosition;
        head_up_canvas.transform.LookAt(ovr_camera_rig.centerEyeAnchor);

        head_up_canvas.transform.Rotate(0, 180f, 0);

        //Upright relativ to the camera (Users head)

        // The rotation on the X and Z axes but allows it to rotate freely around the Y axis
        // head_up_canvas.transform.rotation = Quaternion.Euler(0, head_up_canvas.transform.rotation.eulerAngles.y, 0);
    }


    void Start()
    {
        networkManager = NetworkManager.Instance;
        networkManager.OnRobotInfoDataReceived += HandleReceivedRobotInfoData;
        networkManager.onPingDataReceived += HandleReceivedPingData;
        networkManager.OnConnectionStatus += HandleConnectionStatusChanged;
        disconnectedColor = conenctionStatus_text.color;
        connectedColor = latecy_text.color;
    }


    private void HandleConnectionStatusChanged(bool connected)
    {

        conenctionStatus_text.text = connected ? "Connected" : "Disconnected";
        conenctionStatus_text.color = connected ? connectedColor : disconnectedColor;

    }


    /// <summary>
    /// This method is used to handle the received robot info data.
    /// It updates the robot info on the head up display if the data has changed.
    /// </summary>
    /// <param name="info"></param>
    private void HandleReceivedRobotInfoData(JsonRobotInfo info)
    {
        if (accelerometer != info.accelerometer)
        {
            accelerometer = info.accelerometer;
        }
        if (gyroscope != info.gyroscope)
        {
            gyroscope = info.gyroscope;
        }
        if (magnetometer != info.magnetometer)
        {
            magnetometer = info.magnetometer;
        }
        if (motion != info.motion)
        {
            motion = info.motion;
        }
        if (speed != info.speed)
        {
            speed = info.cms_speed;
            speed_text.text = cms_speed.ToString();
        }
        if (voltage != info.voltage)
        {
            voltage = info.voltage;
            voltage_text.text = voltage.ToString();
        }
        if (battery_precentage != info.battery_precentage)
        {
            battery_precentage = info.battery_precentage;
            battery_precentage_text.text = battery_precentage.ToString();
        }
        if (mode != info.mode)
        {
            mode = info.mode;
            mode_text.text = mode;
        }

    }


    /// <summary>
    /// This method is used to handle the received ping data.
    /// It updates the latency on the head up display.
    /// </summary>
    /// <param name="ping"></param>
    private void HandleReceivedPingData(float ping)
    {
        latecy_text.text = ping.ToString();
        Debug.Log("LatencyPing: " + ping);
    }
    // Update is called once per frame
    void Update()
    {
        update_head_up_canvas_position_and_rotation();
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    void OnDestroy()
    {
        networkManager.OnRobotInfoDataReceived -= HandleReceivedRobotInfoData;
    }

}
