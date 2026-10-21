using UnityEngine;

public class CrusherController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Triggerområden")]
        [SerializeField] private BoxCollider2D boxZone;
        [SerializeField] private BoxCollider2D clearanceZone;

    [Header("Lådan")]
        [SerializeField] private Rigidbody2D boxBody;
        [SerializeField] private Collider2D boxCollider;

    [Header("Klorna")]
        [SerializeField] private Collider2D leftClaw;
        [SerializeField] private Collider2D rightClaw;

    [Header("Villkor för att starta")]
        [SerializeField] private float settleTime = 0.5f;
        [SerializeField] private float maxBoxSpeed = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator pressAnimator;

    [SerializeField] private ParticleSystem crushParticles;
    [SerializeField] private GameObject crushedBoxVisual;

    [Header("Vinst")]
    [SerializeField] private GameRestart gameRestart;

    private bool gameFinished;  

    private bool hasCrushed;
    private float readyTimer;
    private bool hasStarted;    

    private void Start()
    {
        if(boxZone == null || clearanceZone == null ||
        boxBody == null || boxCollider == null ||
        leftClaw == null || rightClaw == null || gameRestart == null)
        {
            Debug.LogError("Koppla alla referenser på pressen i Inspector.", this);
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if(hasStarted)
            return;

        bool boxInPlace = boxZone.Distance(boxCollider).isOverlapped;

        bool leftClawInside = clearanceZone.Distance(leftClaw).isOverlapped;
        bool rightClawInside = clearanceZone.Distance(rightClaw).isOverlapped;
        bool clawsClear = !leftClawInside && !rightClawInside;

    // Kontrollera både rörelse och rotation.
        bool boxStill =
        boxBody.linearVelocity.magnitude <= maxBoxSpeed &&
        Mathf.Abs(boxBody.angularVelocity) <= 5f;

        if (boxInPlace && clawsClear && boxStill)
        {
        readyTimer += Time.fixedDeltaTime;

            if (readyTimer >= settleTime)
            {
                hasStarted = true;
                pressAnimator.SetTrigger("Press");
            }
        }
        else
        {
            // Villkoren måste vara uppfyllda hela väntetiden.
            readyTimer = 0f;
        }
    }
    
    private void Update()
    {
        if (!hasStarted || gameFinished)
            return;

            AnimatorStateInfo state =
            pressAnimator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Press") &&
            state.normalizedTime >= 1f &&
            !pressAnimator.IsInTransition(0))
        {
            WinGame();
        }
    }

    public void CrushBox()
    {
        if (hasCrushed)
            return;

        hasCrushed = true;

        boxBody.gameObject.SetActive(false);
        crushedBoxVisual.SetActive(true);

        if (crushParticles != null)
            crushParticles.Play();
    }
    
    public void WinGame()
    {
        if (gameFinished)
        return;

        gameFinished = true;
        gameRestart.ShowWinPanel();
    }

}
