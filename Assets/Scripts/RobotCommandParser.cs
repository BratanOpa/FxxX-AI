using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class RobotCommandParser : MonoBehaviour
{
    [Header("Koppling till Robotarm")]
    [SerializeField] ManipulatorHinge manipulator;

    [Header("Tid och Fördröjning")]
    [Tooltip("Tid (i sekunder) att vänta mellan varje kommando så armen hinna röra sig färdigt.")]
    [SerializeField] float actionDelay = 0.3f;

    // Kö för att hålla reda på kommandon som ska utföras i ordning
    private Queue<RobotCommand> commandQueue = new Queue<RobotCommand>();
    private bool isProcessingQueue = false;

    // Struktur för att lagra ett kommando och dess värde
    private struct RobotCommand
    {
        public string methodName;
        public float value;

        public RobotCommand(string method, float val)
        {
            methodName = method;
            value = val;
        }
    }

    /// <summary>
    /// Tar emot AI:ns råa svar, köar alla hittade kommandon och returnerar en ren text utan stjärnor för UI.
    /// </summary>
    public string ProcessAiResponse(string response)
    {
        if (manipulator == null)
        {
            Debug.LogWarning("ManipulatorHinge är inte kopplad till RobotCommandParser!");
            return response;
        }

        // 1. Hitta och lägg till kommandona i kön
        ParseAndQueueCommands(response);

        // 2. Starta kö-hanteraren om den inte redan körs
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessCommandQueueRoutine());
        }

        // 3. Rensa bort stjärnkommandona så spelaren slipper se koden i textrutan
        string cleanText = Regex.Replace(response, @"\*\*.*?\*\*", "").Trim();

        return string.IsNullOrEmpty(cleanText) ? response : cleanText;
    }

    private void ParseAndQueueCommands(string response)
    {
        string blockPattern = @"\*\*(.*?)\*\*";
        MatchCollection blockMatches = Regex.Matches(response, blockPattern);
        string commandPattern = @"(?<method>\w+)\((?<value>[-\d.]+)\)";

        foreach (Match blockMatch in blockMatches)
        {
            string innerContent = blockMatch.Groups[1].Value;
            string[] individualCommands = innerContent.Split(',');

            foreach (string cmd in individualCommands)
            {
                Match cmdMatch = Regex.Match(cmd.Trim(), commandPattern);
                if (cmdMatch.Success)
                {
                    string methodName = cmdMatch.Groups["method"].Value;
                    string valueStr = cmdMatch.Groups["value"].Value;

                    if (float.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float angleValue))
                    {
                        // Lägg till i kön istället för att köra direkt
                        commandQueue.Enqueue(new RobotCommand(methodName, angleValue));
                    }
                }
            }
        }
    }

    private IEnumerator ProcessCommandQueueRoutine()
    {
        isProcessingQueue = true;

        while (commandQueue.Count > 0)
        {
            RobotCommand cmd = commandQueue.Dequeue();

            // Utför det aktuella kommandot
            ExecuteCommand(cmd.methodName, cmd.value);

            // Vänta den inställda tiden innan nästa kommando startar
            yield return new WaitForSeconds(actionDelay);
        }

        isProcessingQueue = false;
    }

    private void ExecuteCommand(string methodName, float angleValue)
    {
        // Kör rätt funktion beroende på vad AI:n skrev
        switch (methodName)
        {
            case "RotateArm1":
                manipulator.RotateArm1(angleValue);
                break;
            case "RotateArm2":
                manipulator.RotateArm2(angleValue);
                break;
            case "RotateWrist":
                manipulator.RotateWrist(angleValue);
                break;
            case "RotateClaws":
                manipulator.RotateClaws(angleValue);
                break;
            default:
                Debug.Log("Hittade okänt kommando från AI: " + methodName);
                break;
        }
    }
}