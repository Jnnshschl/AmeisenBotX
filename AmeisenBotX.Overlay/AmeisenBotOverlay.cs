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
            LinesToRender = new List<(SolidBrush, (Point, Point))>();

            OverlayWindow = new StickyWindow(xMemory.Process.MainWindowHandle)
            {
                IsTopmost = true,
                IsVisible = true,
                FPS = 30
            };

            OverlayWindow.Create();

            Gfx = new Graphics(OverlayWindow.Handle, OverlayWindow.Width, OverlayWindow.Height);
            Gfx.Setup();
        }

        public Graphics Gfx { get; }

        public StickyWindow OverlayWindow { get; }

        public XMemory XMemory { get; }

        private List<(SolidBrush, (Point, Point))> LinesToRender { get; }

        public void AddLine(int x1, int y1, int x2, int y2, System.Windows.Media.Color color)
        {
            (SolidBrush, (Point, Point)) line = (Gfx.CreateSolidBrush(color.R, color.G, color.B, color.A), (new Point(x1, y1), new Point(x2, y2)));

            if (!LinesToRender.Contains(line))
            {
                LinesToRender.Add(line);
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

                foreach ((SolidBrush, (Point, Point)) line in LinesToRender)
                {
                    Gfx.DrawLine(line.Item1, new Line(line.Item2.Item1, line.Item2.Item2), 2f);
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
    }
}