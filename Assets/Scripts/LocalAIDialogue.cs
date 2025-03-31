using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json; //Make sure Newtonsoft is installed!

public class LocalAIDialogue : MonoBehaviour
{
    [SerializeField] private TMP_InputField promptField;
    private string apiUrl = "http://localhost:11434/api/generate"; // Ollama API

    // JSON class to parse response
    [System.Serializable]
    private class AIResponse
    {
        public string response; // Extracting only the text response
    }

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
                Debug.Log("Full JSON Response: " + jsonResponse);

                //Parse only the "response" field
                AIResponse aiResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);
                if (aiResponse != null)
                {
                    callback(aiResponse.response); // Return only the actual AI response
                }
                else
                {
                    callback("Error: Could not parse response.");
                }
            }
            else
            {
                Debug.LogError("AI Error: " + request.error);
                callback("Error retrieving response.");
            }
        }
    }

    public void SubmitPrompt()
    {
        Debug.Log("User Input: " + promptField.text);
        StartCoroutine(GetResponse(promptField.text, response => Debug.Log("AI Says: " + response)));
    }
}
