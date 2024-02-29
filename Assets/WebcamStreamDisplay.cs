using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This script is used to display the stream from the robot on a 3D object(screen) in the scene.
/// This is the main source to display the stream from the robot.
/// The script is not optimized and is used for testing purposes. The method used to display the stream is not the best way to do it.
/// It sends requests to the server to fetch the stream every "refreshRate" seconds. It can be improved by using a better method to display the stream.
/// </summary>
public class WebcamStreamDisplay : MonoBehaviour
{
    /// <summary>
    /// The URL to request a snapshot from the robot.
    /// </summary>
    public string snapshotUrl = "http://192.168.1.11:5000/snapshot";

    /// <summary>
    /// The refresh rate of the stream in seconds.
    /// </summary>
    public float refreshRate = 0.1f; // Time in seconds between each snapshot request

    /// <summary>
    /// The renderer of the 3D object used to display the stream. It is used a videoPlayer material to display the stream.
    /// </summary>
    private Renderer _renderer;

    /// <summary>
    /// The WaitForSeconds object used to wait for the specified refresh rate before fetching the next snapshot.
    /// </summary>
    private WaitForSeconds _refreshWait;


    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _refreshWait = new WaitForSeconds(refreshRate);
        StartCoroutine(FetchSnapshotRoutine());
    }

    /// <summary>
    /// Coroutine used to fetch the snapshot from the robot every "refreshRate" seconds.
    /// </summary>
    /// <returns>_refreshWait</returns>
    private IEnumerator FetchSnapshotRoutine()
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(snapshotUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    _renderer.material.mainTexture = texture;
                }
                else
                {
                    Debug.LogError("Error fetching snapshot: " + www.error);
                }
            }

            yield return _refreshWait; // Wait for the specified refresh rate before fetching the next snapshot
        }
    }
}
