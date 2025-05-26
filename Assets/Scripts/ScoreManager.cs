using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private PinController pinController;

    [SerializeField] private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (pinController == null)
        {
            pinController = FindAnyObjectByType<PinController>();
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void CalculateScore()
    {
        if (pinController.FallenPins.Count > 1)
        {
            totalScore += pinController.FallenPins.Count;
        }
        else if (pinController.FallenPins.Count == 1)
        {
            foreach (PinPrefab pin in pinController.FallenPins)
            {
                totalScore += pin.PointValue;
                Debug.Log($"Added {pin.PointValue} points to total score");
            }
        }
        else
            return;

        Debug.Log($"total score: {totalScore}");
    }
}
