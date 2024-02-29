using System.Collections;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(VideoPlayer))]

/// <summary>
/// This script is used to stream video from a URL and handle errors and retries.
/// It does not work! There is a problem with the format that it receives from the server.
/// </summary>
public class RobotCamStream : MonoBehaviour
{
    public string streamUrl = "http://192.168.1.5:8000/stream.m3u8";
    public int maxRetries = 3; // Maximum number of retries
    public float retryDelay = 2f; // Delay in seconds between retries

    private VideoPlayer videoPlayer;
    private int currentRetries = 0;

    /// <summary>
    /// Start is called before the first frame and gets the VideoPlayer component and sets up the video player.
    /// </summary>
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        SetupVideoPlayer();
    }

    /// <summary>
    /// Sets up the video player with the stream URL and other settings.
    /// </summary>
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

    /// <summary>
    /// Tries to prepare the video for playback.
    /// </summary>
    void TryPrepareVideo()
    {
        if (currentRetries < maxRetries)
        {
            videoPlayer.Prepare();
        }
    }

    /// <summary>
    /// Handles the prepare completed event of the video player.
    /// </summary>
    /// <param name="source">The video player source.</param>
    private void HandlePrepareCompleted(VideoPlayer source)
    {
        videoPlayer.Play();
    }

    /// <summary>
    /// Handles the video error event of the video player.
    /// </summary>
    /// <param name="source">The video player source.</param>
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
