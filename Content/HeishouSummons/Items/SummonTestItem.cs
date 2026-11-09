using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ChinaManWuxiaNovel.Content.HeishouSummons.NPCs.SiBranchGregor;

namespace ChinaManWuxiaNovel.Content.HeishouSummons.Items
{
    public class SummonTestItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useTime = 20;
            Item.useAnimation = 20;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.consumable = false;

            Item.rare = ItemRarityID.Blue;
        }

        public override bool? UseItem(Player player)
        {
            int npcIndex = NPC.NewNPC(
                player.GetSource_ItemUse(Item),
                (int)player.Center.X,
                (int)player.Center.Y,
                ModContent.NPCType<Gregor>()
            );

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC summon = Main.npc[npcIndex];

                summon.ai[0] = player.whoAmI;
                summon.netUpdate = true;
            }

            NPC.NewNPC(
                player.GetSource_ItemUse(Item),
                (int)player.Center.X,
                (int)player.Center.Y,
                ModContent.NPCType<Gregor>()
            );

            return true;
        }
    }
}
