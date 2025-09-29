using System.Collections.Generic;
using App;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace _App
{
    public sealed class GameView : BaseView, IEnhancedScrollerDelegate
    {
        [Header("Game")] 
        [SerializeField] private RectTransform _fullTower;
        [SerializeField] private RectTransform _towerArea;
        [SerializeField] private RectTransform _trashArea;

        [Header("Scroller")] 
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private CellView _cellViewPrefab;

        private List<DraggableObject> _towerBlocks;
        private GameService _service;
        private Rect _cellRect;
        private Canvas _masterCanvas;

        public void Initialize(GameService service)
        {
            _service = service;
            _towerBlocks = new List<DraggableObject>();
            _masterCanvas = transform.parent.GetComponent<Canvas>();
            _cellRect = _cellViewPrefab.GetComponent<RectTransform>().rect;
        }

        public ScrollRect GetScroll()
        {
            return _scroller.ScrollRect;
        }

        protected override void OnLanguageChanged()
        {
            base.OnLanguageChanged();
        }

        public override void Setup(ScreenSettings settings)
        {
            _scroller.Delegate = this;
            _scroller.ReloadData();
        }
        
        public override void Deactivate()
        {
            base.Deactivate();

            foreach (var block in _towerBlocks)
            {
                if (block != null)
                {
                    Destroy(block.gameObject);
                }
            }

            _towerBlocks.Clear();
        }

        public void RecreateTower(ProgressSaveData saveData)
        {
            // _towerBlocks = new List<DraggableObject>();
            
            // foreach (var block in _towerBlocks)
            // {
            //     if (block != null)
            //     {
            //         Destroy(block.gameObject);
            //     }
            // }
            
            // Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(_masterCanvas.worldCamera, _towerArea.position);
            // PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            // {
            //     position = screenCenter,
            // };
            
            // var position = _masterCanvas.worldCamera.ScreenToWorldPoint(pointerEventData.position);

            // foreach (var color in saveData.TowerColors)
            // {
            //     // Воссоздать старые кубики и построить башню заново
            //     // Внизу только тестовый вариант - не особо рабочий, времени особо не осталось
                
            //     var dragObject = CreateDragObject(_cellViewPrefab.DraggableObject);

            //     dragObject.Init(this, color);
            //     dragObject.OnBeginDrag(pointerEventData);

            //     dragObject.CopyDragObject.RectTransform.anchorMax = Vector2.zero;
            //     dragObject.CopyDragObject.RectTransform.anchorMin = Vector2.zero;
            //     dragObject.CopyDragObject.RectTransform.anchoredPosition = new Vector2(position.x, position.y + dragObject.RectTransform.rect.height);

            //     position = new Vector2(position.x, position.y + dragObject.RectTransform.rect.height);
                
            //     dragObject.OnEndDrag(pointerEventData);
            //     Destroy(dragObject.gameObject);
            // }
        }
        
        public DraggableObject OnBeginDrag(DraggableObject draggableObject, out RectTransform rectTransform)
        {
            var dragObject = CreateDragObject(draggableObject);

            rectTransform = dragObject.GetComponent<RectTransform>();

            return dragObject;
        }

        public void OnEndDrag(DraggableObject draggableObject, Vector2 eventDataPosition)
        {
            var mainCamera = _masterCanvas.worldCamera;
            bool isInTowerArea = IsInsideArea(_towerArea, eventDataPosition, mainCamera);
            bool isInTrashArea = IsInsideArea(_trashArea, eventDataPosition, mainCamera);
            int maxBlocks = CalculateMaximumBlocks();
            var copyDragObject = draggableObject.CopyDragObject;

            if (isInTowerArea && draggableObject.transform.parent != _towerArea  
                              && IsPlacedOnTop(draggableObject) 
                              && _service.AddToTower(copyDragObject))
            {
                if (_service.TowerHeight > 1)
                {
                    Vector3 basePosition = _towerBlocks[^1].RectTransform.position;
                    copyDragObject.transform.position = new Vector2(basePosition.x, draggableObject.transform.position.y);
                }

                _towerBlocks.Add(copyDragObject);

                draggableObject.AddToTower(_towerArea, _service.TowerHeight);

                if (_service.CurrentLevelType == ELevelType.Infinite && _towerBlocks.Count > maxBlocks / 2)
                {
                    AdjustTowerHeight(copyDragObject.RectTransform.rect.height);
                }
            }
            else if (isInTrashArea && draggableObject.transform.parent == _towerArea)
            {
                int removedBlockIndex = _towerBlocks.IndexOf(draggableObject);

                _towerBlocks.Remove(draggableObject);
                _service.RemoveFromTower(draggableObject);
                DestroyDraggableObject(draggableObject, true);

                AnimateBlocksAfterRemoval(removedBlockIndex, draggableObject.RectTransform.rect.height);

                if (_service.CurrentLevelType == ELevelType.Infinite && _towerBlocks.Count >= maxBlocks / 2)
                {
                    AdjustTowerHeight(-draggableObject.RectTransform.rect.height);
                }
            }
            else
            {
                DestroyDraggableObject(draggableObject);
            }
        }

        private void AdjustTowerHeight(float blockHeightChange)
        {
            float newHeight = _fullTower.sizeDelta.y + blockHeightChange;
            float newYPosition = _fullTower.anchoredPosition.y - blockHeightChange / 2;

            DelayUtility.InvokeAfterSecond(() =>
            {
                _fullTower.DOSizeDelta(new Vector2(_fullTower.sizeDelta.x, newHeight), 0.5f).SetEase(Ease.OutQuad);
                _fullTower.DOAnchorPosY(newYPosition, 0.5f).SetEase(Ease.OutQuad);
            });
            
        }

        private int CalculateMaximumBlocks()
        {
            float towerHeight = _towerArea.rect.height;
            float blockHeight = _cellRect.height; 
            return Mathf.FloorToInt(towerHeight / blockHeight);
        }
        
        private bool IsPlacedOnTop(DraggableObject draggableObject)
        {
            if (_towerBlocks.Count == 0)
            {
                return true;
            }

            var topBlock = _towerBlocks[^1];
            Vector3 topBlockPosition = topBlock.RectTransform.position;

            float acceptableXMin = topBlockPosition.x * 0.75f;
            float acceptableXMax = topBlockPosition.x / 0.75f;

            Vector3 draggablePosition = draggableObject.CopyDragObject.RectTransform.position;

            bool isWithinXRange = draggablePosition.x > acceptableXMin && draggablePosition.x < acceptableXMax;
            bool isAboveTopBlock = draggablePosition.y > topBlockPosition.y;

            return isWithinXRange && isAboveTopBlock;
        }

        private void AnimateBlocksAfterRemoval(int removedBlockIndex, float rectHeight)
        {
            for (int i = removedBlockIndex; i < _towerBlocks.Count; i++)
            {
                var block = _towerBlocks[i];
                block.RectTransform.DOLocalMoveY(block.RectTransform.localPosition.y - rectHeight, 0.3f)
                    .SetEase(Ease.OutQuad);
            }
        }

        private DraggableObject CreateDragObject(DraggableObject original)
        {
            var dragObject = Instantiate(original, transform.position, Quaternion.identity);
            dragObject.name = "DragCube";
            dragObject.transform.SetParent(_masterCanvas.transform, false);
            dragObject.Init(this, original.MainColor);
            return dragObject;
        }
        
        private bool IsInsideArea(RectTransform area, Vector2 screenPosition, Camera mainCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPosition, mainCamera,
                out Vector2 localPoint);
            return IsPointWithinEllipse(localPoint, area.rect.size);
        }

        private bool IsPointWithinEllipse(Vector2 localPoint, Vector2 size)
        {
            float x = localPoint.x / (size.x / 2);
            float y = localPoint.y / (size.y / 2);
            return x * x + y * y <= 1;
        }

        private void DestroyDraggableObject(DraggableObject draggableObject, bool immediate = false)
        {
            draggableObject?.DestroyDragObject(immediate);

            if (immediate)
            {
                Destroy(draggableObject?.gameObject);
            }
        }

        public Vector3 GetMousePosition()
        {
            var mainCamera = _masterCanvas.worldCamera;
            Vector3 pointerPosition = Input.mousePosition;
            pointerPosition.z = _masterCanvas.planeDistance;
            return mainCamera.ScreenToWorldPoint(pointerPosition);
        }

        private void OnDrawGizmos()
        {
            if (_trashArea != null)
            {
                Gizmos.color = Color.red;

                Vector3[] corners = new Vector3[4];
                _trashArea.GetWorldCorners(corners);

                Vector3 center = (corners[0] + corners[2]) / 2;
                float width = Vector3.Distance(corners[0], corners[3]);
                float height = Vector3.Distance(corners[0], corners[1]);

                DrawEllipseGizmo(center, width, height);
            }
        }

        private void DrawEllipseGizmo(Vector3 center, float width, float height)
        {
            int segments = 50;
            float angleStep = 360f / segments;

            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * width / 2, Mathf.Sin(0) * height / 2, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * angleStep * i;
                Vector3 nextPoint =
                    center + new Vector3(Mathf.Cos(angle) * width / 2, Mathf.Sin(angle) * height / 2, 0);
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _service.GetNumberOfCells();
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellRect.width;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            CellView cellView = scroller.GetCellView(_cellViewPrefab) as CellView;
            cellView!.name = "Cell " + dataIndex;
            return _service.GetCell(cellView, dataIndex);
        }
    }
}