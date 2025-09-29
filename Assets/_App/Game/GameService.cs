using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System;

namespace _App
{
    public sealed class GameService : IInitializable, IDisposable
    {
        private readonly LazyInject<GameView> _gameView;
        private readonly List<CellViewSettings> _cellViewSettings;
        private readonly List<DraggableObject> _towerBlocks;
        private readonly PopupService _popupService;
        private readonly NavigationService _navigationService;
        private readonly UserData _userData;

        private ProgressSaveData _currentProgressData;
        
        public ELevelType CurrentLevelType { get; private set; }
        public int TowerHeight => _towerBlocks.Count;
        
        public GameService(LazyInject<GameView> gameView, 
            NavigationService navigationService,
            UserData userData,
            PopupService popupService)
        {
            _gameView = gameView;
            _userData = userData;
            _popupService = popupService;
            _navigationService = navigationService;
            _towerBlocks = new List<DraggableObject>();
            _cellViewSettings = new List<CellViewSettings>();
        }

        public void Initialize()
        {
            var gameView = _gameView.Value;
            if (gameView == null)
            {
                Debug.LogError($"[{nameof(GameService)}] GameView is null");
                return;
            }
            
            LoadSaveData().Forget();
            gameView.Initialize(this);
        }

        private async UniTaskVoid LoadSaveData()
        {
            await _userData.LoadData<ProgressSaveData>();
        }
        
        public void StartLevel(LevelSettings levelSettings)
        {
            CurrentLevelType = levelSettings.LevelType;

            _currentProgressData = levelSettings.LevelType switch
            {
                ELevelType.Default => _userData.DefaultProgressSaveData,
                ELevelType.Infinite => _userData.InfiniteProgressSaveData,
                ELevelType.SameColor => _userData.SameColorProgressSaveData,
                _ => _currentProgressData
            };
            
            foreach (var t in levelSettings.Colors)
            {
                _cellViewSettings.Add(new CellViewSettings
                {
                    Color = t
                });
            }
            
            _navigationService.ScreenTransition<GameView>();
            
            if (_currentProgressData.TowerHeight > 0)
            {
                _gameView.Value.RecreateTower(_currentProgressData);
            }
        }

        public void SaveProgress()
        {
            if (_currentProgressData == null)
            {
                return;
            }
            
            _currentProgressData.TowerHeight = _towerBlocks.Count;

            var colorList = new List<Color>(_towerBlocks.Count);

            for (int i = 0; i < _towerBlocks.Count; ++i)
            {
                colorList.Add(_towerBlocks[i].MainColor);
            }
            
            _currentProgressData.TowerColors = colorList;
            
            _towerBlocks.Clear();
            _cellViewSettings.Clear();
            
            _userData.SaveData<ProgressSaveData>();
        }
        
        public void Dispose()
        {
            SaveProgress();
        }

        public int GetNumberOfCells()
        {
            return _cellViewSettings.Count;
        }

        public EnhancedScrollerCellView GetCell(CellView cellView, int dataIndex)
        {
            cellView.SetData(_cellViewSettings[dataIndex], _gameView.Value);
            return cellView;
        }

        private bool CanAddToTower(DraggableObject draggableObject)
        {
            if (CurrentLevelType == ELevelType.SameColor)
            {
                if (_towerBlocks.Count == 0)
                {
                    return true;
                }
                
                var lastBlock = _towerBlocks[^1];
                return lastBlock.MainColor == draggableObject.MainColor;
            }

            return true;
        }

        public bool AddToTower(DraggableObject draggableObject)
        {
            if (CanAddToTower(draggableObject))
            {
                _towerBlocks.Add(draggableObject);
                _popupService.ShowPopup<MessagePopup>(true, new MessagePopupSettings()
                {
                    Content = "Block added to the tower."
                });
                return true;
            }
            else
            {
                _popupService.ShowPopup<MessagePopup>(true, new MessagePopupSettings()
                {
                    Content = "Cannot add block to the tower."
                });
            }
            
            return false;
        }

        public void RemoveFromTower(DraggableObject draggableObject)
        {
            _towerBlocks.Remove(draggableObject);
            _popupService.ShowPopup<MessagePopup>(true, new MessagePopupSettings()
            {
                Content = "Block removed from the tower."
            });
        }
    }
}
