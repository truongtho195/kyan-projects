﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FlashCard.Database;
using Hardcodet.Wpf.TaskbarNotification;

namespace FlashCard
{
  /// <summary>
  /// Interaction logic for FancyBalloon.xaml
  /// </summary>
  public partial class FancyBalloon : UserControl
  {
    public bool isClosing = false;

    #region BalloonText dependency property

    /// <summary>
    /// Description
    /// </summary>
    public static readonly DependencyProperty InformationTextProperty =
        DependencyProperty.Register("InformationText",
                                    typeof (string),
                                    typeof (FancyBalloon),
                                    new FrameworkPropertyMetadata(""));

    /// <summary>
    /// A property wrapper for the <see cref="InformationTextProperty"/>
    /// dependency property:<br/>
    /// Description
    /// </summary>
    public string InformationText
    {
        get { return (string)GetValue(InformationTextProperty); }
        set { SetValue(InformationTextProperty, value); }
    }





    public string Word
    {
        get { return (string)GetValue(WordProperty); }
        set { SetValue(WordProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Word.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty WordProperty =
        DependencyProperty.Register("Word", typeof(string), typeof(FancyBalloon), new FrameworkPropertyMetadata(""));


    #endregion


    public FancyBalloon()
    {
      InitializeComponent();
      TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
      List<BackSideModel> list = new List<BackSideModel>();
      list.Add(new BackSideModel() { BackSideID = "1", BackSideName = "BackSideName 1"});
      list.Add(new BackSideModel() { BackSideID = "2", BackSideName = "BackSideName 2" });
      list.Add(new BackSideModel() { BackSideID = "3", BackSideName = "BackSideName 3" });
      list.Add(new BackSideModel() { BackSideID = "4", BackSideName = "BackSideName 4" });
      list.Add(new BackSideModel() { BackSideID = "5", BackSideName = "BackSideName 5" });
      list.Add(new BackSideModel() { BackSideID = "6", BackSideName = "BackSideName 6" });
      list.Add(new BackSideModel() { BackSideID = "7", BackSideName = "BackSideName 7" });
      list.Add(new BackSideModel() { BackSideID = "8", BackSideName = "BackSideName 8" });
      //this.lstBackSide.ItemsSource = list;
    }

   
 



    /// <summary>
    /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
    /// and setting the "Handled" property to true, we suppress the popup
    /// from being closed in order to display the fade-out animation.
    /// </summary>
    private void OnBalloonClosing(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      isClosing = true;
    }


    /// <summary>
    /// Resolves the <see cref="TaskbarIcon"/> that displayed
    /// the balloon and requests a close action.
    /// </summary>
    private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
    {
      //the tray icon assigned this attached property to simplify access
      TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
      taskbarIcon.CloseBalloon();
        
    }

    /// <summary>
    /// If the users hovers over the balloon, we don't close it.
    /// </summary>
    private void grid_MouseEnter(object sender, MouseEventArgs e)
    {
      //if we're already running the fade-out animation, do not interrupt anymore
      //(makes things too complicated for the sample)
      if (isClosing) return;

      //the tray icon assigned this attached property to simplify access
      TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
      taskbarIcon.ResetBalloonCloseTimer();
    }


    /// <summary>
    /// Closes the popup once the fade-out animation completed.
    /// The animation was triggered in XAML through the attached
    /// BalloonClosing event.
    /// </summary>
    private void OnFadeOutCompleted(object sender, EventArgs e)
    {
      Popup pp = (Popup)Parent;
      pp.IsOpen = false;
    }

    
  
  }
}
