using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace _App
{
    public sealed class DraggableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _originalRect;
        [SerializeField] private Image _image;

        private RectTransform _dragObjectRectTransform;
        private Vector3 _dragStartOffset;
        private bool isDragging = false;

        private GameView _gameView;

        public Color MainColor { get; private set; }
        public DraggableObject CopyDragObject { get; private set; }
        public RectTransform RectTransform => _originalRect;

        public void Init(GameView gameView, Color color)
        {
            _gameView = gameView;
            MainColor = color;
            _image.color = color;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;

            CopyDragObject = _gameView.OnBeginDrag(this, out var rectTransform);
            _dragObjectRectTransform = rectTransform;

            _dragObjectRectTransform.sizeDelta = _originalRect.sizeDelta;
            _dragObjectRectTransform.localScale = _originalRect.localScale;

            Vector3 worldPosition = _originalRect.position;

            _dragObjectRectTransform.position = worldPosition;

            _dragStartOffset = _dragObjectRectTransform.position - _gameView.GetMousePosition();

            _canvasGroup.alpha = 0.5f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging && _dragObjectRectTransform != null)
            {
                _dragObjectRectTransform.position = _gameView.GetMousePosition() + _dragStartOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            _gameView.OnEndDrag(this, eventData.position);

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void AddToTower(RectTransform towerArea, int towerHeight)
        {
            CopyDragObject.transform.SetParent(towerArea, true);

            float maxOffset = _originalRect.rect.width * 0.5f;
            float randomOffset = Random.Range(-maxOffset, maxOffset);
            
            if (towerHeight <= 1)
            {
                _dragObjectRectTransform.anchoredPosition = new Vector2(_dragObjectRectTransform.anchoredPosition.x,
                    _originalRect.rect.height / 2);
            }
            else
            {
                _dragObjectRectTransform.anchoredPosition = new Vector2(_dragObjectRectTransform.anchoredPosition.x + randomOffset, 
                    _originalRect.rect.height * towerHeight -  _originalRect.rect.height / 2);

                _dragObjectRectTransform.DOLocalMoveY(_dragObjectRectTransform.localPosition.y + 10f, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }
        
        public void DestroyDragObject(bool useAnimation = false)
        {
            if (!useAnimation)
            {
                Destroy(CopyDragObject?.gameObject);
                return;
            }

            CopyDragObject.gameObject.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(CopyDragObject.gameObject));
        }
    }
}