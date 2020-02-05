using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockPaperScissors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public enum FramePhase
    {
        Left,
        Center,
        Right
    }

    public enum FrameDirection
    {
        Down,
        Left,
        Right,
        Up
    }

    public sealed partial class Pawn : UserControl
    {
        private GameTimer timer;
        private TaskCompletionSource<bool> source;
        private FramePhase activeFoot = FramePhase.Left;
        private FramePhase phase = FramePhase.Center;
        private FrameDirection direction = FrameDirection.Down;
        private Point destination;

        public bool IsSelected
        {
            get;
            set;
        }

        public Pawn()
        {
            this.InitializeComponent();
        }

        public Pawn(GameTimer timer)
            : this()
        {
            this.timer = timer;
        }

        public void SetCanvasLocation(double x, double y)
        {
            // Using 40x40px grid and saving every other z-index for the movers.
            var offsetY = 0;
            if (y % 40.0 > 0)
            {
                if (y > Canvas.GetTop(this))
                {
                    // When going from up to down, pawn should be behind the objects.
                    offsetY = -1;
                }
                else if (y < Canvas.GetTop(this))
                {
                    // When going from down to up, pawn should be front of the objects.
                    offsetY = 1;
                }
            }
            var zIndex = (int)Math.Round(y / 40.0) * 2 + offsetY;

            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            Canvas.SetZIndex(this, zIndex);

            // Frame phase equals directly to the sprite sheet column.
            this.TranslateTransform.X = (int)this.phase * -this.Rectangle.Width;

            // Frame direction equals directly to the sprite sheet row.
            this.TranslateTransform.Y = (int)this.direction * -this.Rectangle.Height;
        }

        public Task MoveAsync(double x, double y)
        {
            this.source = new TaskCompletionSource<bool>();
            this.destination = new Point(x, y);
            this.timer.Draw += Timer_Draw;
            return this.source.Task;
        }

        private void Timer_Draw(object sender, EventArgs e)
        {
            try
            {
                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);

                // Animate the walking frames by left-center-right-center-left-...
                var nextPhase = FramePhase.Center;
                if (this.phase == FramePhase.Center)
                {
                    // Remember the previous active foot.
                    nextPhase = (this.activeFoot == FramePhase.Left) ? FramePhase.Right : FramePhase.Left;
                    this.activeFoot = nextPhase;
                }
                this.phase = nextPhase;

                // Check the pawn facing direction.
                var offsetX = x - this.destination.X;
                var offsetY = y - this.destination.Y;
                if (offsetX < 0)
                {
                    this.direction = FrameDirection.Right;
                }
                else if (offsetX > 0)
                {
                    this.direction = FrameDirection.Left;
                }
                else
                {
                    if (offsetY < 0)
                    {
                        this.direction = FrameDirection.Down;
                    }
                    else if (offsetY > 0)
                    {
                        this.direction = FrameDirection.Up;
                    }
                }

                // Move pawn 4px on canvas until final destination is reached.
                var speed = 4.0;
                if (x > this.destination.X)
                {
                    x = Math.Max(x - speed, this.destination.X);
                }
                else if (x < this.destination.X)
                {
                    x = Math.Min(x + speed, this.destination.X);
                }
                if (y > this.destination.Y)
                {
                    y = Math.Max(y - speed, this.destination.Y);
                }
                else if (y < this.destination.Y)
                {
                    y = Math.Min(y + speed, this.destination.Y);
                }
                SetCanvasLocation(x, y);
                if (x == this.destination.X && y == this.destination.Y)
                {
                    this.timer.Draw -= Timer_Draw;
                    this.source.TrySetResult(true);
                }
            }
            catch (Exception ex)
            {
                this.source.TrySetException(ex);
            }
        }
    }
}
