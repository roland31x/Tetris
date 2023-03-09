using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Label[,] blocks = new Label[22, 10];
        BlockStack? currentStack { get; set; }

        int PlayWidth = 10;
        int PlayHeight = 20;
        int FallDelay = 1000;
        int Offset = 3;
        int LinesCleared = 0;
        bool IsPlaying { get; set; }
        bool Alive { get; set; }
        Block? currentBlock { get; set; }
        Block? nextBlock { get; set; }
        Label[,] n_block = new Label[4, 4];
        Block? heldBlock { get; set; }
        Label[,] h_block = new Label[4, 4];
        public MainWindow()
        {
            InitializeComponent();
            Height = 800;
            Width = 1000;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitialDraw();
            IsPlaying = false;
            KeyDown += Play;
            KeyUp += FallDelaySet;          
        }
        void FallDelaySet(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Down)
            {
                FallDelay = 1000;
            }
        }
        void Play(object sender, KeyEventArgs e)
        {
            if (IsPlaying)
            {
                if (e.IsRepeat)
                {
                    return;
                }
                Key pressed = e.Key;

                if (pressed == Key.Left)
                {
                    Offset--;
                    Move(-1);
                }
                if (pressed == Key.Right)
                {
                    Offset++;
                    Move(1);
                }
                if (pressed == Key.D)
                {
                    RotateCurrentBlock_Clockwise();
                }
                if (pressed == Key.A)
                {
                    RotateCurrentBlock_AntiClockwise();
                }
                if (pressed == Key.Down)
                {
                    FallDelay = 100;
                }
                if (pressed == Key.C)
                {
                    if (heldBlock == null)
                    {
                        heldBlock = nextBlock;
                        HeldBlockDraw();
                        if (currentStack.Count == 0)
                        {
                            currentStack.AddNewStack();
                        }
                        nextBlock = currentStack.Pop();
                        NextBlockDraw();
                    }
                    else
                    {
                        currentStack.Push(nextBlock);
                        nextBlock = heldBlock;
                        NextBlockDraw();
                        heldBlock = null;
                        HeldBlockDraw();
                    }

                }
            }           
        }
        void RotateCurrentBlock_AntiClockwise()
        {
            if (currentBlock != null)
            {
                Block Temp = new Block(currentBlock);
                if (CanRotate(Temp,3))
                {
                    currentBlock.Rotate();
                    currentBlock.Rotate();
                    currentBlock.Rotate();
                    Fall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock));
                }
            }
        }
        void RotateCurrentBlock_Clockwise()
        {
            if(currentBlock != null)
            {
                Block Temp = new Block(currentBlock);
                if (CanRotate(Temp,1))
                {
                    currentBlock.Rotate();
                    Fall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock));
                }
            }           
        }
        bool CanRotate(Block b,int times)
        {
            for(int i = 0; i < times; i++)
            {
                b.Rotate();
            }           
            return CanFall(b, b.H, OffsetCheck(this.Offset,b));
        }
        void Move(int i)
        {
            if(currentBlock != null)
            {
                if (CanFall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock)))
                {
                    Fall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock));
                }
                else if (CanFall(currentBlock, currentBlock.H + 1, OffsetCheck(this.Offset, currentBlock))) // wallkick on move
                {
                    Fall(currentBlock, currentBlock.H + 1, OffsetCheck(this.Offset, currentBlock));
                }
                else if (i == -1)
                {
                    Offset++;
                }
                else Offset--;
            }
            
        }
        void InitialDraw()
        {
            DrawGameArea();
            DrawNextBlockArea();
            DrawHeldBlockArea();
        }
        void DrawHeldBlockArea()
        {
            Canvas.SetZIndex(HoldBlock, 2);
            HoldBlock.Width = Width / 4;
            HoldBlock.Height = Height / 4;
            Canvas.SetLeft(HoldBlock, 50);
            Canvas.SetTop(HoldBlock, (Height - HoldBlock.Height) / 2);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    HoldBlock.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(HoldBlock.Width / 4) });

                }
                HoldBlock.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HoldBlock.Height / 4) });
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Label l = new Label
                    {
                        Background = null,
                        Width = HoldBlock.Width / 4 - 1,
                        Height = HoldBlock.Height / 4 - 1,
                    };
                    HoldBlock.Children.Add(l);
                    Grid.SetRow(l, i);
                    Grid.SetColumn(l, j);
                    h_block[i, j] = l;
                }
            }
            TextBlock t = new TextBlock
            {
                Width = NextBlock.Width + 20,
                Height = NextBlock.Height + 10,
                Text = "HELD BLOCK:",
                TextAlignment = TextAlignment.Center,
                FontSize = 33,
                Background = new SolidColorBrush(Colors.MediumAquamarine),
            };
            MainCanvas.Children.Add(t);
            Canvas.SetLeft(t, 40);
            Canvas.SetTop(t, ((Height - NextBlock.Height) / 2) - 80);
            Canvas.SetZIndex(t, 1);

            Label HBorder = new Label
            {
                Width = t.Width + 8,
                Height = t.Height + 8,
                Background = new SolidColorBrush(Colors.Black),
            };
            MainCanvas.Children.Add(HBorder);
            Canvas.SetLeft(HBorder, 40 - 4);
            Canvas.SetTop(HBorder, ((Height - NextBlock.Height) / 2) - 80 - 4);
        }
        void DrawNextBlockArea()
        {
            Canvas.SetZIndex(NextBlock, 2);
            NextBlock.Width = Width / 4;
            NextBlock.Height = Height / 4;
            
            

            Canvas.SetLeft(NextBlock, Width - NextBlock.Width - 50);
            Canvas.SetTop(NextBlock, (Height - NextBlock.Height) / 2);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    NextBlock.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(NextBlock.Width / 4) });

                }
                NextBlock.RowDefinitions.Add(new RowDefinition { Height = new GridLength(NextBlock.Height / 4) });
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Label l = new Label
                    {
                        Background = null,
                        Width = NextBlock.Width / 4 - 1,
                        Height = NextBlock.Height / 4 - 1,
                    };
                    NextBlock.Children.Add(l);
                    Grid.SetRow(l, i);
                    Grid.SetColumn(l, j);
                    n_block[i, j] = l;
                }
            }
            TextBlock t = new TextBlock
            {
                Width = NextBlock.Width + 20,
                Height = NextBlock.Height + 10,
                Text = "NEXT BLOCK:",
                TextAlignment = TextAlignment.Center,
                FontSize = 33,
                Background = new SolidColorBrush(Colors.MediumAquamarine)
            };
            MainCanvas.Children.Add(t);
            Canvas.SetZIndex(t, 1);
            
            Canvas.SetLeft(t, Width - NextBlock.Width - 60);
            Canvas.SetTop(t, ((Height - NextBlock.Height ) / 2 ) - 80);
            Label NBorder = new Label
            {
                Width = t.Width + 8,
                Height = t.Height + 8,
                Background = new SolidColorBrush(Colors.Black),
            };
            MainCanvas.Children.Add(NBorder);
            Canvas.SetLeft(NBorder, Width - NextBlock.Width - 60 - 4);
            Canvas.SetTop(NBorder, ((Height - NextBlock.Height) / 2) - 80 - 4);
        }

        void DrawGameArea()
        {
            Canvas.SetZIndex(Area, 1);
            MainCanvas.Background = new SolidColorBrush(Colors.BlueViolet);
            Area.Background = new SolidColorBrush(Colors.DarkGray);
            Area.Width = (Width * 0.7) * 1 / 2;
            Area.Height = (Height * 0.7);
            
            Label GBorder = new Label
            {
                Width = Area.Width + 8,
                Height = Area.Height + 8,
                Background = new SolidColorBrush(Colors.Black),
            };
            MainCanvas.Children.Add(GBorder);
            Canvas.SetLeft(GBorder, (Width - Area.Width) / 2 - 4);
            Canvas.SetTop(GBorder, (Height - Area.Height) / 2 - 4);

            Canvas.SetLeft(Area, (Width - Area.Width) / 2);
            Canvas.SetTop(Area, (Height - Area.Height) / 2);
            for (int i = 0; i < PlayHeight; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    Area.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Area.Width / PlayWidth) });

                }
                Area.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Area.Height / PlayHeight) });
            }
            for (int i = 0; i < PlayHeight + 2; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    Label l = new Label
                    {
                        Background = null,
                        Width = Area.Width / PlayWidth - 1,
                        Height = Area.Height / PlayHeight - 1,
                        Tag = 0,
                    };
                    if (i >= 2)
                    {
                        Area.Children.Add(l);
                        Grid.SetRow(l, i - 2);
                        Grid.SetColumn(l, j);
                    }
                    blocks[i, j] = l;
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            StartGame();
            StartButton.IsEnabled = false;
        }
        public void ResetGame()
        {
            for(int i = 0; i < PlayHeight + 2; i++)
            {
                for(int j = 0; j < PlayWidth; j++)
                {
                    blocks[i, j].Tag = 0;
                    blocks[i, j].Background = null;
                }
            }
            Alive = true;
            IsPlaying = true;
            LinesCleared = 0;
            FallDelay = 1000;
        }
        async Task StartGame()
        {
            BlockStack blockStack = new BlockStack();
            currentStack = blockStack;
            nextBlock = blockStack.Pop();
            while (Alive)
            {
                await SpawnNewBlock(blockStack);
            }
            MessageBox.Show("You died", "Game Over");
            StartButton.IsEnabled = true;
            IsPlaying = false;
        }
        void HeldBlockDraw()
        {
            if (heldBlock != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        h_block[i, j].Background = null;
                        if (heldBlock.Body[i, j] == 1)
                        {
                            h_block[i, j].Background = heldBlock.Color;
                        }

                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        h_block[i, j].Background = null;
                    }
                }
            }
        }
        void NextBlockDraw()
        {
            if(nextBlock != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        n_block[i, j].Background = null;
                        if (nextBlock.Body[i,j] == 1)
                        {
                            n_block[i, j].Background = nextBlock.Color;
                        } 

                    }                   
                }
            }
        }
        async Task SpawnNewBlock(BlockStack bs)
        {
            this.Offset = 3;         
            Block r = nextBlock;
            currentBlock = r;
            nextBlock = bs.Pop();
            NextBlockDraw();
            if (bs.Count == 0)
            {
                bs.AddNewStack();
            }           
            Spawn(r);
            r.H = 1;         
            while (CanFall(r, r.H, OffsetCheck(this.Offset, r)))
            {
                Fall(r, r.H, OffsetCheck(this.Offset, r));
                await Task.Delay(FallDelay);
                r.H++;
            }
            if(r.H == 1)
            {
                Alive = false;
                return;
            }
            MarkCurrentBlock(r, r.H - 1, OffsetCheck(this.Offset, r));
            await LineCheck();

        }
        int OffsetCheck(int off, Block b)
        {
            int minoffset = 10;
            int maxoffset = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        if(j < minoffset)
                        {
                            minoffset = j;
                        }
                        if(j > maxoffset)
                        {
                            maxoffset = j;
                        }                      
                    }
                }
            }

            if (off + minoffset >= 0 && off + maxoffset < 10)
            {
                return off;
            }
            else if (off + minoffset < 0)
            {
                Offset = -minoffset;
                return -minoffset;
            }
            else
            {
                Offset = 9 - maxoffset;
                return 9 - maxoffset;
            }

        }
        async Task LineCheck()
        {
            bool waitanimation = false;
            int[] okLines = new int[PlayHeight + 2];
            for(int i = 2; i < PlayHeight + 2; i++)
            {
                bool ok = true;
                for (int j = 0; j < PlayWidth; j++)
                {
                    if (Convert.ToInt16(blocks[i,j].Tag) == 0)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    okLines[i] = 1;
                    LinesCleared++;
                    waitanimation = true;
                }
            }
            if (waitanimation)
            {
                await LineDestroy(okLines, Colors.Black);
                await LineDestroy(okLines, Colors.LightGray);
                await LineDestroy(okLines, Colors.Transparent);
                for (int i = 2; i < PlayHeight + 2; i++)
                {
                    if (okLines[i] == 1)
                    {
                        int k = i;
                        while (k > 2)
                        {
                            for (int j = 0; j < PlayWidth; j++)
                            {
                                blocks[k, j].Background = new SolidColorBrush(Colors.Black);
                                (blocks[k - 1, j].Background, blocks[k, j].Background) = (blocks[k, j].Background, blocks[k - 1, j].Background);
                                (blocks[k - 1, j].Tag, blocks[k, j].Tag) = (blocks[k, j].Tag, blocks[k - 1, j].Tag);
                            }
                            k--;
                        }
                    }
                }
            }           
        }
        async Task LineDestroy(int[] okLines, Color color)
        {
            for (int i = 2; i < PlayHeight + 2; i++)
            {
                if (okLines[i] == 1)
                {
                    for (int j = 0; j < PlayWidth; j++)
                    {
                        blocks[i, j].Background = new SolidColorBrush(color);
                        blocks[i, j].Tag = 0;
                    }
                }
            }
            await Task.Delay(500);
        }
        void MarkCurrentBlock(Block b, int k, int Offset)
        {
            for (int i = 0; i < k + 1; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    if (Convert.ToInt16(blocks[i, j].Tag) == 0)
                    {
                        blocks[i, j].Background = null;
                    }
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        blocks[k + i, j + Offset].Tag = 1;
                        blocks[k + i, j + Offset].Background = b.Color;
                    }
                }
            }
        }
        void Spawn(Block b)
        {

            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        blocks[i, j + Offset].Background = b.Color;
                    }
                }
            }           
        }
        void Fall(Block b, int fell, int Offset)
        {
            for (int i = 0; i < PlayHeight + 2; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    if (Convert.ToInt16(blocks[i,j].Tag) == 0)
                    {
                        blocks[i, j].Background = null;
                    }                          
                }
            }
            //await Task.Delay(1000);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        blocks[i + fell, j + Offset].Background = b.Color;
                    }
                }
            }           
        }
        bool CanFall(Block b, int fell, int Offset)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        try
                        {
                            if (Convert.ToInt16(blocks[i + fell, j + Offset].Tag) == 1)
                            {
                                return false;
                            }
                        }
                        catch (Exception)
                        {
                            return false;
                        }                      
                    }
                }
            }
            return true;
        }
    }
}
