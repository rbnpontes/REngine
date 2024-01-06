using System.Collections.Concurrent;
using System.Text;
using REngine.Core.Mathematics;

namespace REngine.Core.Threading;

public static class Monitor
{
#if RENGINE_DEBUGLOCKS
     private static ConcurrentDictionary<ulong, Stack<string>> pLocks = new();
#endif
     
     public static void Enter(object syncObj)
     {
          if(System.Threading.Monitor.IsEntered(syncObj))
               return;
          System.Threading.Monitor.Enter(syncObj);
#if RENGINE_DEBUGLOCKS
          var threadName = Thread.CurrentThread.Name;
          if (threadName is null)
               return;
          var threadId = Hash.Digest(threadName);
          var stackTrace = string.Intern(Environment.StackTrace);
          if (pLocks.TryGetValue(threadId, out var stack))
          {
               stack.Push(stackTrace);
               return;
          }

          stack = new Stack<string>();
          stack.Push(stackTrace);

          pLocks.TryAdd(threadId, stack);
#endif
     }

     public static void Exit(object syncObj)
     {
          if(!System.Threading.Monitor.IsEntered(syncObj))
               return;
          System.Threading.Monitor.Exit(syncObj);
#if RENGINE_DEBUGLOCKS
          var threadName = Thread.CurrentThread.Name;
          if (threadName is null)
               return;
          var threadId = Hash.Digest(threadName);
          if (!pLocks.TryGetValue(threadId, out var stack))
               throw new Exception(
                    "Something is Wrong. You're exit mutex that has not been entered. Did you call Enter ?");
          stack.Pop();
#endif
     }

     public static int GetLockCount()
     {
#if RENGINE_DEBUGLOCKS
          var threadName = Thread.CurrentThread.Name;
          if (threadName is null)
               return 0;
          var threadId = Hash.Digest(threadName);
          pLocks.TryGetValue(threadId, out var stack);
          return stack?.Count ?? 0;
#else
          return 0;
#endif
     } 

     public static void Dump(StringBuilder output)
     {
#if RENGINE_DEBUGLOCKS
          var data = pLocks.ToArray();
          foreach (var pair in data)
          {
               var entries = pair.Value.ToArray();
               output.Append($"{pair.Key}: [");
               if (entries.Length == 0)
               {
                    output.AppendLine("No Locks ]");
                    continue;
               }
               
               foreach (var entry in entries)
               {
                    output.Append(entry);
                    output.AppendLine();
               }

               output.AppendLine("]");
          }
#endif
     }
}