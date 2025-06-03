using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private PinController pinController;
    [SerializeField] private UIController uiController;

    [SerializeField] public int missCounter = 0;
    [SerializeField] private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
        set { totalScore = value; }
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
            missCounter = 0;
        }
        else if (pinController.FallenPins.Count == 1)
        {
            foreach (PinPrefab pin in pinController.FallenPins)
            {
                currentRoundScore += pin.PointValue;
            }
            missCounter = 0;
        }
        else
        {
            missCounter++;
        }

        totalScore += currentRoundScore;

        uiController.SetSceneFreeze(currentRoundScore, totalScore, missCounter);

        if (totalScore > 50)
        {
            totalScore = 25;
        }

    }

    public void ResetUi()
    {
        currentRoundScore = 0;
        uiController.SetResetUi();
    }

}
