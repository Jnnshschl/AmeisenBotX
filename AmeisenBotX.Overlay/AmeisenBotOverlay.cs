using AmeisenBotX.Memory;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Overlay
{
    public class AmeisenBotOverlay
    {
        public AmeisenBotOverlay(XMemory xMemory)
        {
            XMemory = xMemory;
            LinesToRender = new List<(Point, Point)>();

            OverlayWindow = new StickyWindow(xMemory.Process.MainWindowHandle)
            {
                IsTopmost = true,
                IsVisible = true,
                FPS = 30
            };

            OverlayWindow.Create();

            Gfx = new Graphics(OverlayWindow.Handle, OverlayWindow.Width, OverlayWindow.Height);
            Gfx.Setup();

            DefaultLineBrush = Gfx.CreateSolidBrush(0, 255, 255);
        }

        public Graphics Gfx { get; }

        public StickyWindow OverlayWindow { get; }

        public XMemory XMemory { get; }

        private IBrush DefaultLineBrush { get; }

        private List<(Point, Point)> LinesToRender { get; }

        public void AddLine(int x1, int y1, int x2, int y2)
        {
            (Point, Point) line = (new Point(x1, y1), new Point(x2, y2));

            if (!LinesToRender.Contains(line))
            {
                LinesToRender.Add(line);
            }
        }

        public void Draw()
        {
            Gfx.Resize(OverlayWindow.Width, OverlayWindow.Height);
            if (Gfx.IsInitialized)
            {
                Gfx.BeginScene();
                Gfx.ClearScene();

                foreach ((Point, Point) line in LinesToRender)
                {
                    Gfx.DrawLine(DefaultLineBrush, new Line(line.Item1, line.Item2), 2f);
                }

                LinesToRender.Clear();

                Gfx.EndScene();
            }
        }

        public void Exit()
        {
            OverlayWindow.Dispose();
            Gfx.Dispose();
        }

        public void Clear()
        {
            if (LinesToRender.Count > 0)
            {
                LinesToRender.Clear();
            }

            Draw();
        }
    }
}