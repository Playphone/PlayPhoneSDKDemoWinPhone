//
//  MNDashboardPanel.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows;
using System.Windows.Controls;

namespace PlayPhone.MultiNet.Core
 {
  public class MNDashboardPanel : Panel
   {
    protected override Size MeasureOverride(Size availableSize)
     {
      foreach (UIElement child in Children)
       {
        child.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
       }

      return Application.Current.RootVisual.RenderSize;
     }

    protected override Size ArrangeOverride(Size finalSize)
     {
      if (Children.Count != 2)
       {
        return finalSize;
       }

      UIElement top    = Children[0];
      UIElement bottom = Children[1];

      if (bottom.Visibility == Visibility.Visible)
       {
        double topHeight = finalSize.Height - bottom.DesiredSize.Height;

        top.Arrange(new Rect(new Point(0,0),new Size(finalSize.Width,topHeight)));
        bottom.Arrange(new Rect(new Point(0,topHeight),new Size(finalSize.Width,bottom.DesiredSize.Height)));
       }
      else
       {
        top.Arrange(new Rect(new Point(0,0),finalSize));
        bottom.Arrange(new Rect(new Point(0,finalSize.Height),new Size(0,0)));
       }

      return finalSize;
     }
   }
 }
