//
//  MNDirectUIHelper.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows.Controls.Primitives;

namespace PlayPhone.MultiNet
 {
  public static class MNDirectUIHelper
   {
    public delegate void OnShowDashboardEventHandler ();
    public delegate void OnHideDashboardEventHandler ();

    public static event OnShowDashboardEventHandler OnShowDashboard;
    public static event OnHideDashboardEventHandler OnHideDashboard;

    public static void ShowDashboard ()
     {
      LazyInit();

      popup.IsOpen = true;

      OnShowDashboardEventHandler handler = OnShowDashboard;

      if (handler != null)
       {
        handler();
       }
     }

    public static void HideDashboard ()
     {
      LazyInit();

      popup.IsOpen = false;

      OnHideDashboardEventHandler handler = OnHideDashboard;

      if (handler != null)
       {
        handler();
       }
     }

    private static void LazyInit ()
     {
      if (popup == null)
       {
        popup = new Popup();

        popup.Child = MNDirect.GetView();

        MNDirect.GetView().DoGoBack += OnDoGoBack;
       }
     }

    private static void OnDoGoBack ()
     {
      HideDashboard();
     }

    private static Popup popup = null;
   }
 }
