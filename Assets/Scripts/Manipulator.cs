using UnityEngine;

public class ManipulatorHinge : MonoBehaviour
{
    [Header("Hinge Joints för armarna")]
    public HingeJoint2D arm1Hinge;
    public HingeJoint2D arm2Hinge;
    public HingeJoint2D hookBaseHinge;
    public HingeJoint2D hookLeftHinge;
    public HingeJoint2D hookRightHinge;

    [Header("Maskin-inställningar")]
    public float motorSpeed = 200f;
    public float acceleration = 600f; // Hur snabbt den startar/stannar (högre = rappare, lägre = tyngre)

    [Header("Maskin-inställningar")]
    public bool keyboardControle = true;

    private float arm1TargetAngle;
    private float arm2TargetAngle;
    private float hookBaseTargetAngle;
    private float hookLeftTargetAngle;
    private float hookRightTargetAngle;

    void Start()
    {
        // 1. Stäng av krockar mellan ALLA delar i manipulatorn så de inte slåss
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            for (int j = i + 1; j < allColliders.Length; j++)
            {
                Physics2D.IgnoreCollision(allColliders[i], allColliders[j]);
            }
        }

    }

    void Update()
    {
        if (keyboardControle) {
            if (Input.GetKeyDown(KeyCode.A)) RotateJoint(1, -15f);
            if (Input.GetKeyDown(KeyCode.D)) RotateJoint(1, 15f);
            if (Input.GetKeyDown(KeyCode.Q)) RotateJoint(2, -15f);
            if (Input.GetKeyDown(KeyCode.E)) RotateJoint(2, 15f);
            if (Input.GetKeyDown(KeyCode.W)) RotateJoint(3, -15f);
            if (Input.GetKeyDown(KeyCode.S)) RotateJoint(3, 15f);
            if (Input.GetKeyDown(KeyCode.Z)) RotateJoint(4, -15f);
            if (Input.GetKeyDown(KeyCode.X)) RotateJoint(4, 15f);
        }

            // Uppdatera motorerna för alla leder
            UpdateMotor(arm1Hinge, arm1TargetAngle);
            UpdateMotor(arm2Hinge, arm2TargetAngle);
            UpdateMotor(hookBaseHinge, hookBaseTargetAngle);
            UpdateMotor(hookLeftHinge, hookLeftTargetAngle);
            UpdateMotor(hookRightHinge, hookRightTargetAngle);
    }

    void UpdateMotor(HingeJoint2D hinge, float targetAngle)
    {
        JointMotor2D motor = hinge.motor;
        float currentAngle = hinge.jointAngle;
        float angleDiff = targetAngle - currentAngle;

        // Mjuk inbromsning (Soft Brake): 
        // Vi räknar ut en önskad hastighet som blir lägre ju närmare målet vi kommer.
        float stoppingDistance = 5f; // Hur många grader innan målet den ska börja bromsa in
        float speedFactor = Mathf.Clamp01(Mathf.Abs(angleDiff) / stoppingDistance);
        
        float desiredSpeed = 0f;
        if (Mathf.Abs(angleDiff) > 0.2f) // Om vi inte är helt framme
        {
            desiredSpeed = Mathf.Sign(angleDiff) * motorSpeed * speedFactor;
        }

        // Använd Mathf.MoveTowards för att få mjuk acceleration och inbromsning (tyngdkänsla)
        motor.motorSpeed = Mathf.MoveTowards(motor.motorSpeed, desiredSpeed, acceleration * Time.deltaTime);

        hinge.motor = motor;
    }

    public void RotateJoint(int jointIndex, float angleDelta)
    {
        switch (jointIndex)
        {
            case 1:
                arm1TargetAngle = arm1Hinge.jointAngle;
                arm1TargetAngle += angleDelta; 
                break;
            case 2:
                arm2TargetAngle = arm2Hinge.jointAngle;
                arm2TargetAngle += angleDelta; 
                break;
            case 3:
                hookLeftTargetAngle += angleDelta;
                hookRightTargetAngle -= angleDelta; 
                break;
            case 4:
                hookBaseTargetAngle = hookBaseHinge.jointAngle;
                hookBaseTargetAngle += angleDelta;
                break;
        }
    }
}