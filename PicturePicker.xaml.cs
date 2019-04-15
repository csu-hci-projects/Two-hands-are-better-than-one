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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PicturesAndNotes
{
    /// <summary>
    /// Custom Event arguments class for picture drop events
    /// </summary>
    public class PictureDropEventArgs : EventArgs
    {
        /// <summary>
        /// The dropped image source
        /// </summary>
        public ImageSource Src
        { get; set; }
        /// <summary>
        /// The drop position relative to the target Panel
        /// </summary>
        public Point Point
        { get; set; }

        //CTOR
        public PictureDropEventArgs(ImageSource source, Point point)
        {
            Src = source;
            Point = point;
        }
    }

    //The picture dropped delegate
    public delegate void PictureDroppedDelegate(Object sender, PictureDropEventArgs args);

    /// <summary>
    /// A custom Picture picker user control
    /// </summary>
    public sealed partial class PicturePicker : UserControl
    {
        /// <summary>
        /// A private class used to store the position of a picture in the picker
        /// </summary>
        private class CanvasPosition
        {
            /// <summary>
            /// The Element's X position
            /// </summary>
            public double X { get; set; }
            /// <summary>
            /// The Element's Y position
            /// </summary>
            public double Y { get; set; }

            //CTOR
            public CanvasPosition(double x, double y)
            {
                //Store the position
                X = x;
                Y = y;
            }
        }

        //A dictionary used to store Images with their canvas position
        Dictionary<Image, CanvasPosition> m_imagePositions;
        //The list of all of the picker's images
        List<Image> m_images;

        //The gesture recognizer used to manage image dragging
        GestureRecognizer m_gestureRecognizer;
        //The gesture recognizer used to manage slider dragging
        GestureRecognizer m_sliderGestureRecognizer;
        //The picker's current active image
        Image m_activeImage;
        //The picker's active image's position
        CanvasPosition m_activePosition;

        //The target panel for picture dropping
        Panel m_droppingTarget;

        /// <summary>
        /// The Panel that's used as a drop target for the picker's pictures
        /// </summary>
        public Panel DropTarget
        {
            get { return m_droppingTarget; }
            set { m_droppingTarget = value; }
        }

        //The picture dropped event
        public event PictureDroppedDelegate PictureDropped;

        //The current slider position
        double m_sliderY = 0.0;
        //The old slider position
        double m_sliderOldY = 0.0;
        //The maximum slider position
        double m_maxOffset = 0.0;
        //The slider's view ratio
        double m_viewRatio = 1.0;
        //The view's item offset
        double m_itemOffset = 0.0;
        //A flag used to indicate that the slider is active
        bool m_sliderActive = false;

        //CTOR
        public PicturePicker()
        {
            //Initialize the picker's XAML
            this.InitializeComponent();
            //Use the helper to make the picker image instances
            MakeImages();

            //Create a new GestureRecognizer for the image gestures
            m_gestureRecognizer = new GestureRecognizer();
            //Register for the Manipulation started, updated and completed events
            m_gestureRecognizer.ManipulationStarted += m_gestureRecognizer_ManipulationStarted;
            m_gestureRecognizer.ManipulationUpdated += m_gestureRecognizer_ManipulationUpdated;
            m_gestureRecognizer.ManipulationCompleted += m_gestureRecognizer_ManipulationCompleted;
            //Setup the gesture recognizer to handle translations
            m_gestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;

            //Create a new GestureRecognizer for the slider
            m_sliderGestureRecognizer = new GestureRecognizer();
            //Register for the Manipulation started, updated and completed events
            m_sliderGestureRecognizer.ManipulationStarted += m_sliderGestureRecognizer_ManipulationStarted;
            m_sliderGestureRecognizer.ManipulationUpdated += m_sliderGestureRecognizer_ManipulationUpdated;
            m_sliderGestureRecognizer.ManipulationCompleted += m_sliderGestureRecognizer_ManipulationCompleted;
            //Setup the gesture recognizer to handle translations
            m_sliderGestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;

            //Register for the picker's size changed event
            this.PickerStack.SizeChanged += PickerStack_SizeChanged;
        }

        /// <summary>
        /// The Picker size changed event handler
        /// </summary>
        /// <param name="sender">the event's sander</param>
        /// <param name="e">Size changed event args</param>
        void PickerStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Use a helper method to resize the slider
            UpdateSliderHeight();
        }

        /// <summary>
        /// A helper method used to resize the slider
        /// </summary>
        void UpdateSliderHeight()
        {
            //Check if the control's height is grater then 0
            if (this.PickerStack.ActualHeight > 0.0)
            {
                //Calculate the view to content ratio
                m_viewRatio = this.ActualHeight / this.PickerStack.ActualHeight;
                //Make sure the ratio does not exceed 1.0
                if (m_viewRatio > 1.0) m_viewRatio = 1.0;
                //Resize the slider's background
                this.SliderParent.Height = this.ActualHeight;
                //Set the slider's height according to the view ratio
                this.Slider.Height = this.ActualHeight * m_viewRatio;
                //Move the slider and it's parent to the top
                Canvas.SetLeft(this.Slider, this.ActualWidth - this.Slider.Width);
                Canvas.SetTop(this.Slider, 0.0);
                Canvas.SetLeft(this.SliderParent, this.ActualWidth - this.Slider.Width);
                Canvas.SetTop(this.SliderParent, 0.0);
                //nullify the slider y value
                m_sliderY = 0.0;
                //Calculate the slider's max offset
                m_maxOffset = this.ActualHeight - this.Slider.Height;
            }
        }

        /// <summary>
        /// A helper method used to position the picker's images
        /// </summary>
        void UpdateImagePositions()
        {
            //Iterate over all of the picker's images
            foreach (Image img in m_images)
            {
                //Get the image's CanvasPosition
                CanvasPosition p = m_imagePositions[img];
                //Set the image's y position taking the global offset into account
                Canvas.SetTop(img, p.Y - m_itemOffset);
            }
        }

        /// <summary>
        /// The slider's gesture recognizer manipulation started event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">Manipulation started event args</param>
        void m_sliderGestureRecognizer_ManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            //Store the slider's old position
            m_sliderOldY = m_sliderY;
            //Update the slider position using the cumulative translation
            m_sliderY += args.Cumulative.Translation.Y;
            //Make sure the slider does not go out of scope
            if (m_sliderY > m_maxOffset) m_sliderY = m_maxOffset;
            else if (m_sliderY < 0.0) m_sliderY = 0.0;
            //calculate the item offset using the view ratio
            m_itemOffset = m_sliderY / m_viewRatio;
            //Update the slider's position
            Canvas.SetTop(this.Slider, m_sliderY);
            //Use the helper method to update the image positions
            UpdateImagePositions();
        }

        /// <summary>
        /// The slider's gesture recognizer manipulation updated event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">Manipulation updated event args</param>
        void m_sliderGestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            //Calculate the new slider position
            m_sliderY = m_sliderOldY + args.Cumulative.Translation.Y;
            //Make sure the slider does not go out of scope
            if (m_sliderY > m_maxOffset) m_sliderY = m_maxOffset;
            else if (m_sliderY < 0.0) m_sliderY = 0.0;
            //calculate the item offset using the view ratio
            m_itemOffset = m_sliderY / m_viewRatio;
            //Update the slider's position
            Canvas.SetTop(this.Slider, m_sliderY);
            //Use the helper method to update the image positions
            UpdateImagePositions();
        }

        /// <summary>
        /// The slider's gesture recognizer manipulation completed event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">Manipulation completed event args</param>
        void m_sliderGestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            //Calculate the new slider position
            m_sliderY = m_sliderOldY + args.Cumulative.Translation.Y;
            //Make sure the slider does not go out of scope
            if (m_sliderY > m_maxOffset) m_sliderY = m_maxOffset;
            else if (m_sliderY < 0.0) m_sliderY = 0.0;
            //calculate the item offset using the view ratio
            m_itemOffset = m_sliderY / m_viewRatio;
            //Update the slider's position
            Canvas.SetTop(this.Slider, m_sliderY);
            //Use the helper method to update the image positions
            UpdateImagePositions();
        }

        /// <summary>
        /// The images Gesture recognizer manipulation started event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">Manipulation started event args</param>
        void m_gestureRecognizer_ManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            //Get the cumulative translation
            Point p = args.Cumulative.Translation;
            //Update the image's position
            Canvas.SetLeft(m_activeImage, m_activePosition.X + p.X);
            Canvas.SetTop(m_activeImage, m_activePosition.Y + p.Y - m_itemOffset);
        }

        /// <summary>
        /// The images Gesture recognizer manipulation updated event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">Manipulation updated event args</param>
        void m_gestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            //Get the cumulative translation
            Point p = args.Cumulative.Translation;
            //Update the image's position
            Canvas.SetLeft(m_activeImage, m_activePosition.X + p.X);
            Canvas.SetTop(m_activeImage, m_activePosition.Y + p.Y - m_itemOffset);
        }

        /// <summary>
        /// The images Gesture recognizer manipulation completed event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">Manipulation completed event args</param>
        void m_gestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            //Restore the image to it's original position inside the picker
            Canvas.SetLeft(m_activeImage, m_activePosition.X);
            Canvas.SetTop(m_activeImage, m_activePosition.Y - m_itemOffset);
        }

        /// <summary>
        /// A private helper method used to add some images to the picker
        /// </summary>
        private void MakeImages()
        {
            //Create a new dictionary for the images and their position
            m_imagePositions = new Dictionary<Image,CanvasPosition>();
            //Create a new list for the images
            m_images = new List<Image>();
            //Add Some images using the Make and Add Image helper methods
            AddImage(MakeImage(@"ms-appx:///Assets/animals-bear.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-beaver.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-bumble_bee.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-cat.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-cow.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-dog.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-elephant.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-elk.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-giraffe.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-gnu.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-goat.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-owl.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/animals-whale.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/devil_bat.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/food-fried_egg_sunny.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/food-grapes.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/food-kiwi.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/food-peper-cayenne_red_chili_pepper.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/food-strawberry_with_light_shadow.png"));
            AddImage(MakeImage(@"ms-appx:///Assets/ghost.png"));
        }

        /// <summary>
        /// A private helper method used to create an image instance using the given image path
        /// </summary>
        /// <param name="source">The path for the new image</param>
        /// <returns></returns>
        private Image MakeImage(string source)
        {
            //Create a new image instance
            Image img = new Image();
            //Set the image's source using the path
            img.Source = new BitmapImage(new Uri(source, UriKind.Absolute));
            //Set the image's size
            img.Width = 150;
            img.Height = 150;
            //Return the new image instance
            return img;
        }

        /// <summary>
        /// A helper method used to add new image instances to the picker's ui
        /// </summary>
        /// <param name="img"></param>
        private void AddImage(Image img)
        {
            //Add the image to the canvas
            this.PickerStack.Children.Add(img);

            //Setup a left margin 
            double left = 7.5;
            //Set the image's top position using the total image count
            double top = 7.5 + (157.5 * m_images.Count);

            //Position the image
            Canvas.SetLeft(img, left);
            Canvas.SetTop(img, top);

            //Store the image in the images list
            m_images.Add(img);
            //Store the image's position
            m_imagePositions.Add(img, new CanvasPosition(left, top));

            //Make the picker stack bigger
            this.PickerStack.Height = top + 150.0;

            //Register image pointer event handlers
            img.PointerPressed += Image_PointerPressed;
            img.PointerMoved += Image_PointerMoved;
            img.PointerReleased += Image_PointerReleased;
            img.PointerExited += Image_PointerReleased;

            //Recalculate the slider's size
            UpdateSliderHeight();
        }

        /// <summary>
        /// Image pointer pressed event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Pointer down event args</param>
        void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Break if we have an active image or the slider happens to be active 
            if (m_activeImage != null || m_sliderActive) return;
            //Cast the sender to get an image instance
            Image img = sender as Image;
            //Check if we got a valid image instance
            if (img != null )
            {
                //Set the current active image instance
                m_activeImage = img;
                //Set the image's z Index to 1 so it's rendered over other images
                Canvas.SetZIndex(m_activeImage, 1);
                //Store the images position
                m_activePosition = m_imagePositions[m_activeImage];
                //Feed the gesture recognizer a pointer point relative to the image instance
                m_gestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(img));
                //Mark the event as handled
                e.Handled = true;
            }
        }

        /// <summary>
        /// Image pointer moved event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Pointer move event args</param>
        void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Break if we have no active image
            if (m_activeImage == null || m_sliderActive) return;
            //Cast the sender to get an image instance
            Image img = sender as Image;
            //Check if we got a valid image instance
            //and verify the device type as we only handler touch and mouse input
            if (img != null)
            {
                //Feed the intermediate points to the gesture recognizer
                m_gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(img));
                //Mark the event as handled
                e.Handled = true;
            }
        }

        /// <summary>
        /// Image pointer released event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Pointer released event args</param>
        void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Break if we have no active image
            if (m_activeImage == null || m_sliderActive) return;
            //Cast the sender to get an image instance
            Image img = sender as Image;
            //Check if we got a valid image instance
            if (img != null)
            {
                //Get the pointer point relative to the image
                PointerPoint imgPoint = e.GetCurrentPoint(img);
                if(!imgPoint.IsInContact)
                    //Feed the up pointer point to the gesture recognizer
                    m_gestureRecognizer.ProcessUpEvent(imgPoint);
                //Make the gesture recognizer complete the gesture
                m_gestureRecognizer.CompleteGesture();
                //Mark the event as handled
                e.Handled = true;

                //Check if we have a drop target and a PictureDrop delegate is registered
                if (m_droppingTarget != null
                    && PictureDropped != null)
                {
                    //Get the pointer points relative to the target and picker
                    PointerPoint canvasPoint = e.GetCurrentPoint(m_droppingTarget);
                    PointerPoint pickerPoint = e.GetCurrentPoint(this);
                    //Produce the target's and pickers bounding rectangles
                    Rect canvasRect = new Rect(0.0, 0.0, this.DropTarget.ActualWidth, this.DropTarget.ActualHeight);
                    Rect pickerRect = new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight);

                    //Verify the pointer points position, making sure it's contained in the target but not in the picker
                    if (ContainedIn(canvasPoint, canvasRect) && !ContainedIn(pickerPoint, pickerRect)) 
                    {
                        //Adjust the drop position to compensate for the user's finger position relative to the image 
                        Point imgPos = new Point(canvasPoint.Position.X - imgPoint.Position.X, canvasPoint.Position.Y - imgPoint.Position.Y);
                        //Raise the image dropped event
                        this.PictureDropped(this, new PictureDropEventArgs(img.Source,imgPos));
                    }
                }
                //Restore the images z index
                Canvas.SetZIndex(m_activeImage, 0);
                //Clear state
                m_activeImage = null;
                m_activePosition = null;
            }
        }

        /// <summary>
        /// The slider's pointer pressed event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">Pointer down even args</param>
        private void Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Mark the slider active
            m_sliderActive = true;
            //Finalize any old gestures
            m_sliderGestureRecognizer.CompleteGesture();
            //Feed the pointer down event to the gesture recognizer 
            m_sliderGestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(this.Slider));
            //Mark event as handled
            e.Handled = true;
        }

        /// <summary>
        /// The slider's pointer moved event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">Pointer move even args</param>
        private void Slider_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Feed the intermediate move points to the gesture recognizer
            m_sliderGestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this.Slider));
            //Mark event as handled
            e.Handled = true;
        }

        /// <summary>
        /// The slider's pointer released event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">Pointer up even args</param>
        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Mark the slider as inactive
            m_sliderActive = false;
            PointerPoint pointerPoint = e.GetCurrentPoint(this.Slider);
            if(!pointerPoint.IsInContact)
                //Feed the up pointer to the gesture recognizer
                m_sliderGestureRecognizer.ProcessUpEvent(pointerPoint);
            //Make the gesture recognizer finalize the gesture
            m_sliderGestureRecognizer.CompleteGesture();
            //Mark the event as handled
            e.Handled = true;
        }

        /// <summary>
        /// A helper method used to verify if a point is contained in a rectangle
        /// </summary>
        /// <param name="point">The point in question</param>
        /// <param name="rect">The suspected containing ractangle</param>
        /// <returns></returns>
        private bool ContainedIn(PointerPoint point, Rect rect)
        {
            return rect.Contains(point.Position);
        }
    }
}
