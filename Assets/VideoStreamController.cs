
using UnityEngine;
using UnityEngine.Video;


/// <summary>
/// This script is used to play a video stream from a server.
/// Used for testing purposes. It does not work with HSL as intended.
/// </summary>
public class VideoStreamController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = "http://192.168.0.21:5000/stream/stream.m3u8";
        videoPlayer.Play();
    }
}
