// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Actions.Enums;
using AccessibilityInsights.Core.Bases;
using AccessibilityInsights.Core.Results;
using AccessibilityInsights.Desktop.Telemetry;
using AccessibilityInsights.SharedUx.Controls.CustomControls;
using AccessibilityInsights.SharedUx.Dialogs;
using AccessibilityInsights.SharedUx.FileBug;
using AccessibilityInsights.SharedUx.Settings;
using AccessibilityInsights.SharedUx.Utilities;
using AccessibilityInsights.SharedUx.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace AccessibilityInsights.SharedUx.Controls
{
    /// <summary>
    /// Interaction logic for ScannerResultControl.xaml
    /// </summary>
    public partial class ScannerResultControl : UserControl
    {
        /// <summary>
        /// Keeps track of if we should automatically set lv column widths
        /// </summary>
        public bool HasUserResizedLvHeader { get; set; } = false;

        /// <summary>
        /// Fixed width for bug column
        /// </summary>
        const int BugColumnWidth = 80;

        /// <summary>
        /// Constructor
        /// </summary>
        public ScannerResultControl()
        {
            InitializeComponent();
            this.List = new List<ScanListViewItemViewModel>();
            lvDetails.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Thumb_DragDelta), true);
            Resources.Source = new Uri(@"pack://application:,,,/AccessibilityInsights.SharedUx;component/Resources/Styles.xaml", UriKind.Absolute);
        }

        /// <summary>
        /// Action to perform when user needs to log into the server
        /// </summary>
        public Action SwitchToServerLogin { get; set; }

        /// <summary>
        /// Current ecid
        /// </summary>
        private Guid EcId;

        public List<ScanListViewItemViewModel> List { get; private set; }
        public A11yElement Element { get; private set; }

        /// <summary>
        /// Style dictionary
        /// </summary>
        new ResourceDictionary Resources = new ResourceDictionary();

        /// <summary>
        /// App configation
        /// </summary>
        public static ConfigurationModel Configuration
        {
            get
            {
                return ConfigurationManager.GetDefaultInstance()?.AppConfig;
            }
        }

        /// <summary>
        /// Show all scan results
        /// </summary>
        bool ShowAllResults = false;

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CustomControlOverridingAutomationPeer(this, "pane");
        }

        /// <summary>
        /// Set Test data
        /// </summary>
        /// <param name="e"></param>
        public void SetElement(A11yElement e, Guid ecId)
        {
            this.Clear();
            if (e != null && e.ScanResults != null && e.ScanResults.Items.Count != 0)
            {
                this.EcId = ecId;
                this.Element = e;
                SetScannerResultTreeView(e);
            }
        }

        /// <summary>
        /// Set data on the UI
        /// </summary>
        /// <param name="e"></param>
        private void SetScannerResultTreeView(A11yElement e)
        {
            this.List.AddRange(ScanListViewItemViewModel.GetScanListViewItemViewModels(e));
            this.lvDetails.ItemsSource = null;

            // enable UI elements since Clear() disables them. 
            this.btnShowAll.IsEnabled = true;

            UpdateTree();
        }

        /// <summary>
        /// Update tree, list and button text
        /// </summary>
        private void UpdateTree()
        {
            IEnumerable<ScanListViewItemViewModel> itemViewModel = from l in this.List
                                         where l.Status == ScanStatus.Fail || l.Status == ScanStatus.ScanNotSupported || (Configuration.ShowUncertain && l.Status == ScanStatus.Uncertain) || ShowAllResults
                                         orderby l.Status descending, l.Source, l.Header
                                         select l;

            var viewModelCount = itemViewModel.Count();

            this.lvDetails.ItemsSource = itemViewModel;

            btnShowAll.Visibility = Visibility.Visible;

            if (!ShowAllResults)
            {
                int diff = this.List.Count - viewModelCount;
                if (diff > 0)
                {
                    tbShowAll.Text = String.Format(CultureInfo.InvariantCulture, "{0} ({1})", Configuration.ShowUncertain ? Properties.Resources.ScannerResultControl_UpdateTree_Passed_tests : Properties.Resources.ScannerResultControl_UpdateTree_Passed_and_Uncertain_tests, diff);
                    this.btnShowAll.Visibility = Visibility.Visible;
                }
                else
                {
                    this.btnShowAll.Visibility = Visibility.Collapsed;
                }
            }

            if (viewModelCount > 0)
            {
                lvDetails.SelectedIndex = 0;
                this.spHowToFix.DataContext = itemViewModel.First<ScanListViewItemViewModel>();
            }
            else
            {
                this.spHowToFix.DataContext = null;
            }
            this.ShowAllResults = false;
        }

        /// <summary>
        /// Clear control
        /// </summary>
        public void Clear()
        {
            this.lvDetails.ItemsSource = null;
            this.List.Clear();
            this.tbShowAll.Text = "No Test Result";
            this.btnShowAll.IsEnabled = false;
        }

        /// <summary>
        /// Handles snippet click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HyperlinkSnippetClick(object sender, EventArgs e)
        {
            ((sender as Hyperlink).DataContext as ScanListViewItemViewModel).InvokeSnippetLink();
        }

        /// <summary>
        /// Update control based on failure selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvDetails_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            this.spHowToFix.DataContext = (e.AddedItems.Count > 0) ? (ScanListViewItemViewModel)e.AddedItems[0] : null;
        }

        /// <summary>
        /// Click on help link
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Hyperlink).DataContext as ScanListViewItemViewModel).InvokeHelpLink();
        }

        /// <summary>
        /// Show passed scan results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            this.ShowAllResults = true;
            UpdateTree();
            (sender as Button).Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Ensure listview contents are displayed properly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#pragma warning disable CA1801 // unused parameter
        private void lvDetails_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
#pragma warning restore CA1801 // unused parameter
        {
            if ((bool)e.NewValue)
            {
                var visible = this.btnShowAll.Visibility;
                this.ShowAllResults = visible == Visibility.Collapsed;
                UpdateTree();
                this.btnShowAll.Visibility = visible;
            }
        }

        /// <summary>
        /// Navigate to link on enter in listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((sender as ListViewItem).DataContext as ScanListViewItemViewModel).InvokeHelpLink();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Simulate * width for rule column
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvDetails_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!HasUserResizedLvHeader)
            {
                ListView listView = sender as ListView;
                GridView gView = listView.View as GridView;

                if (gView.Columns.Count >= 2)
                {
                    var width = (listView.ActualWidth - SystemParameters.VerticalScrollBarWidth) - gView.Columns[1].ActualWidth;
                    //leave the width as it was, if the resulting width goes in negative.
                    if (width >= 0)
                    {
                        gView.Columns[0].Width = width;
                    }
                }
                
            }
        }

        /// <summary>
        /// Handles click on file bug button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnFileBug_Click(object sender, RoutedEventArgs e)
        {
            var vm = ((Button)sender).Tag as ScanListViewItemViewModel;
            if (vm.BugId.HasValue)
            {
                // Bug already filed, open it in a new window
                try
                {
                    var bugUri = await BugReporter.GetExistingBugUriAsync(vm.BugId.Value).ConfigureAwait(true);
                    System.Diagnostics.Process.Start(bugUri.ToString());
                }
                catch (Exception ex)
                {
                    // Happens when bug is deleted, message describes that work item doesn't exist / possible permission issue
                    MessageDialog.Show(ex.InnerException?.Message);
                    vm.BugId = null;
                }
            }
            else
            {
                // File a new bug
                Logger.PublishTelemetryEvent(TelemetryAction.Scan_File_Bug, new Dictionary<TelemetryProperty, string>() {
                    { TelemetryProperty.By, FileBugRequestSource.HowtoFix.ToString() },
                    { TelemetryProperty.IsAlreadyLoggedIn, BugReporter.IsConnected.ToString(CultureInfo.InvariantCulture) }
                });

                // TODO: figuring out whether a team project has been chosen should not require
                //  looking at the most recent connection, this should be broken out
                if (BugReporter.IsConnected && Configuration.SavedConnection?.IsPopulated == true)
                {
                    Action<int> updateZoom = (int x) => Configuration.ZoomLevel = x;
                    (int? bugId, string newBugId) = FileBugAction.FileNewBug(vm.GetBugInformation(), Configuration.SavedConnection, Configuration.AlwaysOnTop, Configuration.ZoomLevel, updateZoom);

                    vm.BugId = bugId;

                    // Check whether bug was filed once dialog closed & process accordingly
                    if (vm.BugId.HasValue)
                    {
                        vm.LoadingVisibility = Visibility.Visible;
                        try
                        {
                            var success = await FileBugAction.AttachBugData(this.EcId, vm.Element.BoundingRectangle, 
                                vm.Element.UniqueId, newBugId, vm.BugId.Value).ConfigureAwait(false);
                            if (!success)
                            {
                                MessageDialog.Show(Properties.Resources.ScannerResultControl_btnFileBug_Click_There_was_an_error_identifying_the_created_bug_This_may_occur_if_the_ID_used_to_create_the_bug_is_removed_from_its_AzureDevOps_description_Attachments_have_not_been_uploaded);
                                vm.BugId = null;
                            }
                            vm.LoadingVisibility = Visibility.Collapsed;
                        }
                        catch (Exception)
                        {
                            vm.LoadingVisibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    bool? accepted = MessageDialog.Show(Properties.Resources.ScannerResultControl_btnFileBug_Click_Please_log_in_to_AzureDevOps_ensure_both_AzureDevOps_account_name_and_team_project_are_selected);
                    if (accepted.HasValue && accepted.Value)
                    {
                        SwitchToServerLogin();
                    }
                }
            }
        }

        /// <summary>
        /// Moves focus from currently focused element in given direction
        /// </summary>
        /// <param name="dir"></param>
        private static void MoveFocus(FocusNavigationDirection dir)
        {
            if (Keyboard.FocusedElement is FrameworkElement fe)
            {
                fe.MoveFocus(new TraversalRequest(dir));

            }
            else if (Keyboard.FocusedElement is FrameworkContentElement fce)
            {
                fce.MoveFocus(new TraversalRequest(dir));
            }
        }

        /// <summary>
        /// Custom keyboard nav behavior for listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            (sender as ListViewItem).SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.Local);

            if (e.Key == Key.Right && !(Keyboard.FocusedElement is Button))
            {
                MoveFocus(FocusNavigationDirection.Next);
            }
            else if (e.Key == Key.Left && !(Keyboard.FocusedElement is ListViewItem))
            {
                MoveFocus(FocusNavigationDirection.Previous);
            }

            (sender as ListViewItem).SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
        }

        /// <summary>
        /// Pass scrolling events through listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvDetails_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        /// <summary>
        /// Make bug column fixed width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb senderAsThumb = e.OriginalSource as Thumb;
            GridViewColumnHeader header = senderAsThumb.TemplatedParent as GridViewColumnHeader;
            if ((header.Content as string) == Properties.Resources.ScannerResultControl_Thumb_DragDelta_Rule)
            {
                HasUserResizedLvHeader = true;
            }
            if ((header.Content as string) == Properties.Resources.ScannerResultControl_Thumb_DragDelta_Bug)
            {
                header.Column.Width = BugColumnWidth;
            }
        }
    }
}