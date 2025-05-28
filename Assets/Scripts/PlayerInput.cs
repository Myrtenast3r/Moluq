using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    // Power related
    public float minPower = 1.0f;
    public float maxPower = 20f;
    public float chargeSpeed = 20f;
    [SerializeField] private float currentPower;
    [SerializeField] private float adjustedPower;
    private bool isCharging = false;

    // Aiming related
    public float markerMovementSpeed = 50f;
    [SerializeField] private float horizontalAngle = 0f;
    [SerializeField] private float verticalAngle = 0f;

    // UI related
    [SerializeField] private Slider powerMeterUI;

    // Aiming UI related
    [SerializeField] private LineRenderer aimingLine;
    [SerializeField] private Material aimingMaterial;
    [SerializeField] private Material chargingMaterial;
    public int linePoints = 10;
    public float pointSpacing = 0.2f;

    public float verticalAngleOffset = 5f;
    public float aimingLineHelperValue = 0.3f;

    public float dotOffset = 2f;

    [SerializeField] private Transform aimingMarker;
    [SerializeField] private Transform stickLandingMarker;

    // Transforms
    [SerializeField] private Transform stickObject;
    [SerializeField] private Transform stickStartingPosition;
    [SerializeField] private Rigidbody stickRigidBody;

    // Other
    private bool isAimingSet = false;
    private bool isAngleSet = false;

    private bool throwStarted;
    public bool ThrowStarted
    {
        get { return throwStarted; }
    }

    [SerializeField] private bool hasStickStopped = false;
    public bool HasStickStopped
    {
        get { return hasStickStopped; }
        set { hasStickStopped = value; }
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

        aimingLine.gameObject.SetActive(false);

        aimingLine.material = aimingMaterial;
        aimingLine.textureMode = LineTextureMode.Tile;
        aimingLine.material.mainTextureScale = new Vector2(10, 1);

        stickLandingMarker.position = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);

    }

    private void Update()
    {
        #region Keyboard Input


        KeyboardSetAimingSpot();
        //KeyboardSetAngle();
        //AdjustPower();
        //UpdateAimingLineStraight();
        //UpdateAimingLineTrajectory();

        if (throwStarted)
        {
            CheckMovement();
        }

        #endregion
        if (pinController.SceneFrozen && Input.GetKeyDown(KeyCode.R))
        {
            // Reset scene
            stickRigidBody.isKinematic = true;
            stickRigidBody.useGravity = false;
            stickObject.position = stickStartingPosition.position;

            stickObject.transform.Rotate(0, 0, 90f); // rotate the stick
            pinController.ResetScene();
            this.ResetScene();

            //Time.timeScale = 1f; // Add the scene reset call
            //SceneManager.LoadScene(0);
        }

    }

    private void KeyboardSetAimingSpot()
    {
        if (!isAimingSet)
        {
            Vector3 directionToMarker = aimingMarker.position - stickStartingPosition.position;
            Debug.Log($"directionToMarker: {directionToMarker}");

            // Left/Right aim
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                //horizontalAngle -= rotationSpeed * Time.deltaTime;
                //Debug.Log($"horizontal angle: {horizontalAngle}");
                aimingMarker.position += Vector3.left * markerMovementSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                //horizontalAngle += rotationSpeed * Time.deltaTime;
                //Debug.Log($"horizontal angle: {horizontalAngle}");
                aimingMarker.position += Vector3.right * markerMovementSpeed * Time.deltaTime;
            }

            // Up/Down aim
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                //verticalAngle += rotationSpeed * Time.deltaTime; // Invert
                //Debug.Log($"vertical angle: {verticalAngle}");
                aimingMarker.position += Vector3.forward * markerMovementSpeed * Time.deltaTime;

            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                //verticalAngle -= rotationSpeed * Time.deltaTime; // Invert
                //Debug.Log($"vertical angle: {verticalAngle}");
                aimingMarker.position += Vector3.back * markerMovementSpeed * Time.deltaTime;
            }

            horizontalAngle = Mathf.Atan2(directionToMarker.x, directionToMarker.z) * Mathf.Rad2Deg;
            verticalAngle = Mathf.Atan2(directionToMarker.y, directionToMarker.magnitude) * Mathf.Rad2Deg;

            horizontalAngle = Mathf.Clamp(horizontalAngle, -45f, 45f);
            verticalAngle = Mathf.Clamp(verticalAngle, -45f, 30f);

            stickObject.transform.rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90f);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"Aiming set!");
                isAimingSet = true;
                return;
            }
        }

        if (isAimingSet)
        {
            aimingLine.gameObject.SetActive(true);
            KeyboardSetAngle();
        }
    }

    private void KeyboardSetAngle()
    {
        if (!isAngleSet)
        {
            Vector3 directionToMarker = aimingMarker.position - stickStartingPosition.position;
            Debug.Log($"directionToMarker: {directionToMarker}");

            // Up/Down aim
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                verticalAngle -= verticalAngleOffset * Time.deltaTime; // Invert
                //Debug.Log($"vertical angle: {verticalAngle}");
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                verticalAngle += verticalAngleOffset * Time.deltaTime; // Invert
                //Debug.Log($"vertical angle: {verticalAngle}");
            }

            // Clamp the angles to prevent unrealistig aiming
            //verticalAngle = Mathf.Atan2(directionToMarker.y, directionToMarker.magnitude) * Mathf.Rad2Deg;
            verticalAngle = Mathf.Clamp(verticalAngle, -45f, 30f);

            stickObject.transform.rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90f);

            UpdateAimingLineTrajectory();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"Angle set!");
                isAngleSet = true;
            }
            return;
        }
        if (isAngleSet && Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
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
            currentPower += (chargeSpeed / 2) * Time.deltaTime * Mathf.Pow(currentPower, 0.5f); // Pow
            //currentPower += chargeSpeed * Time.deltaTime * Mathf.Log(currentPower + 1, 2); // Log
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower); // Limit max power

            // Calculate in gravity effect
            //float angleFactor = Mathf.Cos(Mathf.Deg2Rad * verticalAngle); // Reduce power as angle increases // Linear curve
            float angleFactor = Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * verticalAngle), 1.5f); // Reduce power as angle increases // Exponential curve
            //float angleFactor = 1 - (Mathf.Sin(Mathf.Deg2Rad * verticalAngle) * 0.5f); // Reduce power as angle increases // Custom curve
            //Debug.Log($"angle factor: {angleFactor}");
            adjustedPower = currentPower * angleFactor;

            //Debug.Log($"power difference: {currentPower - adjustedPower}");

            powerMeterUI.value = adjustedPower;
            UpdateHitMarker();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            ThrowStick();
            isCharging = false;
        }
    }

    private void ThrowStick()
    {
        aimingLine.gameObject.SetActive(false);
        aimingMarker.gameObject.SetActive(false);

        //Rigidbody rb = stickObject.GetComponent<Rigidbody>();
        //rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);

        stickRigidBody.isKinematic = false; // Enable physics
        stickRigidBody.useGravity = true;

        Quaternion aimingRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90);
        Vector3 throwDirection = aimingRotation * Vector3.forward;


        stickRigidBody.AddForce(throwDirection * adjustedPower, ForceMode.Impulse);

        //stickRigidBody.AddTorque(new Vector3(0, 0, spinAmount), ForceMode.Impulse); // Add s

        Debug.Log($"Stick thrown!");
        Debug.Log($"Direction: {throwDirection}");
        Debug.Log($"Adjusted power: {adjustedPower}");

        pinController.StartThrow();
        StartCoroutine(DelayMovementCheck());

        //Debug.Log($"ThrowStick() end");
    }

    private IEnumerator DelayMovementCheck()
    {
        //Debug.Log($"start coroutine DelayMovementCheck");
        yield return new WaitForSeconds(0.5f);
        throwStarted = true;
        CheckMovement();
    }

    private void CheckMovement()
    {
        //Debug.Log($"CheckMovement() start");
        //check the pin movement
        if (stickRigidBody.velocity.magnitude < 0.05f && stickRigidBody.angularVelocity.magnitude < 0.05f)
        {
            hasStickStopped = true;
            //Debug.Log($"Stick stopped moving");
        }
        else
        {
            hasStickStopped = false;
            //Debug.Log($"Stick is still moving");
        }
    }

    #region Aiming line straight version
    
    /*
    private void UpdateAimingLineStraight()
    {
        aimingLine.positionCount = 2;
        Vector3 startingPosition = stickStartingPosition.position;

        Vector3 aimingDirection = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;

        Vector3 endPosition = startingPosition + aimingDirection * aimingLineHelperValue;

        aimingLine.SetPosition(0, startingPosition);
        aimingLine.SetPosition(1, endPosition);
        lineGroundMarker.position = new Vector3(endPosition.x, 0f, endPosition.z);
    }
    */


    #endregion
    #region Aiming line trajectory version
    /// Trajectory aiming version

    
    private void UpdateAimingLineTrajectory()
    {
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
    #endregion

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
    }

    private void UpdateHitMarker()
    {
        Vector3 startingPos = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);
        Vector3 aimingDirection = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;

        Debug.Log($"UpdateHitMarker aiming direction: {aimingDirection}");

        //float gravity = Physics.gravity.y;
        float stickMass = stickRigidBody.mass;
        float launchVelocity = adjustedPower * stickMass;

        // Estimate time to impact
        //float flightTime = Mathf.Sqrt(-2 * startingPos.y / gravity) + (2 * launchVelocity / -gravity);
        float flightTime = CalculateFlightTime();

        // Calculate estimated landing position using projectile physics
        Vector3 projectedPosition = startingPos + aimingDirection * launchVelocity * flightTime;
        //Debug.Log($"UpdateHitMarker startingPos + aimingDirection: {startingPos + aimingDirection}");
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
        aimingLine.gameObject.SetActive(true);
        aimingMarker.gameObject.SetActive(true);
    }
}
