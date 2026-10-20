using System.Text.RegularExpressions;
using UnityEngine;

public class RobotCommandParser : MonoBehaviour
{
    [Header("Koppling till Robotarm")]
    [SerializeField] ManipulatorHinge manipulator;

    /// <summary>
    /// Tar emot AI:ns råa svar, kör alla hittade kommandon och returnerar en ren text utan stjärnor för UI.
    /// </summary>
    public string ProcessAiResponse(string response)
    {
        if (manipulator == null)
        {
            Debug.LogWarning("ManipulatorHinge är inte kopplad till RobotCommandParser!");
            return response;
        }

        // 1. Kör kommandona som finns i texten (stödjer nu flera kommandon separerade med kommatecken)
        ParseAndExecuteCommands(response);

        // 2. Rensa bort stjärnkommandona så spelaren slipper se koden i textrutan
        string cleanText = Regex.Replace(response, @"\*\*.*?\*\*", "").Trim();

        return string.IsNullOrEmpty(cleanText) ? response : cleanText;
    }

    private void ParseAndExecuteCommands(string response)
    {
        // Hitta alla block som är inuti ** ... **
        string blockPattern = @"\*\*(.*?)\*\*";
        MatchCollection blockMatches = Regex.Matches(response, blockPattern);

        // Mönster för att hitta ett enskilt kommando typ RotateArm1(-15)
        string commandPattern = @"(?<method>\w+)\((?<value>[-\d.]+)\)";

        foreach (Match blockMatch in blockMatches)
        {
            string innerContent = blockMatch.Groups[1].Value;

            // Dela upp på kommatecken om AI:n skickat flera kommandon i samma stjärnor
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
                        ExecuteCommand(methodName, angleValue);
                    }
                }
            }
        }
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