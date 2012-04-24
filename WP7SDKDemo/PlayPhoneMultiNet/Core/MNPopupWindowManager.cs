//
//  MNPopupWindowManager.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace PlayPhone.MultiNet.Core
 {
  public sealed class MNPopupWindowManager
   {
    public enum PageMode { SILVERLIGHT, XNA };

    public delegate void OnPageNavigationEventHandler (NavigationEventArgs args);
    public event         OnPageNavigationEventHandler OnPageNavigation;

    public static MNPopupWindowManager Instance
     {
      get
       {
        return instance;
       }
     }

    public PhoneApplicationPage CurrentPage
     {
      get
       {
        return page;
       }
     }

    public PageMode CurrentPageMode
     {
      get
       {
        return pageMode;
       }
     }

    public Panel CurrentPageRoot
     {
      get
       {
        return pageRoot;
       }
     }

    private MNPopupWindowManager ()
     {
      page     = null;
      pageMode = PageMode.SILVERLIGHT;
      pageRoot = null;
     }

    public void NavigatedTo (PhoneApplicationPage page,
                             PageMode             pageMode   = PageMode.SILVERLIGHT,
                             Panel                layoutRoot = null)
     {
      if (page != null)
       {
        MNDebug.warning("MNPopupWindowManager.NavigatedTo called without preceeding NavigatedFrom");

        NavigatingFrom(page);
       }

      this.page     = page;
      this.pageMode = pageMode;
      this.pageRoot = layoutRoot;

      DispatchNavigationEvent(NavigationEventArgs.EventType.NavigatedTo,page,pageMode,layoutRoot);
     }

    public void NavigatingFrom (PhoneApplicationPage page)
     {
      DispatchNavigationEvent(NavigationEventArgs.EventType.NavigatingFrom,page,pageMode,pageRoot);

      this.page     = null;
      this.pageMode = PageMode.SILVERLIGHT;
      this.pageRoot = null;
     }

    private void DispatchNavigationEvent (NavigationEventArgs.EventType type, PhoneApplicationPage page, PageMode mode, Panel parent)
     {
      OnPageNavigationEventHandler handler = OnPageNavigation;

      if (handler != null)
       {
        handler(new NavigationEventArgs(type,page,mode,parent));
       }
     }

    public class NavigationEventArgs
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

    private PhoneApplicationPage page;
    private PageMode             pageMode;
    private Panel                pageRoot;

    private static readonly MNPopupWindowManager instance = new MNPopupWindowManager();
   }
 }
