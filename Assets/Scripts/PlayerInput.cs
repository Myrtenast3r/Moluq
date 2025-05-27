using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    // Drag and release related
    //private Vector2 startPos;
    //private Vector2 endPos;
    //private Vector2 throwDirection;
    //private float throwPower;

    // Power related
    public float minPower = 1.0f;
    public float maxPower = 20f;
    public float chargeSpeed = 20f;
    [SerializeField] private float currentPower;
    [SerializeField] private float adjustedPower;
    private bool isCharging = false;

    // Aiming related
    public float rotationSpeed = 50f;
    private float horizontalAngle = 0f;
    private float verticalAngle = 0f;

    //public float powerMultiplier = 1.0f;
    //public float spinAmount = 0.3f;

    // UI related
    [SerializeField] private Slider powerMeterUI;

    // Aiming UI related
    [SerializeField] private LineRenderer aimingLine;
    [SerializeField] private Material aimingMaterial;
    [SerializeField] private Material chargingMaterial;
    public int linePoints = 10;
    public float pointSpacing = 0.2f;
    public float powerHelperValue = 10f;
    public float aimingLineHelperValue = 10f;

    public float dotOffset = 2f;

    [SerializeField] private Transform groundMarker;
    [SerializeField] private Transform lineGroundMarker;

    // Transforms
    [SerializeField] private Transform stickObject;
    [SerializeField] private Transform stickStartingPosition;
    [SerializeField] private Rigidbody stickRigidBody;

    // Other
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

        aimingLine.material = aimingMaterial;
        aimingLine.textureMode = LineTextureMode.Tile;
        aimingLine.material.mainTextureScale = new Vector2(10, 1);

        groundMarker.position = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);

    }

    private void Update()
    {
        #region Drag and Release input
        //if (Input.GetMouseButtonDown(0))
        //{
        //    startPos = Input.mousePosition;
        //}
        //if (Input.GetMouseButtonUp(0))
        //{
        //    endPos = Input.mousePosition;
        //    throwDirection = (startPos - endPos).normalized;
        //    throwPower = (startPos - endPos).magnitude * powerMultiplier;

        //    ThrowStick();
        //}
        #endregion

        #region Keyboard Input

        KeyboardAim();
        AdjustPower();
        //UpdateAimingLineStraight();
        UpdateAimingLineTrajectory();

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

    private void KeyboardAim()
    {
        // Left/Right aim
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontalAngle -= rotationSpeed * Time.deltaTime;
            //Debug.Log($"horizontal angle: {horizontalAngle}");
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontalAngle += rotationSpeed * Time.deltaTime;
            //Debug.Log($"horizontal angle: {horizontalAngle}");
        }

        // Up/Down aim
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            verticalAngle += rotationSpeed * Time.deltaTime * -1; // Invert
            //Debug.Log($"vertical angle: {verticalAngle}");
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            verticalAngle -= rotationSpeed * Time.deltaTime * -1; // Invert
            //Debug.Log($"vertical angle: {verticalAngle}");
        }



        // Clamp the angles to prevent unrealistig aiming
        horizontalAngle = Mathf.Clamp(horizontalAngle, -45f, 45f);
        verticalAngle = Mathf.Clamp(verticalAngle, -45f, 30f);

        stickObject.transform.rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90f);


    }

    private void AdjustPower()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentPower = minPower; // start at min power
        }

        if (isCharging)
        {
            currentPower += chargeSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower); // Limit max power

            // Calculate in gravity effect
            //float angleFactor = Mathf.Cos(Mathf.Deg2Rad * verticalAngle); // Reduce power as angle increases // Linear curve
            float angleFactor = Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * verticalAngle), 1.2f); // Reduce power as angle increases // Exponential curve
            //float angleFactor = 1 - (Mathf.Sin(Mathf.Deg2Rad * verticalAngle) * 0.5f); // Reduce power as angle increases // Custom curve
            Debug.Log($"angle factor: {angleFactor}");
            adjustedPower = currentPower * angleFactor;

            Debug.Log($"power difference: {currentPower - adjustedPower}");

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
        lineGroundMarker.gameObject.SetActive(false);

        //Rigidbody rb = stickObject.GetComponent<Rigidbody>();
        //rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);

        stickRigidBody.isKinematic = false; // Enable physics
        stickRigidBody.useGravity = true;

        Quaternion aimingRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90);
        Vector3 throwDirection = aimingRotation * Vector3.forward;


        stickRigidBody.AddForce(throwDirection * adjustedPower, ForceMode.Impulse);

        //stickRigidBody.AddTorque(new Vector3(0, 0, spinAmount), ForceMode.Impulse); // Add s

        //Debug.Log($"Stick thrown!");
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
        Quaternion aimRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

        Vector3 aimDirection = aimRotation * Vector3.forward * (isCharging ? adjustedPower : powerHelperValue);

        aimingLine.SetPosition(0, startPosition);

        if (!isCharging)
        {
            aimDirection = aimRotation * Vector3.forward * powerHelperValue; // Use the dummy value for drawing the line when aiming
            //aimingLine.material = aimingMaterial;
            //aimingLine.material.mainTextureScale = new Vector2(dotOffset, 1);
            //aimingLine.material.mainTextureOffset = new Vector2(dotOffset, 0);
            float offset = Time.time * dotOffset;
            aimingLine.material.mainTextureOffset = new Vector2(offset, 0);
        }


        for (int i = 1; i < linePoints; i++)
        {
            float time = i * pointSpacing;
            Vector3 previousPoint = i == 0 ? startPosition : aimingLine.GetPosition(i - 1);
            Vector3 point = previousPoint + aimDirection * pointSpacing + 0.5f * Physics.gravity * time * time;
            //aimingLine.SetPosition(i, point);
            //Debug.DrawRay(startPosition, aimDirection * time, Color.red, 0.1f);

            if (Physics.Raycast(previousPoint, (point - previousPoint).normalized, out RaycastHit hit, (point - previousPoint).magnitude))
            {
                //Debug.Log($"Raycast hit {hit.collider.gameObject.name}");
                aimingLine.SetPosition(i, hit.point);
                aimingLine.positionCount = i + 1;
                Debug.DrawRay(startPosition, aimDirection * time, Color.red, 0.1f);
                return;
            }

            aimingLine.SetPosition(i, point);
            lineGroundMarker.position = new Vector3(point.x, 0f, point.z);
        }

    }
    #endregion

    private void UpdateHitMarker()
    {
        Vector3 startingPos = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);
        Vector3 aimingDirection = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;

        float gravity = Physics.gravity.y;
        float stickMass = stickRigidBody.mass;
        float launchVelocity = (adjustedPower * 0.5f / stickMass);

        // Estimate time to impact
        float flightTime = Mathf.Sqrt(-2 * startingPos.y / gravity) + (2 * launchVelocity / -gravity);

        // Calculate estimated landing position using projectile physics
        Vector3 projectedPosition = startingPos + aimingDirection * launchVelocity * flightTime;
        projectedPosition.y = 0f;

        groundMarker.transform.position = projectedPosition;

    }

    private void ResetScene()
    {
        currentPower = 0;
        adjustedPower = 0;
        throwStarted = false;
        groundMarker.position = new Vector3(stickStartingPosition.position.x, 0, stickStartingPosition.position.z);
        aimingLine.gameObject.SetActive(true);
        lineGroundMarker.gameObject.SetActive(true);
    }
}
