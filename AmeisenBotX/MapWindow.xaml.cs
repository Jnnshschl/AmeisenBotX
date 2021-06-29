﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace AmeisenBotX
{
    public partial class MapWindow : Window
    {
        private int mapTimerBusy;

        public MapWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;

            MapTimer = new(250);
            MapTimer.Elapsed += MapTimer_Elapsed;

            MeBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFFFFFF"));
            EnemyBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFFF5D6C"));
            DeadBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FFACACAC"));
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

            SubTextBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#DCDCDC"));
            SubTextFont = new Font("Bahnschrift Light", 5, System.Drawing.FontStyle.Regular);

            OreBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FF6F4E37"));
            HerbBrush = new SolidBrush((Color)new ColorConverter().ConvertFromString("#FF7BB661"));

            AmeisenBot.WowInterface.Objects.OnObjectUpdateComplete += (IEnumerable<WowObject> wowObjects) => { NeedToUpdateMap = true; };

            InitializeComponent();
        }

        private AmeisenBot AmeisenBot { get; set; }

        private Bitmap Bitmap { get; set; }

        private Brush BlacklistNodeBrush { get; set; }

        private Pen BlacklistNodePen { get; set; }

        private Brush DeadBrush { get; set; }

        private Brush DefaultEntityBrush { get; set; }

        private Brush DungeonNodeBrush { get; set; }

        private Pen DungeonNodePen { get; set; }

        private Brush EnemyBrush { get; set; }

        private Brush FriendBrush { get; set; }

        private Graphics Graphics { get; set; }

        private Brush HerbBrush { get; set; }

        private Timer MapTimer { get; set; }

        private Brush MeBrush { get; set; }

        private bool NeedToUpdateMap { get; set; }

        private Brush NeutralBrush { get; set; }

        private Brush OreBrush { get; set; }

        private Brush PathNodeBrush { get; set; }

        private Pen PathNodePen { get; set; }

        private float Scale { get; set; }

        private Brush SubTextBrush { get; set; }

        private Font SubTextFont { get; set; }

        private Brush TextBrush { get; set; }

        private Font TextFont { get; set; }

        private static Point GetRelativePosition(Vector3 posA, Vector3 posB, float rotation, int x, int y, float scale = 1.0f)
        {
            // X and Y swapped intentionally here !
            float relativeX = x + ((posA.Y - posB.Y) * scale);
            float relativeY = y + ((posA.X - posB.X) * scale);

            float originX = relativeX - x;
            float originY = relativeY - y;

            float rSin = MathF.Sin(rotation);
            float cSin = MathF.Cos(rotation);

            float newX = originX * cSin - originY * rSin;
            float newY = originX * rSin + originY * cSin;

            return new Point((int)(newX + x), (int)(newY + y));
        }

        private static void RenderBlacklistNode(int x, int y, Brush blacklistNodeBrush, Pen blacklistNodePen, Graphics graphics, int size, int radius)
        {
            int offsetStart = (int)(size / 2.0);
            graphics.FillRectangle(blacklistNodeBrush, new(x - offsetStart, y - offsetStart, size, size));
            graphics.DrawEllipse(blacklistNodePen, new(x - radius, y - radius, radius * 2, radius * 2));
        }

        private static void RenderGameobject(int width, int height, string name, Brush dotBrush, Brush textBrush, Font textFont, Graphics graphics, int size = 3)
        {
            int offsetStart = (int)(size / 2.0);
            graphics.FillRectangle(dotBrush, new(width - offsetStart, height - offsetStart, size, size));

            if (!string.IsNullOrEmpty(name))
            {
                float nameWidth = graphics.MeasureString(name, textFont).Width;
                graphics.DrawString(name, textFont, textBrush, width - (nameWidth / 2F), height + 8);
            }
        }

        private static void RenderNode(int x1, int y1, int x2, int y2, Brush dotBrush, Pen linePen, Graphics graphics, int size)
        {
            int offsetStart = (int)(size / 2.0);
            graphics.FillRectangle(dotBrush, new(x1 - offsetStart, y1 - offsetStart, size, size));
            graphics.FillRectangle(dotBrush, new(x2 - offsetStart, y2 - offsetStart, size, size));
            graphics.DrawLine(linePen, x1, y1, x2, y2);
        }

        private static void RenderUnit(int width, int height, string name, string subtext, Brush dotBrush, Brush textBrush, Font textFont, Font subtextFont, Brush subTextBrush, Graphics graphics, int size = 3)
        {
            int offsetStart = (int)(size / 2.0);
            graphics.FillRectangle(dotBrush, new(width - offsetStart, height - offsetStart, size, size));

            if (!string.IsNullOrEmpty(name))
            {
                float nameWidth = graphics.MeasureString(name, textFont).Width;
                graphics.DrawString(name, textFont, textBrush, width - (nameWidth / 2F), height + 8);

                float subtextWidth = graphics.MeasureString(subtext, subtextFont).Width;
                graphics.DrawString(subtext, subtextFont, subTextBrush, width - (subtextWidth / 2F), height + 20);
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            MapTimer.Stop();
            Hide();
        }

        private void ButtonSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (gridSidemenu.Visibility == Visibility.Visible)
            {
                gridSidemenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridSidemenu.Visibility = Visibility.Visible;
            }
        }

        private void CheckboxRenderCurrentPath_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderCurrentPath = true;
        }

        private void CheckboxRenderCurrentPath_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderCurrentPath = false;
        }

        private void CheckboxRenderDungeonPath_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderDungeonNodes = true;
        }

        private void CheckboxRenderDungeonPath_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderDungeonNodes = false;
        }

        private void CheckboxRenderHerbs_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderHerbs = true;
        }

        private void CheckboxRenderHerbs_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderHerbs = false;
        }

        private void CheckboxRenderMe_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderMe = true;
        }

        private void CheckboxRenderMe_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderMe = false;
        }

        private void CheckboxRenderOres_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderOres = true;
        }

        private void CheckboxRenderOres_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderOres = false;
        }

        private void CheckboxRenderPlayerInfo_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderPlayerExtra = true;
        }

        private void CheckboxRenderPlayerInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderPlayerExtra = false;
        }

        private void CheckboxRenderPlayerNames_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderPlayerNames = true;
        }

        private void CheckboxRenderPlayerNames_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderPlayerNames = false;
        }

        private void CheckboxRenderPlayers_Checked(object sender, RoutedEventArgs e)
        {
            checkboxRenderPlayerNames.IsEnabled = true;
            checkboxRenderPlayerInfo.IsEnabled = true;
            AmeisenBot.Config.MapRenderPlayers = true;
        }

        private void CheckboxRenderPlayers_Unchecked(object sender, RoutedEventArgs e)
        {
            checkboxRenderPlayerNames.IsEnabled = false;
            checkboxRenderPlayerInfo.IsEnabled = false;
            AmeisenBot.Config.MapRenderPlayers = false;
        }

        private void CheckboxRenderUnitInfo_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderUnitExtra = true;
        }

        private void CheckboxRenderUnitInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderUnitExtra = false;
        }

        private void CheckboxRenderUnitNames_Checked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderUnitNames = true;
        }

        private void CheckboxRenderUnitNames_Unchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.MapRenderUnitNames = false;
        }

        private void CheckboxRenderUnits_Checked(object sender, RoutedEventArgs e)
        {
            checkboxRenderUnitNames.IsEnabled = true;
            checkboxRenderUnitInfo.IsEnabled = true;
            AmeisenBot.Config.MapRenderUnits = true;
        }

        private void CheckboxRenderUnits_Unchecked(object sender, RoutedEventArgs e)
        {
            checkboxRenderUnitNames.IsEnabled = false;
            checkboxRenderUnitInfo.IsEnabled = false;
            AmeisenBot.Config.MapRenderUnits = false;
        }

        private BitmapImage GenerateMapImage(Bitmap bitmap, Graphics graphics, int width, int height)
        {
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            Graphics.Clear(Color.Transparent);

            if (AmeisenBot.WowInterface.Player != null)
            {
                Vector3 playerPosition = AmeisenBot.WowInterface.Player.Position;
                float playerRotation = AmeisenBot.WowInterface.Player.Rotation;

                // Render current dungeon nodes
                // ---------------------------- >

                if (AmeisenBot.Config.MapRenderDungeonNodes && AmeisenBot.WowInterface.DungeonEngine.Nodes?.Count > 0)
                {
                    RenderDungeonNodes(halfWidth, halfHeight, graphics, Scale, playerPosition, playerRotation);
                }

                // Render current movement path
                // ---------------------------- >

                if (AmeisenBot.Config.MapRenderCurrentPath && AmeisenBot.WowInterface.MovementEngine.Path.Any())
                {
                    RenderCurrentPath(halfWidth, halfHeight, graphics, Scale, playerPosition, playerRotation);
                }

                // Render blacklisted nodes
                // ------------------------ >

                // if (AmeisenBot.WowInterface.BotCache.TryGetBlacklistPosition((int)AmeisenBot.WowInterface.ObjectManager.MapId, playerPosition, 64, out List<Vector3> blacklistNodes))
                // {
                //     for (int i = 0; i < blacklistNodes.Count; ++i)
                //     {
                //         Vector3 node = blacklistNodes[i];
                //         Point nodePositionOnMap = GetRelativePosition(playerPosition, node, playerRotation, halfWidth, halfHeight, scale);
                //
                //         RenderBlacklistNode(nodePositionOnMap.X, nodePositionOnMap.Y, BlacklistNodeBrush, BlacklistNodePen, graphics, 3, 32);
                //     }
                // }

                // Render Gameobjects
                // ------------------ >

                if (AmeisenBot.Config.MapRenderOres)
                {
                    RenderOres(halfWidth, halfHeight, graphics, Scale, playerPosition, playerRotation);
                }

                if (AmeisenBot.Config.MapRenderHerbs)
                {
                    RenderHerbs(halfWidth, halfHeight, graphics, Scale, playerPosition, playerRotation);
                }

                // Render Units/Players
                // -------------------- >

                if (AmeisenBot.Config.MapRenderUnits || AmeisenBot.Config.MapRenderPlayers)
                {
                    RenderUnits(halfWidth, halfHeight, graphics, Scale, playerPosition, playerRotation);
                }

                if (AmeisenBot.Config.MapRenderMe)
                {
                    RenderUnit(halfWidth, halfHeight, AmeisenBot.WowInterface.Db.GetUnitName(AmeisenBot.WowInterface.Player, out string name) ? name : "unknown", "<Me>", MeBrush, TextBrush, TextFont, SubTextFont, SubTextBrush, graphics, 7);
                }
            }

            using MemoryStream memory = new();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            BitmapImage bitmapImageMap = new();
            bitmapImageMap.BeginInit();
            bitmapImageMap.StreamSource = memory;
            bitmapImageMap.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImageMap.EndInit();

            return bitmapImageMap;
        }

        private void MapTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref mapTimerBusy, 1, 0) == 1)
            {
                return;
            }

            try
            {
                if (NeedToUpdateMap && AmeisenBot.StateMachine.CurrentState.Key != BotState.LoadingScreen)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        int width = (int)mapCanvasBackground.ActualWidth;
                        int height = (int)mapCanvasBackground.ActualHeight;

                        mapCanvas.Source = GenerateMapImage(Bitmap, Graphics, width, height);
                    });

                    NeedToUpdateMap = false;
                }
            }
            finally
            {
                mapTimerBusy = 0;
            }
        }

        private void RenderCurrentPath(int halfWidth, int halfHeight, Graphics graphics, float scale, Vector3 playerPosition, float playerRotation)
        {
            List<Vector3> path = AmeisenBot.WowInterface.MovementEngine.Path.ToList();

            for (int i = 0; i < path.Count; ++i)
            {
                Vector3 node = path[i];
                Vector3 prevNode = i == 0 ? playerPosition : path[i - 1];

                Point nodePositionOnMap = GetRelativePosition(playerPosition, node, playerRotation, halfWidth, halfHeight, scale);
                Point prevNodePositionOnMap = GetRelativePosition(playerPosition, prevNode, playerRotation, halfWidth, halfHeight, scale);

                RenderNode(nodePositionOnMap.X, nodePositionOnMap.Y, prevNodePositionOnMap.X, prevNodePositionOnMap.Y, PathNodeBrush, PathNodePen, graphics, 3);
            }
        }

        private void RenderDungeonNodes(int halfWidth, int halfHeight, Graphics graphics, float scale, Vector3 playerPosition, float playerRotation)
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

        private void RenderHerbs(int halfWidth, int halfHeight, Graphics graphics, float scale, Vector3 playerPosition, float playerRotation)
        {
            IEnumerable<WowGameobject> herbNodes = AmeisenBot.WowInterface.Objects.WowObjects
                .ToList()
                .OfType<WowGameobject>()
                .Where(e => Enum.IsDefined(typeof(WowHerbId), e.DisplayId));

            for (int i = 0; i < herbNodes.Count(); ++i)
            {
                WowGameobject gameobject = herbNodes.ElementAt(i);
                Point positionOnMap = GetRelativePosition(playerPosition, gameobject.Position, playerRotation, halfWidth, halfHeight, scale);
                RenderGameobject(positionOnMap.X, positionOnMap.Y, ((WowHerbId)gameobject.DisplayId).ToString(), HerbBrush, TextBrush, TextFont, graphics);
            }
        }

        private void RenderOres(int halfWidth, int halfHeight, Graphics graphics, float scale, Vector3 playerPosition, float playerRotation)
        {
            List<WowGameobject> oreNodes = AmeisenBot.WowInterface.Objects.WowObjects
                .ToList()
                .OfType<WowGameobject>()
                .Where(e => Enum.IsDefined(typeof(WowOreId), e.DisplayId))
                .ToList();

            for (int i = 0; i < oreNodes.Count; ++i)
            {
                WowGameobject gameobject = oreNodes[i];
                Point positionOnMap = GetRelativePosition(playerPosition, gameobject.Position, playerRotation, halfWidth, halfHeight, scale);
                RenderGameobject(positionOnMap.X, positionOnMap.Y, ((WowOreId)gameobject.DisplayId).ToString(), OreBrush, TextBrush, TextFont, graphics);
            }
        }

        private void RenderUnits(int halfWidth, int halfHeight, Graphics graphics, float scale, Vector3 playerPosition, float playerRotation)
        {
            List<WowUnit> wowUnits = AmeisenBot.WowInterface.Objects.WowObjects
                .OfType<WowUnit>()
                .ToList();

            for (int i = 0; i < wowUnits.Count; ++i)
            {
                WowUnit unit = wowUnits[i];

                Brush selectedBrush = unit.IsDead ? DeadBrush : AmeisenBot.WowInterface.Db.GetReaction(AmeisenBot.WowInterface.Player, unit) switch
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
                    if (AmeisenBot.Config.MapRenderPlayers)
                    {
                        string playerName = AmeisenBot.Config.MapRenderPlayerNames && AmeisenBot.WowInterface.Db.GetUnitName(AmeisenBot.WowInterface.Target, out string name) ? name : string.Empty;
                        string playerExtra = AmeisenBot.Config.MapRenderPlayerExtra ? $"<{unit.Level} {unit.Race} {unit.Class}>" : string.Empty;

                        RenderUnit(positionOnMap.X, positionOnMap.Y, playerName, playerExtra, selectedBrush, WowColorsDrawing.GetClassPrimaryBrush(unit.Class), TextFont, SubTextFont, SubTextBrush, graphics, 7);
                    }
                }
                else
                {
                    if (AmeisenBot.Config.MapRenderUnits)
                    {
                        string unitName = AmeisenBot.Config.MapRenderUnitNames && AmeisenBot.WowInterface.Db.GetUnitName(AmeisenBot.WowInterface.Target, out string name) ? name : string.Empty;
                        string unitExtra = AmeisenBot.Config.MapRenderPlayerExtra ? $"<{unit.Level}>" : string.Empty;

                        RenderUnit(positionOnMap.X, positionOnMap.Y, unitName, unitExtra, selectedBrush, TextBrush, TextFont, SubTextFont, SubTextBrush, graphics);
                    }
                }
            }
        }

        private void SetupGraphics()
        {
            int width = (int)mapCanvasBackground.ActualWidth;
            int height = (int)mapCanvasBackground.ActualHeight;

            Bitmap = new(width, height);
            Graphics = Graphics.FromImage(Bitmap);
        }

        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Scale = (float)sliderZoom.Value;
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
            SetupGraphics();

            MapTimer.Start();
            gridSidemenu.Visibility = Visibility.Collapsed;

            checkboxRenderCurrentPath.IsChecked = AmeisenBot.Config.MapRenderCurrentPath;
            checkboxRenderDungeonPath.IsChecked = AmeisenBot.Config.MapRenderDungeonNodes;
            checkboxRenderHerbs.IsChecked = AmeisenBot.Config.MapRenderHerbs;
            checkboxRenderMe.IsChecked = AmeisenBot.Config.MapRenderMe;
            checkboxRenderOres.IsChecked = AmeisenBot.Config.MapRenderOres;
            checkboxRenderPlayerInfo.IsChecked = AmeisenBot.Config.MapRenderPlayerExtra;
            checkboxRenderPlayerNames.IsChecked = AmeisenBot.Config.MapRenderPlayerNames;
            checkboxRenderPlayers.IsChecked = AmeisenBot.Config.MapRenderPlayers;
            checkboxRenderUnitInfo.IsChecked = AmeisenBot.Config.MapRenderUnitExtra;
            checkboxRenderUnitNames.IsChecked = AmeisenBot.Config.MapRenderUnitNames;
            checkboxRenderUnits.IsChecked = AmeisenBot.Config.MapRenderUnits;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetupGraphics();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MapTimer.Stop();
        }
    }
}