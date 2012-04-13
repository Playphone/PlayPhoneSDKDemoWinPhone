//
//  MNDirectButton.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

using PlayPhone.MultiNet.Core;
using Microsoft.Phone.Controls;

namespace PlayPhone.MultiNet
 {
  public class MNDirectButton
   {
	   public const int MNDIRECTBUTTON_TOPLEFT     = HORZ_LEFT_BIT;
	   public const int MNDIRECTBUTTON_TOPRIGHT    = 0;
	   public const int MNDIRECTBUTTON_BOTTOMRIGHT = VERT_BOTTOM_BIT;
	   public const int MNDIRECTBUTTON_BOTTOMLEFT  = HORZ_LEFT_BIT | VERT_BOTTOM_BIT;
	   public const int MNDIRECTBUTTON_LEFT        = HORZ_LEFT_BIT | VERT_CENTER_BIT;
	   public const int MNDIRECTBUTTON_TOP         = HORZ_CENTER_BIT;
	   public const int MNDIRECTBUTTON_RIGHT       = VERT_CENTER_BIT;
	   public const int MNDIRECTBUTTON_BOTTOM      = HORZ_CENTER_BIT | VERT_BOTTOM_BIT;

  	 public static void InitWithLocation (int location)
     {
      MNDirectButton.location = location;

      LazyInit();
     }

    public static bool IsVisible ()
     {
      LazyInit();

      return isVisible;
     }

	   public static bool IsHidden ()
     {
      return !IsVisible();
     }

   	public static void Show()
     {
      LazyInit();

      if (!isVisible)
       {
        ShowButton();

        isVisible = true;
       }
     }

   	public static void Hide()
     {
      LazyInit();

      if (isVisible)
       {
        HideButton();

        isVisible = false;
       }
     }

    private static void ShowButton ()
     {
      if (MNDirectUIHelper.pageMode == MNDirectUIHelper.PageMode.SILVERLIGHT)
       {
        if (popup == null)
         {
          popup = new Popup();

          popup.Child = imageButton;

          popup.LayoutUpdated += OnLayoutUpdated;

          popup.IsOpen = true;
         }
       }
      else // (MNDirectUIHelper.pageMode == MNDirectUIHelper.PageMode.XNA)
       {
        if (MNDirectUIHelper.parent != null)
         {
          MNDirectUIHelper.parent.Children.Add(imageButton);

          MNDirectUIHelper.parent.LayoutUpdated += OnLayoutUpdated;
         }
       }
     }

    private static void HideButton ()
     {
      if (MNDirectUIHelper.pageMode == MNDirectUIHelper.PageMode.SILVERLIGHT)
       {
        if (popup != null)
         {
          popup.LayoutUpdated -= OnLayoutUpdated;

          popup.IsOpen = false;
          popup.Child  = null;
          popup        = null;
         }
       }
      else // (MNDirectUIHelper.pageMode == MNDirectUIHelper.PageMode.XNA)
       {
        if (MNDirectUIHelper.parent != null)
         {
          MNDirectUIHelper.parent.LayoutUpdated -= OnLayoutUpdated;

          MNDirectUIHelper.parent.Children.Remove(imageButton);
         }
       }
     }

    private static void LazyInit ()
     {
      if (imageButton == null)
       {
        isVisible = false;

        imageButton = new ImageButton();

        MNDirectUIHelper.OnShowDashboard += OnDashboardShow;
        MNDirectUIHelper.OnHideDashboard += OnDashboardHide;

        MNDirectUIHelper.OnPageNavigation += OnPageNavigation;
       }
     }

    private static void CalculateButtonPos (int location, out int x, out int y)
     {
      double buttonWidth  = imageButton.DesiredSize.Width;
      double buttonHeight = imageButton.DesiredSize.Height;

      if ((location & HORZ_CENTER_BIT) != 0)
       {
        x = (int)((Application.Current.RootVisual.RenderSize.Width - buttonWidth) / 2);
       }
      else if ((location & HORZ_LEFT_BIT) != 0)
       {
        x = 0;
       }
      else
       {
        x = (int)(Application.Current.RootVisual.RenderSize.Width - buttonWidth);
       }

      if ((location & VERT_CENTER_BIT) != 0)
       {
        y = (int)((Application.Current.RootVisual.RenderSize.Height - buttonHeight) / 2);
       }
      else if ((location & VERT_BOTTOM_BIT) != 0)
       {
        y = (int)(Application.Current.RootVisual.RenderSize.Height - buttonHeight);
       }
      else
       {
        y = (int)MNPlatformWinPhone.GetSystemTrayHeight();
       }
     }

    private static void OnLayoutUpdated (object source, EventArgs args)
     {
      if (MNDirectUIHelper.pageMode == MNDirectUIHelper.PageMode.SILVERLIGHT)
       {
        if (popup.IsOpen)
         {
          int x,y;

          CalculateButtonPos(location,out x,out y);

          if ((int)popup.HorizontalOffset != x || (int)popup.VerticalOffset != y)
           {
            popup.HorizontalOffset = x;
            popup.VerticalOffset   = y;
           }
         }
       }
      else // (MNDirectUIHelper.pageMode == MNDirectUIHelper.PageMode.XNA)
       {
        Canvas canvas = MNDirectUIHelper.parent as Canvas;

        if (canvas != null)
         {
          int x,y;

          CalculateButtonPos(location,out x,out y);

          Canvas.SetLeft(imageButton,x);
          Canvas.SetTop(imageButton,y);
         }
       }
     }

    private static void OnDashboardShow ()
     {
      autoShow = IsVisible();

      if (autoShow)
       {
        Hide();
       }
     }

    private static void OnDashboardHide ()
     {
      if (autoShow)
       {
        Show();
       }
     }

    private static void OnPageNavigation (MNDirectUIHelper.NavigationEventArgs args)
     {
      if      (args.Type == MNDirectUIHelper.NavigationEventArgs.EventType.NavigatedTo)
       {
        if (isVisible)
         {
          ShowButton();
         }
       }
      else if (args.Type == MNDirectUIHelper.NavigationEventArgs.EventType.NavigatingFrom)
       {
        if (isVisible)
         {
          HideButton();
         }
       }
     }

    private const int VERT_BOTTOM_BIT = 0x01;
    private const int VERT_CENTER_BIT = 0x02;
    private const int HORZ_LEFT_BIT   = 0x04;
    private const int HORZ_CENTER_BIT = 0x08;

    private static bool        isVisible   = false;
    private static Popup       popup       = null;
    private static ImageButton imageButton = null;
    private static bool        autoShow    = false;
    private static int         location    = MNDIRECTBUTTON_TOPRIGHT;

    #region ImageButton control

    private class ImageButton : Panel
     {
      public ImageButton ()
       {
        buttonImage      = new Image();
        buttonImage.Tap += OnButtonTap;

        UpdateButtonImage();

        Children.Add(buttonImage);

        MNDirect.GetSession().SessionStatusChanged += OnSessionStatusChanged;
        MNDirect.GetSession().UserChanged          += OnSessionUserChanged;
       }

      // desired size of the control is the desired size of button image,
      protected override Size MeasureOverride(Size availableSize)
       {
        Size resultSize;

        if (Children.Count > 0)
         {
          Children[0].Measure(availableSize);

          resultSize = Children[0].DesiredSize;
         }
        else
         {
          resultSize = new Size(0.0,0.0);
         }

        return resultSize;
       }

      protected override Size ArrangeOverride(Size finalSize)
       {
        if (Children.Count > 0)
         {
          Children[0].Arrange(new Rect(new Point(0,0),finalSize));
         }

        return finalSize;
       }

      private void UpdateButtonImage ()
       {
        StreamResourceInfo bitmapStream = GetButtonImageStream();

        if (bitmapStream != null)
         {
          BitmapImage bitmapImage = new BitmapImage();
          bitmapImage.SetSource(bitmapStream.Stream);

          buttonImage.Source = bitmapImage;
         }
        else
         {
          MNDebug.warning("MNDirectUIButton image not found");
         }
       }

      private void OnButtonTap (object source, GestureEventArgs args)
       {
        MNDirectUIHelper.ShowDashboard();
       }

      private void OnSessionStatusChanged (int oldStatus, int newStatus)
       {
        if (IsLoggedInByStatus(oldStatus) != IsLoggedInByStatus(newStatus))
         {
          UpdateButtonImage();
         }
       }

      private void OnSessionUserChanged (long userId)
       {
        UpdateButtonImage();
       }

      private static bool IsLoggedInByStatus (int status)
       {
        return status != MNConst.MN_OFFLINE && status != MNConst.MN_CONNECTING;
       }

      private static string GetImageNameStatusSuffix (int status, long userId)
       {
        if (userId == MNConst.MN_USER_ID_UNDEFINED)
         {
          return "ns";
         }
        else if (IsLoggedInByStatus(status))
         {
          return "au";
         }
        else
         {
          return "ou";
         }
       }

      private static string GetImageNamePosSuffix (int pos)
       {
        string result;

        if      ((pos & VERT_CENTER_BIT) != 0)
         {
          result = "m";
         }
        else if ((pos & VERT_BOTTOM_BIT) != 0)
         {
          result = "b";
         }
        else
         {
          result = "t";
         }

        if      ((pos & HORZ_CENTER_BIT) != 0)
         {
          result += 'c';
         }
        else if ((pos & HORZ_LEFT_BIT) != 0)
         {
          result += 'l';
         }
        else
         {
          result += 'r';
         }

        return result;
       }

      private StreamResourceInfo GetButtonImageStream ()
       {
        StringBuilder name = new StringBuilder(BUTTON_IMAGE_FILENAME_PREFIX);

        name.Append(GetImageNamePosSuffix(location));

        string shortName = name.ToString() + BUTTON_IMAGE_EXTENSION;

        name.Append('_');
        name.Append(GetImageNameStatusSuffix(MNDirect.GetSessionStatus(),MNDirect.GetSession().GetMyUserId()));
        name.Append(BUTTON_IMAGE_EXTENSION);

        string fullName = name.ToString();

        StreamResourceInfo stream = MNPlatformWinPhone.GetResourceStream(fullName);

        if (stream == null)
         {
          stream = MNPlatformWinPhone.GetResourceStream(shortName);
         }

        return stream;
       }

      private Image buttonImage = null;

      private const string BUTTON_IMAGE_FILENAME_PREFIX = "Assets/MNDirectButton/mn_direct_button_";
      private const string BUTTON_IMAGE_EXTENSION = ".png";
     }

    #endregion
   }
 }
