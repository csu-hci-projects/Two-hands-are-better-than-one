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
using System.ComponentModel;
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
    //A delegate used for color picked events
    public delegate void ColorPickedDelegate(Object sender, Windows.UI.Color color);

    /// <summary>
    /// A custom control used to populate the InkColorPicker control 
    /// </summary>
    public sealed partial class InkColor : UserControl, INotifyPropertyChanged
    {
        //CTOR
        public InkColor()
        {
            //Set the default color to black
            this.Color = Windows.UI.Colors.Black;
            //Set the default border color to blue
            this.BorderColor = new SolidColorBrush(Windows.UI.Colors.Blue);
            //Initialize the components XAML
            this.InitializeComponent();
            //Set the target's data context to this
            this.Target.DataContext = this;
        }

        /// <summary>
        /// The color pickers color property
        /// </summary>
        private Windows.UI.Color m_color;
        public Windows.UI.Color Color
        { 
            get { return m_color; }
            set 
            {
                if (this.m_color != value)
                {
                    this.m_color = value;
                    this.RaisePropertyChanged("Color");
                    this.BackgroundColor = new SolidColorBrush(this.m_color);
                }
            } 
        }

        /// <summary>
        /// The Color picker's background color property
        /// </summary>
        private Brush m_backgroundColor;
        public Brush BackgroundColor
        {
            get { return m_backgroundColor; }
            set
            {
                if (m_backgroundColor != value)
                {
                    m_backgroundColor = value;
                    this.RaisePropertyChanged("BackgroundColor");
                }
            }
        }

        /// <summary>
        /// The color picker's border color property
        /// </summary>
        private Brush m_border;
        public Brush BorderColor
        {
            get { return m_border; }
            set
            {
                if (this.m_border != value)
                {
                    this.m_border = value;
                    this.RaisePropertyChanged("BorderColor");
                }
            }
        }

        /// <summary>
        /// The color picker's border thickness
        /// </summary>
        private Thickness m_borderSize;
        public Thickness BorderSize
        {
            get { return m_borderSize; }
            set
            {
                if (this.m_borderSize != value)
                {
                    this.m_borderSize = value;
                    this.RaisePropertyChanged("BorderSize");
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        /// <summary>
        /// The color picked event 
        /// </summary>
        public event ColorPickedDelegate ColorPicked;
        private void Target_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (ColorPicked != null) ColorPicked(this, this.Color);
        }
    }
}
