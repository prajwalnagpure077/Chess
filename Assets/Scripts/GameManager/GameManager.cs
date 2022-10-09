using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static gridGenerator;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] UnityEngine.UI.Image gameFinishPanel;
    [SerializeField] TextMeshProUGUI GameEndMessage;

    public override void Awake()
    {
        Instance = this;
        gameFinishPanel.gameObject.SetActive(false);
        switchPlayerTurn(true);
    }

    public static bool GameContinues(int attackTroop, int defendTroop)
    {
        if (cells[defendTroop].troop.troopType == TroopType.king)
        {
            var _defendTroop = cells[defendTroop].troop;
            var _attackTroop = cells[attackTroop].troop;


            //Draw
            if (_defendTroop.troopType == _attackTroop.troopType)
            {
                Instance.GameEndMessage.text = "Draw";
            }

            //Lost
            else if (_defendTroop.isPlayer)
            {
                Instance.GameEndMessage.text = "You Lost";
            }

            //Won
            else
            {
                Instance.GameEndMessage.text = "You Won";
            }

            Instance.gameFinish();
            return false;
        }

        return true;
    }

    void gameFinish()
    {
        gameFinishPanel.color = new Color(0, 0, 0, 0);
        gameFinishPanel.gameObject.SetActive(true);
        gameFinishPanel.DOColor(Color.Lerp(Color.black, new Color(0, 0, 0, 0), 0.6f), 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void loadScene(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void switchPlayerTurn(bool t)
    {
        gridGenerator.IsPlayerTurn = t;
    }
}