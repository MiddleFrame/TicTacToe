using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotManager : Singleton<SlotManager>
{
    #region Field
    /// <summary>
    /// ����� �� ���������� �������
    /// </summary>
    [SerializeField, Tooltip("����� �� ���������� �������")]
    private bool _isNeedGizmos = true;

    /// <summary>
    /// ������ �� ������ � ��������
    /// </summary>
    [SerializeField, Tooltip("������ �� ������ � ��������")]
    private Transform _transformParent;

    [Space]
    [Space]


    /// <summary>
    /// ������ �� ������ �������
    /// </summary>
    [SerializeField, Tooltip("������ �� ������ �������")]
    private float _buttonBorder;

    /// <summary>
    /// ������ �� ������ ������ �� �����
    /// </summary>
    [SerializeField, Tooltip("������ �� ������ ������ �� �����")]
    private float _widthBorder;

    [Space]
    [Space]

    /// <summary>
    /// ������ ����� �����
    /// </summary>
    [SerializeField, Tooltip("������ ����� �����")]
    private float _widthCard;


    /// <summary>
    /// ������� ������
    /// </summary>
    [SerializeField, Tooltip("������� ������ (�� X)")]
    private float _deckPosition;

    /// <summary>
    /// ������ ���� ����
    /// </summary>
    [SerializeField, Tooltip("������ ���� ����")]
    private float _angleDelta;

    /// <summary>
    /// ������ ������ ����
    /// </summary>
    [SerializeField, Tooltip("������ ������ ����")]
    private float _heightDelta;

    /// <summary>
    /// ���������� ������
    /// </summary>
    [SerializeField, Tooltip("������������ ���������� ������")]
    private int _slotsCount;

    /// <summary>
    /// ������ ��������� �����
    /// </summary>
    [SerializeField, Tooltip("������ ��������� �����")]
    private int _rechangerHeight;

    /// <summary>
    /// �������������
    /// </summary>
    [SerializeField, Tooltip("�������������")]
    private Rechanger _rechanger;

    /// <summary>
    /// ������� ���� �������� � ���
    /// </summary>
    [SerializeField, Tooltip("������� ���� �������� � ���")]
    private int _cardPerTurn;

    /// <summary>
    /// ����� ������ ������ ������������
    /// </summary>
    private PlayerInfo _currentPlayerSet;

    public PlayerInfo CurrentPlayerSet
    {
        get { return _currentPlayerSet; }
    }


    /// <summary>
    /// ����������� �� ���������� �� ���� ����
    /// </summary>
    private bool _isRechangerUsed = false;



    #endregion 

    private void OnDrawGizmos()
    {
        if (_isNeedGizmos)
        {
            float PositionY = ScreenManager.Instance.GetHeight(_buttonBorder);
            float StepX = (Camera.main.pixelWidth - _widthBorder * 2 + _widthCard * _slotsCount) / (_slotsCount + 1);
            Gizmos.color = Color.green;
            for (int i = 0; i < _slotsCount; i++)
            {
                float posY = PositionY + (Mathf.Sin(Mathf.PI * (i + 1) / (_slotsCount + 1))) * _heightDelta;
                Gizmos.DrawCube(Camera.main.ScreenToWorldPoint(new Vector2(_widthBorder - _widthCard * _slotsCount / 2 + StepX * (i + 1), posY)), Vector3.one / 2);
            }
            Gizmos.DrawLine(Camera.main.ScreenToWorldPoint(new Vector2(_widthBorder - _widthCard * _slotsCount / 2, PositionY)),
                Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth - _widthBorder + _widthCard * _slotsCount / 2, PositionY)));
        }
    }

    public void AddCard(PlayerInfo player)
    {
        if (player.HandPool.Count >= _slotsCount) return;
        if (player.DeckPool.Count == 0) return;

        int endBorder = player.DeckPool.Count;
        if (endBorder != 1) endBorder -= 1;

        int card = Random.Range(0, endBorder);
        player.HandPool.Add(player.DeckPool[card]);
        player.DeckPool.RemoveRange(card, 1);
        card = player.HandPool.Count - 1;
        player.HandPool[card].SetTransformParent(_transformParent);
        player.HandPool[card].SetTransformPosition(ScreenManager.Instance.GetWidth(_deckPosition), ScreenManager.Instance.GetHeight(_buttonBorder));
        player.HandPool[card].SetTransformRotation(0);

        player.HandPool[card].gameObject.SetActive(true);
        PrintCArd();

    }
    public void RemoveCard(PlayerInfo player, int id)
    {
        if (player.HandPool.Count == 0) return;
        if (id >= player.HandPool.Count) return;

        player.DeckPool.Add(player.HandPool[id]);
        player.HandPool.RemoveRange(id, 1);
        player.DeckPool[player.DeckPool.Count - 1].gameObject.SetActive(false);
        player.DeckPool[player.DeckPool.Count - 1].SetTransformRotation(0);
        player.DeckPool[player.DeckPool.Count - 1].SetTransformParent(CardManager.Instance.transform);
    }

    public void RemoveCard(PlayerInfo player, Card card)
    {
        if (player.HandPool.Count == 0) return;
        if (!player.HandPool.Contains(card)) return;

        player.DeckPool.Add(card);
        player.HandPool.Remove(card);
        player.DeckPool[player.DeckPool.Count - 1].gameObject.SetActive(false);
        player.DeckPool[player.DeckPool.Count - 1].SetTransformRotation(0);
        player.DeckPool[player.DeckPool.Count - 1].SetTransformParent(CardManager.Instance.transform);
    }

    private void PrintCArd()
    {
        for (int i = 0; i < PlayerManager.Instance.GetCurrentPlayer().HandPool.Count; i++)
        {
            Debug.LogFormat("[{0}/{1}] ����� � ���� � ��������� {2} ", i + 1, PlayerManager.Instance.GetCurrentPlayer().HandPool.Count, PlayerManager.Instance.GetCurrentPlayer().HandPool[i].Info.CardName);
        }
        for (int i = 0; i < PlayerManager.Instance.GetCurrentPlayer().DeckPool.Count; i++)
        {
            Debug.LogFormat("[{0}/{1}] ����� � ������ � ��������� {2} ", i + 1, PlayerManager.Instance.GetCurrentPlayer().DeckPool.Count, PlayerManager.Instance.GetCurrentPlayer().DeckPool[i].Info.CardName);
        }
    }


    /*    private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddCard(PlayerManager.Instance.GetCurrentPlayer());
                _rechanger.Show();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                RemoveCard(PlayerManager.Instance.GetCurrentPlayer(), 0);
                _rechanger.Hide();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UpdateCardPosition(false);
            }
        }
    */
    public void ResetRechanher()
    {
        _isRechangerUsed = false;
        //Debug.Log(_isRechangerUsed);
    }

    public void UpdateCardPosition(bool instantly = true, Card card = null)
    {
        float currentCount = PlayerManager.Instance.GetCurrentPlayer().HandPool.Count;
        int curIndex = PlayerManager.Instance.GetCurrentPlayer().HandPool.IndexOf(card);
        if (curIndex != -1)
        {
            currentCount -= 1;
            PlayerManager.Instance.GetCurrentPlayer().HandPool.Remove(card);

            PlayerManager.Instance.GetCurrentPlayer().HandPool.Add(card);
        }

        float PositionY = ScreenManager.Instance.GetHeight(_buttonBorder);
        float StepPos = (ScreenManager.Instance.ScreenDefault.x - _widthBorder * 2 + _widthCard * currentCount) / (currentCount + 1);
        float StepRot = (_angleDelta * 2) / (currentCount + 1);

        for (int i = 0; i < currentCount; i++)
        {
            float posY = PositionY + (Mathf.Sin(Mathf.PI * (i + 1) / (currentCount + 1))) * _heightDelta * ScreenManager.Instance.GetHeightRatio();
            Vector2 finPosition = new Vector2((_widthBorder + StepPos * (i + 1) - _widthCard * currentCount / 2) * ScreenManager.Instance.GetWidthRatio(), posY);
            PlayerManager.Instance.GetCurrentPlayer().HandPool[i].HandPosition = finPosition;
            PlayerManager.Instance.GetCurrentPlayer().HandPool[i].SetTransformPosition(finPosition.x, finPosition.y, instantly);

            PlayerManager.Instance.GetCurrentPlayer().HandPool[i].SetTransformSize(0.7f, instantly);

            PlayerManager.Instance.GetCurrentPlayer().HandPool[i].HandRotation = _angleDelta - StepRot * (i + 1);
            PlayerManager.Instance.GetCurrentPlayer().HandPool[i].SetTransformRotation(_angleDelta - StepRot * (i + 1), instantly);


            PlayerManager.Instance.GetCurrentPlayer().HandPool[i].SetSideCard(PlayerManager.Instance.GetCurrentPlayer().SideId);


        }
    }  
    
    public void UpdateCardPosition(PlayerInfo player, bool instantly = true, Card card = null)
    {
        if (player == null) return;
        float currentCount = player.HandPool.Count;
        int curIndex = player.HandPool.IndexOf(card);
        if (curIndex != -1)
        {
            currentCount -= 1;
            player.HandPool.Remove(card);

            player.HandPool.Add(card);
        }

        float PositionY = ScreenManager.Instance.GetHeight(_buttonBorder);
        float StepPos = (ScreenManager.Instance.ScreenDefault.x - _widthBorder * 2 + _widthCard * currentCount) / (currentCount + 1);
        float StepRot = (_angleDelta * 2) / (currentCount + 1);

        for (int i = 0; i < currentCount; i++)
        {
            float posY = PositionY + (Mathf.Sin(Mathf.PI * (i + 1) / (currentCount + 1))) * _heightDelta * ScreenManager.Instance.GetHeightRatio();
            Vector2 finPosition = new Vector2((_widthBorder + StepPos * (i + 1) - _widthCard * currentCount / 2) * ScreenManager.Instance.GetWidthRatio(), posY);
            player.HandPool[i].HandPosition = finPosition;
            player.HandPool[i].SetTransformPosition(finPosition.x, finPosition.y, instantly);

            player.HandPool[i].SetTransformSize(0.7f, instantly);

            player.HandPool[i].HandRotation = _angleDelta - StepRot * (i + 1);
            player.HandPool[i].SetTransformRotation(_angleDelta - StepRot * (i + 1), instantly);

            player.HandPool[i].SetSideCard(player.SideId);
        }
    }

    public void NewTurn(PlayerInfo player)
    {
        if (player.EntityType == PlayerType.AI)
        {
            for (int i = 0; i < _currentPlayerSet.HandPool.Count; i++)
            {
                _currentPlayerSet.HandPool[i].CancelDragging();
            }
            return;
        }
        if (_currentPlayerSet != null)
        {
            for (int i = 0; i < _currentPlayerSet.HandPool.Count; i++)
            {
                _currentPlayerSet.HandPool[i].gameObject.SetActive(false);
            }
        }
        _currentPlayerSet = player;
        for (int i = 0; i < _currentPlayerSet.HandPool.Count; i++)
        {
            _currentPlayerSet.HandPool[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < _cardPerTurn; i++)
        {
            SlotManager.Instance.AddCard(_currentPlayerSet);
        }

        SlotManager.Instance.UpdateCardPosition(false);
        ResetRechanher();
    }


    public void ShowRechanger()
    {
        Debug.Log(_isRechangerUsed);

        if (!_isRechangerUsed)
        {
            _rechanger.Show();
        }
    }

    public void HideRechanger()
    {

        _rechanger.Hide();
    }

    public void UseRechanger(Card card)
    {
        if (!_isRechangerUsed)
        {
            _isRechangerUsed = true;
            RemoveCard(PlayerManager.Instance.GetCurrentPlayer(), card);
            AddCard(PlayerManager.Instance.GetCurrentPlayer());
            UpdateCardPosition(false);
        }
    }

    public bool IsOnRechanger(float posY)
    {
        if (_isRechangerUsed) return false;
        return posY < _rechanger.Height;
    }

    public void ResetHandPool(PlayerInfo player)
    {
        while (player.HandPool.Count > 0)
        {
            Debug.Log(player.HandPool[0].Info.name);
            RemoveCard(player, player.HandPool[0]);
        }
    }
}
