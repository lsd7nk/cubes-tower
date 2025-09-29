using UnityEngine.UI;

namespace _App
{
    public sealed class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }
    }
}