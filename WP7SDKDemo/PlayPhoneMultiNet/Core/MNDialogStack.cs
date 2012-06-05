//
//  MNDialogStack.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;

using Microsoft.Phone.Controls;

namespace PlayPhone.MultiNet.Core
 {
  public static class MNDialogStack
   {
    public interface IDismissible
     {
      void Dismiss ();
     }

    public static bool HandleBackButton
     {
      get
       {
        return backButtonHandlingEnabled;
       }

      set
       {
        if (backButtonHandlingEnabled != value)
         {
          backButtonHandlingEnabled = value;

          lock (thisLock)
           {
            if (stack.Count > 0)
             {
              if (backButtonHandlingEnabled)
               {
                StartBackButtonHandling();
               }
              else
               {
                StopBackButtonHandling();
               }
             }
           }
         }
       }
     }

    public static void AppendDialog (IDismissible dialog)
     {
      lock (thisLock)
       {
        stack.Add(dialog);

        if (stack.Count == 1 && backButtonHandlingEnabled)
         {
          StartBackButtonHandling();
         }
       }
     }

    public static void RemoveDialog (IDismissible dialog)
     {
      lock (thisLock)
       {
        stack.Remove(dialog);

        if (stack.Count == 0 && backButtonHandlingEnabled)
         {
          StopBackButtonHandling();
         }
       }
     }

    private static bool DismissTop ()
     {
      bool dismissed = false;

      lock (thisLock)
       {
        if (stack.Count > 0)
         {
          stack[stack.Count - 1].Dismiss();

          dismissed = true;
         }
       }

      return dismissed;
     }

    public static bool DismissTopDialog ()
     {
      return DismissTop();
     }

    private static void OnBackKeyPress (object sender, CancelEventArgs args)
     {
      args.Cancel = DismissTop();
     }

    private static void StartBackButtonHandling ()
     {
      PhoneApplicationFrame appFrame = Application.Current.RootVisual as PhoneApplicationFrame;

      if (appFrame != null)
       {
        appFrame.BackKeyPress += OnBackKeyPress;
       }
     }

    private static void StopBackButtonHandling ()
     {
      PhoneApplicationFrame appFrame = Application.Current.RootVisual as PhoneApplicationFrame;

      if (appFrame != null)
       {
        appFrame.BackKeyPress -= OnBackKeyPress;
       }
     }

    private static List<IDismissible> stack = new List<IDismissible>();
    private static object             thisLock = new object();
    private static bool               backButtonHandlingEnabled = true;
   }
 }
