using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinController : MonoBehaviour
{
    [SerializeField] private GameObject pinPrefab;
    [SerializeField] private List<PinPrefab> pinPrefabScripts = new List<PinPrefab>();
    [SerializeField] private List<PinPrefab> stoppedPins = new List<PinPrefab>();
    public float timeLimit = 5.0f;
    private float timeSinceThrow = 0f;

    private bool sceneFrozen = false;
    public bool SceneFrozen
    {
        get { return sceneFrozen; }
    }

    [SerializeField] private PlayerInput playerInputScript;

    private void Start()
    {
        if (playerInputScript == null)
        {
            playerInputScript = FindAnyObjectByType<PlayerInput>();
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var instObject = Instantiate(pinPrefab, transform.GetChild(i).transform);
            var instObjectScript = instObject.GetComponent<PinPrefab>();
            pinPrefabScripts.Add( instObjectScript );
            instObjectScript.SetPointValue(i);
        }
    }

    private void Update()
    {

        if (sceneFrozen)
        {
            return;
        }

        bool allPinsStopped = false;

        foreach (PinPrefab pin in pinPrefabScripts)
        {
            if (pin.HasStopped)
            {
                if (!stoppedPins.Contains(pin))
                {
                    stoppedPins.Add(pin);
                }
            }
        }

        if (stoppedPins.Count == pinPrefabScripts.Count)
        {
            allPinsStopped = true;
            Debug.Log("All pins stopped");
        }

        //If all pins stopped, freeze the scene immideately
        if (allPinsStopped && playerInputScript.HasStickStopped)
        {
            Debug.Log($"All pins and stick stopped moving");
            FreezeScene();
        }

        // If movement occurs after time limit. force freeze
        if (playerInputScript.ThrowStarted)
        {
            // Track time 
            timeSinceThrow += Time.deltaTime;
            Debug.Log($"timeSinceThrow: {timeSinceThrow}");

            if (timeSinceThrow >= timeLimit)
            {
                Debug.Log($"Time limit exceeded");
                FreezeScene();
            }
        }
    }

    private void FreezeScene()
    {
        Time.timeScale = 0f; // Pause the scene
        sceneFrozen = true;
        Debug.Log($"Scene frozen!");
    }

    public void ResetScene()
    {   
        foreach (var pin in pinPrefabScripts)
        {
            pin.ResetPin();
        }    
        stoppedPins.Clear();

        Time.timeScale = 1f;
        sceneFrozen = false;
        timeSinceThrow = 0f;
    }

    public void StartThrow()
    {
        foreach (PinPrefab pin in pinPrefabScripts)
        {
            pin.StartThrow();
        }   
    }

}
