using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CS.Util
{
    public static class DisposableTimer
    {
        public static IDisposable Start(TimeSpan interval, Action action)
        {
            var proxy = new SynchronizationContextProxy();
            var timer = new Timer();
            timer.SynchronizingObject = proxy;
            timer.AutoReset = true;
            timer.Interval = interval.TotalMilliseconds;
            timer.Elapsed += (sender, args) => action();
            timer.Start();

            return new timerDispoer(timer);
        }

        private class timerDispoer : IDisposable
        {
            private Timer _timer;

            public timerDispoer(Timer timer)
            {
                _timer = timer;
            }

            public void Dispose()
            {
                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
            }
        }
    }
}
