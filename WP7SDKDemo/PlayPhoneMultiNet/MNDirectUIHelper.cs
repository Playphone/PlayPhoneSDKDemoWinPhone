//
//  MNDirectUIHelper.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet
 {
  public static class MNDirectUIHelper
   {
    public enum Style { FULLSCREEN, POPUP };

    public delegate void OnShowDashboardEventHandler ();
    public delegate void OnHideDashboardEventHandler ();

    public static event OnShowDashboardEventHandler OnShowDashboard;
    public static event OnHideDashboardEventHandler OnHideDashboard;

    public static void SetDashboardStyle (Style style)
     {
      dashboardStyle = style;
     }

    public static Style GetDashboardStyle ()
     {
      return dashboardStyle;
     }

    public static void ShowDashboard ()
     {
      LazyInit();

      UpdateDashboardSize();

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
        CreatePopup();

        MNDirect.GetView().DoGoBack += OnDoGoBack;
       }
     }

    private static void OnDoGoBack ()
     {
      HideDashboard();
     }

    private static void CreatePopup ()
     {
      if (dashboardStyle == Style.POPUP)
       {
        CreateDashboardPopup();
       }
      else
       {
        CreateDashboardFullScreen();
       }
     }

    private static void CreateDashboardPopup ()
     {
      popup = new Popup();

      Border border = new Border();

      border.BorderBrush     = new SolidColorBrush(Color.FromArgb(0x80,0x33,0x33,0x33));
      border.BorderThickness = new Thickness(PopupBorderWidth);
      border.CornerRadius    = new CornerRadius(PopupCornerRadius);

      border.Child = MNDirect.GetView();
      popup.Child  = border;
     }

    private static void UpdateDashboardSize ()
     {
      if (dashboardStyle == Style.POPUP)
       {
        Border border = (Border)popup.Child;

        Size   rootSize      = Application.Current.RootVisual.RenderSize;
        double sysTrayHeight = MNPlatformWinPhone.GetSystemTrayHeight();

        border.Height = rootSize.Height - sysTrayHeight - 2 * PopupMargin;
        border.Width  = rootSize.Width  - 2 * PopupMargin;

        Thickness margin = new Thickness(PopupMargin);

        margin.Top += sysTrayHeight;

        border.Margin = margin;
       }
      else if (dashboardStyle == Style.FULLSCREEN)
       {
        popup.VerticalOffset = MNPlatformWinPhone.GetSystemTrayHeight();
       }
     }

    private static void CreateDashboardFullScreen ()
     {
      popup = new Popup();

      popup.Child = MNDirect.GetView();
     }

    private static Popup popup = null;
    private static Style dashboardStyle = Style.POPUP;

    private const int PopupBorderWidth  = 5;
    private const int PopupCornerRadius = 5;
    private const int PopupMargin       = 5;
   }
 }
