using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebcamStreamDisplay : MonoBehaviour
{
    public string snapshotUrl = "http://192.168.1.11:5000/snapshot"; // URL to request a snapshot
    public float refreshRate = 0.1f; // Time in seconds between each snapshot request

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
