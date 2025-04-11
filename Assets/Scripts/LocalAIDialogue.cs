using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;
using System.IO;
using System;
using Unity.Tutorials.Core.Editor; //Make sure Newtonsoft is installed!

public class LocalAIDialogue : MonoBehaviour
{
    [SerializeField] private TMP_InputField promptField;
    [SerializeField] private TextMeshProUGUI responseTMP;

    private string apiUrl = "http://localhost:11434/api/generate"; // Ollama API


    public void SubmitPrompt()
    {
        Debug.Log("User Input: " + promptField.text);
        promptField.text += " || Response with immidiate and short answer like human conversation and do not use special character like * or use bulleting and numbering in your response";
        StartCoroutine(StreamResponse(promptField.text));
        promptField.text = "";
        responseTMP.text = "Thinking...";
    }

    public IEnumerator StreamResponse(string prompt)
    {
        string jsonData = "{\"model\":\"gemma3:4b\",\"prompt\":\"" + prompt + "\",\"stream\":true}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();

            string fullResponse = "";
            int lastProcessedLength = 0;

            while (!request.isDone)
            {
                string fullRaw = request.downloadHandler.text;

                if (fullRaw.Length > lastProcessedLength)
                {
                    string newData = fullRaw.Substring(lastProcessedLength);
                    lastProcessedLength = fullRaw.Length;

                    using (StringReader reader = new StringReader(newData))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                AIStreamResponse chunk = JsonConvert.DeserializeObject<AIStreamResponse>(line);
                                if (chunk != null && chunk.response != null)
                                {
                                    fullResponse += chunk.response;
                                    responseTMP.text = fullResponse;
                                }
                            }
                        }
                    }
                }

                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AI Error: " + request.error);
                responseTMP.text = "Error getting AI response.";
            }
        }
    }
}

[Serializable]
public class AIStreamResponse
{
    public string response;
    public bool done;
}