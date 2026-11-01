using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinaManWuxiaNovel.Content.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChinaManWuxiaNovel.Content.UI
{
    public class HeishouPacksLeashUISystem : ModSystem
    {
        private UserInterface hongyuanInterface;
        private HeishouPacksLeashUIState hongyuanUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                hongyuanUI = new HeishouPacksLeashUIState();
                hongyuanUI.Activate();

                hongyuanInterface = new UserInterface();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ)
                return;

            bool hasHelmet = Main.LocalPlayer
                .GetModPlayer<LordOfHongyuan>()
                .HasHongLuJadeEye;

            // Show the UI only while the helmet is equipped.
            if (hasHelmet)
            {
                if (hongyuanInterface.CurrentState != hongyuanUI)
                    hongyuanInterface.SetState(hongyuanUI);

                hongyuanInterface.Update(gameTime);
            }
            else
            {
                if (hongyuanInterface.CurrentState != null)
                    hongyuanInterface.SetState(null);
            }
        }

        public override void ModifyInterfaceLayers(
            System.Collections.Generic.List<GameInterfaceLayer> layers
        )
        {
            int mouseTextIndex = layers.FindIndex(
                layer => layer.Name.Equals("Vanilla: Mouse Text")
            );

            if (mouseTextIndex != -1)
            {
                layers.Insert(
                    mouseTextIndex,
                    new LegacyGameInterfaceLayer(
                        "ChinaManWuxiaNovel: Hongyuan UI",
                        delegate
                        {
                            if (hongyuanInterface?.CurrentState != null)
                            {
                                hongyuanInterface.Draw(
                                    Main.spriteBatch,
                                    new GameTime()
                                );
                            }

                            return true;
                        },
                        InterfaceScaleType.UI
                    )
                );
            }
        }

        public override void Unload()
        {
            hongyuanInterface = null;
            hongyuanUI = null;
        }
    }
}
