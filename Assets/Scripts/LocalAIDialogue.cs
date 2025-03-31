using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LocalAIDialogue : MonoBehaviour
{
    [SerializeField] TMP_InputField promptField;
    private string apiUrl = "http://localhost:11434/api/generate"; // Ollama API

    public IEnumerator GetResponse(string prompt, System.Action<string> callback)
    {
        string jsonData = "{\"model\":\"mistral\",\"prompt\":\"" + prompt + "\",\"stream\":false}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                callback(jsonResponse); // Process the response
            }
            else
            {
                Debug.LogError("AI Error: " + request.error);
                callback("Error retrieving response.");
            }
        }
    }
}
