﻿using LandingPage.Models;
using LandingPage.ViewModels;
using PlayniteCommon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LandingPage.Views
{
    /// <summary>
    /// Interaktionslogik für ShelvesView.xaml
    /// </summary>
    public partial class ShelvesView : UserControl
    {
        public ShelvesView()
        {
            InitializeComponent();
        }

        private static DispatcherTimer dispatcherTimer = null;

        private static GameModel clickedModel = null;

        private void ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && !(e.OriginalSource is TextBlock))
            {
                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
                    dispatcherTimer.Interval = TimeSpan.FromMilliseconds(250);
                    dispatcherTimer.Tick += DispatcherTimer_Tick;
                }
                dispatcherTimer.Start();

                if (item.DataContext is GameModel model)
                {
                    clickedModel = model;
                    //model.OpenCommand?.Execute(null);
                }
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            clickedModel?.OpenCommand?.Execute(null);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                if (item.DataContext is GameModel model)
                {
                    dispatcherTimer?.Stop();
                    model.StartCommand?.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bt && bt.DataContext is GameModel game)
            {
                infoPopup.Dispatcher.Invoke(() => {
                    infoPopup.Description.IsOpen = false;
                }, System.Windows.Threading.DispatcherPriority.Normal);

                game.StartCommand?.Execute(null);
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bt && bt.DataContext is GameModel game)
            {
                infoPopup.Dispatcher.Invoke(() => {
                    infoPopup.Description.IsOpen = false;
                }, System.Windows.Threading.DispatcherPriority.Normal);
                game.OpenCommand?.Execute(null);
            }
        }

        public void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWidth(sender, e);
        }

        private void UpdateWidth(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged || e.WidthChanged)
            {
                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newWidth = ActualWidth;
                    var listBoxes = ShelvesItemsControl.ItemContainerGenerator.Items
                    .Select(item => ShelvesItemsControl.ItemContainerGenerator.ContainerFromItem(item))
                    .OfType<ContentPresenter>()
                    .Select(ele => ele.ContentTemplate.FindName("GamesListBox", ele))
                    .OfType<ListBox>();
                    foreach (var listBox in sender is ListBox lb ? new[] { lb } : listBoxes)
                    {
                        var itemCount = listBox.ItemsSource?.Cast<object>().Count() ?? 0;
                        if (listBox.IsVisible && itemCount > 0)
                        {
                            FrameworkElement container = null;
                            for (int i = 0; i < itemCount; ++i)
                            {
                                if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement element)
                                {
                                    container = element;
                                    break;
                                }
                            }
                            var desiredWidth = listBox.DesiredSize.Width;
                            var itemWidth = container.ActualWidth + container.Margin.Left + container.Margin.Right;
                            var scrollViewer = UiHelper.FindVisualChildren<ScrollViewer>(listBox).FirstOrDefault();
                            // itemWidth = desiredWidth / itemCount;
                            var availableWidth = newWidth - 60;
                            FrameworkElement panel = VisualTreeHelper.GetParent(this) as FrameworkElement;
                            while (!(panel is GridNodeView))
                            {
                                panel = VisualTreeHelper.GetParent(panel) as FrameworkElement;
                            }
                            availableWidth = panel.ActualWidth - 60;
                            var newListWidth = Math.Floor(availableWidth / Math.Max(itemWidth, 1)) * itemWidth;
                            if (listBox.MaxWidth != newListWidth)
                            {
                                listBox.MaxWidth = Math.Max(0, newListWidth);
                            }
                        }
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        static readonly Random rng = new Random();

        private void Description_Closed(object sender, EventArgs e)
        {
            if (rng.NextDouble() <= 0.25)
            {
                GC.Collect();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bt)
            {
                if (bt.DataContext is ShelveViewModel svm)
                {
                    if (svm.ShelveProperties.Order == Order.Ascending)
                    {
                        svm.ShelveProperties.Order = Order.Descending;
                    }
                    else
                    {
                        svm.ShelveProperties.Order = Order.Ascending;
                    }
                }
            }
        }

        GameDetailsPopup infoPopup = new GameDetailsPopup();

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is GameModel model && model.Game != ShelveViewModel.DummyGame)
            {
                if (DataContext is ShelvesViewModel shelvesViewModel && shelvesViewModel.Shelves.ShowDetails)
                {
                    if (UiHelper.FindVisualChildren<Grid>(element, "ImageGrid").FirstOrDefault() is Grid imageGrid)
                    {
                        infoPopup.Dispatcher.Invoke(() => {
                            infoPopup.DataContext = element.DataContext;
                            infoPopup.Description.PlacementTarget = imageGrid;
                            infoPopup.Description.IsOpen = true;
                            if (infoPopup.Player != null)
                            {
                                infoPopup.Player.IsMuted = shelvesViewModel.Shelves.TrailerVolume <= 0.0;
                                infoPopup.Player.Volume = shelvesViewModel.Shelves.TrailerVolume;
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                    }
                }

                if (DataContext is ShelvesViewModel viewModel)
                {
                    viewModel.CurrentlyHoveredGame = model.Game;
                }
            }
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is GameModel model)
            {
                if (UiHelper.FindVisualChildren<Grid>(element, "ImageGrid").FirstOrDefault() is Grid imageGrid)
                {
                    infoPopup.Dispatcher.Invoke(() => {
                        if (infoPopup.Player != null)
                        {
                            infoPopup.Player.Volume = 0;
                        }
                        infoPopup.Description.IsOpen = false;
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                }

                if (DataContext is ShelvesViewModel viewModel)
                {
                    viewModel.CurrentlyHoveredGame = null;
                }
            }
        }

        private void Description_Opened(object sender, EventArgs e)
        {
            foreach (var window in Application.Current.Windows.Cast<Window>())
            {
                var type = window.GetType();
            }
        }
    }
}
