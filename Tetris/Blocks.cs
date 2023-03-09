using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tetris
{
    class BlockStack
    {
        Stack<Block> blocks { get; set; }
        public int Count { get {  return blocks.Count; } }
        Block? Held { get; set; }
        static readonly Random rng = new Random();
        public BlockStack() 
        {
            List<int> nr = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                nr.Add(i);
            }
            blocks = new Stack<Block>();
            for(int i = 0; i < 7; i++)
            {
                int seed = nr[rng.Next(0, nr.Count)];
                blocks.Push(new Block(seed));
                nr.Remove(seed);
            }
            Held = null;
        }
        public void AddNewStack()
        {
            List<int> nr = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                nr.Add(i);
            }
            for (int i = 0; i < 7; i++)
            {
                int seed = nr[rng.Next(0, nr.Count)];
                blocks.Push(new Block(seed));
                nr.Remove(seed);
            }
        }
        public void Push(Block block)
        {
            blocks.Push(block);
        }
        public Block Pop()
        {
            return blocks.Pop();
        }
        public Block Peek()
        {
            if (blocks.Count > 0)
            {
                return blocks.Peek();
            }
            else return new Block(0);
        }
        public void GetHeldBlock()
        {
            if (Held != null)
            {
                blocks.Push(Held);
            }
            else return;
        }
        public bool Hold(Block b)
        {
            if (Held == null)
            {
                Held = b;
                return true;
            }
            else return false;
        }
    }
    class Block
    {
        public int[,] Body { get; set; }
        public int Seed { get; set; }
        public int H { get; set; }
        public SolidColorBrush Color { get; set;}
        public Block(int seed)
        {
            Color = GetColor(seed);
            Body = new int[4, 4];
            FillBody(seed);
            Seed = seed;
            H = 0;
        }
        public Block(Block b)
        {
            Color = b.Color;
            Body = new int[4, 4];
            FillBody(b);
            Seed = b.Seed;
            H = b.H;
        }

        public void Rotate()
        {
            if (Seed == 0)
            {              
                for (int i = 0; i < 4; i++)
                {
                    for (int j = i; j < 4; j++)
                    {
                        if (i != j)
                        {
                            (Body[i, j], Body[j, i]) = (Body[j, i], Body[i, j]);
                        }
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    (Body[i, 0], Body[i,3]) = (Body[i, 3], Body[i, 0]);
                    (Body[i, 1], Body[i, 2]) = (Body[i, 2], Body[i, 1]);
                }
            }
            else if(Seed == 1)
            {
                return;
            }
            else 
            { 
                for(int i = 0; i < 3; i++)
                {
                    for(int j = i; j < 3; j++)
                    {
                        if(i != j)
                        {
                            (Body[i, j], Body[j, i]) = (Body[j, i], Body[i, j]);
                        }
                    }
                }
                for (int i = 0; i < 3; i++)
                {

                    (Body[i, 0], Body[i, 2]) = (Body[i, 2], Body[i, 0]);
                }
            }
            
        }
        void FillBody(Block b)
        {
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    if (b.Body[i,j] == 1)
                    {
                        this.Body[i, j] = 1;
                    }
                }
            }
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
            Body[0, 0] = 1;
            Body[0, 1] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1;
        }
        void J()
        {
            Body[0, 0] = 1;
            Body[1, 0] = 1;
            Body[1, 1] = 1;
            Body[1, 2] = 1;
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
