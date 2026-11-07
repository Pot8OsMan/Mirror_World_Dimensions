using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ChinaManWuxiaNovel.Content.HeishouSummons.NPCs.SiBranchGregor
{
    public class Gregor : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.width = 20;
            NPC.height = 40;

            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = 500;

            NPC.friendly = true;
            NPC.townNPC = true;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.knockBackResist = 0f;
        }

        public override void AI()
        {
            // Movement AI will be implemented here. When I'm less lazy
        }
    }
}
