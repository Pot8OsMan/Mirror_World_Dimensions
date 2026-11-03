using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ChinaManWuxiaNovel.Content.UI
{
    public class HeishouPacksLeashUIState : UIState
    {
        private DraggableUIImageButton toggleButton;
        private UIPanel menuPanel;

        private bool isDragging;
        private bool isDragged;
        private Vector2 dragOffset;
        private Vector2 dragStartPosition;

        private UIPanel navigationPanel;
        private UIPanel contentPanel;

        private string currentPage = "Overview";

        public override void OnInitialize()
        {
            var helmetTexture = ModContent.Request<Texture2D>(
                "ChinaManWuxiaNovel/Content/Items/Armor/TheLordOfHongyuanHongLuHead"
            );

            var menuTexture = ModContent.Request<Texture2D>(
                "ChinaManWuxiaNovel/Content/UI/The_Lord_of_Hongyuan_Hong_Lu_Evade_Sprite"
            );

            toggleButton = new DraggableUIImageButton(helmetTexture);

            toggleButton.Left.Set(500f, 0f);
            toggleButton.Top.Set(30f, 0f);
            toggleButton.Width.Set(64f, 0f);
            toggleButton.Height.Set(64f, 0f);

            Append(toggleButton);

            toggleButton.OnLeftMouseDown += StartDragging;
            toggleButton.OnLeftMouseUp += StopDragging;
            toggleButton.OnLeftClick += ToggleMenu;

            // MENU
            menuPanel = new UIPanel();

            menuPanel.Left.Set(200f, 0f);
            menuPanel.Top.Set(80f, 0f);
            menuPanel.Width.Set(600f, 0f);
            menuPanel.Height.Set(400f, 0f);

            menuPanel.SetPadding(12f);

            navigationPanel = new UIPanel();

            navigationPanel.Left.Set(0f, 0f);
            navigationPanel.Top.Set(0f, 0f);
            navigationPanel.Width.Set(160f, 0f);
            navigationPanel.Height.Set(400f, 0f);

            navigationPanel.SetPadding(8f);

            menuPanel.Append(navigationPanel);


            // CONTENT AREA

            contentPanel = new UIPanel();

            contentPanel.Left.Set(170f, 0f);
            contentPanel.Top.Set(0f, 0f);
            contentPanel.Width.Set(-182f, 1f);
            contentPanel.Height.Set(400f, 0f);

            contentPanel.SetPadding(10f);

            menuPanel.Append(contentPanel);


            AddNavigationButton("Overview", 10f);
            AddNavigationButton("Recipes", 60f);
            AddNavigationButton("Summons", 110f);
            AddNavigationButton("Powers", 160f);
            AddNavigationButton("Status", 210f);


            ShowPage("Overview");
        }

        private void StartDragging(Terraria.UI.UIMouseEvent evt, UIElement listeningElement)
        {
            isDragging = true;
            isDragged = false;

            // Mouse position relative to the button.
            dragOffset = evt.MousePosition - toggleButton.GetDimensions().Position();

            dragStartPosition = evt.MousePosition;
        }

        private void StopDragging(Terraria.UI.UIMouseEvent evt, UIElement listeningElement)
        {
            isDragging = false;
        }

        private void ToggleMenu(
            Terraria.UI.UIMouseEvent evt,
            UIElement listeningElement
        )
        {
            if (isDragged)
            {
                isDragged = false;
                return;
            }

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
            if (toggleButton == null || !toggleButton.IsDragging)
                return;

            Vector2 mousePosition = Main.MouseScreen;

            toggleButton.Left.Set(
                mousePosition.X - toggleButton.DragOffset.X,
                0f
            );

            toggleButton.Top.Set(
                mousePosition.Y - toggleButton.DragOffset.Y,
                0f
            );

            toggleButton.Recalculate();
        }

        private void AddNavigationButton(string pageName, float top)
        {
            var button = new UITextPanel<string>(
                pageName,
                0.6f,
                true
            );

            button.Left.Set(0f, 0f);
            button.Top.Set(top, 0f);

            button.Width.Set(140f, 0f);
            button.Height.Set(38f, 0f);
            
            button.OnLeftClick += (evt, element) =>
            {
                ShowPage(pageName);
            };

            navigationPanel.Append(button);
        }

        private void ShowPage(string pageName)
        {
            currentPage = pageName;

            // Clear previous page contents.
            contentPanel.RemoveAllChildren();

            switch (pageName)
            {
                case "Overview":
                    OverviewPage();
                    break;

                case "Recipes":
                    RecipesPage();
                    break;

                case "Summons":
                    SummonsPage();
                    break;

                case "Powers":
                    PowersPage();
                    break;

                case "Status":
                    StatusPage();
                    break;
            }
        }

        private void OverviewPage()
        {
            var title = new UIText("Heishou Packs", 1.2f);

            title.Left.Set(10f, 0f);
            title.Top.Set(10f, 0f);

            contentPanel.Append(title);

            var description = new UIText(
                "Welcome to your personal summon management interface.",
                0.8f
            );

            description.Left.Set(10f, 0f);
            description.Top.Set(45f, 0f);

            contentPanel.Append(description);
        }

        private void RecipesPage()
        {
            var title = new UIText("Summon Recipes", 1.1f);

            title.Left.Set(10f, 0f);
            title.Top.Set(10f, 0f);

            contentPanel.Append(title);
        }

        private void SummonsPage()
        {
            var title = new UIText("Heishou Pack Manager", 1.1f);

            title.Left.Set(10f, 0f);
            title.Top.Set(10f, 0f);

            contentPanel.Append(title);
        }

        private void PowersPage()
        {
            var title = new UIText("Lord Powers", 1.1f);

            title.Left.Set(10f, 0f);
            title.Top.Set(10f, 0f);

            contentPanel.Append(title);
        }

        private void StatusPage()
        {
            var title = new UIText("Progression & Status", 1.1f);

            title.Left.Set(10f, 0f);
            title.Top.Set(10f, 0f);

            contentPanel.Append(title);
        }

        private void OpenRecipeDetails(string recipeName)
        {
            contentPanel.RemoveAllChildren();

            var title = new UIText(recipeName, 1.2f);

            title.Left.Set(10f, 0f);
            title.Top.Set(10f, 0f);

            contentPanel.Append(title);

            var backButton = new UITextPanel<string>("Back", 0.8f);

            backButton.Left.Set(10f, 0f);
            backButton.Top.Set(50f, 0f);

            backButton.Width.Set(100f, 0f);
            backButton.Height.Set(35f, 0f);

            backButton.OnLeftClick += (evt, element) =>
            {
                ShowPage("Recipes");
            };

            contentPanel.Append(backButton);
        }
    }
}
