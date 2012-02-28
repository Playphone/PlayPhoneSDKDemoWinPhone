//
//  MNDebug.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System.Diagnostics;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNDebug
   {
    public static void todo (string message)
     {
      todoLogger.Log(message);
     }

    public static void NotImpl (string feature)
     {
      todoLogger.Log(feature + " is not implemented");
     }

    public static void debug (string message)
     {
      Debug.WriteLine("DEBUG: " + message);
     }

    public static void warning (string message)
     {
      Debug.WriteLine("WARNING: " + message);
     }

    public static void error (string message)
     {
      Debug.WriteLine("ERROR: " + message);
     }

    private static MNTodoLogger todoLogger = new MNTodoLogger();
   }

  internal class MNTodoLogger
   {
    public void Log (string toDoMessage)
     {
      if (!loggedEvents.ContainsKey(toDoMessage))
       {
        Debug.WriteLine("TODO: " + toDoMessage);

        loggedEvents[toDoMessage] = true;
       }
     }

    private Dictionary<string,bool> loggedEvents = new Dictionary<string,bool>();
    private object                  thisLock     = new object();
   }
 }
