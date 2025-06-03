using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private PinController pinController;
    [SerializeField] private UIController uiController;

    [SerializeField] private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
    }
    [SerializeField] private int currentRoundScore = 0;

    private bool isSceneFrozen = false;
    public bool IsSceneFrozen
    {
        get { return isSceneFrozen; }
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
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void CalculateTotalScore()
    {
        if (pinController.FallenPins.Count > 1)
        {
            currentRoundScore += pinController.FallenPins.Count;
        }
        else if (pinController.FallenPins.Count == 1)
        {
            foreach (PinPrefab pin in pinController.FallenPins)
            {
                currentRoundScore += pin.PointValue;
            }
        }

        totalScore += currentRoundScore;
        uiController.SetSceneFreeze(currentRoundScore, totalScore);
    }

    public void ResetUi()
    {
        currentRoundScore = 0;
        uiController.SetResetUi();
    }

}
