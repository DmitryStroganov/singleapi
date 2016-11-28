using System.Collections.Generic;

namespace SingleApi.Common
{
    internal class TaskQueue<T>
    {
        private readonly ServiceHostControllerEnpointParameters enpointParameters;
        private readonly Stack<T> taskStack;

        public TaskQueue(ServiceHostControllerEnpointParameters enpointParameters)
        {
            taskStack = new Stack<T>();
            this.enpointParameters = enpointParameters;
        }

        public void Enqueue(T t)
        {
            taskStack.Push(t);
        }

        public IEnumerable<T> Dequeue()
        {
            for (var i = 0; i < enpointParameters.MaxThreads; i++)
            {
                yield return taskStack.Pop();
            }
        }
    }
}