using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


// Class adapted from Esther's work on the 2U1D branch. Has Download and Upload functions
public static class ServerCommunication
{
    //Request data from MongoDB
    public static IEnumerator Download(string id, System.Action<PlayerData> callback = null)
    {
        string url = "http://hrl-server2.int.colorado.edu:3000/SPACE_Boyer/SPACE_subjects_data/" + id + "/lastTrial";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            Debug.Log("Download URL: " + url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Download Error: " + request.error);
                if (callback != null)
                {
                    callback.Invoke(null);
                }
            }
            else
            {
                if (callback != null)
                {
                    Debug.Log("Download Response: " + request.downloadHandler.text);
                    callback.Invoke(PlayerData.Parse(request.downloadHandler.text));
                }
            }
        }
    }


    // Send data to MongoDB
    // In the Upload() function, we expect a JSON string of a player profile. 
    // This profile is defined in the PlayerData class and is the data received by the Download() function
    public static IEnumerator Upload(string profile, string id, System.Action<bool> callback = null)
    {
        string url = "http://hrl-server2.int.colorado.edu:3000/SPACE_Boyer/SPACE_subjects_data/" + id + "/addTrial";
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(profile);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log("Uploading to URL: " + url);
            Debug.Log("Payload: " + profile);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Upload Error: " + request.error);
                if (callback != null)
                {
                    callback.Invoke(false);
                }
            }
            else
            {
                Debug.Log("Upload Response: " + request.downloadHandler.text);
                if (callback != null)
                {
                    callback.Invoke(request.downloadHandler.text != "{}");
                }
            }
        }
    }

    public static IEnumerator FirstUpload(string id, System.Action<bool> callback = null)
    {
        string url = "http://hrl-server2.int.colorado.edu:3000/SPACE_Boyer/SPACE_subjects_data/" + id + "/doesExist?";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("FirstUpload Error: " + request.error);
                if (callback != null)
                {
                    callback.Invoke(false);
                }
            }
            else
            {
                Debug.Log("FirstUpload Response: " + request.downloadHandler.text);
                if (callback != null)
                {
                    callback.Invoke(request.downloadHandler.text != "{}");
                }
            }
        }
    }


}

