using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS.Util
{
    /// <summary>
    /// Create a new instance of this class within a using() block, and set a new Synchronization Context.
    /// Most useful in conjunction with async calls to prevent them from looking to re-enter the calling context
    /// which can cause deadlocks.
    /// </summary>
    public class SynchronizationContextChange : IDisposable
    {
        private SynchronizationContext _previous;
        /// <summary>
        /// Creates a new instance of SynchronizationContextChange
        /// </summary>
        /// <param name="newContext">The new SynchronizationContext, or null.</param>
        public SynchronizationContextChange(SynchronizationContext newContext = null)
        {
            _previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(newContext);
        }

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(_previous);
        }
    }
}
