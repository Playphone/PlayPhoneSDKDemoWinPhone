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

    public static bool IsVisible ()
     {
      LazyInit();

      return popup.IsOpen;
     }

	   public static bool IsHidden ()
     {
      return !IsVisible();
     }

  	 public static void InitWithLocation (int location)
     {
      MNDirectButton.location = location;

      LazyInit();
     }

   	public static void Show()
     {
      LazyInit();

      popup.IsOpen = true;
     }

   	public static void Hide()
     {
      LazyInit();

      popup.IsOpen = false;
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

    private static StreamResourceInfo GetButtonImageStream ()
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

    private static void LazyInit ()
     {
      if (popup == null)
       {
        popup = new Popup();

        buttonImage = new Image();
        buttonImage.Tap += OnButtonTap;

        popup.Child = buttonImage;

        popup.LayoutUpdated += OnLayoutUpdated;

        UpdateButtonImage();

        MNDirect.GetSession().SessionStatusChanged += OnSessionStatusChanged;
        MNDirect.GetSession().UserChanged   += OnSessionUserChanged;

        MNDirectUIHelper.OnShowDashboard += OnDashboardShow;
        MNDirectUIHelper.OnHideDashboard += OnDashboardHide;
       }
     }

    private static void UpdateButtonImage ()
     {
      StreamResourceInfo bitmapStream = GetButtonImageStream();

      if (bitmapStream != null)
       {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.SetSource(bitmapStream.Stream);

        buttonImage.Source = bitmapImage;

        popup.Width  = bitmapImage.PixelWidth;
        popup.Height = bitmapImage.PixelHeight;
       }
      else
       {
        MNDebug.warning("MNDirectUIButton image not found");
       }
     }

    private static void OnButtonTap (object source, GestureEventArgs args)
     {
      MNDirectUIHelper.ShowDashboard();

      Hide();
     }

    private static void CalculateButtonPos (int location, out int x, out int y)
     {
      if ((location & HORZ_CENTER_BIT) != 0)
       {
        x = (int)((Application.Current.RootVisual.RenderSize.Width - popup.Width) / 2);
       }
      else if ((location & HORZ_LEFT_BIT) != 0)
       {
        x = 0;
       }
      else
       {
        x = (int)(Application.Current.RootVisual.RenderSize.Width - popup.Width);
       }

      if ((location & VERT_CENTER_BIT) != 0)
       {
        y = (int)((Application.Current.RootVisual.RenderSize.Height - popup.Height) / 2);
       }
      else if ((location & VERT_BOTTOM_BIT) != 0)
       {
        y = (int)(Application.Current.RootVisual.RenderSize.Height - popup.Height);
       }
      else
       {
        y = (int)MNPlatformWinPhone.GetSystemTrayHeight();
       }
     }

    private static void OnLayoutUpdated (object source, EventArgs args)
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

    private static void OnDashboardShow ()
     {
      autoShow = IsVisible();
     }

    private static void OnDashboardHide ()
     {
      if (autoShow)
       {
        Show();
       }
     }

    private static bool IsLoggedInByStatus (int status)
     {
      return status != MNConst.MN_OFFLINE && status != MNConst.MN_CONNECTING;
     }

    private static void OnSessionStatusChanged (int oldStatus, int newStatus)
     {
      if (IsLoggedInByStatus(oldStatus) != IsLoggedInByStatus(newStatus))
       {
        UpdateButtonImage();
       }
     }

    public static void OnSessionUserChanged (long userId)
     {
      UpdateButtonImage();
     }

    private const int VERT_BOTTOM_BIT = 0x01;
    private const int VERT_CENTER_BIT = 0x02;
    private const int HORZ_LEFT_BIT   = 0x04;
    private const int HORZ_CENTER_BIT = 0x08;
    private const string BUTTON_IMAGE_FILENAME_PREFIX = "Assets/MNDirectButton/mn_direct_button_";
    private const string BUTTON_IMAGE_EXTENSION = ".png";

    private static Popup popup       = null;
    private static Image buttonImage = null;
    private static bool  autoShow    = false;
    private static int   location    = MNDIRECTBUTTON_TOPRIGHT;
   }
 }
