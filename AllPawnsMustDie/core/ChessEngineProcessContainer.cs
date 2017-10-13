using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Wrapper for the raw process executable
    /// </summary>
    public sealed class ChessEngineProcessContainer : IChessEngineProcess, IDisposable
    {
        /// <summary>
        /// Create a new container, given the raw process (loaded by IChessEngineProcessLoader
        /// implemenation somewhere)
        /// </summary>
        /// <param name="rawProcess">Process object that is a chess engine</param>
        public ChessEngineProcessContainer(Process rawProcess)
        {
            if (rawProcess == null)
            {
                throw new ArgumentNullException("rawProcess");
            }

            // It's possible this isn't started yet, but cache the object for use later
            engineRawProcess = rawProcess;
        }

        /// <summary>
        /// Invoked when the process has data to read
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;
        
        /// <summary>
        /// When we have both a valid process and a valid listener, forward the
        /// data
        /// </summary>
        /// <param name="sender">passed along</param>
        /// <param name="e">passed along</param>
        public void OnProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Forward the events from the process to our listeners (if any)
            if (OnDataReceived != null)
            {
                OnDataReceived(sender, e);
            }
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                // Dispose of the process
                engineRawProcess.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Returns the input stream for the process
        /// </summary>
        StreamWriter IChessEngineProcess.InputStream
        {
            // Set on creation, so the raw process should never be null
            // The stream always could be, but shouldn't under normal conditions
            get { return engineRawProcess.StandardInput; }
        }

        /// <summary>True if Disposed already</summary>
        public bool Disposed { get { return disposed; } }

        private bool disposed = false;
        private Process engineRawProcess = null;
    }
}
