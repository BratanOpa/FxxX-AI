using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaConnector : MonoBehaviour
{
    //Ollama är ai:n vi använder (open source och gratis). Den går lokalt på datorn och för att kommunicera med den måste vi göra våra inputs från unity till en JSON. och sedan skicka den JSONen till Ollama.
    //Att skriva i terminal: ollama run llama3.2:1b

    //Canvas saker
    [SerializeField] TMP_InputField promptInput;
    [SerializeField] TMP_Text responseText;

    public void SendPromt()
    {
        string prompt = promptInput.text;

        StartCoroutine(SendToOllama(prompt));
    }

    IEnumerator SendToOllama(string prompt)
    {

        //Gör en Ollama JSON från vår canvas input
        OllamaRequest requestData = new OllamaRequest { model = "llama3.2:1b", messages = new Message[] { new Message { role = "user", content = "You are the joker" + prompt } }, stream = false };
        string json = JsonUtility.ToJson(requestData);

        //Gör JSON till bytes för att skicka till Ollama
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        //Skicka datan till Ollama
        using (UnityWebRequest request = new UnityWebRequest("http://localhost:11434/api/chat", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OllamaResponse result = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                string aiResponse = result.message.content;
                responseText.text = aiResponse;
            }
            else
            {
                responseText.text = "Error: " + request.error;
            }
        }

    }

    [System.Serializable]
    public class OllamaRequest
    {
        public string model;
        public Message[] messages;
        public bool stream;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class OllamaResponse
    {
        public ResponseMessage message;
    }

    [System.Serializable]
    public class ResponseMessage
    {
        public string role;
        public string content;
    }
}
