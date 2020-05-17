using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace AmeisenBotX
{
    public partial class MapWindow : Window
    {
        public MapWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;
            MapTimer = new Timer(10);
            MapTimer.Elapsed += MapTimer_Elapsed;
            InitializeComponent();

            MeBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFFFFFF"));
            EnemyBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFF5D6C"));
            FriendBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FF8CBA51"));
            NeutralBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFFE277"));
            DefaultEntityBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFB4F2E1"));

            TextBrush = new SolidBrush(Color.White);
            TextFont = new Font("Bahnschrift Light", 6, System.Drawing.FontStyle.Regular);

            AmeisenBot.WowInterface.ObjectManager.OnObjectUpdateComplete += (List<WowObject> wowObjects) => { NeedToUpdateMap = true; };
        }

        private AmeisenBot AmeisenBot { get; set; }

        private Timer MapTimer { get; set; }

        private Brush MeBrush { get; set; }

        private Brush EnemyBrush { get; set; }

        private Brush FriendBrush { get; set; }

        private Brush NeutralBrush { get; set; }

        private Brush DefaultEntityBrush { get; set; }

        private Brush TextBrush { get; set; }

        private Font TextFont { get; set; }

        private bool NeedToUpdateMap { get; set; }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            MapTimer.Stop();
            Hide();
        }

        private BitmapImage GenerateMapImage(int width, int height)
        {
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            using Bitmap bitmap = new Bitmap(width, height);
            using Graphics graphics = Graphics.FromImage(bitmap);

            if (AmeisenBot.WowInterface.ObjectManager.Player != null)
            {
                List<WowUnit> wowUnits = AmeisenBot.WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().ToList();

                foreach (WowUnit unit in wowUnits)
                {
                    Brush selectedBrush = (AmeisenBot.WowInterface.HookManager.GetUnitReaction(AmeisenBot.WowInterface.ObjectManager.Player, unit)) switch
                    {
                        WowUnitReaction.HostileGuard => EnemyBrush,
                        WowUnitReaction.Hostile => EnemyBrush,
                        WowUnitReaction.Neutral => NeutralBrush,
                        WowUnitReaction.Friendly => FriendBrush,
                        _ => DefaultEntityBrush,
                    };

                    double scale = Math.Max(0.75, Math.Max(mapCanvasBackground.ActualWidth, mapCanvasBackground.ActualHeight) * 0.0048);

                    Point positionOnMap = GetRelativePosition
                    (
                        AmeisenBot.WowInterface.ObjectManager.Player.Position,
                        unit.Position,
                        AmeisenBot.WowInterface.ObjectManager.Player.Rotation,
                        halfWidth,
                        halfHeight,
                        scale
                    );

                    if (unit.GetType() == typeof(WowPlayer))
                    {
                        RenderUnit(positionOnMap.X, positionOnMap.Y, unit.Name, selectedBrush, TextBrush, TextFont, graphics, 5);
                    }
                    else
                    {
                        RenderUnit(positionOnMap.X, positionOnMap.Y, string.Empty, selectedBrush, TextBrush, TextFont, graphics);
                    }
                }

                RenderUnit(halfWidth, halfHeight, AmeisenBot.WowInterface.ObjectManager.Player.Name, MeBrush, TextBrush, TextFont, graphics, 5);
            }

            using MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            BitmapImage bitmapImageMap = new BitmapImage();
            bitmapImageMap.BeginInit();
            bitmapImageMap.StreamSource = memory;
            bitmapImageMap.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImageMap.EndInit();

            return bitmapImageMap;
        }

        private Point GetRelativePosition(Vector3 posA, Vector3 posB, double rotation, int x, int y, double scale = 1.0)
        {
            // X and Y swapped intentionally here !
            double relativeX = x + ((posA.Y - posB.Y) * scale);
            double relativeY = y + ((posA.X - posB.X) * scale);

            double originX = relativeX - x;
            double originY = relativeY - y;

            double rSin = Math.Sin(rotation);
            double cSin = Math.Cos(rotation);

            double newX = originX * cSin - originY * rSin;
            double newY = originX * rSin + originY * cSin;

            return new Point((int)(newX + x), (int)(newY + y));
        }

        private void RenderUnit(int width, int height, string name, Brush dotBrush, Brush textBrush, Font textFont, Graphics graphics, int size = 3)
        {
            int positionX = width;
            int positionY = height;


            int offsetStart = (int)Math.Floor(size / 2.0);
            graphics.FillRectangle(dotBrush, new Rectangle(positionX - offsetStart, positionY - offsetStart, size, size));

            if (!string.IsNullOrEmpty(name))
            {
                float nameWidth = graphics.MeasureString(name, textFont).Width;
                graphics.DrawString(name, textFont, textBrush, positionX - (nameWidth / 2F), positionY + 8);
            }
        }

        private void MapTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (NeedToUpdateMap)
            {
                Dispatcher.Invoke(() =>
                {
                    int width = (int)Math.Ceiling(mapCanvasBackground.ActualWidth);
                    int height = (int)Math.Ceiling(mapCanvasBackground.ActualHeight);

                    mapCanvas.Source = GenerateMapImage(width, height);
                });

                NeedToUpdateMap = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MapTimer.Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MapTimer.Stop();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                MapTimer.Start();
            }
            else
            {
                MapTimer.Stop();
            }
        }
    }
}