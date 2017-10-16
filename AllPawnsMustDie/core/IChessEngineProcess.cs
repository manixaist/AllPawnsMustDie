using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AllPawnsMustDie
{
    /// <summary>
    /// IChessEngineProcess is the minimal set of functionality to wrap the
    /// binary process for the engine.
    /// </summary>
    public interface IChessEngineProcess
    {
        /// <summary>
        /// Invoked when the process has data to read
        /// </summary>
        event EventHandler<DataReceivedEventArgs> OnDataReceived;

        /// <summary>
        /// Returns the input stream for the process
        /// </summary>
        StreamWriter InputStream { get; }
    }

    /// <summary>
    /// Interface to abstract loading of the actual process
    /// </summary>
    public interface IChessEngineProcessLoader
    {
        /// <summary>
        /// Starts the process passed in
        /// </summary>
        /// <param name="fullPathToExe">full path including the exe</param>
        /// <param name="dataReceivedHandler">handler to get callbacks on data</param>
        IChessEngineProcess Start(string fullPathToExe, DataReceivedEventHandler dataReceivedHandler);
    }
}
