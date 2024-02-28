using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class RobotCamStream : MonoBehaviour
{
    public string streamUrl = "http://192.168.1.5:8000/stream.m3u8";
    public int maxRetries = 3; // Maximum number of retries
    public float retryDelay = 2f; // Delay in seconds between retries

    private VideoPlayer videoPlayer;
    private int currentRetries = 0;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        SetupVideoPlayer();
    }

    void SetupVideoPlayer()
    {
        videoPlayer.url = streamUrl;
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = GetComponent<Renderer>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.isLooping = false; // Change to false to prevent auto-retry on reaching the end
        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.prepareCompleted += HandlePrepareCompleted;

        TryPrepareVideo();
    }

    void TryPrepareVideo()
    {
        if (currentRetries < maxRetries)
        {
            videoPlayer.Prepare();
        }
    }

    private void HandlePrepareCompleted(VideoPlayer source)
    {
        videoPlayer.Play();
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("Video Player Error: " + message);
        currentRetries++;

        if (currentRetries < maxRetries)
        {
            Debug.Log($"Retry {currentRetries}/{maxRetries} in {retryDelay} seconds.");
            Invoke(nameof(TryPrepareVideo), retryDelay); // Wait for retryDelay seconds before retrying
        }
        else
        {
            Debug.LogError("Max retries reached. Stopping attempts to play video.");
        }
    }
}
