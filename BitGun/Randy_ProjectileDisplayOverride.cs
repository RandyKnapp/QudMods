using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World.Parts
{
    [Serializable]
    public class Randy_ProjectileDisplayOverride : IPart
    {
        public string ColorString;
        public string Chars;
        public string Colors;

        public override void Register(GameObject Object)
        {
            ColorString = string.IsNullOrEmpty(ColorString) ? string.Empty : ColorString;
            Object.RegisterPartEvent(this, "ProjectileEntering");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ProjectileEntering")
            {
                var cell = E.GetParameter("Cell") as Cell;
                ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer2(false);

                scrapBuffer2.Goto(cell.X, cell.Y);
                var current = scrapBuffer2[cell.X, cell.Y];

                char display = current.Char;
                if (!string.IsNullOrEmpty(Chars))
                {
                    display = Chars[Stat.RandomCosmetic(0, Chars.Length - 1)];
                }

                string colorDisplay = ColorString;
                if (!string.IsNullOrEmpty(Colors))
                {
                    colorDisplay = "&" + Colors[Stat.RandomCosmetic(0, Colors.Length - 1)];
                }

                scrapBuffer2.Write(colorDisplay + display);
            }
            return base.FireEvent(E);
        }
    }
}