using System.Diagnostics;
using System.Threading;
using Spect.Net.SpectrumEmu.Abstraction.Providers;

namespace Spect.Net.SpectrumEmu.Providers
{
    /// <summary>
    /// This class implements a clock provider that allows access to the 
    /// high resoultion system clock.
    /// </summary>
    public class ClockProvider : VmComponentProviderBase, IClockProvider
    {
        /// <summary>
        /// Initializes the provider
        /// </summary>
        public ClockProvider()
        {
        }

        /// <summary>
        /// The component provider should be able to reset itself
        /// </summary>
        public override void Reset()
        {
        }

        /// <summary>
        /// Retrieves the frequency of the clock. This value shows new
        /// number of clock ticks per second.
        /// </summary>
        public long GetFrequency() => Stopwatch.Frequency;

        /// <summary>
        /// Retrieves the current counter value of the clock.
        /// </summary>
        public long GetCounter() => Stopwatch.GetTimestamp();

        /// <summary>
        /// Waits until the specified counter value is reached
        /// </summary>
        /// <param name="counterValue">Counter value to reach</param>
        /// <param name="token">Token that can cancel the wait cycle</param>
        public void WaitUntil(long counterValue, CancellationToken token)
        {
            // --- Calculate the number of milliseconds to wait
            var frequency = GetFrequency();
            var millisec = frequency / 1000;

            // --- Wait until we have up to 4 milliseconds left
            while (!token.IsCancellationRequested)
            {
                var current = GetCounter();
                if (current >= counterValue) return;
                
                var millisecs = (counterValue - current) / millisec;
                if (millisecs < 4) break;
                Thread.Sleep(2);
            }

            // --- Use SpinWait
            while (!token.IsCancellationRequested)
            {
                if (counterValue <= GetCounter()) break;
                Thread.SpinWait(1);
            }
        }
    }
}