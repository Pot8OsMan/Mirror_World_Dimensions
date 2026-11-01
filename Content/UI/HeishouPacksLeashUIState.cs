using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChinaManWuxiaNovel.Content.UI
{
    public class HeishouPacksLeashUIState : UIState
    {
        private UIImageButton toggleButton;
        private UIPanel menuPanel;

        private bool isMenuOpen;

        public override void OnInitialize()
        {
            var helmetTexture = ModContent.Request<Texture2D>(
                "ChinaManWuxiaNovel/Content/Items/Armor/TheLordOfHongyuanHongLuHead"
            );

            toggleButton = new UIImageButton(helmetTexture);

            toggleButton.Left.Set(20f, 0f);
            toggleButton.Top.Set(300f, 0f);
            toggleButton.Width.Set(48f, 0f);
            toggleButton.Height.Set(48f, 0f);

            toggleButton.OnLeftClick += ToggleMenu;

            Append(toggleButton);

            // Create the menu, but keep it hidden initially.
            menuPanel = new UIPanel();

            menuPanel.Left.Set(80f, 0f);
            menuPanel.Top.Set(300f, 0f);
            menuPanel.Width.Set(300f, 0f);
            menuPanel.Height.Set(200f, 0f);

            menuPanel.SetPadding(12f);

        }

        private void ToggleMenu(
            Terraria.UI.UIMouseEvent evt,
            UIElement listeningElement
        )
        {
            if (menuPanel.Parent == null)
            {
                Append(menuPanel);
            }
            else
            {
                RemoveChild(menuPanel);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
