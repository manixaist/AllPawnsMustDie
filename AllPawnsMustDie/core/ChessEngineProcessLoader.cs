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
    /// The ChessEngineProcessLoader object is responsible for the actual loading
    /// of the chess engine process.  This decouples knowledge of the Process
    /// from the UCIChessEngine object, and distills it down to the basics needed;
    /// e.g the data event and the input stream
    /// </summary>
    public sealed class ChessEngineProcessLoader : IChessEngineProcessLoader
    {
        /// <summary>
        /// Starts the process passed in at the given path
        /// </summary>
        /// <param name="fullPathToExe">full path including the exe</param>
        /// <param name="dataReceivedHandler">handler to get callbacks on data from the interface</param>
        /// <returns>IChessEngineProcess wrapping the process, caller owns it now</returns>
        IChessEngineProcess IChessEngineProcessLoader.Start(string fullPathToExe, DataReceivedEventHandler dataReceivedHandler)
        {
            // Set process and startup variables and launch process
            Process rawProcess = new Process();
            rawProcess.EnableRaisingEvents = true;
            rawProcess.StartInfo.CreateNoWindow = true;
            rawProcess.StartInfo.RedirectStandardOutput = true;
            rawProcess.StartInfo.RedirectStandardInput = true;
            rawProcess.StartInfo.RedirectStandardError = true;
            rawProcess.StartInfo.UseShellExecute = false;
            rawProcess.StartInfo.FileName = fullPathToExe;

            // Subscribe to data arriving on the output stream
            rawProcess.OutputDataReceived += dataReceivedHandler;

            // Create the wrapper object
            IChessEngineProcess engineProcess = new ChessEngineProcessContainer(rawProcess);
            engineProcess.OnDataReceived += ((ChessEngineProcessContainer)engineProcess).OnProcessDataReceived;

            // Start the process up
            if (!rawProcess.Start())
            {
                // Bad path? Invalid exe file? For now just throw
                throw new ArgumentException();
            }

            // Start async read of that output stream.
            rawProcess.BeginOutputReadLine();
            return engineProcess;
        }
    }
}
