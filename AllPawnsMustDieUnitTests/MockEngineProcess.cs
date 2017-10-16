using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using AllPawnsMustDie;

namespace AllPawnsMustDieUnitTests
{
    /// <summary>
    /// The mock stream mostly goes with the mock process, but can also be used
    /// directly if needed.  The mock stream will intercept writes and trigger
    /// events for the attached 'process'.  This is why it goes closely with
    /// the mock process class.
    /// </summary>
    internal class MockStream : MemoryStream
    {
        /// <summary>
        /// If we have a mock process, inject it here
        /// </summary>
        /// <param name="process">mock process IChessEngineProcess implementation</param>
        public MockStream(MockEngineProcess process = null)
        {
            mockProcess = process;
        }

        /// <summary>
        /// Flush the stream
        /// </summary>
        public override void Flush()
        {
            base.Flush();
        }

        /// <summary>
        /// Intercept the write and respond as needed if there is a process attached,
        /// also clearing the stream (we're responding as the process).  If not, 
        /// then just write the data to the underlying stream and wait for someone
        /// else to read it back for verification.
        /// </summary>
        /// <param name="buffer">byte array to write</param>
        /// <param name="offset">offest to stream (ignored/passed)</param>
        /// <param name="count">count of bytes in the buffer (ignored/passed)</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Convert the byte array into a string for comparisons
            string input = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            string data = String.Empty;

            // Check against known commands that demand a response from this layer
            if (String.Compare(input, 0, UCIChessEngine.Uci, 0, UCIChessEngine.Uci.Length) == 0)
            {
                data = UCIChessEngine.UciOk;
            }
            else if (String.Compare(input, 0, UCIChessEngine.IsReady, 0, UCIChessEngine.IsReady.Length) == 0)
            {
                data = UCIChessEngine.ReadyOk;
            }
            else if (input.StartsWith("go movetime"))
            {
                // Moves don't matter, but the string must be filled in for the engine to read
                data = String.Concat(UCIChessEngine.BestMoveResponse, " e2e4");
            }

            // If we have a response
            if (data != String.Empty)
            {
                // Voodoo with Reflection to create our own DataReceivedEventArgs
                DataReceivedEventArgs e = CreateMockDataReceivedEventArgs(data);
                // Send it to the listener, if we have one
                mockProcess?.OnDataToForward(this, e);
            }

            // If no process is hooked up, don't clear this, the test will read
            // it right back.  If there is mock process, clear this out, because
            // no one else will - mostly to make reading the debug stream easier
            // the data could overwrite since the string comparisons above are
            // indexed and lengthed.
            if (mockProcess != null)
            {
                // Commenting this out should break nothing, it's just for readability in debug
                Array.Clear(buffer, 0, buffer.Length);
            }
            base.Write(buffer, offset, count);
        }

        /// <summary>
        /// DataReceivedEventArgs is not sealed, but its constructor is private
        /// and the underlying data cannot be set.  I found this solution on 
        /// https://stackoverflow.com/a/1354557 which is probably not cool for 
        /// "production" code, but it seems too sweet not to use for testing.
        /// 
        /// I changed the code from the link really in formatting only and added
        /// comments.
        /// </summary>
        /// <param name="mockData">string we want to set DataReceivedEventArgs._data to</param>
        /// <returns>Our massaged DataReceivedEventArgs</returns>
        private DataReceivedEventArgs CreateMockDataReceivedEventArgs(string mockData)
        {
            // What this does is create a "zeroed out" version of the object type
            // given and no constructors are run - it's a complete blank.
            DataReceivedEventArgs MockEventArgs =
                (DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices
                 .GetUninitializedObject(typeof(DataReceivedEventArgs));

            // This works as is because there is only 1 field that should
            // match this query, the private '_data' field
            FieldInfo[] EventFields = typeof(DataReceivedEventArgs)
                .GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

            // This field is also the ONLY thing we care about setting, so why 
            // the underlying class has no way of overriding it (like declaring
            // it protected) and is also not declared as sealed is odd to me
            if (EventFields.Count() > 0)
            {
                // Reflection makes access modifiers meaningless...
                EventFields[0].SetValue(MockEventArgs, mockData);
            }
            else
            {
                throw new ApplicationException(
                    "Failed to find _data field!");
            }
            return MockEventArgs;
        }

        /// <summary>cached mock IChessEngineProcess implementation</summary>
        private MockEngineProcess mockProcess;
    }

    /// <summary>
    /// Mock IChessEngineProcess implementation
    /// </summary>
    internal class MockEngineProcess : IChessEngineProcess, IDisposable
    {
        /// <summary>
        /// Initialize the mock
        /// </summary>
        public MockEngineProcess()
        {
            // Create the underlying mock stream and attach a real writer to it
            mockProcessStream = new MockStream(this);
            writer = new StreamWriter(mockProcessStream);
            writer.AutoFlush = true;
        }

        /// <summary>
        /// Required for the interface, but not used in the mock
        /// </summary>
        #pragma warning disable CS0067
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;
        #pragma warning restore CS0067

        /// <summary>
        /// Invoked when the process has data to read
        /// </summary>
        public event DataReceivedEventHandler OutputDataReceived;

        /// <summary>
        /// Required for the mock consumer, dispose of our stream internally
        /// </summary>
        public void Dispose()
        {
            mockProcessStream.Dispose();
        }
        
        /// <summary>
        /// Returns the input stream for the process
        /// </summary>
        StreamWriter IChessEngineProcess.InputStream { get { return writer; } }

        /// <summary>
        /// When the attached stream has data to write back (response via
        /// StandardOutput) it sends it here.  The mock process will forward it
        /// to any listeners via the standard Process.OutputDataReceived event
        /// </summary>
        /// <param name="sender">forwarded</param>
        /// <param name="e">forwarded</param>
        public void OnDataToForward(object sender, DataReceivedEventArgs e)
        {
            // Forward the events from the process to our listeners (if any)
            if (OutputDataReceived != null)
            {
                // This would be the real UCIChessEngine object in most cases
                OutputDataReceived(sender, e);
            }
        }

        /// <summary>mock stream</summary>
        private MemoryStream mockProcessStream = null;
        /// <summary>Real writer that wraps the mock stream</summary>
        private StreamWriter writer = null;
    }
}
