using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllPawnsMustDie;

namespace AllPawnsMustDieUnitTests
{
    /// <summary>
    /// Mock implementation of IChessEngineProcessLoader to get our mock
    /// IChessEngineProcess injected
    /// </summary>
    internal class MockEngineProcessLoader : IChessEngineProcessLoader
    {
        /// <summary>
        /// Create a new mock process and cache it
        /// </summary>
        public MockEngineProcessLoader()
        {
            mockProcess = new MockEngineProcess();
        }

        /// <summary>
        /// Load the mock process
        /// </summary>
        /// <param name="fullPathToExe">ignored</param>
        /// <param name="dataReceivedHandler">handler to invoke when process has data</param>
        /// <returns>MockChessEngineProcess</returns>
        IChessEngineProcess IChessEngineProcessLoader.Start(
            string fullPathToExe, DataReceivedEventHandler dataReceivedHandler)
        {
            // Subscribe to data arriving on the output stream
            mockProcess.OutputDataReceived += dataReceivedHandler;
            return mockProcess;
        }

        /// <summary>
        /// Mock IChessEngineProcess implementation
        /// </summary>
        public MockEngineProcess mockProcess = null;
    }
}
