using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PinPrefab : MonoBehaviour
{
    [Header("Physics")]
	[SerializeField] private Rigidbody rb;
	[SerializeField] private Transform pinBase;
    [SerializeField] private MeshCollider pinCollider;
    private Camera mainCam;

    [Header("UI related")]
    [SerializeField] private Canvas pointCanvas;
    [SerializeField] private TextMeshProUGUI pointText;
    private LineRenderer uilineRenderer;

    [Header("Public variables")]
    public float spacingOffset = 0.01f;
    public float minSpacingRadius = 0.025f;
    public float positioningTime = 0.5f;
    [SerializeField] private int pointValue;
    public int PointValue
    {
        get { return pointValue; }
    }

    [Header("Booleans")]
    private bool throwStarted = false;
    [SerializeField] private bool hasFallen;
	public bool HasFallen
	{
		get { return hasFallen; }
		set { hasFallen = value; }
	}

	[SerializeField] private bool hasStopped;
	public bool HasStopped
	{
		get { return hasStopped; }
		set { hasStopped = value; }
	}

    private void Start()
    {
		if (rb == null)
		{
            rb = GetComponent<Rigidbody>();
        }

        mainCam = Camera.main;
        pinCollider = GetComponent<MeshCollider>();


        uilineRenderer = gameObject.AddComponent<LineRenderer>();
        uilineRenderer.positionCount = 2;
        uilineRenderer.startWidth = 0.02f;
        uilineRenderer.endWidth = 0.02f;
        uilineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        pointText.SetText(pointValue.ToString());
        SetPointUI();
    }

    private void Update()
    {
		if (!throwStarted) {
			return;
		}

		if (transform.up.y < 0.1f && !hasFallen)
		{
			hasFallen = true;
            AudioManager.instance.PlayPinHitGround(); 
		}

		if (throwStarted)
		{
			CheckMovement();
		}
    }

    private void LateUpdate()
    {
        SetPointUI();
    }

    public void SetPointValue(int value)
    {
        pointValue = value + 1;
    }

	public void StartThrow()
	{
		StartCoroutine(DelayMovementCheck());
	}

	private IEnumerator DelayMovementCheck()
	{
		yield return new WaitForSeconds(0.5f);
		throwStarted = true;
		CheckMovement();
	}

	private void CheckMovement()
	{
        LimitPinRolling();

        //check the pin movement
        if (rb.velocity.magnitude < 0.05f && rb.angularVelocity.magnitude < 0.05f)
        {
            hasStopped = true;
        }
        else
        {
            hasStopped = false;
        }
    }

	public void ResetPin()
	{
        // Disable collider during lerping to avoid unexpected behaviour!! 
        //MeshCollider collider = GetComponent<MeshCollider>();
        //collider.enabled = false;
        pinCollider.enabled = false;

        rb.useGravity = false;

        if (hasFallen)
        {
            AdjustPinSpacing();
        }
        else
        {
            Vector3 newPosition = new Vector3(pinBase.position.x, 0.11f, pinBase.position.z);
            StartCoroutine(InterpolatePosition(transform.position, newPosition, positioningTime));
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        //collider.enabled = true;
        //pinCollider.enabled = true;
        //rb.useGravity = true;

        throwStarted = false;
        hasFallen = false;
        hasStopped = false;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Pin"))
        {
            if (hasFallen)
            {
                AudioManager.instance.PlayWoodHitWood();
            }
        }

        Vector3 randomTorque = new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f)
            );

        rb.AddTorque(randomTorque, ForceMode.Impulse);

    }

    private void LimitPinRolling()
    {
        if (hasFallen)
        {
            if (rb.velocity.magnitude < 0.2f)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void AdjustPinSpacing()
    {
        Vector3 bestDirection = Vector3.zero;
        float maxOpenSpace = float.MaxValue;
        Vector3[] possibleDirections = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        Collider[] overlappingPins = Physics.OverlapSphere(transform.position, minSpacingRadius);
        int overlappingCount = overlappingPins.Count(collider => collider.gameObject != this && collider.gameObject.tag != "Stick");

        foreach (Vector3 direction in possibleDirections)
        {
            Vector3 testPosition = transform.position + (direction * spacingOffset);
            int nearbyCount = Physics.OverlapSphere(testPosition, minSpacingRadius).Length;

            // Find the direction with the least nearby collisions
            if (nearbyCount < maxOpenSpace)
            {
                maxOpenSpace = nearbyCount;
                bestDirection = direction;
            }
        }

        Vector3 newPosition = transform.position + bestDirection * spacingOffset;
        StartCoroutine(InterpolatePosition(transform.position, new Vector3(newPosition.x, 0.11f, newPosition.z), positioningTime));

    }

    private IEnumerator InterpolatePosition(Vector3 start, Vector3 end, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        pinCollider.enabled = true;
        rb.useGravity = true;
        //Debug.Log($"resetting pins done");
    }

    private void SetPointUI()
    {
        float distanceFromCamera = Vector3.Distance(mainCam.transform.position, transform.position);
        float heightOffset = Mathf.Clamp(distanceFromCamera * 0.15f, 0.3f, 2f);

        Vector3 uiPosition = transform.position + new Vector3(0, heightOffset, 0f);
        pointCanvas.transform.position = uiPosition;

        Vector3 direction = mainCam.transform.position - pointCanvas.transform.position;
        direction.y = 0f;
        direction *= -1f; // Flip so that it faces the camera

        pointCanvas.transform.rotation = Quaternion.LookRotation(direction);

        if (uilineRenderer != null && pointCanvas != null)
        {
            uilineRenderer.SetPosition(0, this.transform.position);
            uilineRenderer.SetPosition(1, pointCanvas.transform.position);
        }
    }
}
