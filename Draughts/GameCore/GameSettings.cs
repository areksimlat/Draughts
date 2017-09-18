using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore
{
    public class GameSettings
    {
        public bool International { get; set; }
        public int PieceColor { get; set; }
        public int Depth { get; set; }
        public int MoveTimeInSec { get; set; }

        public GameSettings()
        {

        }
    }
}
