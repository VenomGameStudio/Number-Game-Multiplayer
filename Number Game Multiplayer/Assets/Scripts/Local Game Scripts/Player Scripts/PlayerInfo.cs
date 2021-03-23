using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text score;
    [SerializeField] private Button playBtn;

    private int randomNum;

    public void OnClickPlayBtn()
    {
        randomNum = Random.Range(0, 10);
        score.text = randomNum.ToString();
    }
}
