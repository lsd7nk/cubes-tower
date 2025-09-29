using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using UnityEngine;

namespace _App
{
    public sealed class CellView : EnhancedScrollerCellView
    {
        [SerializeField] private Image _image;
        [SerializeField] private DraggableObject _draggableObject;

        public DraggableObject DraggableObject => _draggableObject;
        
        public void SetData(CellViewSettings settings, GameView gameView)
        {
            if (settings == null)
            {
                return;
            }
            
            _image.color = settings.Color;

            _draggableObject.Init(gameView, settings.Color);
        }
    }

    public sealed class CellViewSettings
    {
        public Color Color;
    }
}