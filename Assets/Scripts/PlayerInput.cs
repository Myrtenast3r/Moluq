using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
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
    private float currentPower;
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
    public float powerHelperValue = 25f;

    public float dotOffset = 2f;

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
        UpdateAimingLine();

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
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontalAngle += rotationSpeed * Time.deltaTime;
        }

        // Up/Down aim
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            verticalAngle += rotationSpeed * Time.deltaTime * -1; // Invert
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            verticalAngle -= rotationSpeed * Time.deltaTime * -1; // Invert
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
            currentPower += (chargeSpeed / 2) * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower); // Limit max power
            powerMeterUI.value = currentPower;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            ThrowStick();
        }
    }

    private void ThrowStick()
    {

        //Rigidbody rb = stickObject.GetComponent<Rigidbody>();
        //rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);

        stickRigidBody.isKinematic = false; // Enable physics
        stickRigidBody.useGravity = true;

        Quaternion aimingRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90);
        Vector3 throwDirection = aimingRotation * Vector3.forward;
        stickRigidBody.AddForce(throwDirection * currentPower, ForceMode.Impulse);

        //stickRigidBody.AddTorque(new Vector3(0, 0, spinAmount), ForceMode.Impulse); // Add s

        Debug.Log($"Stick thrown!");
        Debug.Log($"Direction: {throwDirection}");
        Debug.Log($"Power: {currentPower}");

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

    private void UpdateAimingLine()
    {
        aimingLine.positionCount = linePoints;
        Vector3 startPosition = stickStartingPosition.position;
        Quaternion aimRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        Vector3 aimDirection;

        if (!isCharging)
        {
            aimDirection = aimRotation * Vector3.forward * powerHelperValue; // Use the dummy value for drawing the line when aiming
            aimingLine.material = aimingMaterial;
            //aimingLine.material.mainTextureScale = new Vector2(dotOffset, 1);
            //aimingLine.material.mainTextureOffset = new Vector2(dotOffset, 0);
            float offset = Time.time * dotOffset;
            aimingLine.material.mainTextureOffset = new Vector2(offset, 0);
        }
        else
        {
            aimDirection = aimRotation * Vector3.forward * currentPower; // Use the real power value for drawing the line when charging
            aimingLine.material = chargingMaterial;
        }

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * pointSpacing;
            Vector3 point = startPosition + aimDirection * time + 0.5f * Physics.gravity * time * time;
            aimingLine.SetPosition(i, point);
        }
    }

    private void ResetScene()
    {

    }
}
