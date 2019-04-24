/*
Copyright (c) <2013>, Intel Corporation All Rights Reserved.
 
The source code, information and material ("Material") contained herein is owned by Intel Corporation or its suppliers or licensors, and title to such Material remains with Intel Corporation
or its suppliers or licensors. The Material contains proprietary information of Intel or its suppliers and licensors. The Material is protected by worldwide copyright laws and treaty provisions. 
No part of the Material may be used, copied, reproduced, modified, published, uploaded, posted, transmitted, distributed or disclosed in any way without Intel's prior express written permission. 
No license under any patent, copyright or other intellectual property rights in the Material is granted to or conferred upon you, either expressly, by implication, inducement, estoppel or otherwise. 
Any license under such intellectual property rights must be express and approved by Intel in writing.
 
Unless otherwise agreed by Intel in writing, you may not remove or alter this notice or any other notice embedded in Materials by Intel or Intel’s suppliers or licensors in any way.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PicturesAndNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.PicturePicker.DropTarget = this.InkInputControl.Target;
            this.PicturePicker.PictureDropped += PicturePicker_PictureDropped;

            this.SizeChanged += MainPage_SizeChanged;
        }

        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationViewState viewState = ApplicationView.Value;
            this.InactiveStack.Visibility = viewState == ApplicationViewState.Snapped ? Windows.UI.Xaml.Visibility.Visible
                : Windows.UI.Xaml.Visibility.Collapsed;
            this.MainGrid.Visibility = viewState != ApplicationViewState.Snapped ? Windows.UI.Xaml.Visibility.Visible
                : Windows.UI.Xaml.Visibility.Collapsed;
        }

        void PicturePicker_PictureDropped(object sender, PictureDropEventArgs args)
        {
            this.InkInputControl.AddImage(args.Src, args.Point);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {  
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.InkInputControl.Clear();
        }

        private void InkColorPicked(Object sender, Windows.UI.Color color)
        {
            this.InkInputControl.InkColor = color;
        }

        private void PicturePicker_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }
    }
}
