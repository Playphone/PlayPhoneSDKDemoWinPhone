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
using Microsoft.Phone.Controls;
using System.ComponentModel;

namespace PlayPhone.MultiNet
 {
  public static class MNDirectUIHelper
   {
    public enum Style    { FULLSCREEN, POPUP };
    public enum PageMode { SILVERLIGHT, XNA };

    public delegate void OnShowDashboardEventHandler  ();
    public delegate void OnHideDashboardEventHandler  ();

    public static event OnShowDashboardEventHandler  OnShowDashboard;
    public static event OnHideDashboardEventHandler  OnHideDashboard;

    public static void SetDashboardStyle (Style style)
     {
      LazyInit(style);
     }

    public static Style GetDashboardStyle ()
     {
      LazyInit();

      return dashboardView.DashboardStyle;
     }

    public static void ShowDashboard ()
     {
      LazyInit();

      if (!isVisible)
       {
        OpenDashboard();

        isVisible = true;
       }

      OnShowDashboardEventHandler handler = OnShowDashboard;

      if (handler != null)
       {
        handler();
       }
     }

    public static void HideDashboard ()
     {
      LazyInit();

      if (isVisible)
       {
        CloseDashboard();

        isVisible = false;
       }

      OnHideDashboardEventHandler handler = OnHideDashboard;

      if (handler != null)
       {
        handler();
       }
     }

    private static void OpenDashboard ()
     {
      if (pageMode == PageMode.SILVERLIGHT)
       {
        popup        = new Popup();
        popup.Child  = dashboardView.RootView;

        UpdateDashboardSize();

        popup.IsOpen = true;
       }
      else // (pageMode == PageMode.XNA)
       {
        if (parent != null)
         {
          parent.Children.Add(dashboardView.RootView);

          UpdateDashboardSize();
         }
       }

      MNDialogStack.AppendDialog(dismissibleDialog);
     }

    private static void CloseDashboard ()
     {
      MNDialogStack.RemoveDialog(dismissibleDialog);

      if (pageMode == PageMode.SILVERLIGHT)
       {
        if (popup != null)
         {
          popup.IsOpen = false;
          popup.Child  = null;
          popup        = null;
         }
       }
      else // (pageMode == PageMode.XNA)
       {
        if (parent != null)
         {
          if (!parent.Children.Remove(dashboardView.RootView))
           {
            MNDebug.warning("unable to remove dashboard view from layout root");
           }
         }
       }
     }

    public static void NavigatedTo (PhoneApplicationPage page,
                                    PageMode             pageMode   = PageMode.SILVERLIGHT,
                                    Panel                layoutRoot = null)
     {
      LazyInit();

      if (MNDirectUIHelper.page != null)
       {
        MNDebug.warning("NavigatedTo called without preceeding NavigatedFrom");

        NavigatingFrom(MNDirectUIHelper.page);
       }

      MNDirectUIHelper.page     = page;
      MNDirectUIHelper.pageMode = pageMode;
      MNDirectUIHelper.parent   = layoutRoot;

      DispatchNavigationEvent(NavigationEventArgs.EventType.NavigatedTo,page,pageMode,layoutRoot);

      if (isVisible)
       {
        OpenDashboard();
       }
     }

    public static void NavigatingFrom (PhoneApplicationPage page)
     {
      LazyInit();

      if (isVisible)
       {
        CloseDashboard();
       }

      DispatchNavigationEvent(NavigationEventArgs.EventType.NavigatingFrom,page,pageMode,parent);

      MNDirectUIHelper.page     = null;
      MNDirectUIHelper.pageMode = PageMode.SILVERLIGHT;
      MNDirectUIHelper.parent   = null;
     }

    private static void DispatchNavigationEvent (NavigationEventArgs.EventType type, PhoneApplicationPage page, PageMode mode, Panel parent)
     {
      OnPageNavigationEventHandler handler = OnPageNavigation;

      if (handler != null)
       {
        handler(new NavigationEventArgs(type,page,mode,parent));
       }
     }

    private static void LazyInit (Style dashboardStyle = Style.POPUP)
     {
      if (dashboardView == null)
       {
        dashboardView = new DashboardView(dashboardStyle);

        MNDirect.GetView().DoGoBack += OnDoGoBack;
       }
     }

    private static void OnDoGoBack ()
     {
      HideDashboard();
     }

    private static void UpdateDashboardSize ()
     {
      dashboardView.UpdateSize();

      if (dashboardView.DashboardStyle == Style.FULLSCREEN)
       {
        popup.VerticalOffset = MNPlatformWinPhone.GetSystemTrayHeight();
       }
     }

    private class DashboardView
     {
      public DashboardView (Style dashboardStyle = Style.POPUP)
       {
        border = null;

        SetDashboardStyle(dashboardStyle);
       }

      public Style DashboardStyle
       {
        get
         {
          return border == null ? Style.FULLSCREEN : Style.POPUP;
         }

        set
         {
          SetDashboardStyle(value);
         }
       }

      public UIElement RootView
       {
        get
         {
          return (UIElement)border ?? (UIElement)MNDirect.GetView();
         }
       }

      public void UpdateSize ()
       {
        if (DashboardStyle == Style.POPUP)
         {
          Size   rootSize      = Application.Current.RootVisual.RenderSize;
          double sysTrayHeight = MNPlatformWinPhone.GetSystemTrayHeight();

          border.Height = rootSize.Height - sysTrayHeight - 2 * PopupMargin;
          border.Width  = rootSize.Width  - 2 * PopupMargin;

          Thickness margin = new Thickness(PopupMargin);

          margin.Top += sysTrayHeight;

          border.Margin = margin;
         }
       }

      private void SetDashboardStyle (Style style)
       {
        if (DashboardStyle == style)
         {
          return;
         }

        if (style == Style.FULLSCREEN)
         {
          border.Child = null;
          border       = null;
         }
        else
         {
          CreateBorder();

          border.Child = MNDirect.GetView();
         }
       }

      private void CreateBorder ()
       {
        border = new Border();

        border.BorderBrush     = new SolidColorBrush(Color.FromArgb(0x80,0x33,0x33,0x33));
        border.BorderThickness = new Thickness(PopupBorderWidth);
        border.CornerRadius    = new CornerRadius(PopupCornerRadius);
       }

      private Border border;

      private const int PopupBorderWidth  = 5;
      private const int PopupCornerRadius = 5;
      private const int PopupMargin       = 5;
     }

    internal class NavigationEventArgs
     {
      public enum EventType { NavigatedTo, NavigatingFrom };

      public EventType            Type   { get; private set; }
      public PhoneApplicationPage Page   { get; private set; }
      public PageMode             Mode   { get; private set; }
      public Panel                Parent { get; private set; }

      public NavigationEventArgs (EventType type, PhoneApplicationPage page, PageMode pageMode, Panel parent)
       {
        Type   = type;
        Page   = page;
        Mode   = pageMode;
        Parent = parent;
       }
     }

    private class DismissibleDashboardDialog : MNDialogStack.IDismissible
     {
      public void Dismiss ()
       {
        HideDashboard();
       }
     }

    internal delegate void OnPageNavigationEventHandler (NavigationEventArgs args);
    internal static  event OnPageNavigationEventHandler OnPageNavigation;

    private static Popup                popup         = null;
    private static DashboardView        dashboardView = null;
    private static bool                 isVisible     = false;
    private static DismissibleDashboardDialog dismissibleDialog = new DismissibleDashboardDialog();
    internal static PhoneApplicationPage page         = null;
    internal static PageMode             pageMode     = PageMode.SILVERLIGHT;
    internal static Panel                parent       = null;
   }
 }
