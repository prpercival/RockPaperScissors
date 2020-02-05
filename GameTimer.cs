using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace RockPaperScissors
{
    public class GameTimer
    {
        private static TimeSpan updateInterval = TimeSpan.FromSeconds(1.0 / 24.0); // FPS 24
        private Stopwatch stopwatch = Stopwatch.StartNew();

        public GameTimer()
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public event EventHandler<EventArgs> Draw;

        private void CompositionTarget_Rendering(object sender, object e)
        {
            if (Draw != null)
            {
                // Draw after interval reached, otherwise wait for the next round.
                if (this.stopwatch.Elapsed >= updateInterval)
                {
                    this.stopwatch.Reset();
                    this.stopwatch.Start();

                    Draw(this, EventArgs.Empty);
                }
            }
        }
    }
}
