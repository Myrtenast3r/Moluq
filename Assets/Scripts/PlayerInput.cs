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
    public float maxPower = 50f;
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

    [SerializeField] private Transform stickObject;
    [SerializeField] private Transform stickStartingPosition;
    [SerializeField] private Rigidbody stickRigidBody;


    private void Start()
    {
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

        #endregion
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
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
            verticalAngle += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            verticalAngle -= rotationSpeed * Time.deltaTime;
        }

        stickObject.transform.rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 90);

        Debug.Log($"Horizontal angle = {horizontalAngle}");
        Debug.Log($"Vertical angle = {verticalAngle}");


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

        Debug.Log($"Stick thrown!");
        Debug.Log($"Direction: {throwDirection}");
        Debug.Log($"Power: {currentPower}");

    }
}
