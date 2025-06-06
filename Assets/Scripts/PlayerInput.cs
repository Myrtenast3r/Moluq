using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    // Power related
    public float minPower = 1.0f;
    public float maxPower = 15f;
    public float chargeSpeed = 20f;
    public float powerDamperValue = 0.85f;
    [SerializeField] private float currentPower;
    [SerializeField] private float adjustedPower;
    private bool isCharging = false;

    // Aiming related
    public float markerMovementSpeed = 50f;
    [SerializeField] private float horizontalAngle = 0f;
    [SerializeField] private float verticalAngle = 0f;

    // UI related
    [SerializeField] private Slider powerMeterUI;
    [SerializeField] private Button resetButton;

    // Aiming UI related
    [SerializeField] private LineRenderer aimingLine;
    public int linePoints = 10;
    public float pointSpacing = 0.2f;
    public float verticalAngleAmount = 5f;
    public float aimingLineHelperValue = 0.3f;
    public float dotOffset = 2f;

    // Transforms
    [SerializeField] private Transform stickObject;
    [SerializeField] private Transform stickStartingPosition;
    [SerializeField] private Rigidbody stickRigidBody;
    [SerializeField] private Transform aimingMarker;
    [SerializeField] private Transform stickLandingMarker;

    // Other
    private bool isAimingSet = false;
    private bool isAngleSet = false;

    private bool throwStarted;
    public bool ThrowStarted
    {
        get { return throwStarted; }
    }

    [SerializeField] private PinController pinController;

    private void Start()
    {
        if (pinController == null)
        {
            pinController = FindAnyObjectByType<PinController>();
        }

        if (stickObject == null)
        {
            Debug.Log($"Stick object null, add handling");
        }
        if (stickStartingPosition == null)
        {
            Debug.Log($"Stick starting position null, add handling");
        }

        stickRigidBody = stickObject.gameObject.GetComponent<Rigidbody>();
        stickRigidBody.isKinematic = true;

        stickObject.position = stickStartingPosition.position;
        stickObject.transform.Rotate(0, 0, 90f); // rotate the stick
        /// In the future, add function for the player to change the rotation of the stick
        /// 

        aimingLine.gameObject.SetActive(true);
        aimingLine.textureMode = LineTextureMode.Tile;
        aimingLine.material.mainTextureScale = new Vector2(10, 1);

        stickLandingMarker.position = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);

    }

    private void Update()
    {
        KeyboardSetAimingSpot();
    }

    private void KeyboardSetAimingSpot()
    {
        UpdateAimingLineTrajectory();

        if (!isAimingSet)
        {
            Vector3 directionToMarker = aimingMarker.position - stickStartingPosition.position;

            // Left/Right aim
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                if (horizontalAngle > -50.0f)
                {
                    aimingMarker.position += Vector3.left * markerMovementSpeed * Time.deltaTime;
                }
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                if (horizontalAngle < 45.0f)
                {
                    aimingMarker.position += Vector3.right * markerMovementSpeed * Time.deltaTime;
                }
            }

            // Up/Down aim
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                if (aimingMarker.position.z < 7.0f && horizontalAngle < 44.5f && horizontalAngle > -49.5f)
                {
                    aimingMarker.position += Vector3.forward * markerMovementSpeed * Time.deltaTime;
                }

            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                if (aimingMarker.position.z > -3.0f && horizontalAngle < 44.5f && horizontalAngle > -49.5f)
                {
                    aimingMarker.position += Vector3.back * markerMovementSpeed * Time.deltaTime;
                }
            }

            horizontalAngle = Mathf.Atan2(directionToMarker.x, directionToMarker.z) * Mathf.Rad2Deg;
            verticalAngle = Mathf.Atan2(directionToMarker.y, directionToMarker.magnitude) * Mathf.Rad2Deg;

            horizontalAngle = Mathf.Clamp(horizontalAngle, -50f, 45f);
            verticalAngle = Mathf.Clamp(verticalAngle, -45f, 30f);

            stickObject.transform.rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90f);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isAimingSet = true;
                return;
            }
        }

        if (isAimingSet)
        {
            KeyboardSetAngle();
        }
    }

    private void KeyboardSetAngle()
    {
        if (!isAngleSet)
        {
            Vector3 directionToMarker = aimingMarker.position - stickStartingPosition.position;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                verticalAngle -= verticalAngleAmount * Time.deltaTime * markerMovementSpeed;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                verticalAngle += verticalAngleAmount * Time.deltaTime * markerMovementSpeed;
            }

            verticalAngle = Mathf.Clamp(verticalAngle, -45f, 30f);

            stickObject.transform.rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90f);

            UpdateAimingLineTrajectory();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isAngleSet = true;
                isCharging = true;
            }
            return;
        }

        if (isCharging)
        {
            AdjustPower();
        }

    }

    private void AdjustPower()
    {
        if (!isCharging) { return; }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentPower = minPower; // start at min power
        }

        if (isCharging)
        {
            AudioManager.instance.PlayChargePower();

            currentPower += (chargeSpeed / 2) * Time.deltaTime * Mathf.Pow(currentPower, 0.5f); // Pow
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower); // Limit max power

            // Calculate in gravity effect
            //float angleFactor = Mathf.Cos(Mathf.Deg2Rad * verticalAngle); // Reduce power as angle increases // Linear curve
            float angleFactor = Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * verticalAngle), 1.5f); // Reduce power as angle increases // Exponential curve
            //float angleFactor = 1 - (Mathf.Sin(Mathf.Deg2Rad * verticalAngle) * 0.5f); // Reduce power as angle increases // Custom curves
            adjustedPower = currentPower * angleFactor;

            powerMeterUI.value = currentPower;
            UpdateHitMarker();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            AudioManager.instance.StopChargePower();
            ThrowStick();
            isCharging = false;
        }
    }

    private void ThrowStick()
    {
        AudioManager.instance.PlayThrowStick();

        aimingLine.gameObject.SetActive(false);
        aimingMarker.gameObject.SetActive(false);
        stickLandingMarker.gameObject.SetActive(false);

        stickRigidBody.isKinematic = false; // Enable physics
        stickRigidBody.useGravity = true;
        stickRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Quaternion aimingRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90);
        Vector3 throwDirection = aimingRotation * Vector3.forward;

        stickRigidBody.AddForce(throwDirection * adjustedPower * powerDamperValue, ForceMode.Impulse);

        pinController.StartThrow();
        StartCoroutine(DelayMovementCheck());
    }

    private IEnumerator DelayMovementCheck()
    {
        yield return new WaitForSeconds(0.5f);
        throwStarted = true;
        //CheckMovement();
    }
    
    private void UpdateAimingLineTrajectory()
    {
        float offset = Time.time * dotOffset;
        aimingLine.material.mainTextureOffset = new Vector2(offset, 0);

        if (isCharging)
        {
            return;
        }

        aimingLine.positionCount = linePoints;
        Vector3 startPosition = stickStartingPosition.position;
        Vector3 endPosition = aimingMarker.position;

        // Define control point to create curve based on the aiming angle
        Vector3 controlPoint = (startPosition + endPosition) / 2; // Mid point
        controlPoint.y += verticalAngle * -1 * aimingLineHelperValue; // add helper value if necessary

        for (int i = 0; i < linePoints; i++)
        {
            float t = i / (float)(linePoints - 1); // Normalize between 0 and 1
            Vector3 curvedPoint = QuadraticBezier(startPosition, controlPoint, endPosition, t);
            aimingLine.SetPosition(i, curvedPoint);
        }
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
    }

    private void UpdateHitMarker()
    {
        Vector3 startingPos = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);
        Vector3 aimingDirection = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.back;

        float stickMass = stickRigidBody.mass;
        float launchVelocity = adjustedPower / stickMass;

        // Estimate time to impact
        float flightTime = CalculateFlightTime();

        // Calculate estimated landing position using projectile physics
        Vector3 projectedPosition = startingPos + aimingDirection * launchVelocity * flightTime;
        projectedPosition.y = 0f;

        stickLandingMarker.transform.position = projectedPosition;
    }

    float CalculateFlightTime()
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float verticalVelocity = adjustedPower * Mathf.Sin(Mathf.Deg2Rad * verticalAngle);

        float timeToPeak = verticalVelocity / gravity;  // Time to reach highest point
        float totalTime = timeToPeak * 2f;  // Double for full arc landing

        return totalTime;
    }

    private void ResetScene()
    {
        currentPower = 0;
        adjustedPower = 0;
        throwStarted = false;
        isAimingSet = false;
        isAngleSet = false;
        stickLandingMarker.position = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);
        aimingMarker.position = Vector3.zero;
        stickLandingMarker.gameObject.SetActive(true);
        aimingLine.gameObject.SetActive(true);
        aimingMarker.gameObject.SetActive(true);
        stickRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    public void ResetSceneFromUI()
    {
        if (pinController.SceneFrozen)
        {
            Time.timeScale = 1.0f;
            // Reset scene
            stickRigidBody.isKinematic = true;
            stickRigidBody.useGravity = false;
            stickObject.position = stickStartingPosition.position;

            stickObject.transform.Rotate(0, 0, 90f); // rotate the stick
            pinController.ResetScene();
            this.ResetScene();
            ScoreManager.Instance.ResetUi();
        }
    }
}
