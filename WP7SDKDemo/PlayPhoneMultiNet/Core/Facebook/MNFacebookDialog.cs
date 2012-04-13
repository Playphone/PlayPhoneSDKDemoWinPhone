//
//  MNFacebookDialog.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;

using System.ComponentModel;

using Microsoft.Phone.Controls;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Core.Facebook
 {
  public abstract class MNFacebookDialog : MNDialogStack.IDismissible
   {
    public delegate void DialogEventHandler (DialogResult result);

    public event DialogEventHandler DialogCompleted;

    protected abstract Uri  GetDialogUri ();

    public MNFacebookDialog ()
     {
      dialogPopup = null;
      webBrowser  = null;
     }

    public void Show ()
     {
      if (dialogPopup != null)
       {
        return; // already shown
       }

      dialogPopup = new Popup();

      Border border = new Border();

      border.BorderBrush     = new SolidColorBrush(Colors.Cyan);
      border.BorderThickness = new Thickness(DialogBorderWidth);

      border.CornerRadius = new CornerRadius(DialogBorderWidth);
      border.Margin       = new Thickness(DialogMargin);

      webBrowser  = new WebBrowser();

      webBrowser.Loaded     += OnBrowserLoaded;
      webBrowser.Navigating += OnBrowserNavigating;
      webBrowser.Navigated  += OnBrowserNavigated;

      border.Child = webBrowser;
      dialogPopup.Child  = border;

      Size rootSize = Application.Current.RootVisual.RenderSize;

      border.Height = rootSize.Height - 2 * DialogMargin;
      border.Width  = rootSize.Width  - 2 * DialogMargin;

      dialogPopup.IsOpen = true;

      MNDialogStack.AppendDialog(this);
     }

    public void Dismiss ()
     {
      if (dialogPopup != null)
       {
        DialogResult result = new DialogResult(this);

        result.Cancel();

        DismissDialogWithResult(result);
       }
     }

    protected void DismissDialogWithResult (DialogResult result)
     {
      DialogEventHandler handler = DialogCompleted;

      if (handler != null)
       {
        handler(result);
       }

      DismissPopup();
     }

    private void DismissPopup ()
     {
      MNDialogStack.RemoveDialog(this);

      if (dialogPopup != null)
       {
        webBrowser.Loaded     -= OnBrowserLoaded;
        webBrowser.Navigating -= OnBrowserNavigating;
        webBrowser.Navigated  -= OnBrowserNavigated;

        dialogPopup.IsOpen = false;
        dialogPopup        = null;
       }
     }

    private void OnBrowserLoaded (object sender, RoutedEventArgs e)
     {
      webBrowser.Navigate(GetDialogUri());
     }

    protected virtual void OnBrowserNavigating (NavigatingEventArgs e)
     {
     }

    protected virtual void OnBrowserNavigated  (NavigationEventArgs e)
     {
     }

    private void OnBrowserNavigating (object sender, NavigatingEventArgs e)
     {
      OnBrowserNavigating(e);
     }

    private void OnBrowserNavigated (object sender, NavigationEventArgs e)
     {
      OnBrowserNavigated(e);
     }

    public class DialogResult
     {
      public Uri    ResultUri        { get; private set; }
      public bool   Cancelled        { get; private set; }
      public bool   Succeeded        { get; private set; }
      public string ErrorMessage     { get; private set; }
      public MNFacebookDialog Source { get; private set; }

      public DialogResult (MNFacebookDialog dialog, Uri resultUri = null)
       {
        ResultUri    = resultUri;
        Source       = dialog;
        Cancelled    = false;
        Succeeded    = true;
        ErrorMessage = null;
       }

      public void Cancel ()
       {
        Cancelled    = true;
        Succeeded    = false;
        ErrorMessage = null;
       }

      public void SetError (string message)
       {
        Cancelled    = false;
        Succeeded    = false;
        ErrorMessage = message;
       }
     }

    private Popup      dialogPopup;
    private WebBrowser webBrowser;

    private const int DialogMargin      = 5;
    private const int DialogBorderWidth = 5;
   }
 }
