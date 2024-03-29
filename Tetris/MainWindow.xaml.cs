﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
        const int PlayWidth = 10; // tetris playfield is 10x20 visible with 2 hidden rows above
        const int PlayHeight = 20;
        Label[,] blocks = new Label[22, 10]; 

        BlockStack? currentStack { get; set; } // the stack of blocks we will be pulling our blocks from

        int CurrentLevel = 1; // game starts from level 1
        int Score = 0; 
        int FallDelay = 1000;  // how often the game makes a block fall / check for block death
        int LinesCleared = 0; // how many lines have been cleared

        int Offset = 3; // This is the horizontal offset of the playfield, by default and on block spawns this gets reset to 3 so the block can spawn in the middle with a left offset for blocks that aren't the I or O
        
        bool IsPlaying { get; set; } // checks wether the game was started or not
        bool Alive { get; set; } // used to see if blocks can spawn or there is a block obstructing the spawn aka the playfield has been filled up


        Block? currentBlock { get; set; } // the block that is currently falling down
        Block? nextBlock { get; set; } // the next block that will be falling down
        Label[,] n_block = new Label[4, 4]; // the visual representation of the block of it
        Block? heldBlock { get; set; } // the block that is currently held by the player
        Label[,] h_block = new Label[4, 4]; // same as nextblock matrix

        Label progress { get; set; } // the level progressbar that fills up as the player clears lines

        public MainWindow()
        {           
            InitializeComponent();
            Height = 800;
            Width = 1000;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitialDraw(); // draws the areas that need to be calculated programatically instead of just placed in the designer
            CurrentLevelLabel.Content = $"LEVEL {CurrentLevel}";
            IsPlaying = false;
            KeyDown += Play; 
        }
        async void Play(object sender, KeyEventArgs e) 
        {
            // marked async so we can await the block manipulation functions, to avoid fall checking conflicts
            if (IsPlaying)
            {
                //if (e.IsRepeat)
                //{
                //    return;
                //}
                Key pressed = e.Key;

                if (pressed == Key.Left)
                {
                    Offset--;
                    await Move(-1); // tries to move block to the left
                }
                if (pressed == Key.Right)
                {
                    Offset++;
                    await Move(1); // tries to move block to the right
                }
                if (pressed == Key.D)
                {
                    await RotateCurrentBlock_Clockwise();
                }
                if (pressed == Key.A)
                {
                    await RotateCurrentBlock_AntiClockwise(); 
                }
                if (pressed == Key.Down)
                {
                    // THIS IS SSTILL WIP
                    // if block can fall downwards 1 line, it will, if not, nothing happens 
                    Block gameBlock = currentBlock;
                    if (CanFall(gameBlock, gameBlock.H + 1, OffsetCheck(this.Offset, gameBlock)) && gameBlock.IsAlive)
                    {
                        gameBlock.H++;
                        await Fall(gameBlock, gameBlock.H, OffsetCheck(this.Offset, gameBlock));
                    }
                    //else
                    //{
                    //    gameBlock.IsAlive = false;
                    //}
                    //FallDelay = 100;
                }
                if (pressed == Key.C) 
                {
                    // holding block mechanism, checks wether the player is holding a block ( if it is then it gets put on top of the block stack so it can drop next ), if not then the next block that would fall is stored.
                    if (heldBlock == null)
                    {
                        heldBlock = nextBlock;
                        HeldBlockDraw(); // shows it visually 
                        if (currentStack.Count == 0) // we have to check if the stack is empty , if it is we have to add a new one because we have to draw the next block that will fall
                        {
                            currentStack.AddNewStack();
                        }
                        nextBlock = currentStack.Pop();
                        NextBlockDraw();
                    }
                    else // held block is pushed back on top of the stack and visuals are updated
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
        async Task RotateCurrentBlock_AntiClockwise()
        {
            if (currentBlock != null)
            {
                Block Temp = new Block(currentBlock); // we create a temporary copy of the current block, rotate it 3 times to achieve an anti-clockwise rotation and check wether it fits in new position or not
                if (CanRotate(Temp,3))
                {
                    currentBlock.Rotate();
                    currentBlock.Rotate();
                    currentBlock.Rotate();
                    await Fall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock));
                }
            }
        }
        async Task RotateCurrentBlock_Clockwise() 
        {
            if(currentBlock != null) // same function as the anti-clockwise, but we only check if the block can fit by rotating it once
            {
                Block Temp = new Block(currentBlock);
                if (CanRotate(Temp,1))
                {
                    currentBlock.Rotate();
                    await Fall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock));
                }
            }           
        }
        bool CanRotate(Block b,int times) 
        {
            // checks what positions the block can rotate to, aka implementing a primite "wall-kick" algorithm.
            for (int i = 0; i < times; i++)
            {
                b.Rotate();
            }
            if (CanFall(b, b.H, OffsetCheck(this.Offset, b)))
            {
                return true;
            }
            else if (CanFall(b, b.H, OffsetCheck(this.Offset - 1, b)))
            {
                Offset -= 1;
                return true;
            }
            else if (CanFall(b, b.H, OffsetCheck(this.Offset + 1, b)))
            {
                Offset += 1;
                return true;
            }
            else if (CanFall(b, b.H + 1, OffsetCheck(this.Offset, b)))
            {
                currentBlock.H += 1;
                return true;
            }
            else if (CanFall(b, b.H + 1, OffsetCheck(this.Offset - 1, b)))
            {
                Offset -= 1;
                currentBlock.H += 1;
                return true;
            }
            else if (CanFall(b, b.H + 1, OffsetCheck(this.Offset + 1, b)))
            {
                Offset += 1;
                currentBlock.H += 1;
                return true;
            }           
            else return false;
        }
        async Task Move(int i)
        {
            if(currentBlock != null)
            {
                if (CanFall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock)))
                {
                    await Fall(currentBlock, currentBlock.H, OffsetCheck(this.Offset, currentBlock));
                }
                else if (CanFall(currentBlock, currentBlock.H + 1, OffsetCheck(this.Offset, currentBlock))) // wallkick on move
                {
                    await Fall(currentBlock, currentBlock.H + 1, OffsetCheck(this.Offset, currentBlock));
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
            DrawProgressBar();
            ScoreLabel.Content = Score.ToString();

        }
        void DrawProgressBar()
        {
            Label backgr = new Label()
            {
                Width = ProgressBackground.Width - 8,
                Height = ProgressBackground.Height - 8,
                Background = App.Current.Resources["RainbowGradient1"] as LinearGradientBrush
            };
            MainCanvas.Children.Add(backgr);
            Canvas.SetLeft(backgr, Canvas.GetLeft(ProgressBackground) + 4);
            Canvas.SetTop(backgr, Canvas.GetTop(ProgressBackground) + 4);
            Panel.SetZIndex(backgr, 2);

            progress = new Label()
            {
                Width = ProgressBackground.Width - 6,
                Height = ProgressBackground.Height - 8,
                Background = App.Current.Resources["BlackBlueGradient"] as LinearGradientBrush
            };
            Panel.SetZIndex(progress, 3);
            MainCanvas.Children.Add(progress);
            Canvas.SetRight(progress, Canvas.GetLeft(ProgressBackground) - 12);
            Canvas.SetTop(progress, Canvas.GetTop(ProgressBackground) + 4);
        }
        async Task UpdateProgress()
        {
            ScoreLabel.Content = Score.ToString();
            double startwidth = progress.Width;
            double percent = (double)LinesCleared / 12;
            if(percent > 1)
            {
                percent = 1;
            }
            double endwidth = ProgressBackground.Width - (ProgressBackground.Width - 4) * percent;
            for(double i = startwidth; i >= endwidth; i--)
            {
                progress.Width = i;
                await Task.Delay(20);
            }            
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
                        Width = HoldBlock.Width / 4,
                        Height = HoldBlock.Height / 4,
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
                FontFamily = App.Current.Resources["Bauhaus93"] as FontFamily,
                Background = new SolidColorBrush(Colors.LightSteelBlue),
            };
            MainCanvas.Children.Add(t);
            Canvas.SetLeft(t, 40);
            Canvas.SetTop(t, ((Height - NextBlock.Height) / 2) - 80);
            Canvas.SetZIndex(t, 1);

            Label HBorder = new Label
            {
                Width = t.Width + 8,
                Height = t.Height + 8,
                Background = App.Current.Resources["BlueBlueGradient"] as LinearGradientBrush,
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
                        Width = NextBlock.Width / 4,
                        Height = NextBlock.Height / 4,
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
                FontFamily = App.Current.Resources["Bauhaus93"] as FontFamily,
                Background = new SolidColorBrush(Colors.LightCoral)
            };
            MainCanvas.Children.Add(t);
            Canvas.SetZIndex(t, 1);
            
            Canvas.SetLeft(t, Width - NextBlock.Width - 60);
            Canvas.SetTop(t, ((Height - NextBlock.Height ) / 2 ) - 80);
            Label NBorder = new Label
            {
                Width = t.Width + 8,
                Height = t.Height + 8,
                Background = App.Current.Resources["BlueBlueGradient"] as LinearGradientBrush,
            };
            MainCanvas.Children.Add(NBorder);
            Canvas.SetLeft(NBorder, Width - NextBlock.Width - 60 - 4);
            Canvas.SetTop(NBorder, ((Height - NextBlock.Height) / 2) - 80 - 4);
        }

        void DrawGameArea()
        {
            Canvas.SetZIndex(Area, 1);
            //MainCanvas.Background = new SolidColorBrush(Colors.BlueViolet);
            Area.Background = App.Current.Resources["BlackBlueGradient"] as LinearGradientBrush;
            Area.Width = (Width * 0.7) * 1 / 2;
            Area.Height = (Height * 0.7);
            
            Label GBorder = new Label
            {
                Width = Area.Width + 8,
                Height = Area.Height + 8,
                Background = App.Current.Resources["BlueBlueGradient"] as LinearGradientBrush,
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
                        Width = Area.Width / PlayWidth,
                        Height = Area.Height / PlayHeight,
                        Tag = 0,                     
                    };
                    //Border border = new Border
                    //{
                    //    BorderBrush = null,
                    //    BorderThickness = new Thickness(1),
                    //};
                    //border.Child = l;
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
            Score = 0;
            CurrentLevel = 1;
            progress.Width = ProgressBackground.Width - 8;
            heldBlock = null;
            LevelCheck();
            UpdateProgress();
            //FallDelay = 1000;
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
                if(heldBlock.Seed > 1)
                {
                    Canvas.SetLeft(HoldBlock, 50 + 30);
                }
                else
                {
                    Canvas.SetLeft(HoldBlock, 50);
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        h_block[i, j].Background = null;
                        if (heldBlock.Body[i, j] == 1)
                        {
                            h_block[i, j].Background = heldBlock.image;
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
                if (nextBlock.Seed > 1)
                {
                    Canvas.SetLeft(NextBlock, Width - NextBlock.Width - 50 + 30);
                }
                else
                {
                    Canvas.SetLeft(NextBlock, Width - NextBlock.Width - 50);
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        n_block[i, j].Background = null;
                        if (nextBlock.Body[i,j] == 1)
                        {
                            n_block[i, j].Background = nextBlock.image;
                        } 

                    }                   
                }
            }
        }
        async Task SpawnNewBlock(BlockStack bs)
        {
            this.Offset = 3;         
            Block gameBlock = nextBlock;
            currentBlock = gameBlock;
            nextBlock = bs.Pop();
            NextBlockDraw();
            if (bs.Count == 0)
            {
                bs.AddNewStack();
            }           
            Spawn(gameBlock);

            await PlayWithBlock(gameBlock);


        }
        async Task PlayWithBlock(Block gameBlock)
        {
            gameBlock.H = 0;
            while (gameBlock.IsAlive)
            {
                if(CanFall(gameBlock, gameBlock.H + 1, OffsetCheck(this.Offset, gameBlock)))
                {
                    gameBlock.H++;
                    Fall(gameBlock, gameBlock.H, OffsetCheck(this.Offset, gameBlock));
                }
                else
                {
                    gameBlock.IsAlive = false;
                    break;
                }
                await Task.Delay(FallDelay);
            }
            if (gameBlock.H <= 1)
            {
                Alive = false;
                return;
            }
            MarkCurrentBlock(gameBlock, gameBlock.H, OffsetCheck(this.Offset, gameBlock));
            gameBlock.IsAlive = false;         
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
                int RoundLines = 0;
                for(int i = 0; i < 7; i++)
                {
                    await LineDestroy(okLines, 250, Block.GetImage(i));
                }
                await LineDestroy(okLines, 500, Colors.Black); // colors.black has no meaning, just a different parameter to display it as transparent

                for (int i = 2; i < PlayHeight + 2; i++)
                {
                    if (okLines[i] == 1)
                    {
                        RoundLines++;
                        int k = i;
                        while (k > 2)
                        {
                            for (int j = 0; j < PlayWidth; j++)
                            {
                                (blocks[k - 1, j].Background, blocks[k, j].Background) = (blocks[k, j].Background, blocks[k - 1, j].Background);
                                (blocks[k - 1, j].Tag, blocks[k, j].Tag) = (blocks[k, j].Tag, blocks[k - 1, j].Tag);
                            }
                            k--;
                        }
                    }
                }
                switch (RoundLines)
                {
                    case 1:
                        Score += 40 * CurrentLevel;
                        break;
                    case 2:
                        Score += 100 * CurrentLevel;
                        break;
                    case 3:
                        Score += 300 * CurrentLevel;
                        break;
                    case 4:
                        Score += 1200 * CurrentLevel;
                        break;                        
                }
                if(LinesCleared - (10 + CurrentLevel) >= 0)
                {
                    await UpdateProgress();
                    LinesCleared -= (10 + CurrentLevel);
                    progress.Width = ProgressBackground.Width - 8;
                    CurrentLevel++;
                    //MessageBox.Show($"You advanced to level {CurrentLevel}!");
                    LevelCheck();                                      
                }
                UpdateProgress();
            }           
        }
        void LevelCheck()
        {
            CurrentLevelLabel.Content = $"LEVEL {CurrentLevel}";
            if(CurrentLevel <= 8)
            {
                FallDelay = 1000 - CurrentLevel * 100;
            }
            else
            {
                FallDelay = 100;
            }
        }
        async Task LineDestroy(int[] okLines,int delay, object color)
        {
            for (int i = 2; i < PlayHeight + 2; i++)
            {
                if (okLines[i] == 1)
                {
                    for (int j = 0; j < PlayWidth; j++)
                    {
                        if(color is ImageBrush)
                        {
                            blocks[i, j].Background = color as ImageBrush;
                        }
                        else
                        {
                            blocks[i, j].Background = null;
                        }
                        blocks[i, j].Tag = 0;
                    }
                }
            }
            await Task.Delay(delay);
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
                        //(blocks[i, j].Parent as Border).BorderBrush = null;
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
                        blocks[k + i, j + Offset].Background = b.image;
                        //(blocks[k + i, j + Offset].Parent as Border).BorderBrush = new SolidColorBrush(Colors.Black);
                    }
                }
            }
            Score += b.H;
            UpdateProgress();
        }
        void Spawn(Block b)
        {

            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        blocks[i, j + Offset].Background = b.image;
                        //(blocks[i, j + Offset].Parent as Border).BorderBrush = new SolidColorBrush(Colors.Black);
                    }
                }
            }           
        }
        async Task Fall(Block b, int fell, int Offset)
        {
            for (int i = 0; i < PlayHeight + 2; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    if (Convert.ToInt16(blocks[i,j].Tag) == 0)
                    {
                        blocks[i, j].Background = null;
                        //(blocks[i, j].Parent as Border).BorderBrush = null;
                    }
                    //else
                    //{
                    //    blocks[i, j].Background = new SolidColorBrush(Colors.GhostWhite);
                    //}
                }
            }
            //await Task.Delay(1000);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        //blocks[i + fell, j + Offset].Background = b.Color;
                        blocks[i + fell, j + Offset].Background = b.image;
                        //(blocks[i + fell, j + Offset].Parent as Border).BorderBrush = new SolidColorBrush(Colors.Black);
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
