using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private Transform sceneFreezePanel;
    [SerializeField] private Transform totalMissesPanel;

    [SerializeField] private GameObject missMarkerPrefab;

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

        SetMissesUI(currentRoundScore);
    }

    public void SetResetUi()
    {
        sceneFreezePanel.gameObject.SetActive(!sceneFreezePanel.gameObject.activeSelf);
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
}

