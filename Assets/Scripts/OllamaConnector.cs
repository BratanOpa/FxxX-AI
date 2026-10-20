using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaConnector : MonoBehaviour
{
    // Canvas saker
    [SerializeField] TMP_InputField promptInput;
    [SerializeField] TMP_Text responseText;

    [Header("Koppling till Command Parser")]
    [SerializeField] RobotCommandParser commandParser; // Dra in ditt skript här!

    // Lista för att spara hela konversationen
    private List<Message> messageHistory = new List<Message>();

    void Start()
    {
        messageHistory.Add(new Message
        {
            role = "system",

            content = "Du är en fast monterad robotarm på ett rullband. Du kan INTE förflytta dig i rummet, utan enbart röra dina fyra axlar. " +
                "Du MÅSTE ALLTID svara EXAKT på EN enda rad utan radbrytningar, på max en kort mening. " +
                "När du utför en rörelse ska du lägga till kommandot inuti fetstilta stjärnor i slutet av samma rad. " +
                "Observera att värdet i kommandot är **delta grader** (hur mycket leden ska vridas från sin nuvarande position, t.ex. +15 eller -15), inte en absolut vinkel. " +
                "Ställ ALDRIG motfrågor till användaren. Om du är osäker, chansa på en rimlig rörelse och utför den ändå så bra du kan. " +
                "Tillgängliga kommandon: RotateArm1(angleDelta), RotateArm2(angleDelta), RotateWrist(angleDelta), RotateClaws(angleDelta). " +
                "Exempel på svar:\n" +
                "Nu vrider jag första axeln lite till vänster! **RotateArm1(-15)**\n" +
                "Såhär ser ett svar ut utan rörelse:\n" +
                "Det klarar jag tyvärr inte av."
        });
    }

    public void SendPromt()
    {
        string prompt = promptInput.text;
        if (string.IsNullOrWhiteSpace(prompt)) return;

        Message userMessage = new Message { role = "user", content = prompt };
        messageHistory.Add(userMessage);

        promptInput.text = "";

        StartCoroutine(SendToOllama());

        // Behåll fokus på rutan så du kan skriva direkt igen
        promptInput.ActivateInputField();
        promptInput.Select();
    }

    IEnumerator SendToOllama()
    {
        OllamaRequest requestData = new OllamaRequest
        {
            model = "phi4",
            messages = messageHistory.ToArray(),
            stream = false
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

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

                // SKRIVER UT HELA SVARET (INKLUSIVE KOMMANDO) I KONSOLEN
                Debug.Log("AI: " + aiResponse);

                // Låt det nya kommandoskriptet hantera tolkning och exekvering
                if (commandParser != null)
                {
                    responseText.text = commandParser.ProcessAiResponse(aiResponse);
                }
                else
                {
                    responseText.text = aiResponse;
                    Debug.LogWarning("RobotCommandParser saknas i OllamaConnector!");
                }

                messageHistory.Add(new Message { role = "assistant", content = aiResponse });
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