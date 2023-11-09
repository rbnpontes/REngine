using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
    public interface IExecutionPipelineVar
    {
        object? Value { get; set; }
    }
    public interface IExecutionPipeline
    {
        public IExecutionPipeline Load(Stream stream);
        public IExecutionPipeline Execute();
        /// <summary>
        /// This method clears all registered events
        /// Must be called at main thread.
        /// </summary>
        /// <returns></returns>
        public IExecutionPipeline ClearAllEvents();

        public IExecutionPipeline AddEvent(string eventName, Action<IExecutionPipeline> callback);
        public IExecutionPipeline AddEvent(ulong eventHashCode, Action<IExecutionPipeline> callback);

        public IExecutionPipeline RemoveEvent(string eventName, Action<IExecutionPipeline> callback);
        public IExecutionPipeline RemoveEvent(ulong eventHashCode, Action<IExecutionPipeline> callback);
        public IExecutionPipeline ClearEvents(string eventName);
        public IExecutionPipeline ClearEvents(ulong eventHashCode);

        /// <summary>
        /// Execute call into Begining of Execute call
        /// In other words, this invoke will be scheduled to
        /// Thread that is called Execute, in our case
        /// Main Thread.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IExecutionPipeline Invoke(Action action);

        public IExecutionPipelineVar GetOrCreateVar(string name);
        public IExecutionPipelineVar GetOrCreateVar(ulong varHashCode);
    }
}
