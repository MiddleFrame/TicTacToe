using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnHistorySingleCell : MonoBehaviour
{
    const float _alphaSpeed = 4;
    const float _positionSpeed = 4;
    const float _scaleSpeed = 4;

    private  Image _sprt;

    public float CellSize
    {
        get { return _cellSize; }
    }
    private float _cellSize;

    private bool _isSizeCoroutineWork = false;

    public Vector2 Position
    {
        get { return _position; }
    }
    private Vector2 _position;
    private bool _isPositionCoroutineWork = false;

    public float Alpha
    {
        get { return _alpha; }
        set { _alpha = value; }
    }
    private float _alpha;
    private bool _isAlphaCoroutineWork = false;

    private Transform _trns;

    private CellState _currentState;
    public CellState CurrentState
    {
        get { return _currentState; }
    }

    private void Awake()
    {
        _sprt = GetComponent<Image>();
        _trns = GetComponent<Transform>();
    }

    public void SetSprite(CellState i)
    {
        _currentState = i;
        _sprt.sprite = ThemeManager.Instance.GetSprite(i);
    }

    public void SetAlpha(float fAlpha, bool instantly = true)
    {
        _alpha = fAlpha;


        if (instantly)
        {
            Color vr = _sprt.color;
            vr.a = _alpha;
            _sprt.color = vr;
        }
        else if (!_isAlphaCoroutineWork) StartCoroutine(AlphaIEnumerator());
    }

    public void SetTransformSize(float s, bool instantly = true)
    {
        _cellSize = s;
        if (instantly) _trns.localScale = new Vector2(s, s);
        else if (!_isSizeCoroutineWork) StartCoroutine(ScaleIEnumerator());
    }


    public void SetTransformPosition(float x, float y, bool instantly = true)
    {
        _position = new Vector2(x, y);
        if (instantly) _trns.position = _position;
        else if (!_isPositionCoroutineWork) StartCoroutine(PositionIEnumerator());
    }

    private IEnumerator AlphaIEnumerator()
    {
        _isAlphaCoroutineWork = true;
        float prevS = _alpha;
        Color vr = _sprt.color;
        float step = (prevS - vr.a) / 100f * _alphaSpeed;
        int i = 0;
        while (i <= 100/ _alphaSpeed)
        {
            vr = _sprt.color;
            if (prevS != _alpha)
            {
                prevS = _alpha;
                step = (prevS - vr.a) / 100f * _alphaSpeed;
                i = 0;
            }
            vr.a += step;
            _sprt.color = vr;
            i++;
            yield return null;
        }
        _isAlphaCoroutineWork = false;
        yield break;
    }

    private IEnumerator PositionIEnumerator()
    {
        _isPositionCoroutineWork = true;
        Vector2 prevPos = _position;

        Vector2 currentPosition = _trns.position;
        Vector2 step = (prevPos - currentPosition) / 100f * _positionSpeed;
        int i = 0;
        while (i <= 100/ _positionSpeed)
        {
            currentPosition = _trns.position;
            if (prevPos != _position)
            {
                prevPos = _position;

                step = (prevPos - currentPosition) / 100f * _positionSpeed;
                i = 0;
            }

            _trns.position = currentPosition + step;
            i++;
            yield return null;
        }
        _trns.position = _position;
        _isPositionCoroutineWork = false;
        yield break;
    }

    private IEnumerator ScaleIEnumerator()
    {
        _isSizeCoroutineWork = true;
        float prevS = _cellSize;
        float step = (_cellSize - _trns.localScale.x) / 100f * _scaleSpeed;
        int i = 0;
        while (i <= 100/ _scaleSpeed)
        {
            if (prevS != _cellSize)
            {
                prevS = _cellSize;
                step = (_cellSize - _trns.localScale.x) / 100f * _scaleSpeed;
                i = 0;
            }
            _trns.localScale = _trns.localScale + new Vector3(step, step);
            i++;
            yield return null;
        }
        _isSizeCoroutineWork = false;
        yield break;
    }
}