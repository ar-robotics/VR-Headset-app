using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;
using WebSocketSharp;


public class WebcamStreamDisplay : MonoBehaviour
{
    public string snapshotUrl = "http://192.168.1.11:5000/snapshot";
    public float refreshRate = 0.1f;

    private Renderer _renderer;
    private WaitForSeconds _refreshWait;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _refreshWait = new WaitForSeconds(refreshRate);
        StartCoroutine(FetchSnapshotRoutine());
    }

    private IEnumerator FetchSnapshotRoutine()
    {
        while (true)
        {
            // Start a new asynchronous request
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(snapshotUrl);
            yield return www.SendWebRequest();  // Asynchronously wait for the web request to complete

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Asynchronously get the texture content
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                if (texture != null)
                {
                    ApplyTexture(texture);
                }
            }
            else
            {
                // Debug.LogError("Error fetching snapshot: " + www.error);
            }

            www.Dispose(); // Clean up the web request
            yield return _refreshWait; // Wait before making the next request
        }
    }

    private void ApplyTexture(Texture2D texture)
    {
        _renderer.material.mainTexture = texture;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

}
