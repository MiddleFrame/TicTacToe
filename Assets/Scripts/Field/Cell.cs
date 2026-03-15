using System.Collections;
using Coroutine.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Theme.Interfaces;
using Zenject;



public class Cell : MonoBehaviour
{
    private const float SCALE_COUNT_FRAME = 25;
    private const float POSITION_COUNT_FRAME = 25;
    private const float FIGURE_COUNT_FRAME = 25;

    private const float FIGURE_SCALE_DURATION = 0.6f;

    #region Dependency

    private ICoroutineService _coroutineService;
    private IThemeService _themeService;
    
    [Inject]
    private void Construct(ICoroutineService coroutineService,IThemeService themeService)
    {
        _coroutineService = coroutineService;
        _themeService = themeService;
    }

    #endregion
 

    public static float AnimationTime
    {

        get => Mathf.Max(SCALE_COUNT_FRAME, POSITION_COUNT_FRAME, FIGURE_COUNT_FRAME);
    }


    public float CellSize
    {
        get { return _cellSize; }
    }
    private float _cellSize;

    private bool _isSizeCoroutineWork;

    public Vector2 Position
    {
        get { return _position; }
    }
    private Vector2 _position;
    private bool _isPositionCoroutineWork;
    private bool _isFigureCoroutineWork = false;

    public Vector2Int Id
    {
        get { return _id; }
        set { _id = value; }
    }
    private Vector2Int _id;

    private RectTransform _transformRect;

    public CellFigure Figure
    {
        get { return _figure; }
    }

    private CellFigure _figure;

    public CellSubState SubState
    {
        get { return _subState; }
    }

    private CellSubState _subState;

    private bool _isCellClear;

    private Image _image;
    private Image _subImage;
    private Image _highlightImage;

    void Awake()
    {
        _transformRect = GetComponent<RectTransform>();
        _subImage = transform.GetChild(0).GetComponent<Image>();
        _highlightImage = transform.GetChild(1).GetComponent<Image>();
        _image = transform.GetChild(2).GetComponent<Image>();
    }

    public void SetFigure(int s, bool isNeedPlace = true, bool isQueue = true)
    {
        if (!isNeedPlace)
            _figure = (CellFigure)s;

        switch ((CellFigure)s)
        {
            case CellFigure.None:
                if (isNeedPlace)
                {
                    if (isQueue) _coroutineService.AddCoroutine(IFigureFillProcess((CellFigure)s, true));
                    else StartCoroutine(IFigureFillProcess((CellFigure)s, true));
                }
                break;
            case CellFigure.P1:
                _image.fillMethod = Image.FillMethod.Vertical;
                //_image.fillOrigin = 0; 
                if (isNeedPlace)
                {
                    if (isQueue) _coroutineService.AddCoroutine(IFigureFillProcess((CellFigure)s, false));
                    else StartCoroutine(IFigureFillProcess((CellFigure)s, false));
                }
                break;
            case CellFigure.P2:
                _image.fillMethod = Image.FillMethod.Radial360;
                //_image.fillOrigin = 0;
                if (isNeedPlace)
                {
                    if (isQueue) _coroutineService.AddCoroutine(IFigureFillProcess((CellFigure)s, false));
                    else StartCoroutine(IFigureFillProcess((CellFigure)s, false));
                }
                break;

        }
    }

    public void SetFigure(CellFigure s, bool isNeedPlace = true, bool isQueue = true)
    {
        _figure = s;
        switch (s)
        {
            case CellFigure.None:
                if (isNeedPlace)
                {
                    if (isQueue) _coroutineService.AddCoroutine(IFigureFillProcess(s, true));
                    else StartCoroutine(IFigureFillProcess(s, true));
                }
                break;
            case CellFigure.P1:
                _image.fillMethod = Image.FillMethod.Vertical;
                //_image.fillOrigin = 0;
                if (isNeedPlace)
                {
                    if (isQueue) _coroutineService.AddCoroutine(IFigureFillProcess(s, false));
                    else StartCoroutine(IFigureFillProcess(s, false));
                }
                break;
            case CellFigure.P2:
                _image.fillMethod = Image.FillMethod.Radial360;
                //_image.fillOrigin = 0;
                if (isNeedPlace)
                {
                    if (isQueue) _coroutineService.AddCoroutine(IFigureFillProcess(s, false));
                    else StartCoroutine(IFigureFillProcess(s, false));
                }
                break;


        }
    }

    public void SetTransformSize(float reals, bool instantly = true)
    {
        _cellSize = reals;
        if (instantly) _transformRect.sizeDelta = new Vector2(_cellSize, _cellSize);
        else if (!_isSizeCoroutineWork) StartCoroutine(ScaleIEnumerator());
    }

    public void SetTransformParent(Transform parent)
    {
        _transformRect.SetParent(parent);
        SetTransformPosition(0, 0);
        _transformRect.localScale = Vector3.one;
    }

    public void SetTransformPosition(float x, float y, bool instantly = true)
    {
        _position = new Vector3(x, y, 0);
        if (instantly)
        {
            _transformRect.localPosition = _position;
        }
        else if (!_isPositionCoroutineWork) StartCoroutine(PositionIEnumerator());
    }

    public void SetSubState(Sprite sprite, Color color, CellSubState cellSubState, bool isQueue = true)
    {
        if (_subState == cellSubState && _subImage.sprite == sprite) return;
        _subState = cellSubState;
        if (isQueue) _coroutineService.AddCoroutine(ISubStateFillProcess(sprite, color, false));
        else StartCoroutine(ISubStateFillProcess(sprite, color, false));
    }

    public void ResetSubState(bool isQueue = true)
    {
        _subState = CellSubState.None;
        Color color = Color.white;
        color.a = 0;
        if (isQueue) _coroutineService.AddCoroutine(ISubStateFillProcess(null, color, true));
        else StartCoroutine(ISubStateFillProcess(null, color, true));
    }


    public void ResetSubStateWithPlace(CellFigure cellFigure)
    {
        Color color = Color.white;
        color.a = 0;
        _subState = CellSubState.None;
        SetFigure(cellFigure, false);
        StartCoroutine(IQueueCoroutineInCell(
             ISubStateFillProcess(null, color, true),
             IFigureFillProcess(cellFigure, false)
             ));
    } 
    
    public void ResetFigureWithPlaceState(Sprite sprite,
                                Color color,
                                CellSubState cellSub)
    {
        
        _subState = cellSub;
        SetFigure(CellFigure.None, false);
        StartCoroutine(IQueueCoroutineInCell(
            IFigureFillProcess(CellFigure.None,true),
            ISubStateFillProcess(sprite,color,false)
             ));
    }

    public void HighlightCell(Sprite sprite, Color color)
    {
        _highlightImage.sprite = sprite;
        _highlightImage.color = color;
    }

    public void UnhighlightCell()
    {
        _highlightImage.sprite = null;
        Color color = Color.white;
        color.a = 0;
        _highlightImage.color = color;
    }

    private IEnumerator ScaleIEnumerator()
    {
        _isSizeCoroutineWork = true;
        float prevS = _cellSize;
        float step = (_cellSize - _transformRect.sizeDelta.x) /SCALE_COUNT_FRAME;
        int i = 0;
        while (i < SCALE_COUNT_FRAME)
        {
            if (prevS != _cellSize)
            {
                prevS = _cellSize;
                step = (_cellSize - _transformRect.sizeDelta.x) / SCALE_COUNT_FRAME;
                i = 0;
            }
            _transformRect.sizeDelta += new Vector2(step, step);
            i++;
            yield return null;
        }
        _transformRect.sizeDelta += new Vector2(step, step);
        _isSizeCoroutineWork = false;
        yield break;
    }

    private IEnumerator PositionIEnumerator()
    {
        _isPositionCoroutineWork = true;
        Vector2 prevPos = _position;

        Vector2 currentPosition = _transformRect.localPosition;
        Vector2 step = (prevPos - currentPosition) / POSITION_COUNT_FRAME;
        int i = 0;
        while (i < POSITION_COUNT_FRAME)
        {
            currentPosition = _transformRect.localPosition;
            if (prevPos != _position)
            {
                prevPos = _position;

                step = (prevPos - currentPosition) / POSITION_COUNT_FRAME;
                i = 0;
            }

            _transformRect.localPosition = currentPosition + step;
            i++;
            yield return null;
        }
        _transformRect.localPosition = _position;
        _isPositionCoroutineWork = false;
        yield break;
    }

    private IEnumerator IFigureFillProcess(CellFigure s, bool reverse)
    /*
    {        
        if (!reverse)
        {
            _image.sprite = _themeService.GetSprite(s);

        }
        _isFigureCoroutineWork = true;
        _image.fillAmount = (reverse) ? 1 : 0;
        float step = 1 / FIGURE_COUNT_FRAME;
        int i = 0;
        while (i <FIGURE_COUNT_FRAME)
        {
            _image.fillAmount += (reverse) ? -step : step;
            i++;
            yield return null;
        }
        _image.fillAmount = (reverse) ? 0 : 1;
        if (reverse) _image.sprite = _themeService.GetSprite(s);
        _isFigureCoroutineWork = false;
        _figure = s;

    }*/



    {
        if (!reverse)
            _image.sprite = _themeService.GetSprite(s);
        else {_figure = s; _image.sprite = _themeService.GetSprite(s); yield break;}
        _isFigureCoroutineWork = true;

        Vector3 from = reverse ? Vector3.one : Vector3.one * 0.5f;
        Vector3 to   = reverse ? Vector3.one * 0.5f : Vector3.one;

        float time = 0f;

        while (time < FIGURE_SCALE_DURATION)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / FIGURE_SCALE_DURATION);

            float eased = QuickEase(t);

            _image.rectTransform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        _image.rectTransform.localScale = to;

        if (reverse)
            _image.sprite = _themeService.GetSprite(s);

        _isFigureCoroutineWork = false;
        _figure = s;
    }

    private float QuickEase(float t)
    {
        t = Mathf.Clamp01(t);

        float c4 = (2f * Mathf.PI) / 3f;

        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    private IEnumerator ISubStateFillProcess(Sprite s, Color32 color, bool reverse)
    {

        if (!reverse)
        {
            _subImage.sprite = s;
            _subImage.color = color;
        }

        _isFigureCoroutineWork = true;
        _subImage.fillAmount = (reverse) ? 1 : 0;
        float step = 1 / FIGURE_COUNT_FRAME;
        int i = 0;
        while (i < FIGURE_COUNT_FRAME)
        {
            _subImage.fillAmount += (reverse) ? -step : step;
            i++;
            yield return null;
        }
        _subImage.fillAmount = (reverse) ? 0 : 1;
        if (reverse)
        {
            _subImage.sprite = s;
            _subImage.color = color;
        }
        _isFigureCoroutineWork = false;
        //_figure = (CellFigure)s;
    }
    
    public IEnumerator IQueueCoroutineInCell(params IEnumerator[] enumerators)
    {
        for (int i = 0; i < enumerators.Length; i++)
        {
            yield return StartCoroutine(enumerators[i]);
        }
    }

    public bool GetIsCellClear()
    {
        return _isCellClear;
    }

    public void SetIsCellClear(bool state)
    {
        _isCellClear = state;
    }
}

public enum CellSubState
{
    None,
    Freeze
}

public enum CellFigure
{
    None = 0,
    P1 = 1,
    P2 = 2
}

