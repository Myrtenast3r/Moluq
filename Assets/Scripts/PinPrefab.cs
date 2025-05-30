using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PinPrefab : MonoBehaviour
{
	[SerializeField] private Rigidbody rb;

	private bool throwStarted = false;
	[SerializeField] private Transform pinBase;

    [SerializeField] private int pointValue;
    public int PointValue
    {
        get { return pointValue; }
    }

    //public float spinAmount = 0.3f;

    public float spacingOffset = 0.01f;
    public float minSpacingRadius = 0.025f;

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
    }

    private void Update()
    {
		if (!throwStarted) {
			return;
		}

		if (transform.up.y < 0.1f)
		{
			hasFallen = true;
		}

		if (throwStarted)
		{
			CheckMovement();
		}
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
        if (hasFallen)
        {
           // Debug.Log($"Adjusting spacing for fallen pin {this.transform.parent.name}");
            AdjustPinSpacing();
        }
        else
        {
            Vector3 newPosition = new Vector3(pinBase.position.x, 0.01f, pinBase.position.z);

            transform.position = newPosition;
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        throwStarted = false;
        hasFallen = false;
        hasStopped = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("GroundLayer"))
        {
            return;
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
        Collider[] overlappingPins = Physics.OverlapSphere(transform.position, minSpacingRadius);
        int overlappingCount = overlappingPins.Count(collider => collider.gameObject != this && collider.gameObject.tag != "Stick");

        //if (overlappingCount == 0) 
        //{
        //    Debug.Log($"No overlapping pins, reset position without adjusting");
        //    transform.position = new Vector3(pinBase.position.x, 0.01f, pinBase.position.z);
        //}
        //else
        //{

        //}
        //Debug.Log($"Overlapping colliders {overlappingCount}, adjusting position for {this.transform.parent.name}");
        Vector3 bestDirection = Vector3.zero;
        float maxOpenSpace = 0f;

        // Analyze the surrounding area in multiple directions
        Vector3[] possibleDirections = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

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

            // Move the pin toward the most open space
            transform.position += bestDirection * spacingOffset;
        }
        //transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}
