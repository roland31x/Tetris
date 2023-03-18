using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tetris
{
    class BlockStack
    {
        static readonly Random rng = new Random();


        Stack<Block> blocks { get; set; } 
        public int Count { get {  return blocks.Count; } }
      
        public BlockStack() 
        {
            // tetris works in a way where each of the 6 blocks come in a random order, so worst case scenario is 1 block showing up once every 11 blocks ( at the start of the first stack and end of the 2nd stack ). 
            List<int> nr = new List<int>(); // creates list of numbers from 0 to 6
            for (int i = 0; i < 7; i++)
            {
                nr.Add(i);
            }
            blocks = new Stack<Block>();
            for(int i = 0; i < 7; i++)  
            {
                int seed = nr[rng.Next(0, nr.Count)];  // we randomize the order of the blocks by picking out a number from the 0 - 6 list and adding that block seed to the stack
                blocks.Push(new Block(seed));
                nr.Remove(seed); 
            }
        }
        public void AddNewStack()
        {
            // refills the stack with 6 blocks in random order
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
        // standard stack methods
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
    }
    class Block
    {
        public int[,] Body { get; private set; }  // binary matrix that contains where the block has it's body on a 4x4 matrix
        public int Seed { get; private set; } // seed means the ID of the block, an easier way to identify which block it is
        public int H { get; set; } // H - aka the height property or the Vertical offset on the gameboard. This gets incremented as the block falls downwards
        public SolidColorBrush Color { get; private set; } // color of the block ( unused now )
        public ImageBrush image { get; private set; } // the image background of the block
        public bool IsAlive { get; set; } // extra bool to check if block should fall more or not ( used to avoid hardstuck blocks )
        public Block(int seed) // default constructor after seed
        {
            IsAlive = true;
            image = GetImage(seed);
            Color = GetColor(seed);
            Body = new int[4, 4];
            FillBody(seed);
            Seed = seed;
            H = 0;
        }
        public Block(Block b) // cloning constructor ( used for checking rotations )
        {
            image = b.image;
            Color = b.Color;
            Body = new int[4, 4];
            FillBody(b);
            Seed = b.Seed;
            H = b.H;
        }

        public void Rotate() // standard rotation algo, transposing the matrix and switching the columns
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
        void FillBody(Block b) // cloning the body
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
        void FillBody(int i) // creating the body based on seed, each method represents the shape
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
        static SolidColorBrush GetColor(int i)
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
        public static ImageBrush GetImage(int i)
        {
            switch (i)
            {
                case 0:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/cyan.jpg")));
                case 1:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/yellow.jpg")));
                case 2:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/purple.jpg")));
                case 3:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/green.jpg")));
                case 4:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/red.jpg")));
                case 5:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/blue.jpg")));
                case 6:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/orange.jpg")));
                default:
                    return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/red.jpg")));
            }
        }
    }
}
