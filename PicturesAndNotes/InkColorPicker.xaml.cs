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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PicturesAndNotes
{
    //The Ink color chaged delegate
    public delegate void InkColorChangeDelegate(Object sender, Windows.UI.Color color);

    /// <summary>
    /// A custom control used to pick the current ink color
    /// </summary>
    public sealed partial class InkColorPicker : UserControl
    {
        //The list of all of the picker's InkColor objects
        private List<InkColor> m_colors;
        //A brush used for the borders of inactive InkColor objects
        private Brush m_inActiveBrush;

        //CTOR
        public InkColorPicker()
        {
            //Initialize component's XAML
            this.InitializeComponent();
            //Use the helper method to produce picker colors
            ProcudeColors();
        }

        /// <summary>
        /// A helper method used to produce the pickers colors
        /// </summary>
        private void ProcudeColors()
        {
            //Set the inactive boder color to Transparent
            m_inActiveBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);

            //Create a new InkColor list
            m_colors = new List<InkColor>();

            //Create the first active color using the ProducePicker helper
            InkColor active = ProcudePicker(Windows.UI.Colors.Black);
            //Set the active InkColor object's border color to the mian border's color 
            active.BorderColor = this.MainBorder.BorderBrush;
            //Add the active InkColor to the picker list
            AddPicker(active);
            //Add some more InkColors using the ProducePicker and AddPicker helpers
            AddPicker(ProcudePicker(Windows.UI.Colors.Red));
            AddPicker(ProcudePicker(Windows.UI.Colors.Yellow));
            AddPicker(ProcudePicker(Windows.UI.Colors.Orange));
            AddPicker(ProcudePicker(Windows.UI.Colors.Green));
            AddPicker(ProcudePicker(Windows.UI.Colors.Blue));
        }

        /// <summary>
        /// A helper method used to create a new InkColor instance using the provided color
        /// </summary>
        /// <param name="color">The ink's color</param>
        /// <returns></returns>
        private InkColor ProcudePicker(Windows.UI.Color color)
        {
            //Create a new InkColor instance
            InkColor picker = new InkColor();
            //Set the object's color
            picker.Color = color;
            //Set the object's boder to the inactive color
            picker.BorderColor = m_inActiveBrush;
            //Set the object's border Size
            picker.BorderSize = new Thickness(3.5);
            //Set the pickers Width to 105
            picker.Width = 105.0;
            //Return the new InkColor instance
            return picker;
        }

        /// <summary>
        /// A helper method used to add a InkColor instance to the UI
        /// </summary>
        /// <param name="picker"></param>
        private void AddPicker(InkColor picker)
        {
            //Add a color picked event handler
            picker.ColorPicked += ColorPicked;
            //Add the InkColor instance to the InkColor list
            m_colors.Add(picker);
            //Add the InkColor instance to the UI stack
            this.PickerStack.Children.Add(picker);
        }

        //InkColor picked event
        public event ColorPickedDelegate InkColorPicked;
        //Color picked event handler
        void ColorPicked(object sender, Windows.UI.Color color)
        {
            //Get the source InkColor instance
            InkColor pressed = sender as InkColor;
            //Check if the instance is valid
            if (pressed != null)
            {
                //Iterate over all of the pickers InkColors and update their border color to highlight the active one
                foreach (InkColor inkcolor in m_colors)
                {
                    if (inkcolor == pressed) inkcolor.BorderColor = this.MainBorder.BorderBrush;
                    else inkcolor.BorderColor = m_inActiveBrush;
                }
            }
            //Raise the InkColorPicked event
            if (InkColorPicked != null) InkColorPicked(this, color);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double nWidth = (e.NewSize.Width - 50.0)/6.0;
            foreach (InkColor ink in m_colors)
                ink.Width = nWidth;
        }
    }
}
