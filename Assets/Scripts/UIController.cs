using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private Transform sceneFreezePanel;
    [SerializeField] private Button sceneResetButton;
    [SerializeField] private Transform totalMissesPanel;
    [SerializeField] private Transform gameStatusPanel;
    [SerializeField] private Button actionButton;

    [SerializeField] private GameObject missMarkerPrefab;

    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI currentRoundScoreText;
    [SerializeField] private TextMeshProUGUI gameStatusMessageText;

    private bool isReloadGame = false;

    [SerializeField] private PlayerInput playerInputScript;

    private void Start()
    {
        sceneFreezePanel.gameObject.SetActive(false);
        gameStatusPanel.gameObject.SetActive(false);
        totalScoreText.SetText($"Throw at the pins!");

        sceneResetButton.onClick.AddListener(OnSceneResetButtonClick);
        actionButton.onClick.AddListener(OnActionButtonClick);
    }

    public void SetSceneFreeze(int currentRoundScore, int totalScore, int missCounter)
    {
        sceneFreezePanel.gameObject.SetActive(totalScore < 50 && missCounter < 3);
        currentRoundScoreText.SetText($"Points gained this round: {currentRoundScore}");
        totalScoreText.SetText($"Total score: {totalScore}");

        SetMissesUI(currentRoundScore);
        SetGameStatusUI(totalScore, missCounter);
    }

    public void SetResetUi()
    {
        sceneFreezePanel.gameObject.SetActive(false);
    }

    private void SetMissesUI(int roundScore)
    {
        if (roundScore == 0)
        {
            Instantiate(missMarkerPrefab, totalMissesPanel);
        }
        else
        {
            foreach (Transform child in totalMissesPanel)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void SetGameStatusUI(int totalScore, int missCounter)
    {

        if (totalScore < 50)
        {
            if (missCounter < 3)
            {
                return;
            }
            else
            {
                gameStatusPanel.gameObject.SetActive(true);
                gameStatusMessageText.SetText($"Three consecutive misses. Game over.");
                isReloadGame = true;
                return;
            }
        }
        else if (totalScore == 50)
        {
            gameStatusPanel.gameObject.SetActive(true);
            gameStatusMessageText.SetText($"Exactly 50 points!. You are the winner, Congratulations!");
            isReloadGame = true;
        }
        else
        {
            gameStatusPanel.gameObject.SetActive(true);
            gameStatusMessageText.SetText($"Total score: {totalScore - 50} over 50. Reducing total score back to 25.");
        }
    }

    private void OnSceneResetButtonClick()
    {
        playerInputScript.ResetSceneFromUI();
    }

    private void OnActionButtonClick()
    {
        if (!isReloadGame)
        {
            gameStatusPanel.gameObject.SetActive(false);
            ScoreManager.Instance.TotalScore = 25;
            OnSceneResetButtonClick();
            totalScoreText.SetText($"Total score: {25}");
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }
}

