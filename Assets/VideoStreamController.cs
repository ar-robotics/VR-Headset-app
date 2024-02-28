
using UnityEngine;
using UnityEngine.Video;

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
