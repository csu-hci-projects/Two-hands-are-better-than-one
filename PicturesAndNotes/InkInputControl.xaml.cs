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

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.Devices.Input;

//PointerPoint
using Windows.UI.Input;
//InkManager, InkStroke, InkStrokeRenderingSegment and InkDrawingAttributes
using Windows.UI.Input.Inking;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PicturesAndNotes
{

    public sealed partial class InkInputControl : UserControl
    {
        DispatcherTimer timer;
        DateTimeOffset startTime;
        DateTimeOffset lastTime;
        DateTimeOffset stopTime;
        int timesTicked = 1;
        int timesToTick = 10;

        //The current ink drawing attributes, used to set the strokes color, pen size and shape
        InkDrawingAttributes m_inkAttr = null;
        //The main ink manager instance, used to produce nice Bezier curves from pointer input
        InkManager m_inkMan = null;
        //A custom ink rendered instance, used to push the strokes provided by an Ink Manager onto a XAML Panel instance
        //as well as rendering a simplified representation of an unfinished stroke
        InkRenderer m_renderer = null;
        //A stored active pointer id, we only allow a single active pointer(pen) device at a time
        uint m_activePointerId = 0;

        //A store of CanvaImage instances dropped in by the user
        List<CanvasImage> m_canvasImages;

        /// <summary>
        /// The current ink input color
        /// </summary>
        /// 

        //https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.dispatchertimer
        public void DispatcherTimerSetup()
        {
            if (timer == null) {
                timer = new DispatcherTimer();
                timer.Tick += dispatcherTimer_Tick;
                timer.Interval = new TimeSpan(0, 0, 1);
            //IsEnabled defaults to false
            //            TimerLog.Text += "dispatcherTimer.IsEnabled = " + dispatcherTimer.IsEnabled + "\n";
            startTime = DateTimeOffset.Now;
        }
            lastTime = startTime;
            //            TimerLog.Text += "Calling dispatcherTimer.Start()\n";
            timer.Start();
            //IsEnabled should now be true after calling start
//            TimerLog.Text += "dispatcherTimer.IsEnabled = " + dispatcherTimer.IsEnabled + "\n";
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            TimeSpan span = time - lastTime;
            lastTime = time;
            //Time since last tick should be very very close to Interval
//            TimerLog.Text += timesTicked + "\t time since last tick: " + span.ToString() + "\n";
            timesTicked++;
            if (timer.IsEnabled)
            {
                stopTime = time;
                //                TimerLog.Text += "Calling dispatcherTimer.Stop()\n";
                //IsEnabled should now be false after calling stop
//                TimerLog.Text += "dispatcherTimer.IsEnabled = " + dispatcherTimer.IsEnabled + "\n";
//                span = stopTime - startTime;
//                TimerLog.Text += "Total Time Start-Stop: " + span.ToString() + "\n";
            }
        }
        public async void StopTimer_Tick(Object sender, object e)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            timer.Stop();
            TimeSpan span = startTime - lastTime;

            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile ticketsFile = await storageFolder.CreateFileAsync("timelog.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);

            //Use stream to write to the file
            var stream = await ticketsFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

            using (var outputStream = stream.GetOutputStreamAt(stream.Size))
            {
                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    dataWriter.WriteString("Time: " + span + "\n"); 
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }
                    stream.Dispose(); // Or use the stream variable (see previous code snippet) with a using statement as well.



                string path = @"c:\temp\MyTest.txt";
            // This text is added only once to the file.
            
    }
        public Windows.UI.Color InkColor
        {
            get { return m_inkAttr.Color; }
            set { m_inkAttr.Color = value; }
        }

        /// <summary>
        /// The current ink input pen shape
        /// </summary>
        public Windows.UI.Input.Inking.PenTipShape PenShape
        {
            get { return m_inkAttr.PenTip; }
            set { m_inkAttr.PenTip = value; }
        }

        /// <summary>
        /// The current ink input pen size
        /// </summary>
        public Size PenSize
        {
            get { return m_inkAttr.Size; }
            set { m_inkAttr.Size = value; }
        }

        /// <summary>
        /// The current rendering target canvas
        /// </summary>
        public Canvas Target
        {
            get { return this.inkCanvas; }
        }

        //CTOR
        public InkInputControl()
        {
            //Initialize XAML
            this.InitializeComponent();

            //Create a new InkDrawingAttributes instance
            m_inkAttr = new InkDrawingAttributes();
            //Set firt to curve to true to get nice Bezier curves
            m_inkAttr.FitToCurve = true;
            //We ignore the presuer as our custom ink renderer does not know how to render it.
            m_inkAttr.IgnorePressure = true;
            //Set the default ink rendering size to 1.0
            m_inkAttr.Size = new Size(1.0, 1.0);
            //Set the defualt ink pen tip to circle
            m_inkAttr.PenTip = PenTipShape.Circle;
            //Set the default ink color to black
            m_inkAttr.Color = Windows.UI.Colors.Black;

            //Create a new Ink Manager instance
            m_inkMan = new InkManager();
            //Set the default ink rendering attributes
            m_inkMan.SetDefaultDrawingAttributes(m_inkAttr);

            //Create a new Ink Renderer instance using the control's inkCanvas object as the target
            m_renderer = new InkRenderer(this.inkCanvas);
            m_renderer.UseActiveInkColor = false;

            //Create an empty list for CanvaImage objects
            m_canvasImages = new List<CanvasImage>();
        }

        /// <summary>
        /// A public method used to clear ink ask well as images from the canvas
        /// </summary>
        public void Clear()
        {
            //Make the rederer clear all permanent ink
            m_renderer.ClearPermInk();
            //Clear the local CanvasImage list
            m_canvasImages.Clear();
            //Clear all of the inkCanvas children
            this.inkCanvas.Children.Clear();
            //Select all strokes handled by the ink manager
            SelectAllStrokes();
            //Delete all selected strokes from the local ink manager instance
            m_inkMan.DeleteSelected();
        }

        /// <summary>
        /// A public method used to add CanvasImage instance at the given position
        /// </summary>
        /// <param name="source">The image source for the new CanvasImage instance</param>
        /// <param name="position">The new CanvasImage's position</param>
        public void AddImage(ImageSource source, Point position)
        {
            //Creat a new CanvasImage instance
            CanvasImage img = new CanvasImage();
            //Set the image's source
            img.Image.Source = source;
            //We use a predifined size of 150x150 for all images
            //this is the size they have in the picker.
            img.Image.Width = 150;
            img.Image.Height = 150;
            
            //Add a new Image object to the inkCanvas
            this.inkCanvas.Children.Add(img.Image);
            //Set the new images position
            img.Position(position.X-100, position.Y);

            this.DispatcherTimerSetup();

        }

        /// <summary>
        /// A private method used to keep the canvas and its clipping rectangle in sync with the window's size
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Size changed event arguments</param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Set the new inkCanvas size
            this.inkCanvas.Width = e.NewSize.Width;
            this.inkCanvas.Height = e.NewSize.Height;
            //Set a new inkCanvas clipping rectangle
            this.inkCanvas.Clip = new RectangleGeometry() { Rect = new Rect (0.0,0.0,e.NewSize.Width, e.NewSize.Height)};
        }

        /// <summary>
        /// The pointer pressed event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">The pointer pressed event args</param>
        private void inkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Break if we already have an active pointer
            if (m_activePointerId != 0) return;
            //We only handle pen input here so we check the device type
            if ((e.Pointer.PointerDeviceType == PointerDeviceType.Pen || e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                && e.Pointer.IsInContact)
            {
                //Get a pointer point relative to the inkCanvas
                PointerPoint pointerPoint = e.GetCurrentPoint(this.inkCanvas);
                //Begin live stroke rendering at the pointerPoint's position
                m_renderer.StartRendering(pointerPoint, m_inkAttr);
                //Set the InkManager's mode to inking
                m_inkMan.Mode = InkManipulationMode.Inking;
                //Use the InkManager to process the pointer down event
                m_inkMan.ProcessPointerDown(pointerPoint);
                //Set the pointer device id as the currently active pointer id
                m_activePointerId = e.Pointer.PointerId;
                //Mark the event as handled
                e.Handled = true;
            }
        }

        /// <summary>
        /// The pointer moved event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">The pointer moved event args</param>
        private void inkCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Break if the event's pointer is not the currently active pointer
            if (m_activePointerId != e.Pointer.PointerId) return;
            //We only handle pen input here so we check the device type
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen || e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                //Check if the pen has contact or is just hovering
                if (e.Pointer.IsInContact)
                {
                    //Get a pointer point relative to the inkCanvas
                    PointerPoint pointerPoint = e.GetCurrentPoint(this.inkCanvas);
                    //Update the live stroek using the pointerPoints's position
                    m_renderer.UpdateStroek(pointerPoint);

                    //Get all of the event's intermediate points
                    IList<PointerPoint> interPointerPoints = e.GetIntermediatePoints(this.inkCanvas);
                    //Use the InkManager to process all of the pointer updates
                    for (int i = interPointerPoints.Count - 1; i >= 0; --i)
                        m_inkMan.ProcessPointerUpdate(interPointerPoints[i]);
                    //Mark the event as handled
                    e.Handled = true;
                }
                else // If the pen has no contact chances are it was lifted off while not in the controls aera
                {
                    //Use a private helper method to finalize pen input
                    HandlePenUp(e);
                }
            }
        }

        /// <summary>
        /// The pointer up event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">The pointer up event args</param>
        private void inkCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (m_activePointerId != e.Pointer.PointerId) return;
            //We only handle pen input here so we check the device type
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen || e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                //Use a private helper method to finalize pen input
                HandlePenUp(e);
            }
        }

        private void HandlePenUp(PointerRoutedEventArgs e)
        {
            //Get a pointer point relative to the inkCanvas
            PointerPoint pointerPoint = e.GetCurrentPoint(this.inkCanvas);
            //Use the InkManager to process the pointer up event
            m_inkMan.ProcessPointerUp(pointerPoint);
            //Stop live stroke rendering
            m_renderer.FinishRendering(pointerPoint);
            //Get all of the InkManger's strokes
            IReadOnlyList<InkStroke> strokes = m_inkMan.GetStrokes();
            //Get the last stroke's index
            int lastStrokeIndex = strokes.Count - 1;
            //check if the last index is valid and add a permanent ink rendering using the local InkAttributes
            if (lastStrokeIndex >= 0)
                m_renderer.AddPermaInk(strokes[lastStrokeIndex], m_inkAttr);
            //Clear the active pointer id
            m_activePointerId = 0;
            //Mark the event as handled
            e.Handled = true;
        }

        /// <summary>
        /// A Helper method used to select all of the InkManager's strokes
        /// </summary>
        private void SelectAllStrokes()
        {
            //Iterate through all of the InkManager's strokes and mark them selected
            foreach (InkStroke stroke in m_inkMan.GetStrokes())
                stroke.Selected = true;
        }
    }
}
