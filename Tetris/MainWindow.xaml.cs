using System;
using System.Collections.Generic;
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
        int PlayWidth = 10;
        int PlayHeight = 20;
        int FallDelay = 200;
        int Offset = 3;
        bool Alive { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Height = 800;
            Width = 1000;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitialDraw();
            KeyDown += Play;
            Alive = true;
        }
        void Play(object sender, KeyEventArgs e)
        {
            Key pressed = e.Key;
            if(pressed == Key.Left)
            {
                if(Offset > 0)
                {
                    Offset--;
                }
            }
            if(pressed == Key.Right)
            {
                if(Offset < PlayWidth - 4)
                {
                    Offset++;
                }
            }
        }
        void InitialDraw()
        {
            Area.Background = new SolidColorBrush(Colors.Gray);
            Area.Width = ( Width * 0.7 ) * 1 / 2;
            Area.Height = ( Height * 0.7 );
            Canvas.SetLeft(Area, (Width - Area.Width) / 2 );
            Canvas.SetTop(Area, (Height - Area.Height) / 2 );
            for(int i = 0; i < PlayHeight; i++)
            {
                for(int j = 0; j < PlayWidth; j++)
                {
                    Area.ColumnDefinitions.Add(new ColumnDefinition{ Width = new GridLength(Area.Width / PlayWidth) } );

                }
                Area.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Area.Height / PlayHeight) } );
            }
            for (int i = 0; i < PlayHeight + 2; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    Label l = new Label
                    {
                        Background = new SolidColorBrush(Colors.Black),
                        Width = Area.Width / PlayWidth - 1,
                        Height = Area.Height / PlayHeight - 1,
                        Tag = 0,
                    };
                    if(i >= 2)
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
            StartGame();
            StartButton.IsEnabled = false;
        }
        async Task StartGame()
        {
            while (Alive)
            {
                await SpawnNewBlock();
            }
            MessageBox.Show("You died", "Game Over");          
        }
        async Task SpawnNewBlock()
        {
            Block r = new Block(3);
            await Spawn(r);
            int k = 1;
            while (CanFall(r, k,Offset))
            {
                await Fall(r, k,Offset);
                k++;
            }
            if(k == 1)
            {
                Alive = false;
                return;
            }
            MarkCurrentBlock(r, k,Offset);
            LineCheck();

        }
        void LineCheck()
        {

        }
        void MarkCurrentBlock(Block b, int k, int Offset)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        blocks[k + i - 1, j + Offset].Tag = 1;
                    }
                }
            }
        }
        async Task Spawn(Block b)
        {
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    if (b.Body[i, j] == 1)
                    {
                        blocks[i, j + 3].Background = b.Color;
                    }
                }
            }
            await Task.Delay(FallDelay);
        }
        async Task Fall(Block b, int fell, int Offset)
        {
            for (int i = 0; i < fell + 1; i++)
            {
                for (int j = 0; j < PlayWidth; j++)
                {
                    if (Convert.ToInt16(blocks[i,j].Tag) == 0)
                    {
                        blocks[i, j].Background = new SolidColorBrush(Colors.Black);
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
            await Task.Delay(FallDelay);
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
