using TMPro;
using UnityEngine;

public class RingScoreDisplay : MonoBehaviour
{
    [SerializeField] private RingScore target;
    [SerializeField] private TMP_Text label;
    private int shownScore = -1;

    private void Update()
    {
        if (target == null || label == null || shownScore == target.Score) return;
        shownScore = target.Score;
        label.text = "Score: " + shownScore;
    }
}

