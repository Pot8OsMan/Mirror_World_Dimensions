using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ChinaManWuxiaNovel.Content.Players
{
    public class LordOfHongyuan : ModPlayer
    {
        public bool HasHongLuJadeEye;

        public override void ResetEffects()
        {
            HasHongLuJadeEye = false;
        }
    }
}
