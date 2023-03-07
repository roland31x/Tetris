using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tetris
{
    class Block
    {
        public int[,] Body { get; set; }
        public SolidColorBrush Color { get; set;}
        public Block(int seed)
        {
            Color = GetColor(seed);
            Body = new int[4, 4];
            FillBody(seed);
        }
        void FillBody(int i)
        {
            switch (i)
            {
                case 0:
                    I(); break;
                case 1:
                    O(); break;
                case 2:
                    T(); break;
                case 3:
                    S(); break;
                case 4:
                    Z(); break;
                case 5:
                    J(); break;
                case 6:
                    L(); break;
                
            }
        }
        void I()
        {
            Body[1, 0] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1;
            Body[1, 3] = 1;
        }
        void O()
        {
            Body[0, 1] = 1;
            Body[0, 2] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1;
        }
        void T()
        {
            Body[0, 1] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1; 
            Body[1, 0] = 1;
        }
        void S()
        {
            Body[0, 1] = 1;
            Body[0, 2] = 1;
            Body[1, 1] = 1;
            Body[1, 0] = 1;
        }
        void Z()
        {
            Body[0, 1] = 1;
            Body[0, 2] = 1;
            Body[1, 2] = 1;
            Body[1, 3] = 1;
        }
        void J()
        {
            Body[0, 1] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1;
            Body[1, 3] = 1;
        }
        void L()
        {
            Body[0, 2] = 1;
            Body[1, 0] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1;
        }
        SolidColorBrush GetColor(int i)
        {
            switch (i)
            {
                case 0:
                    return new SolidColorBrush(Colors.Cyan);
                case 1:
                    return new SolidColorBrush(Colors.Yellow);
                case 2:
                    return new SolidColorBrush(Colors.Purple);
                case 3:
                    return new SolidColorBrush(Colors.Green);
                case 4:
                    return new SolidColorBrush(Colors.Red);
                case 5:
                    return new SolidColorBrush(Colors.Blue);
                case 6:
                    return new SolidColorBrush(Colors.Orange);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }
    }
}
