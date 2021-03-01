using AmeisenBotX.Memory;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using System.Collections.Generic;

namespace AmeisenBotX.Overlay
{
    public class AmeisenBotOverlay
    {
        public AmeisenBotOverlay(XMemory xMemory)
        {
            XMemory = xMemory;
            LinesToRender = new();
            RectanglesToRender = new();

            OverlayWindow = new(xMemory.Process.MainWindowHandle)
            {
                IsTopmost = true,
                IsVisible = true,
                FPS = 30
            };

            OverlayWindow.Create();

            Gfx = new(OverlayWindow.Handle, OverlayWindow.Width, OverlayWindow.Height);
            Gfx.Setup();
        }

        public Graphics Gfx { get; }

        public StickyWindow OverlayWindow { get; }

        public XMemory XMemory { get; }

        private List<(SolidBrush, (Point, Point))> LinesToRender { get; }

        private List<(SolidBrush, (Point, Point))> RectanglesToRender { get; }

        public void AddLine(int x1, int y1, int x2, int y2, System.Drawing.Color color)
        {
            (SolidBrush, (Point, Point)) rectangle = (Gfx.CreateSolidBrush(color.R, color.G, color.B, color.A), (new Point(x1, y1), new Point(x2, y2)));

            if (!LinesToRender.Contains(rectangle))
            {
                LinesToRender.Add(rectangle);
            }
        }

        public void AddRectangle(int x, int y, int w, int h, System.Drawing.Color color)
        {
            (SolidBrush, (Point, Point)) line = (Gfx.CreateSolidBrush(color.R, color.G, color.B, color.A), (new Point(x, y), new Point(x + w, y + h)));

            if (!RectanglesToRender.Contains(line))
            {
                RectanglesToRender.Add(line);
            }
        }

        public void Clear()
        {
            if (LinesToRender.Count > 0)
            {
                LinesToRender.Clear();
            }

            Draw();
        }

        public void Draw()
        {
            Gfx.Resize(OverlayWindow.Width, OverlayWindow.Height);

            if (Gfx.IsInitialized)
            {
                Gfx.BeginScene();
                Gfx.ClearScene();

                for (int i = 0; i < LinesToRender.Count; ++i)
                {
                    Gfx.DrawLine(LinesToRender[i].Item1, new(LinesToRender[i].Item2.Item1, LinesToRender[i].Item2.Item2), 2f);
                }

                for (int i = 0; i < RectanglesToRender.Count; ++i)
                {
                    Gfx.FillRectangle(RectanglesToRender[i].Item1, RectanglesToRender[i].Item2.Item1.X, RectanglesToRender[i].Item2.Item1.Y, RectanglesToRender[i].Item2.Item2.X, RectanglesToRender[i].Item2.Item2.Y);
                }

                LinesToRender.Clear();
                RectanglesToRender.Clear();

                Gfx.EndScene();
            }
        }

        public void Exit()
        {
            Gfx.Dispose();
            OverlayWindow.Dispose();
            OverlayWindow.Join();
        }
    }
}