using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;

    public void SetTotalScoreText(int score)
    {
        totalScoreText.text = $"Total score: " + score.ToString();
    }
}

