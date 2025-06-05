using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickPrefab : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pin"))
        {
            Debug.Log($"Stick hit pin");
            AudioManager.instance.PlayWoodHitWood();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("GroundLayer"))
        {
            Debug.Log($"Stick hit ground");
            AudioManager.instance.PlayStickHitGround();
        }
    }
}
