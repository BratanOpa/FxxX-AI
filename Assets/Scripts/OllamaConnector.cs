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

    // UI-text för att visa antal skickade prompts
    [SerializeField] TMP_Text promptCountText;

    [Header("Koppling till Command Parser")]
    [SerializeField] RobotCommandParser commandParser;

    // Lista för att spara hela konversationen
    private List<Message> messageHistory = new List<Message>();

    // Variabel för att räkna antalet prompts
    private int promptCount = 0;

    void Start()
    {
        // Uppdatera UI-texten från start
        UpdatePromptCountUI();

        messageHistory.Add(new Message
        {

            role = "system",

            content =
            "Du är motorn i en robotarm. Din uppgift är att översätta ALLA typer av användarindata (även kreativa som 'dansa', 'vinka' eller otydliga beskrivningar) till en serie exekverbara kommandon.\n" +
            "VIKTIGT OM VINKLARNA:\n" +
            "Alla vinklar är **delta-värden** (relativa förändringar i grader). T.ex. RotateArm1(15) betyder att armen ska vridas 15 grader *till* från sin nuvarande position, inte till en absolut position.\n" +
            "ROBOTENS STRUKTUR:\n" +
            "- Arm 1: Innersta segmentet.\n" +
            "- Arm 2: Mittensegmentet.\n" +
            "- Arm 3 / Wrist: Yttersta segmentet.\n" +
            "- Klor (Claws): Gripklorna längst upp[cite: 1].\n" +
            "ENDA TILLÅTNA KOMMANDON:\n" +
            "RotateArm1(deltaVinkel)\n" +
            "RotateArm2(deltaVinkel)\n" +
            "RotateWrist(deltaVinkel)\n" +
            "RotateClaws(deltaVinkel)\n" +
            "ABSOLUTA REGLER:\n" +
            "1. SKRIV ALDRIG NÅGON VANLIG TEXT, FÖRKLARING ELLER URSÄKT. Svara uteslutande med kommandon.\n" +
            "2. Varje kommando MÅSTE omslutas av exakt två stjärnor på varje sida, t.ex. **RotateArm1(15)**.\n" +
            "3. Om användaren ber om något abstrakt (som att 'dansa'), kedja ihop flera stegvisa delta-kommandon.\n" +
            "4. Om en del är oklart, tolka det generöst till närmaste riktiga armdel.\n\n" +
            "EXEMPEL PÅ KORREKTA SVAR:\n" +
            "Användare: 'Kör en liten dans'\n" +
            "Svar: **RotateArm1(30)** **RotateArm2(-30)** **RotateClaws(45)** **RotateArm1(-30)** **RotateArm2(30)** **RotateClaws(-45)**\n\n" +
            "Användare: 'Vrid arm 1 lite till vänster'\n" +
            "Svar: **RotateArm1(-15)**\n\n" +
            "Användare: 'Öppna klorna lite'\n" +
            "Svar: **RotateClaws(20)**"
        });

    }

    public void SendPromt()
    {
        string prompt = promptInput.text;
        if (string.IsNullOrWhiteSpace(prompt)) return;

        // Öka räknaren med 1 och uppdatera UI
        promptCount++;
        UpdatePromptCountUI();

        Debug.Log("User: " + prompt);

        // Visa användarens meddelande direkt på skärmen (och meddelande om att AI tänker)
        responseText.text = "<b>Du:</b> " + prompt + "\n<i>Väntar på roboten...</i>";

        Message userMessage = new Message { role = "user", content = prompt };
        messageHistory.Add(userMessage);

        promptInput.text = "";

        StartCoroutine(SendToOllama(prompt));

        // Behåll fokus på rutan
        promptInput.ActivateInputField();
        promptInput.Select();
    }

    private void UpdatePromptCountUI()
    {
        if (promptCountText != null)
        {
            promptCountText.text = "Amount prompts: " + promptCount;
        }
    }

    IEnumerator SendToOllama(string userPrompt)
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

                // SKRIVER UT HELA SVARET I KONSOLEN
                Debug.Log("AI: " + aiResponse);

                // Kör tolkningen via parsern om den finns
                string parsedOutput = "";
                if (commandParser != null)
                {
                    parsedOutput = commandParser.ProcessAiResponse(aiResponse);
                }
                else
                {
                    parsedOutput = aiResponse;
                    Debug.LogWarning("RobotCommandParser saknas i OllamaConnector!");
                }

                // VISAR BÅDE ANVÄNDARENS PROMPT OCH AI-SVARET I SKÄRMTEXTEN
                responseText.text = "<b>Du:</b> " + userPrompt + "\n<b>Robot:</b> " + parsedOutput;

                messageHistory.Add(new Message { role = "assistant", content = aiResponse });
            }
            else
            {
                responseText.text = "<b>Du:</b> " + userPrompt + "\n<b>Error:</b> " + request.error;
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