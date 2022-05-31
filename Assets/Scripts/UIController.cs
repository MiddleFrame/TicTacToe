using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{

    /// <summary>
    /// ��������� ��������� � �������� �� ����� ����!
    /// 
    /// </summary>

    [SerializeField] private bool _isNeedGizmos;

    [SerializeField] private Text _playerOneScoreText;
    private static Text _playerOneScoreStat;
    private static int _playerOneScoreCount = 0;

    [SerializeField] private Text _playerTwoScorText;
    private static Text _playerTwoScoreStat;
    private static int _playerTwoScoreCount = 0;


    [Space]
    [Header("LOGS")]
    [SerializeField] private TurnHistorySingleCell _logPref;
    [SerializeField] private Transform _logParent;
    [SerializeField] private Vector2 _defaultScreenResolution;
    [SerializeField] private Vector2 _screenBorderX;
    [SerializeField] private Vector2 _screenBorderY;

    [SerializeField] private GameObject _gameOverPanel;
    private List<TurnHistorySingleCell> _listLog = new List<TurnHistorySingleCell>();
    private float _startPositionX;
    private float _endPositionX;

    private float _startPositionY;
    private float _endPositionY;
    private float _logSize;
    private float _logBeginX;

    public void Initialization()
    {
        _playerOneScoreStat = _playerOneScoreText;
        _playerOneScoreStat.text = "0";
        _playerTwoScoreStat = _playerTwoScorText;
        _playerTwoScoreStat.text = "0";
        InitializeLog();
    }



    private void InitializeLog()
    {
        GetStartPosition();
        InitializeCellSize();
        for (int i = 0; i < 6; i++)
        {
            TurnHistorySingleCell vr = Instantiate(_logPref);
            vr.transform.SetParent(_logParent);
            _listLog.Add(vr);
            _listLog[i].SetSprite((i % 2 == 0) ? CellState.p1 : CellState.p2);
            //_listLog[i].SetTransformPosition(_logBeginX + _logSize * (i), _startPositionY * 0.5f + _endPositionY * 0.5f);
        }
        NewTurn(true);
        int currentPlayer = ((int)_listLog.Count / 2) - 1;
    }

    private void OnDrawGizmos()
    {
        if (_isNeedGizmos)
        {
            float StartPositionX = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth * ((_screenBorderX.x) / _defaultScreenResolution.x), 0)).x;
            float EndPositionX = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth * ((_defaultScreenResolution.x - _screenBorderX.y) / _defaultScreenResolution.x), 0)).x;
            float EndPositionY = Camera.main.ScreenToWorldPoint(new Vector2(0, Camera.main.pixelHeight * ((_defaultScreenResolution.y - _screenBorderY.y) / _defaultScreenResolution.y))).y;
            float StartPositionY = Camera.main.ScreenToWorldPoint(new Vector2(0, Camera.main.pixelHeight * ((_screenBorderY.x / _defaultScreenResolution.y)))).y;


            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector2(StartPositionX, StartPositionY), new Vector2(EndPositionX, StartPositionY));
            Gizmos.DrawLine(new Vector2(EndPositionX, StartPositionY), new Vector2(EndPositionX, EndPositionY));
            Gizmos.DrawLine(new Vector2(EndPositionX, EndPositionY), new Vector2(StartPositionX, EndPositionY));
            Gizmos.DrawLine(new Vector2(StartPositionX, EndPositionY), new Vector2(StartPositionX, StartPositionY));
        }
    }

    public void NewTurn(bool instantly = true)
    {
        int currentPlayer = ((int)_listLog.Count / 2) - 1;
        for (int i = 0; i < _listLog.Count - 1; i++)
        {
            if (i == currentPlayer)
            {
                _listLog[i].SetAlpha(1, instantly);
                _listLog[i].SetTransformSize(_logSize, instantly);
            }
            else
            {
                _listLog[i].SetAlpha(Mathf.Abs((1.0f) / (float)(currentPlayer - i)), instantly);
                _listLog[i].SetTransformSize(Mathf.Abs((1.0f) / (float)(currentPlayer - i)) * _logSize, instantly);
            }
            _listLog[i].SetTransformPosition(_logBeginX + _logSize * (i), _startPositionY * 0.5f + _endPositionY * 0.5f, instantly);

        }
        var k = _listLog[0];

        _listLog[_listLog.Count - 1].SetAlpha(0);
        _listLog[_listLog.Count - 1].SetTransformSize(0);
        _listLog[_listLog.Count - 1].SetTransformPosition(_logBeginX + _logSize * (_listLog.Count - 1), _startPositionY * 0.5f + _endPositionY * 0.5f, instantly);
        _listLog.RemoveAt(0);
        _listLog.Add(k);
    }

    public static void AddScore(int player, int score)
    {
        switch (player)
        {
            case 1:
                _playerOneScoreCount += score;
                if (_playerOneScoreCount >= 100) GameplayManager.Instance.ChangeGameplayState(GameplayState.GameOver);
                _playerOneScoreStat.text = _playerOneScoreCount.ToString();
                break;
            case 2:
                _playerTwoScoreCount += score;

                if (_playerTwoScoreCount >= 100) GameplayManager.Instance.ChangeGameplayState(GameplayState.GameOver);
                _playerTwoScoreStat.text = _playerTwoScoreCount.ToString();
                break;

        }
    }

    private void InitializeCellSize()
    {
        float cellSizeY = ((_endPositionY - _startPositionY) / 1f);
        float cellSizeX = ((_endPositionX - _startPositionX) / 5f);

        _logSize = (cellSizeY < cellSizeX) ? cellSizeY : cellSizeX;

        _logBeginX = _startPositionX + (_endPositionX - _startPositionX - 4 * _logSize) / 2;
    }

    private void GetStartPosition()
    {
        _startPositionX = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth * ((_screenBorderX.x) / _defaultScreenResolution.x), 0)).x;
        _endPositionX = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth * ((_defaultScreenResolution.x - _screenBorderX.y) / _defaultScreenResolution.x), 0)).x;

        _startPositionY = Camera.main.ScreenToWorldPoint(new Vector2(0, Camera.main.pixelHeight * (_screenBorderY.x) / _defaultScreenResolution.y)).y;
        _endPositionY = Camera.main.ScreenToWorldPoint(new Vector2(0, Camera.main.pixelHeight * (_defaultScreenResolution.y - _screenBorderY.y) / _defaultScreenResolution.y)).y;

    }


    public void EndButtonPressed()
    {
        if (TurnController.Instance.CheckCanTurn())
        {
            GameplayManager.Instance.ChangeGameplayState(GameplayState.NewTurn);
        }
    }

    public void StateGameOverPanel(bool state)
    {
        _gameOverPanel.SetActive(state);
    }

    public void RestartGame()
    {
        StateGameOverPanel(false);
        GameplayManager.Instance.ChangeGameplayState(GameplayState.RestartGame);
    }
}
