using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ChinaManWuxiaNovel.Content.UI
{
    public class DraggableUIImageButton : UIImageButton
    {
        public bool IsDragging { get; private set; }
        public Vector2 DragOffset { get; private set; }

        public DraggableUIImageButton(Asset<Texture2D> texture) : base(texture) { }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            if (evt.Target != this)
                return;

            IsDragging = true;
            DragOffset = evt.MousePosition - GetDimensions().Position();
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            IsDragging = false;
        }
    }
}
