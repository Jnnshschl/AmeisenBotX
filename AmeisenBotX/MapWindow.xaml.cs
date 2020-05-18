using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.States;
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

            MeBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFFFFFF"));
            EnemyBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFF5D6C"));
            FriendBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FF8CBA51"));
            NeutralBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFFE277"));
            DefaultEntityBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFB4F2E1"));

            DungeonNodeBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FF808080"));
            DungeonNodePen = new Pen((Color)new ColorConverter().ConvertFromString("#FFFFFFFF"), 1);

            PathNodeBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FF00FFFF"));
            PathNodePen = new Pen((Color)new ColorConverter().ConvertFromString("#FFE0FFFF"), 1);

            BlacklistNodeBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFF0000"));
            BlacklistNodePen = new Pen((Color)new ColorConverter().ConvertFromString("#FFFF0000"), 1);

            TextBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFFFFFF"));
            TextFont = new Font("Bahnschrift Light", 6, System.Drawing.FontStyle.Regular);

            AmeisenBot.WowInterface.ObjectManager.OnObjectUpdateComplete += (List<WowObject> wowObjects) => { NeedToUpdateMap = true; };

            InitializeComponent();
        }

        private AmeisenBot AmeisenBot { get; set; }

        private Brush BlacklistNodeBrush { get; set; }

        private Pen BlacklistNodePen { get; set; }

        private Brush DefaultEntityBrush { get; set; }

        private Brush DungeonNodeBrush { get; set; }

        private Pen DungeonNodePen { get; set; }

        private Brush EnemyBrush { get; set; }

        private Brush FriendBrush { get; set; }

        private Timer MapTimer { get; set; }

        private Brush MeBrush { get; set; }

        private bool NeedToUpdateMap { get; set; }

        private Brush NeutralBrush { get; set; }

        private Brush PathNodeBrush { get; set; }

        private Pen PathNodePen { get; set; }

        private Brush TextBrush { get; set; }

        private Font TextFont { get; set; }

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

                double scale = Math.Min(Math.Max(0.75, Math.Max(mapCanvasBackground.ActualWidth, mapCanvasBackground.ActualHeight) * 0.0048), 4.0);
                Vector3 playerPosition = AmeisenBot.WowInterface.ObjectManager.Player.Position;
                double playerRotation = AmeisenBot.WowInterface.ObjectManager.Player.Rotation;

                // render DungeonPath

                if (AmeisenBot.WowInterface.DungeonEngine.Nodes?.Count > 0)
                {
                    for (int i = 1; i < AmeisenBot.WowInterface.DungeonEngine.Nodes.Count; ++i)
                    {
                        Vector3 node = AmeisenBot.WowInterface.DungeonEngine.Nodes[i].Position;
                        Vector3 prevNode = AmeisenBot.WowInterface.DungeonEngine.Nodes[i - 1].Position;

                        Point nodePositionOnMap = GetRelativePosition(playerPosition, node, playerRotation, halfWidth, halfHeight, scale);
                        Point prevNodePositionOnMap = GetRelativePosition(playerPosition, prevNode, playerRotation, halfWidth, halfHeight, scale);

                        RenderNode(nodePositionOnMap.X, nodePositionOnMap.Y, prevNodePositionOnMap.X, prevNodePositionOnMap.Y, DungeonNodeBrush, DungeonNodePen, graphics, 3);
                    }
                }

                // render Movement

                if (AmeisenBot.WowInterface.MovementEngine.Path?.Count > 0)
                {
                    for (int i = 0; i < AmeisenBot.WowInterface.MovementEngine.Path.Count; ++i)
                    {
                        Vector3 node = AmeisenBot.WowInterface.MovementEngine.Path[i];
                        Vector3 prevNode = i == 0 ? playerPosition : AmeisenBot.WowInterface.MovementEngine.Path[i - 1];

                        Point nodePositionOnMap = GetRelativePosition(playerPosition, node, playerRotation, halfWidth, halfHeight, scale);
                        Point prevNodePositionOnMap = GetRelativePosition(playerPosition, prevNode, playerRotation, halfWidth, halfHeight, scale);

                        RenderNode(nodePositionOnMap.X, nodePositionOnMap.Y, prevNodePositionOnMap.X, prevNodePositionOnMap.Y, PathNodeBrush, PathNodePen, graphics, 3);
                    }
                }

                // render Blacklist Nodes

                if (AmeisenBot.WowInterface.BotCache.TryGetBlacklistPosition((int)AmeisenBot.WowInterface.ObjectManager.MapId, playerPosition, 64, out List<Vector3> blacklistNodes))
                {
                    for (int i = 0; i < blacklistNodes.Count; ++i)
                    {
                        Vector3 node = blacklistNodes[i];
                        Point nodePositionOnMap = GetRelativePosition(playerPosition, node, playerRotation, halfWidth, halfHeight, scale);

                        RenderBlacklistNode(nodePositionOnMap.X, nodePositionOnMap.Y, BlacklistNodeBrush, BlacklistNodePen, graphics, 3, 32);
                    }
                }

                // render Units

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

                    Point positionOnMap = GetRelativePosition(playerPosition, unit.Position, playerRotation, halfWidth, halfHeight, scale);

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

        private void MapTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (NeedToUpdateMap && AmeisenBot.StateMachine.CurrentState.Key != BotState.LoadingScreen)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    int width = (int)Math.Ceiling(mapCanvasBackground.ActualWidth);
                    int height = (int)Math.Ceiling(mapCanvasBackground.ActualHeight);

                    mapCanvas.Source = GenerateMapImage(width, height);
                });

                NeedToUpdateMap = false;
            }
        }

        private void RenderBlacklistNode(int x, int y, Brush blacklistNodeBrush, Pen blacklistNodePen, Graphics graphics, int size, int radius)
        {
            int offsetStart = (int)Math.Floor(size / 2.0);
            graphics.FillRectangle(blacklistNodeBrush, new Rectangle(x - offsetStart, y - offsetStart, size, size));
            graphics.DrawEllipse(blacklistNodePen, new Rectangle(x - radius, y - radius, radius * 2, radius * 2));
        }

        private void RenderNode(int x1, int y1, int x2, int y2, Brush dotBrush, Pen linePen, Graphics graphics, int size)
        {
            int offsetStart = (int)Math.Floor(size / 2.0);
            graphics.FillRectangle(dotBrush, new Rectangle(x1 - offsetStart, y1 - offsetStart, size, size));
            graphics.FillRectangle(dotBrush, new Rectangle(x2 - offsetStart, y2 - offsetStart, size, size));
            graphics.DrawLine(linePen, x1, y1, x2, y2);
        }

        private void RenderUnit(int width, int height, string name, Brush dotBrush, Brush textBrush, Font textFont, Graphics graphics, int size = 3)
        {
            int offsetStart = (int)Math.Floor(size / 2.0);
            graphics.FillRectangle(dotBrush, new Rectangle(width - offsetStart, height - offsetStart, size, size));

            if (!string.IsNullOrEmpty(name))
            {
                float nameWidth = graphics.MeasureString(name, textFont).Width;
                graphics.DrawString(name, textFont, textBrush, width - (nameWidth / 2F), height + 8);
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MapTimer.Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MapTimer.Stop();
        }
    }
}