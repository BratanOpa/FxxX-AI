using UnityEngine;
using System.Collections;

public class DoorUp : MonoBehaviour
{
    [Tooltip("Dra det objekt vars position dörren ska flyga till när den öppnas")]
    public Transform targetPositionObject;

    [Tooltip("Hur lång tid det tar för dörren att nå målet (i sekunder)")]
    public float duration = 1.0f;

    private Vector3 closedPosition;
    private bool isOpen = false;
    private Coroutine activeRoutine;

    void Start()
    {
        Time.timeScale = 0f;
        // Spara dörrens ursprungliga position som dess "stängda" läge när spelet startar
        closedPosition = transform.position;
    }

    // Funktion för att öppna dörren
    public void OpenDoor()
    {
        Time.timeScale = 1f;
        if (targetPositionObject == null)
        {
            Debug.LogWarning("Inget mål-objekt är tilldelat!");
            return;
        }

        MoveTo(targetPositionObject.position, true);
    }

    // Funktion för att stänga dörren
    public void CloseDoor()
    {
        MoveTo(closedPosition, false);
    }

    // Alternativ: En funktion som växlar mellan öppen och stängd (perfekt för en enskild knapp)
    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void MoveTo(Vector3 targetPos, bool newState)
    {
        // Om dörren redan rör sig, stoppa den pågående animationen så vi kan byta riktning smidigt
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(MoveRoutine(targetPos, newState));
    }

    private IEnumerator MoveRoutine(Vector3 endPosition, bool newState)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // SmoothStep ger en mjuk start och ett mjukt stopp
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        // Säkerställ att dörren hamnar exakt på rätt plats
        transform.position = endPosition;
        isOpen = newState;
        activeRoutine = null;
    }
}