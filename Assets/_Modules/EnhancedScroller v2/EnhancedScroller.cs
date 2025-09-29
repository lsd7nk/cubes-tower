using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace EnhancedUI.EnhancedScroller
{
    public delegate void CellViewVisibilityChangedDelegate(EnhancedScrollerCellView cellView);
    
    public delegate void CellViewWillRecycleDelegate(EnhancedScrollerCellView cellView);
    
    public delegate void ScrollerScrolledDelegate(EnhancedScroller scroller, Vector2 val, float scrollPosition);
    
    public delegate void ScrollerSnappedDelegate(EnhancedScroller scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView);
    
    public delegate void ScrollerScrollingChangedDelegate(EnhancedScroller scroller, bool scrolling);
    
    public delegate void ScrollerTweeningChangedDelegate(EnhancedScroller scroller, bool tweening);
    
    public delegate void CellViewInstantiated(EnhancedScroller scroller, EnhancedScrollerCellView cellView);
    
    public delegate void CellViewReused(EnhancedScroller scroller, EnhancedScrollerCellView cellView);
    
    [RequireComponent(typeof(ScrollRect))]
    public class EnhancedScroller : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region Public
        
        public enum ScrollDirectionEnum
        {
            Vertical,
            Horizontal
        }
        
        public enum CellViewPositionEnum
        {
            Before,
            After
        }
        
        public enum ScrollbarVisibilityEnum
        {
            OnlyIfNeeded,
            Always,
            Never
        }
        
        public ScrollDirectionEnum scrollDirection;
        
        public float spacing;
        
        public RectOffset padding;
        
        [SerializeField]
        private bool loop;
        
		public bool loopWhileDragging = true;
        
		public float maxVelocity;
        
        [SerializeField]
        private ScrollbarVisibilityEnum scrollbarVisibility;
        
        public bool snapping;
        
        public float snapVelocityThreshold;
        
        public float snapWatchOffset;
        
        public float snapJumpToOffset;
        
        public float snapCellCenterOffset;
        
        public bool snapUseCellSpacing;
        
        public TweenType snapTweenType;
        
        public float snapTweenTime;
        
		public bool snapWhileDragging;
        
        public bool forceSnapOnEndDrag;
        
        public bool interruptTweeningOnDrag;
        
        public bool tweenPaused;
        
        private float _lookAheadBefore;
        public float lookAheadBefore
        {
            get
            {
                return _lookAheadBefore;
            }
            set
            {
                _lookAheadBefore = Mathf.Abs(value);
            }
        }
        
        private float _lookAheadAfter;
        public float lookAheadAfter
        {
            get
            {
                return _lookAheadAfter;
            }
            set
            {
                _lookAheadAfter = Mathf.Abs(value);
            }
        }

        private float _calculateStartCellBias;
        public float CalculateStartCellBias
        {
            get
            {
                return _calculateStartCellBias;
            }
            set
            {
                _calculateStartCellBias = value;
            }
        }
        
        public CellViewVisibilityChangedDelegate cellViewVisibilityChanged;

        public CellViewWillRecycleDelegate cellViewWillRecycle;
        
        public ScrollerScrolledDelegate scrollerScrolled;
        
        public ScrollerSnappedDelegate scrollerSnapped;
        
        public ScrollerScrollingChangedDelegate scrollerScrollingChanged;
        
        public ScrollerTweeningChangedDelegate scrollerTweeningChanged;

        public CellViewInstantiated cellViewInstantiated;
        
        public CellViewReused cellViewReused;
        
        public IEnhancedScrollerDelegate Delegate { get { return _delegate; } set { _delegate = value; _reloadData = true; } }
        
        public float ScrollPosition
        {
            get
            {
                return _scrollPosition;
            }
            set
            {
				if (loop)
				{
				}
				else
				{
                    value = Mathf.Clamp(value, 0, ScrollSize); 
				}

                if (_scrollPosition != value)
                {
                    _scrollPosition = value;
                    if (scrollDirection == ScrollDirectionEnum.Vertical)
                    {
                        _scrollRect.verticalNormalizedPosition = 1f - (_scrollPosition / ScrollSize);
                    }
                    else
                    {
                        _scrollRect.horizontalNormalizedPosition = (_scrollPosition / ScrollSize);
                    }
                }
            }
        }

        public float ScrollSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    return Mathf.Max(_container.rect.height - _scrollRectTransform.rect.height, 0);
                else
                    return Mathf.Max(_container.rect.width - _scrollRectTransform.rect.width, 0);
            }
        }

        public float NormalizedScrollPosition
        {
            get
            {
                var scrollPosition = ScrollPosition;
                return (scrollPosition <= 0 ? 0 : _scrollPosition / ScrollSize);
            }
        }
        
        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                if (loop != value)
                {
                    var originalScrollPosition = _scrollPosition;

                    loop = value;

                    _Resize(false);

                    if (loop)
                    {
                        ScrollPosition = _loopFirstScrollPosition + originalScrollPosition;
                    }
                    else
                    {
                        ScrollPosition = originalScrollPosition - _loopFirstScrollPosition;
                    }

                    ScrollbarVisibility = scrollbarVisibility;
                }
            }
        }
        
        public ScrollbarVisibilityEnum ScrollbarVisibility
        {
            get
            {
                return scrollbarVisibility;
            }
            set
            {
                scrollbarVisibility = value;

                if (_scrollbar != null)
                {
                    if (_cellViewOffsetArray != null && _cellViewOffsetArray.Count > 0)
                    {
                        if (scrollDirection == ScrollDirectionEnum.Vertical)
                        {
                            ScrollRect.verticalScrollbar = _scrollbar;
                        }
                        else
                        {
                            ScrollRect.horizontalScrollbar = _scrollbar;
                        }

                        if (_cellViewOffsetArray.Last() < ScrollRectSize || loop)
                        {

                            _scrollbar.gameObject.SetActive(scrollbarVisibility == ScrollbarVisibilityEnum.Always);
                        }
                        else
                        {

                            _scrollbar.gameObject.SetActive(scrollbarVisibility != ScrollbarVisibilityEnum.Never);
                        }

                        if (!_scrollbar.gameObject.activeSelf)
                        {
                            ScrollRect.verticalScrollbar = null;
                            ScrollRect.horizontalScrollbar = null;
                        }
                    }
                }
            }
        }

        public Vector2 Velocity
        {
            get
            {
                return _scrollRect.velocity;
            }
            set
            {
                _scrollRect.velocity = value;
            }
        }

        public float LinearVelocity
        {
            get
            {
                return (scrollDirection == ScrollDirectionEnum.Vertical ? _scrollRect.velocity.y : _scrollRect.velocity.x);
            }
            set
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    _scrollRect.velocity = new Vector2(0, value);
                }
                else
                {
                    _scrollRect.velocity = new Vector2(value, 0);
                }
            }
        }

        public bool IsScrolling
        {
            get; private set;
        }

        public bool IsTweening
        {
            get; private set;
        }

        public int StartCellViewIndex
        {
            get
            {
                return _activeCellViewsStartIndex;
            }
        }

        public int EndCellViewIndex
        {
            get
            {
                return _activeCellViewsEndIndex;
            }
        }

        public int StartDataIndex
        {
            get
            {
                return _activeCellViewsStartIndex % NumberOfCells;
            }
        }
        
        public int EndDataIndex
        {
            get
            {
                return _activeCellViewsEndIndex % NumberOfCells;
            }
        }

        public int NumberOfCells
        {
            get
            {
                return (_delegate != null ? _delegate.GetNumberOfCells(this) : 0);
            }
        }

        public ScrollRect ScrollRect
        {
            get
            {
                return _scrollRect;
            }
        }

        public float ScrollRectSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    return _scrollRectTransform.rect.height;
                else
                    return _scrollRectTransform.rect.width;
            }
        }

        public LayoutElement FirstPadder
        {
            get
            {
                return _firstPadder;
            }
        }

        public LayoutElement LastPadder
        {
            get
            {
                return _lastPadder;
            }
        }

        public RectTransform Container
        {
            get
            {
                return _container;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScrollerCellView cellPrefab)
        {
            var cellView = _GetRecycledCellView(cellPrefab);
            if (cellView == null)
            {
                var go = Instantiate(cellPrefab.gameObject);
                cellView = go.GetComponent<EnhancedScrollerCellView>();
                cellView.transform.SetParent(_container);
                cellView.transform.localPosition = Vector3.zero;
                cellView.transform.localRotation = Quaternion.identity;

                if (cellViewInstantiated != null)
                {
                    cellViewInstantiated(this, cellView);
                }
            }
            else
            {
                cellView.gameObject.SetActive(true);

                if (cellViewReused != null)
                {
                    cellViewReused(this, cellView);
                }
            }

            return cellView;
        }

        public void ReloadData(float scrollPositionFactor = 0)
        {
            _reloadData = false;

            _RecycleAllCells();

            if (_delegate != null)
                _Resize(false);

            if (_scrollRect == null || _scrollRectTransform == null || _container == null)
            {
                _scrollPosition = 0f;
                return;
            }

            _scrollPosition = Mathf.Clamp(scrollPositionFactor * ScrollSize, 0, ScrollSize);
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                _scrollRect.verticalNormalizedPosition = 1f - scrollPositionFactor;
            }
            else
            {
                _scrollRect.horizontalNormalizedPosition = scrollPositionFactor;
            }

            _RefreshActive();
        }

        public void RefreshActiveCellViews()
        {
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                _activeCellViews[i].RefreshCellView();
            }
        }

        public void ClearAll()
        {
            ClearActive();
            ClearRecycled();
        }

        public void ClearActive()
        {
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                DestroyImmediate(_activeCellViews[i].gameObject);
            }
            _activeCellViews.Clear();
        }

        public void ClearRecycled()
        {
            for (var i = 0; i < _recycledCellViews.Count; i++)
            {
                DestroyImmediate(_recycledCellViews[i].gameObject);
            }
            _recycledCellViews.Clear();
        }

        public void ToggleLoop()
        {
            Loop = !loop;
        }

        public void IgnoreLoopJump(bool ignore)
        {
            _ignoreLoopJump = ignore;
        }

        public void SetScrollPositionImmediately(float scrollPosition)
        {
            ScrollPosition = scrollPosition;
            _RefreshActive();
        }

        public enum LoopJumpDirectionEnum
        {
            Closest,
            Up,
            Down
        }

        public void JumpToDataIndex(int dataIndex,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            TweenType tweenType = TweenType.immediate,
            float tweenTime = 0f,
            Action jumpComplete = null,
            LoopJumpDirectionEnum loopJumpDirection = LoopJumpDirectionEnum.Closest,
            bool forceCalculateRange = false
            )
        {
            var cellOffsetPosition = 0f;

            if (cellOffset != 0)
            {
                var cellSize = (_delegate != null ? _delegate.GetCellViewSize(this, dataIndex) : 0);

                if (useSpacing)
                {
                    cellSize += spacing;

                    if (dataIndex > 0 && dataIndex < (NumberOfCells - 1)) cellSize += spacing;
                }

                cellOffsetPosition = cellSize * cellOffset;
            }

            if (scrollerOffset == 1f)
            {
                cellOffsetPosition += padding.bottom;
            }

            var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;

            var newScrollPosition = 0f;

            if (loop)
            {
				var numberOfCells = NumberOfCells;
                var set1CellViewIndex = _loopFirstCellIndex - (numberOfCells - dataIndex);
				var set2CellViewIndex = _loopFirstCellIndex + dataIndex;
				var set3CellViewIndex = _loopFirstCellIndex + numberOfCells + dataIndex;

                var set1Position = GetScrollPositionForCellViewIndex(set1CellViewIndex, CellViewPositionEnum.Before) + offset;
                var set2Position = GetScrollPositionForCellViewIndex(set2CellViewIndex, CellViewPositionEnum.Before) + offset;
                var set3Position = GetScrollPositionForCellViewIndex(set3CellViewIndex, CellViewPositionEnum.Before) + offset;

                var set1Diff = (Mathf.Abs(_scrollPosition - set1Position));
                var set2Diff = (Mathf.Abs(_scrollPosition - set2Position));
                var set3Diff = (Mathf.Abs(_scrollPosition - set3Position));

				var setOffset = -(scrollerOffset * ScrollRectSize);

				var currentSet = 0;
				var currentCellViewIndex = 0;
				var nextCellViewIndex = 0;

				if (loopJumpDirection == LoopJumpDirectionEnum.Up || loopJumpDirection == LoopJumpDirectionEnum.Down)
				{
					currentCellViewIndex = GetCellViewIndexAtPosition(_scrollPosition - setOffset + 0.0001f);

					if (currentCellViewIndex < numberOfCells)
					{
						currentSet = 1;
						nextCellViewIndex = dataIndex;
					}
					else if (currentCellViewIndex >= numberOfCells && currentCellViewIndex < (numberOfCells * 2))
					{
						currentSet = 2;
						nextCellViewIndex = dataIndex + numberOfCells;
					}
					else
					{
						currentSet = 3;
						nextCellViewIndex = dataIndex + (numberOfCells * 2);
					}
				}

                switch (loopJumpDirection)
                {
                    case LoopJumpDirectionEnum.Closest:

                        if (set1Diff < set2Diff)
                        {
                            if (set1Diff < set3Diff)
                            {
                                newScrollPosition = set1Position;
                            }
                            else
                            {
                                newScrollPosition = set3Position;
                            }
                        }
                        else
                        {
                            if (set2Diff < set3Diff)
                            {
                                newScrollPosition = set2Position;
                            }
                            else
                            {
                                newScrollPosition = set3Position;
                            }
                        }

                        break;

                    case LoopJumpDirectionEnum.Up:

						if (nextCellViewIndex < currentCellViewIndex)
						{
							newScrollPosition = (currentSet == 1 ? set1Position : (currentSet == 2 ? set2Position : set3Position));
						}
						else
						{
							if (currentSet == 1 && (currentCellViewIndex == dataIndex))
							{
								newScrollPosition = set1Position - _singleLoopGroupSize;
							}
							else
							{
								newScrollPosition = (currentSet == 1 ? set3Position : (currentSet == 2 ? set1Position : set2Position));
							}
						}

                        break;

                    case LoopJumpDirectionEnum.Down:

						if (nextCellViewIndex > currentCellViewIndex)
						{
							newScrollPosition = (currentSet == 1 ? set1Position : (currentSet == 2 ? set2Position : set3Position));
						}
						else
						{
							if (currentSet == 3 && (currentCellViewIndex == nextCellViewIndex))
							{
								newScrollPosition = set3Position + _singleLoopGroupSize;
							}
							else
							{
								newScrollPosition = (currentSet == 1 ? set2Position : (currentSet == 2 ? set3Position : set1Position));
							}
						}

						break;

                }

				if (useSpacing)
				{
					newScrollPosition -= spacing;
				}
            }
            else
            {
                newScrollPosition = GetScrollPositionForDataIndex(dataIndex, CellViewPositionEnum.Before) + offset;

                newScrollPosition = Mathf.Clamp(newScrollPosition - (useSpacing ? spacing : 0), 0, ScrollSize);
            }

            if (newScrollPosition == _scrollPosition)
            {
                if (jumpComplete != null) jumpComplete();
                return;
            }

            StartCoroutine(TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete, forceCalculateRange));
        }
        
        public void Snap()
        {
            if (NumberOfCells == 0) return;

            _snapJumping = true;

            LinearVelocity = 0;

            _snapInertia = _scrollRect.inertia;
            _scrollRect.inertia = false;

            var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(snapWatchOffset));

            _snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);

            _snapDataIndex = _snapCellViewIndex % NumberOfCells;

            JumpToDataIndex(_snapDataIndex, snapJumpToOffset, snapCellCenterOffset, snapUseCellSpacing, snapTweenType, snapTweenTime, SnapJumpComplete);
        }
        
        public float GetScrollPositionForCellViewIndex(int cellViewIndex, CellViewPositionEnum insertPosition)
        {
            if (NumberOfCells == 0) return 0;
            if (cellViewIndex < 0) cellViewIndex = 0;

            if (cellViewIndex == 0 && insertPosition == CellViewPositionEnum.Before)
            {
                return 0;
            }
            else
            {
                if (cellViewIndex < _cellViewOffsetArray.Count)
                {

                    if (insertPosition == CellViewPositionEnum.Before)
                    {
                        return _cellViewOffsetArray[cellViewIndex - 1] + spacing + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left);
                    }
                    else
                    {
                        return _cellViewOffsetArray[cellViewIndex] + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left);
                    }
                }
                else
                {
                    return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];
                }
            }
        }

        public float GetScrollPositionForDataIndex(int dataIndex, CellViewPositionEnum insertPosition)
        {
            return GetScrollPositionForCellViewIndex(loop ? _delegate.GetNumberOfCells(this) + dataIndex : dataIndex, insertPosition);
        }
        
        public int GetCellViewIndexAtPosition(float position)
        {
            return _GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
        }

        public EnhancedScrollerCellView GetCellViewAtDataIndex(int dataIndex)
        {
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                if (_activeCellViews[i].dataIndex == dataIndex)
                {
                    return _activeCellViews[i];
                }
            }

            return null;
        }

        public void ToggleTweenPaused(float newTweenTime = -1)
        {
            if (!tweenPaused)
            {
                tweenPaused = true;
                _tweenPauseToggledOff = false;
            }
            else
            {
                tweenPaused = false;
                _tweenPauseToggledOff = true;
                _tweenPauseNewTweenTime = newTweenTime;
            }
        }

        public void InterruptTween()
        {
            _interruptTween = true;
        }

        #endregion

        #region Private

        private bool _initialized = false;

        private bool _updateSpacing = false;

        private ScrollRect _scrollRect;

        private RectTransform _scrollRectTransform;

        private Scrollbar _scrollbar;

        private RectTransform _container;

        private HorizontalOrVerticalLayoutGroup _layoutGroup;

        private IEnhancedScrollerDelegate _delegate;

        private bool _reloadData;

        private bool _refreshActive;

        private SmallList<EnhancedScrollerCellView> _recycledCellViews = new SmallList<EnhancedScrollerCellView>();

        private LayoutElement _firstPadder;

        private LayoutElement _lastPadder;

        private RectTransform _recycledCellViewContainer;

        private SmallList<float> _cellViewSizeArray = new SmallList<float>();

        private SmallList<float> _cellViewOffsetArray = new SmallList<float>();

        public float _scrollPosition;

        private SmallList<EnhancedScrollerCellView> _activeCellViews = new SmallList<EnhancedScrollerCellView>();
        
        private int _activeCellViewsStartIndex;

        private int _activeCellViewsEndIndex;
        
        private int _loopFirstCellIndex;
        
        private int _loopLastCellIndex;
        
        private float _loopFirstScrollPosition;
        
        private float _loopLastScrollPosition;
        
        private float _loopFirstJumpTrigger;
        
        private float _loopLastJumpTrigger;
        
        private float _lastScrollRectSize;
        
        private bool _lastLoop;
        
        private int _snapCellViewIndex;
        
        private int _snapDataIndex;
        
        private bool _snapJumping;
        
        private bool _snapInertia;
        
        private ScrollbarVisibilityEnum _lastScrollbarVisibility;
        
		private float _singleLoopGroupSize;
        
		private bool _snapBeforeDrag;
        
		private bool _loopBeforeDrag;
        
        private bool _ignoreLoopJump;
        
        private int _dragFingerCount;
        
        private bool _interruptTween;
        
        private Vector2 _dragPreviousPos;
        
        private enum ListPositionEnum
        {
            First,
            Last
        }
        
        private void _Resize(bool keepPosition)
        {
            var originalScrollPosition = _scrollPosition;

            _cellViewSizeArray.Clear();
            var offset = _AddCellViewSizes();

            if (loop)
            {
                var cellCount = _cellViewSizeArray.Count;
                
                if (offset < ScrollRectSize)
                {
                    int additionalRounds = Mathf.CeilToInt((float)Mathf.CeilToInt(ScrollRectSize / offset) / 2.0f) * 2;
                    _DuplicateCellViewSizes(additionalRounds, cellCount);
                    _loopFirstCellIndex = cellCount * (1 + (additionalRounds / 2));
                }
                else
                {
                    _loopFirstCellIndex = cellCount;
                }

                _loopLastCellIndex = _loopFirstCellIndex + cellCount - 1;

                _DuplicateCellViewSizes(2, cellCount);
            }

            _CalculateCellViewOffsets();

            if (scrollDirection == ScrollDirectionEnum.Vertical)
                _container.sizeDelta = new Vector2(_container.sizeDelta.x, _cellViewOffsetArray.Last() + padding.top + padding.bottom);
            else
                _container.sizeDelta = new Vector2(_cellViewOffsetArray.Last() + padding.left + padding.right, _container.sizeDelta.y);

            if (loop)
            {
                _loopFirstScrollPosition = GetScrollPositionForCellViewIndex(_loopFirstCellIndex, CellViewPositionEnum.Before) + (spacing * 0.5f);
                _loopLastScrollPosition = GetScrollPositionForCellViewIndex(_loopLastCellIndex, CellViewPositionEnum.After) - ScrollRectSize + (spacing * 0.5f);

                _loopFirstJumpTrigger = _loopFirstScrollPosition - ScrollRectSize;
                _loopLastJumpTrigger = _loopLastScrollPosition + ScrollRectSize;
            }

            _ResetVisibleCellViews();

            if (keepPosition)
            {
                ScrollPosition = originalScrollPosition;
            }
            else
            {
                if (loop)
                {
                    ScrollPosition = _loopFirstScrollPosition;
                }
                else
                {
                    ScrollPosition = 0;
                }
            }

            ScrollbarVisibility = scrollbarVisibility;
        }
        
        private void _UpdateSpacing(float spacing)
        {
            _updateSpacing = false;
            _layoutGroup.spacing = spacing;
            ReloadData(NormalizedScrollPosition);
        }
        
        private float _AddCellViewSizes()
        {
            var offset = 0f;
			_singleLoopGroupSize = 0;
            for (var i = 0; i < NumberOfCells; i++)
            {
                _cellViewSizeArray.Add(_delegate.GetCellViewSize(this, i) + (i == 0 ? 0 : _layoutGroup.spacing));
				_singleLoopGroupSize += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
                offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
            }

            return offset;
        }
        
        private void _DuplicateCellViewSizes(int numberOfTimes, int cellCount)
        {
            for (var i = 0; i < numberOfTimes; i++)
            {
                for (var j = 0; j < cellCount; j++)
                {
                    _cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? _layoutGroup.spacing : 0));
                }
            }
        }
        
        private void _CalculateCellViewOffsets()
        {
            _cellViewOffsetArray.Clear();
            var offset = 0f;
            for (var i = 0; i < _cellViewSizeArray.Count; i++)
            {
                offset += _cellViewSizeArray[i];
                _cellViewOffsetArray.Add(offset);
            }
        }
        
        private EnhancedScrollerCellView _GetRecycledCellView(EnhancedScrollerCellView cellPrefab)
        {
            for (var i = 0; i < _recycledCellViews.Count; i++)
            {
                if (_recycledCellViews[i].cellIdentifier == cellPrefab.cellIdentifier)
                {
                    var cellView = _recycledCellViews.RemoveAt(i);
                    return cellView;
                }
            }

            return null;
        }
        
        private void _ResetVisibleCellViews()
        {
            int startIndex;
            int endIndex;

            _CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            var i = 0;
            SmallList<int> remainingCellIndices = new SmallList<int>();
            while (i < _activeCellViews.Count)
            {
                if (_activeCellViews[i].cellIndex < startIndex || _activeCellViews[i].cellIndex > endIndex)
                {
                    _RecycleCell(_activeCellViews[i]);
                }
                else
                {
                    remainingCellIndices.Add(_activeCellViews[i].cellIndex);
                    i++;
                }
            }

            if (remainingCellIndices.Count == 0)
            {
                for (i = startIndex; i <= endIndex; i++)
                {
                    _AddCellView(i, ListPositionEnum.Last);
                }
            }
            else
            {
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < remainingCellIndices.First())
                    {
                        _AddCellView(i, ListPositionEnum.First);
                    }
                }
                
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > remainingCellIndices.Last())
                    {
                        _AddCellView(i, ListPositionEnum.Last);
                    }
                }
            }

            _activeCellViewsStartIndex = startIndex;
            _activeCellViewsEndIndex = endIndex;

            _SetPadders();
        }
        
        private void _RecycleAllCells()
        {
            while (_activeCellViews.Count > 0) _RecycleCell(_activeCellViews[0]);
            _activeCellViewsStartIndex = 0;
            _activeCellViewsEndIndex = 0;
        }
        
        private void _RecycleCell(EnhancedScrollerCellView cellView)
        {
            if (cellViewWillRecycle != null) cellViewWillRecycle(cellView);

            _activeCellViews.Remove(cellView);

            _recycledCellViews.Add(cellView);

            cellView.transform.gameObject.SetActive(false);

            cellView.dataIndex = 0;
            cellView.cellIndex = 0;
            cellView.active = false;

            if (cellViewVisibilityChanged != null) cellViewVisibilityChanged(cellView);
        }
        
        private void _AddCellView(int cellIndex, ListPositionEnum listPosition)
        {
            if (NumberOfCells == 0) return;
            
            var dataIndex = cellIndex % NumberOfCells;
            var cellView = _delegate.GetCellView(this, dataIndex, cellIndex);

            cellView.cellIndex = cellIndex;
            cellView.dataIndex = dataIndex;
            cellView.active = true;

            cellView.transform.SetParent(_container, false);
            cellView.transform.localScale = Vector3.one;

            LayoutElement layoutElement = cellView.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = cellView.gameObject.AddComponent<LayoutElement>();

            if (scrollDirection == ScrollDirectionEnum.Vertical)
                layoutElement.minHeight = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);
            else
                layoutElement.minWidth = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);

            if (listPosition == ListPositionEnum.First)
                _activeCellViews.AddStart(cellView);
            else
                _activeCellViews.Add(cellView);

            if (listPosition == ListPositionEnum.Last)
                cellView.transform.SetSiblingIndex(_container.childCount - 2);
            else if (listPosition == ListPositionEnum.First)
                cellView.transform.SetSiblingIndex(1);

            if (cellViewVisibilityChanged != null) cellViewVisibilityChanged(cellView);
        }
        
        private void _SetPadders()
        {
            if (NumberOfCells == 0) return;

            var firstSize = _cellViewOffsetArray[_activeCellViewsStartIndex] - _cellViewSizeArray[_activeCellViewsStartIndex];
            var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[_activeCellViewsEndIndex];

            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                _firstPadder.minHeight = firstSize;
                _firstPadder.gameObject.SetActive(_firstPadder.minHeight > 0);

                _lastPadder.minHeight = lastSize;
                _lastPadder.gameObject.SetActive(_lastPadder.minHeight > 0);
            }
            else
            {
                _firstPadder.minWidth = firstSize;
                _firstPadder.gameObject.SetActive(_firstPadder.minWidth > 0);

                _lastPadder.minWidth = lastSize;
                _lastPadder.gameObject.SetActive(_lastPadder.minWidth > 0);
            }
        }
        
        private void _RefreshActive()
        {
            int startIndex;
            int endIndex;
            var velocity = Vector2.zero;

            if (loop && !_ignoreLoopJump)
            {
                if (_scrollPosition < _loopFirstJumpTrigger)
                {
                    velocity = _scrollRect.velocity;
                    ScrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - _scrollPosition) + spacing;
                    _scrollRect.velocity = velocity;
                }
                else if (_scrollPosition > _loopLastJumpTrigger)
                {
                    velocity = _scrollRect.velocity;
                    ScrollPosition = _loopFirstScrollPosition + (_scrollPosition - _loopLastJumpTrigger) - spacing;
                    _scrollRect.velocity = velocity;
                }
            }

            _CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            if (startIndex == _activeCellViewsStartIndex && endIndex == _activeCellViewsEndIndex) return;

            _ResetVisibleCellViews();
        }
        
        private void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;

            var startPosition = _scrollPosition - lookAheadBefore + _calculateStartCellBias;
            var endPosition = _scrollPosition + (scrollDirection == ScrollDirectionEnum.Vertical ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width) + lookAheadAfter;

            startIndex = GetCellViewIndexAtPosition(startPosition);
            endIndex = GetCellViewIndexAtPosition(endPosition);
        }
        
        private int _GetCellIndexAtPosition(float position, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex) return startIndex;

            var middleIndex = (startIndex + endIndex) / 2;
            
            var pad = scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left;
            if ((_cellViewOffsetArray[middleIndex] + pad) >= (position + (pad == 0 ? 0 : 1.00001f)))
                return _GetCellIndexAtPosition(position, startIndex, middleIndex);
            else
                return _GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
        }
        
        void Awake()
        {
            GameObject go;

            _scrollRect = this.GetComponent<ScrollRect>();
            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
            
            if (_scrollRect.content != null)
            {
                DestroyImmediate(_scrollRect.content.gameObject);
            }

            go = new GameObject("Container", typeof(RectTransform));
            go.transform.SetParent(_scrollRectTransform);
            if (scrollDirection == ScrollDirectionEnum.Vertical)
                go.AddComponent<VerticalLayoutGroup>();
            else
                go.AddComponent<HorizontalLayoutGroup>();
            _container = go.GetComponent<RectTransform>();

            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                _container.anchorMin = new Vector2(0, 1);
                _container.anchorMax = Vector2.one;
                _container.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                _container.anchorMin = Vector2.zero;
                _container.anchorMax = new Vector2(0, 1f);
                _container.pivot = new Vector2(0, 0.5f);
            }
            _container.localPosition = Vector3.zero;
            _container.localRotation = Quaternion.identity;
            _container.localScale = Vector3.one;
            _container.offsetMax = Vector2.zero;
            _container.offsetMin = Vector2.zero;

            _scrollRect.content = _container;

            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                _scrollbar = _scrollRect.verticalScrollbar;
            }
            else
            {
                _scrollbar = _scrollRect.horizontalScrollbar;
            }

            _layoutGroup = _container.GetComponent<HorizontalOrVerticalLayoutGroup>();
            _layoutGroup.spacing = spacing;
            _layoutGroup.padding = padding;
            _layoutGroup.childAlignment = TextAnchor.UpperLeft;
            _layoutGroup.childForceExpandHeight = true;
            _layoutGroup.childForceExpandWidth = true;

            _scrollRect.horizontal = scrollDirection == ScrollDirectionEnum.Horizontal;
            _scrollRect.vertical = scrollDirection == ScrollDirectionEnum.Vertical;
            
            go = new GameObject("First Padder", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(_container, false);
            _firstPadder = go.GetComponent<LayoutElement>();

            go = new GameObject("Last Padder", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(_container, false);
            _lastPadder = go.GetComponent<LayoutElement>();

            go = new GameObject("Recycled Cells", typeof(RectTransform));
            go.transform.SetParent(_scrollRect.transform, false);
            _recycledCellViewContainer = go.GetComponent<RectTransform>();
            _recycledCellViewContainer.gameObject.SetActive(false);

            _lastScrollRectSize = ScrollRectSize;
            _lastLoop = loop;
            _lastScrollbarVisibility = scrollbarVisibility;

            _initialized = true;
        }
        
		public void OnBeginDrag(PointerEventData data)
		{
            _dragFingerCount++;
            if (_dragFingerCount > 1) return;

			_snapBeforeDrag = snapping;
			if (!snapWhileDragging)
			{
				snapping = false;
			}

			_loopBeforeDrag = loop;
			if (!loopWhileDragging)
			{
				loop = false;
			}

            if (IsTweening && interruptTweeningOnDrag)
            {
                _interruptTween = true;
            }
        }

        public void OnDrag(PointerEventData data)
        {
            _dragPreviousPos = data.position;
        }
        
        public void OnEndDrag(PointerEventData data)
		{
            _dragFingerCount--;
            if (_dragFingerCount < 0) _dragFingerCount = 0;

			snapping = _snapBeforeDrag;
			loop = _loopBeforeDrag;

            if (forceSnapOnEndDrag && snapping && _dragPreviousPos == data.position)
            {
                Snap();
            }
        }

        void Update()
        {
            if (_updateSpacing)
            {
                _UpdateSpacing(spacing);
                _reloadData = false;
            }

            if (_reloadData)
            {
                ReloadData();
            }
            
            if (
                    (loop && _lastScrollRectSize != ScrollRectSize)
                    ||
                    (loop != _lastLoop)
                )
            {
                _Resize(true);
                _lastScrollRectSize = ScrollRectSize;

                _lastLoop = loop;
            }

            if (_lastScrollbarVisibility != scrollbarVisibility)
            {
                ScrollbarVisibility = scrollbarVisibility;
                _lastScrollbarVisibility = scrollbarVisibility;
            }
            
            if (LinearVelocity != 0 && !IsScrolling)
            {
                IsScrolling = true;
                if (scrollerScrollingChanged != null) scrollerScrollingChanged(this, true);
            }
            else if (LinearVelocity == 0 && IsScrolling)
            {
                IsScrolling = false;
                if (scrollerScrollingChanged != null) scrollerScrollingChanged(this, false);
            }
        }
        
        void OnValidate()
        {
            if (_initialized && spacing != _layoutGroup.spacing)
            {
                _updateSpacing = true;
            }
        }
        
        void LateUpdate()
        {
			if (maxVelocity > 0)
			{
				if (scrollDirection == ScrollDirectionEnum.Horizontal)
				{
					Velocity = new Vector2(Mathf.Clamp(Mathf.Abs(Velocity.x), 0, maxVelocity) * Mathf.Sign(Velocity.x), Velocity.y);
				}
				else
				{
					Velocity = new Vector2(Velocity.x, Mathf.Clamp(Mathf.Abs(Velocity.y), 0, maxVelocity) * Mathf.Sign(Velocity.y));
				}
			}
        }

        void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
        }

        void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(_ScrollRect_OnValueChanged);
        }
        
        private void _ScrollRect_OnValueChanged(Vector2 val)
        {
            if (scrollDirection == ScrollDirectionEnum.Vertical)
                _scrollPosition = (1f - val.y) * ScrollSize;
            else
                _scrollPosition = val.x * ScrollSize;
            _scrollPosition = Mathf.Clamp(_scrollPosition, 0, ScrollSize);

            if (scrollerScrolled != null) scrollerScrolled(this, val, _scrollPosition);

            if (snapping && !_snapJumping)
            {
                if (Mathf.Abs(LinearVelocity) <= snapVelocityThreshold && LinearVelocity != 0)
                {
                    var normalized = NormalizedScrollPosition;
                    if (loop || (!loop && normalized > 0 && normalized < 1.0f))
                    {
                        Snap();
                    }
                }
            }

            _RefreshActive();

        }
        
        private void SnapJumpComplete()
        {
            _snapJumping = false;
            _scrollRect.inertia = _snapInertia;

            EnhancedScrollerCellView cellView = null;
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                if (_activeCellViews[i].dataIndex == _snapDataIndex)
                {
                    cellView = _activeCellViews[i];
                    break;
                }
            }

            if (scrollerSnapped != null) scrollerSnapped(this, _snapCellViewIndex, _snapDataIndex, cellView);
        }

        #endregion

        #region Tweening

        public enum TweenType
        {
            immediate,
            linear,
            spring,
            easeInQuad,
            easeOutQuad,
            easeInOutQuad,
            easeInCubic,
            easeOutCubic,
            easeInOutCubic,
            easeInQuart,
            easeOutQuart,
            easeInOutQuart,
            easeInQuint,
            easeOutQuint,
            easeInOutQuint,
            easeInSine,
            easeOutSine,
            easeInOutSine,
            easeInExpo,
            easeOutExpo,
            easeInOutExpo,
            easeInCirc,
            easeOutCirc,
            easeInOutCirc,
            easeInBounce,
            easeOutBounce,
            easeInOutBounce,
            easeInBack,
            easeOutBack,
            easeInOutBack,
            easeInElastic,
            easeOutElastic,
            easeInOutElastic
        }

        private float _tweenTimeLeft;
        private bool _tweenPauseToggledOff;
        private float _tweenPauseNewTweenTime;
        
        IEnumerator TweenPosition(TweenType tweenType, float time, float start, float end, Action tweenComplete, bool forceCalculateRange)
        {
            if (!(tweenType == TweenType.immediate || time == 0))
            {
                _scrollRect.velocity = Vector2.zero;

                IsTweening = true;
                if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, true);

                _tweenTimeLeft = 0;
                var newPosition = 0f;

                while (_tweenTimeLeft < time && !_interruptTween)
                {
                    if (!tweenPaused)
                    {
                        if (_tweenPauseToggledOff)
                        {
                            _tweenPauseToggledOff = false;
                            start = ScrollPosition;
                            time = (_tweenPauseNewTweenTime < 0 ? _tweenTimeLeft : _tweenPauseNewTweenTime);
                            _tweenTimeLeft = 0;
                        }

                        switch (tweenType)
                        {
                            case TweenType.linear: newPosition = linear(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.spring: newPosition = spring(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInQuad: newPosition = easeInQuad(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutQuad: newPosition = easeOutQuad(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutQuad: newPosition = easeInOutQuad(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInCubic: newPosition = easeInCubic(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutCubic: newPosition = easeOutCubic(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutCubic: newPosition = easeInOutCubic(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInQuart: newPosition = easeInQuart(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutQuart: newPosition = easeOutQuart(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutQuart: newPosition = easeInOutQuart(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInQuint: newPosition = easeInQuint(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutQuint: newPosition = easeOutQuint(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutQuint: newPosition = easeInOutQuint(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInSine: newPosition = easeInSine(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutSine: newPosition = easeOutSine(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutSine: newPosition = easeInOutSine(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInExpo: newPosition = easeInExpo(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutExpo: newPosition = easeOutExpo(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutExpo: newPosition = easeInOutExpo(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInCirc: newPosition = easeInCirc(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutCirc: newPosition = easeOutCirc(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutCirc: newPosition = easeInOutCirc(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInBounce: newPosition = easeInBounce(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutBounce: newPosition = easeOutBounce(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutBounce: newPosition = easeInOutBounce(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInBack: newPosition = easeInBack(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutBack: newPosition = easeOutBack(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutBack: newPosition = easeInOutBack(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInElastic: newPosition = easeInElastic(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeOutElastic: newPosition = easeOutElastic(start, end, (_tweenTimeLeft / time)); break;
                            case TweenType.easeInOutElastic: newPosition = easeInOutElastic(start, end, (_tweenTimeLeft / time)); break;
                        }
                        
                        ScrollPosition = newPosition;
                        
                        _tweenTimeLeft += Time.unscaledDeltaTime;
                    }

                    yield return null;
                }
            }

            if (_interruptTween)
            {
                _interruptTween = false;
                _snapJumping = false;
                _scrollRect.inertia = _snapInertia;

                IsTweening = false;
                if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, false);
            }
            else
            {
                ScrollPosition = end;

                if (forceCalculateRange || tweenType == TweenType.immediate || time == 0)
                {
                    _RefreshActive();
                }

                if (tweenComplete != null) tweenComplete();

                IsTweening = false;
                if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, false);
            }
        }


        private float linear(float start, float end, float val)
        {
            return Mathf.Lerp(start, end, val);
        }

        private static float spring(float start, float end, float val)
        {
            val = Mathf.Clamp01(val);
            val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
            return start + (end - start) * val;
        }

        private static float easeInQuad(float start, float end, float val)
        {
            end -= start;
            return end * val * val + start;
        }

        private static float easeOutQuad(float start, float end, float val)
        {
            end -= start;
            return -end * val * (val - 2) + start;
        }

        private static float easeInOutQuad(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val + start;
            val--;
            return -end / 2 * (val * (val - 2) - 1) + start;
        }

        private static float easeInCubic(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val + start;
        }

        private static float easeOutCubic(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val + 1) + start;
        }

        private static float easeInOutCubic(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val + 2) + start;
        }

        private static float easeInQuart(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val + start;
        }

        private static float easeOutQuart(float start, float end, float val)
        {
            val--;
            end -= start;
            return -end * (val * val * val * val - 1) + start;
        }

        private static float easeInOutQuart(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val + start;
            val -= 2;
            return -end / 2 * (val * val * val * val - 2) + start;
        }

        private static float easeInQuint(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val * val + start;
        }

        private static float easeOutQuint(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val * val * val + 1) + start;
        }

        private static float easeInOutQuint(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val * val * val + 2) + start;
        }

        private static float easeInSine(float start, float end, float val)
        {
            end -= start;
            return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
        }

        private static float easeOutSine(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
        }

        private static float easeInOutSine(float start, float end, float val)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
        }

        private static float easeInExpo(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
        }

        private static float easeOutExpo(float start, float end, float val)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
        }

        private static float easeInOutExpo(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
            val--;
            return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
        }

        private static float easeInCirc(float start, float end, float val)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
        }

        private static float easeOutCirc(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * Mathf.Sqrt(1 - val * val) + start;
        }

        private static float easeInOutCirc(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
            val -= 2;
            return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
        }

        private static float easeInBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            return end - easeOutBounce(0, end, d - val) + start;
        }

        private static float easeOutBounce(float start, float end, float val)
        {
            val /= 1f;
            end -= start;
            if (val < (1 / 2.75f))
            {
                return end * (7.5625f * val * val) + start;
            }
            else if (val < (2 / 2.75f))
            {
                val -= (1.5f / 2.75f);
                return end * (7.5625f * (val) * val + .75f) + start;
            }
            else if (val < (2.5 / 2.75))
            {
                val -= (2.25f / 2.75f);
                return end * (7.5625f * (val) * val + .9375f) + start;
            }
            else
            {
                val -= (2.625f / 2.75f);
                return end * (7.5625f * (val) * val + .984375f) + start;
            }
        }

        private static float easeInOutBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            if (val < d / 2) return easeInBounce(0, end, val * 2) * 0.5f + start;
            else return easeOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
        }

        private static float easeInBack(float start, float end, float val)
        {
            end -= start;
            val /= 1;
            float s = 1.70158f;
            return end * (val) * val * ((s + 1) * val - s) + start;
        }

        private static float easeOutBack(float start, float end, float val)
        {
            float s = 1.70158f;
            end -= start;
            val = (val / 1) - 1;
            return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
        }

        private static float easeInOutBack(float start, float end, float val)
        {
            float s = 1.70158f;
            end -= start;
            val /= .5f;
            if ((val) < 1)
            {
                s *= (1.525f);
                return end / 2 * (val * val * (((s) + 1) * val - s)) + start;
            }
            val -= 2;
            s *= (1.525f);
            return end / 2 * ((val) * val * (((s) + 1) * val + s) + 2) + start;
        }

        private static float easeInElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;
            val = val / d;
            if (val == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }
            val = val - 1;
            return -(a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        private static float easeOutElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;

            val = val / d;
            if (val == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        private static float easeInOutElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;

            val = val / (d / 2);
            if (val == 2) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (val < 1)
            {
                val = val - 1;
                return -0.5f * (a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
            }
            val = val - 1;
            return a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }

        #endregion
    }
}