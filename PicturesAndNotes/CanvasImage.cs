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

using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PicturesAndNotes
{
    /// <summary>
    /// A custom wrapper class used to manage an Image instance place in a canvas.
    /// It provides popter event and gesture manipulation handlers.
    /// </summary>
    class CanvasImage
    {
        //The wrappers image instance
        Image m_image;
        //The wrappers GestureRecognizer instance
        GestureRecognizer m_gestureRecognizer;
        //The Image's transformation matrix
        Matrix m_inMatrix;
        //The Image's origin from transformation matrix
        Matrix m_originFromTranslation;
        //The Image's origin to transformation matrix
        Matrix m_originToTranslation;


        /// <summary>
        /// Internal image access property
        /// </summary>
        public Image Image
        { get { return m_image; } }

        //CTOR
        public CanvasImage()
        {
            //Create a new Gesture Recognizer instance
            m_gestureRecognizer = new GestureRecognizer();
            //Add manipulation event handlers
            m_gestureRecognizer.ManipulationStarted += m_gestureRecognizer_ManipulationStarted;
            m_gestureRecognizer.ManipulationUpdated += m_gestureRecognizer_ManipulationUpdated;
            m_gestureRecognizer.ManipulationCompleted += m_gestureRecognizer_ManipulationCompleted;
            //Setup the recognizer to work with rotation, sacle and tranlstaions
            m_gestureRecognizer.GestureSettings = GestureSettings.ManipulationRotate
                                                  | GestureSettings.ManipulationScale
                                                  | GestureSettings.ManipulationTranslateX
                                                  | GestureSettings.ManipulationTranslateY;

            //Create a new image instance
            m_image = new Image();
            //Add pointer pressed, moved and released event handlers
            m_image.PointerPressed += m_image_PointerPressed;
            m_image.PointerMoved += m_image_PointerMoved;
            m_image.PointerReleased += m_image_PointerReleased;

            //Initiate the image's render transformation with a new MatrixTransformation instance
            m_image.RenderTransform = new MatrixTransform();
            //Create a new identity matrix
            m_inMatrix = Matrix.Identity;
            //Set the transformation's matrix to the identity matrix
            (m_image.RenderTransform as MatrixTransform).Matrix = m_inMatrix;

            //Calculate the origin translations
            m_originFromTranslation = Translation(75.0, 75.0);
            m_originToTranslation = Translation(-75.0, -75.0);
        }

        /// <summary>
        /// A public method used to position the image in a canvas
        /// </summary>
        /// <param name="X">The image's x position</param>
        /// <param name="Y">The image's y position</param>
        public void Position(double X, double Y)
        {
            //Set the images Canvas position
            Canvas.SetLeft(m_image, X);
            Canvas.SetTop(m_image, Y);
            //Set the image's z index to 1 so it's rendered over the canvas ink
            Canvas.SetZIndex(m_image, 1);
        }

        /// <summary>
        /// The image's pointer pressed event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The Pointer pressed event args</param>
        private void m_image_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //Check the pointer device type, we only handle touch devices
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                //Mark the event as handled
                e.Handled = true;
                //Feed the down poiter point to the gesture recognizer 
                m_gestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(m_image));
            }
            else
            {
                //Mark the event as unhandled
                e.Handled = false;
            }
        }

        /// <summary>
        /// The image's pointer moved event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_image_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //Check the pointer device type, we only handle touch devices
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                //Mark the event as handled
                e.Handled = true;
                //Feed the intermediate movement pointer points to the gesture recognizer
                m_gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(m_image));
            }
            else
            {
                //Mark the event as unhandled
                e.Handled = false;
            }
        }

        private void m_image_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                //Mark the event as handled
                e.Handled = true;
                //Feed the up pointer point to the gesture recognizer
                m_gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(m_image));
                //Make the gesture recognizer complete a gesture 
                m_gestureRecognizer.CompleteGesture();
            }
            else
            {
                //Mark the event as handled
                e.Handled = false;
            }
        }

        /// <summary>
        /// The gesture recognizers manipulation started event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="args">Manipulation started event args</param>
        private void m_gestureRecognizer_ManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            //Set the wrapper's image z index to 2, so it won't get stuck under other images
            Canvas.SetZIndex(m_image, 2);
            //Store the images transformation matrix
            m_inMatrix = (m_image.RenderTransform as MatrixTransform).Matrix;
            //Calculate a scale matrix
            Matrix scaleMatrix = Scale(args.Cumulative.Scale);
            //Calculate a rotation matrix
            Matrix rotationMatrix = Rotation(args.Cumulative.Rotation);
            //Calculate a translation matrix
            Matrix translationMatrix = Translation(args.Cumulative.Translation.X, args.Cumulative.Translation.Y);

            //Calculate an updated transformation matrix
            Matrix mat = MatMull(MatMull(MatMull(MatMull(m_originToTranslation, scaleMatrix), translationMatrix),rotationMatrix), m_originFromTranslation);
            //Combine the new and old transformations and use them for the images render transformation
            (m_image.RenderTransform as MatrixTransform).Matrix = MatMull(mat, m_inMatrix);
        }

        /// <summary>
        /// The gesture recognizers manipulation updated event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="args">Manipulation started event args</param>
        private void m_gestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            //Calculate a scale matrix
            Matrix scaleMatrix = Scale(args.Cumulative.Scale);
            //Calculate a rotation matrix
            Matrix rotationMatrix = Rotation(args.Cumulative.Rotation);
            //Calculate a translation matrix
            Matrix translationMatrix = Translation(args.Cumulative.Translation.X, args.Cumulative.Translation.Y);

            //Calculate an updated transformation matrix
            Matrix mat = MatMull(MatMull(MatMull(MatMull(m_originToTranslation, scaleMatrix), translationMatrix), rotationMatrix), m_originFromTranslation);
            //Combine the new and old transformations and use them for the images render transformation
            (m_image.RenderTransform as MatrixTransform).Matrix = MatMull(mat, m_inMatrix);
        }

        /// <summary>
        /// The gesture recognizers manipulation completed event handler
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="args">Manipulation started event args</param>
        private void m_gestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            //Reset the image's z index
            Canvas.SetZIndex(m_image, 1);
            //Calculate a scale matrix
            Matrix scaleMatrix = Scale(args.Cumulative.Scale);
            //Calculate a rotation matrix
            Matrix rotationMatrix = Rotation(args.Cumulative.Rotation);
            //Calculate a translation matrix
            Matrix translationMatrix = Translation(args.Cumulative.Translation.X, args.Cumulative.Translation.Y);

            //Calculate an updated transformation matrix
            Matrix mat = MatMull(MatMull(MatMull(MatMull(m_originToTranslation, scaleMatrix), translationMatrix), rotationMatrix), m_originFromTranslation);
            //Combine the new and old transformations and use them for the images render transformation
            (m_image.RenderTransform as MatrixTransform).Matrix = MatMull(mat, m_inMatrix);
        }

        /// <summary>
        /// A heper method used to calculate a rotation matrix
        /// </summary>
        /// <param name="angle">The rotation angle in degrees</param>
        /// <returns></returns>
        private Matrix Rotation(double angle)
        {
            //Translate degrees to radians
            double angnleRad = Rad(angle);
            //Start out with an identity matrix
            Matrix r = Matrix.Identity;
            //Calculate rotation matrix
            r.M11 = Math.Cos(angnleRad);
            r.M21 = -Math.Sin(angnleRad);
            r.M12 = Math.Sin(angnleRad);
            r.M22 = Math.Cos(angnleRad);
            //Return the result
            return r;
        }

        /// <summary>
        /// A helper method used to calculate a translation matrix
        /// </summary>
        /// <param name="x">X translation</param>
        /// <param name="y">Y translation</param>
        /// <returns></returns>
        private Matrix Translation(double x, double y)
        {
            //Start out with an idenity matrix
            Matrix r = Matrix.Identity;
            //Calculate translation
            r.OffsetX = x;
            r.OffsetY = y;
            //Return the result
            return r;
        }

        /// <summary>
        /// A helper method used to calculate a scaling matrix
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Matrix Scale(double scale)
        {
            //Start out with an identity matrix
            Matrix r = Matrix.Identity;
            //Calculate scale
            r.M11 = scale;
            r.M22 = scale;
            //Return result
            return r;
        }

        /// <summary>
        /// A helper function used to translate from degrees to radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double Rad(double angle)
        {
            //Calculate radians from degrees
            return (Math.PI * angle) / 180.0;
        }

        /// <summary>
        /// A helper method used to perform matrix multiplication AxB
        /// </summary>
        /// <param name="a">The A matrix</param>
        /// <param name="b">The B matrix</param>
        /// <returns></returns>
        private Matrix MatMull(Matrix a, Matrix b)
        {
            //Start out with an identity matrix
            Matrix r = Matrix.Identity;
            //Calculate the multiplication result
            r.M11 = (a.M11 * b.M11) + (a.M12 * b.M21);
            r.M12 = (a.M11 * b.M12) + (a.M12 * b.M22);
            r.M21 = (a.M21 * b.M11) + (a.M22 * b.M21);
            r.M22 = (a.M21 * b.M12) + (a.M22 * b.M22);
            r.OffsetX = (a.OffsetX * b.M11) + (a.OffsetY * b.M21) + b.OffsetX;
            r.OffsetY = (a.OffsetX * b.M12) + (a.OffsetY * b.M22) + b.OffsetY;
            //Return the result
            return r;
        }
    }
}
