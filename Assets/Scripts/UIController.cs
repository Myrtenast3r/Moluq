using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private Transform sceneFreezePanel;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI currentRoundScoreText;

    private void Start()
    {
        sceneFreezePanel.gameObject.SetActive(false);
    }

    public void SetSceneFreeze(int currentRoundScore, int totalScore)
    {
        sceneFreezePanel.gameObject.SetActive(true);
        currentRoundScoreText.SetText($"Points gained this round: {currentRoundScore}");
        totalScoreText.SetText($"Total score: {totalScore}");
    }

    public void SetResetUi()
    {
        sceneFreezePanel.gameObject.SetActive(!sceneFreezePanel.gameObject.activeSelf);
    }
}

