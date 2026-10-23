using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Reset : MonoBehaviour
{
    [Tooltip("Dra din UI Image RectTransform hit")]
    public RectTransform targetImageRect;
    public void TriggerSquish()
    {
        if (targetImageRect == null) return;

        StopAllCoroutines();
        StartCoroutine(SquishRoutine());
    }

    private IEnumerator SquishRoutine()
    {
        Vector3 originalScale = Vector3.one;
        // Ändra dessa värden för att justera hur "hård" squishen ska vara
        // T.ex. X blir bredare (1.25) och Y blir plattare (0.75)
        Vector3 squishedScale = new Vector3(originalScale.x * 1.25f, originalScale.y * 0.75f, originalScale.z);

        float duration = 0.08f; // Hur snabb squishen är
        float elapsed = 0f;

        // 1. Squisha till det hoptryckta läget
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            targetImageRect.localScale = Vector3.Lerp(originalScale, squishedScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        // 2. Studsa tillbaka till originalstorlek
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            targetImageRect.localScale = Vector3.Lerp(squishedScale, originalScale, elapsed / duration);
            yield return null;
        }

        targetImageRect.localScale = originalScale;

        Invoke("PlayAgain", 1f);

    }
    public void PlayAgain()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
