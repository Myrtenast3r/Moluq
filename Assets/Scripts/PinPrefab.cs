using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinPrefab : MonoBehaviour
{
	[SerializeField] private Rigidbody rb;
	public int pointValue;
	private bool throwStarted = false;
	[SerializeField] private Transform pinBase;

	//[SerializeField] private float velocityMagnitudeHelper;
	//[SerializeField] private float angularVelocityMagnitudeHelper;

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

		//velocityMagnitudeHelper = rb.velocity.magnitude;
		//angularVelocityMagnitudeHelper = rb.angularVelocity.magnitude;
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
        Debug.Log($"RaisePin()");

        Vector3 newPosition = pinBase.position;
        newPosition.y += 0.01f;

        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        hasFallen = false;
    }
}
