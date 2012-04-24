//
//  MNPopupWindow.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace PlayPhone.MultiNet.Core
 {
  public class MNPopupWindow
   {
    public MNPopupWindow ()
     {
      silverlightPopup = null;
      isVisible        = false;
      size             = Size.Empty;
      pos              = new Point();
     }

    public bool IsVisible
     {
      get
       {
        return isVisible;
       }
     }

    public UIElement Child
     {
      get
       {
        return child;
       }

      set
       {
        SetChild(value);
       }
     }

    public void Show ()
     {
      if (isVisible)
       {
        return;
       }

      if (MNPopupWindowManager.Instance.CurrentPageMode == MNPopupWindowManager.PageMode.XNA)
       {
        ShowXNAPopup();
       }
      else // SILVERLIGHT
       {
        ShowSilverlightPopup();
       }

      isVisible = true;
     }

    public void Hide ()
     {
      if (!isVisible)
       {
        return;
       }

      if (MNPopupWindowManager.Instance.CurrentPageMode == MNPopupWindowManager.PageMode.XNA)
       {
        HideXNAPopup();
       }
      else // SILVERLIGHT
       {
        HideSilverlightPopup();
       }

      isVisible = false;
     }

    private void ShowSilverlightPopup ()
     {
      silverlightPopup       = new Popup();
      silverlightPopup.Child = child;

      UpdateSilverlightPopupSize();

      silverlightPopup.HorizontalOffset = pos.X;
      silverlightPopup.VerticalOffset   = pos.Y;

      silverlightPopup.IsOpen = true;
     }

    private void HideSilverlightPopup ()
     {
      if (silverlightPopup != null)
       {
        silverlightPopup.IsOpen = false;
        silverlightPopup.Child  = null;
        silverlightPopup        = null;
       }
     }

    private void ShowXNAPopup ()
     {
      Panel pageRoot = MNPopupWindowManager.Instance.CurrentPageRoot;

      if (pageRoot != null)
       {
        pageRoot.Children.Add(child);

        Canvas canvas = pageRoot as Canvas;

        if (canvas != null)
         {
          Canvas.SetLeft(child,pos.X);
          Canvas.SetTop(child,pos.Y);
         }
       }
     }

    private void HideXNAPopup ()
     {
      Panel pageRoot = MNPopupWindowManager.Instance.CurrentPageRoot;

      if (pageRoot != null)
       {
        pageRoot.Children.Remove(child);
       }
     }

    public void SetSize (Size size)
     {
      this.size = size;

      if (isVisible)
       {
        if (MNPopupWindowManager.Instance.CurrentPageMode == MNPopupWindowManager.PageMode.XNA)
         {
          // no need to update anything in XNA mode
         }
        else // SILVERLIGHT
         {
          if (silverlightPopup != null)
           {
            UpdateSilverlightPopupSize();
           }
         }
       }
     }

    public void SetPos (Point pos)
     {
      this.pos = pos;

      if (isVisible)
       {
        if (MNPopupWindowManager.Instance.CurrentPageMode == MNPopupWindowManager.PageMode.XNA)
         {
          Canvas canvas = MNPopupWindowManager.Instance.CurrentPageRoot as  Canvas;

          if (canvas != null)
           {
            Canvas.SetLeft(child,pos.X);
            Canvas.SetTop(child,pos.Y);
           }
         }
        else // SILVERLIGHT
         {
          if (silverlightPopup != null)
           {
            silverlightPopup.HorizontalOffset = pos.X;
            silverlightPopup.VerticalOffset   = pos.Y;
           }
         }
       }
     }

    private void UpdateSilverlightPopupSize ()
     {
      Size newSize = size;

      if (newSize.IsEmpty)
       {
        newSize = Application.Current.RootVisual.RenderSize;
       }

      silverlightPopup.MinWidth  = newSize.Width;
      silverlightPopup.MinHeight = newSize.Height;
     }

    private void SetChild (UIElement child)
     {
      if (MNPopupWindowManager.Instance.CurrentPageMode == MNPopupWindowManager.PageMode.XNA)
       {
        if (isVisible)
         {
          HideXNAPopup(); 
         }

        this.child = child;

        if (isVisible)
         {
          ShowXNAPopup(); 
         }
       }
      else  // SILVERLIGHT
       {
        this.child = child;

        if (isVisible && silverlightPopup != null)
         {
          silverlightPopup.Child = child;
         }
       }
     }

    private Popup     silverlightPopup;
    private bool      isVisible;
    private UIElement child;
    private Size      size;
    private Point     pos;
   }
 }
