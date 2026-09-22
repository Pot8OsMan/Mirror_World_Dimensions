using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ChinaManWuxiaNovel.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class TheLordOfHongyuanHongLuHead : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = Item.sellPrice(platinum: 9, gold: 6, silver: 5, copper: 1);
            Item.rare = ItemRarityID.Red;
            Item.defense = 18;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "[Combat Start] Apply 1 Defense Power Up (5% defense) with every other allies. (Max 12)";
            player.moveSpeed += 8f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Emerald, 1);
            recipe.AddIngredient(ItemID.GoldBar, 5);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.SuspiciousLookingEye, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
