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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

//PointerPoint
using Windows.UI.Input;
//InkManager, InkStroke, InkStrokeRenderingSegment and InkDrawingAttributes
using Windows.UI.Input.Inking;

namespace PicturesAndNotes
{
    /// <summary>
    /// A custom class used to render InkStrokes onto a XAML Panel instance
    /// </summary>
    class InkRenderer
    {
        //The rendering target
        private readonly Panel m_target = null;
        //The active pointer id
        private uint m_activePointerId = 0;
        //The currently active stroke
        PolyLineSegment m_activeStroke = null;
        //The currently active path
        private Path m_activePath = null;

        //Permanent ink path collections
        private List<Path> m_permInk = null;

        /// <summary>
        /// A flag used to set if the rederer should use the active ink color to render
        /// </summary>
        public bool UseActiveInkColor
        { get; set; }

        /// <summary>
        /// A proeprty used to access the renderers target panel
        /// </summary>
        public Panel Target { get { return m_target; } }
        /// <summary>
        /// A property used to set or get the active stroke's rendering color
        /// </summary>
        public Color ActiveInkRenderingColor { get; private set; }
        /// <summary>
        /// A property used to set or get the active stroke's pen size
        /// </summary>
        public double ActiveInkRenderingWidth { get; private set; }

        //CTOR
        public InkRenderer(Panel target)
        {
            //Check if we got a valid target panel
            if (target != null)
            {
                //Store the target reference
                m_target = target;
                //Create a new permanent ink list
                m_permInk = new List<Path>();
                //Set the default active stroke color to Turquoise
                ActiveInkRenderingColor = Colors.Turquoise;
                //Set the default active stroke thickness to 3.0
                ActiveInkRenderingWidth = 3.0;
                UseActiveInkColor = true;
            }
                //Throw an Argument Exception if we go a null target
            else throw new ArgumentException("The render target can't be null");
        }

        /// <summary>
        /// A public method used to start active stroke rendering at the given point
        /// </summary>
        /// <param name="pointerPoint">The start position for the active stroke rendering</param>
        /// <param name="drawAttr">The active stroke drawing attributes, note that we only respect it's pen shape setting. </param>
        public void StartRendering(PointerPoint pointerPoint, InkDrawingAttributes drawAttr)
        {
            //Break if we already have an active pointer
            if (m_activePointerId != 0) return;
            //Get the PointerPoint's pointer device id
            uint pId = pointerPoint.PointerId;

            //Create a new PolyLineSegment instance
            PolyLineSegment stroke = new PolyLineSegment();
            //Create a new PathFigure instance
            PathFigure figure = new PathFigure();
            //Create a new PathGeometry instance
            PathGeometry geometry = new PathGeometry();
            //Create a new Path instance
            Path path = new Path();

            //Add the start point to the stroke PolyLineSegment
            stroke.Points.Add(pointerPoint.Position);

            //Set the figure's start point
            figure.StartPoint = pointerPoint.Position;
            //Add the stroke PolyLineSegment to the figure's segments
            figure.Segments.Add(stroke);

            //Add the figure to the geometry's figures
            geometry.Figures.Add(figure);

            //Set the new geometry as the path's data
            path.Data = geometry;
            //Set the path's stroke color to the ActiveInkRenderingColor
            if (UseActiveInkColor)
                path.Stroke = new SolidColorBrush(ActiveInkRenderingColor);
            else
                path.Stroke = new SolidColorBrush(drawAttr.Color);
            //Set the path's stroke thickness to ActiveInkRenderingWidth
            path.StrokeThickness = ActiveInkRenderingWidth;
            //Set the path's stroke line join to a value that best represents the drawAttr's pen tip
            path.StrokeLineJoin = drawAttr.PenTip == PenTipShape.Circle ? Windows.UI.Xaml.Media.PenLineJoin.Round
                                                                        : Windows.UI.Xaml.Media.PenLineJoin.Miter;
            //Set the path's stroke line cap to a value that best represents the drawAttr's pen tip
            path.StrokeStartLineCap = drawAttr.PenTip == PenTipShape.Circle ? Windows.UI.Xaml.Media.PenLineCap.Round
                                                                            : Windows.UI.Xaml.Media.PenLineCap.Square;
            //Make the new stroke the current active stroke
            m_activeStroke = stroke;
            //Make the new path the current active path
            m_activePath = path;
            //Add the new path to the target panel
            m_target.Children.Add(path);
            //Store the active pointer device id
            m_activePointerId = pId;
        }

        /// <summary>
        /// A public method used to update the current active stroke
        /// </summary>
        /// <param name="pointerPoint">The pointer point used to update the active stroke</param>
        public void UpdateStroek(PointerPoint pointerPoint)
        {
            //Get the PointerPoint's pointer id
            uint pId = pointerPoint.PointerId;
            //Chec if the pointer point comes from the active pointer device
            //and check if we have an active stroke
            if (m_activePointerId == pId
                && m_activeStroke != null)
                //Add a new point to the active stroke
                m_activeStroke.Points.Add(pointerPoint.Position);
        }

        /// <summary>
        /// A public method used to finish rendering of the active stroke
        /// </summary>
        /// <param name="pointerPoint">The final pointer point for the active stroke</param>
        public void FinishRendering(PointerPoint pointerPoint)
        {
            //Get the PointerPoint's pointer id
            uint pId = pointerPoint.PointerId;
            //Check if the pointer point comes from the active pointer device
            //check if we have an active stroke and path
            if (m_activePointerId == pId
                && m_activeStroke != null
                && m_activePath != null)
            {
                //Remove the active path from the target panel
                m_target.Children.Remove(m_activePath);

                //Clear state
                m_activePath = null;
                m_activeStroke = null;
                m_activePointerId = 0;
            }
        }

        /// <summary>
        /// A public method used to add a permanent ink rendering using the provided InkStroke
        /// </summary>
        /// <param name="stroke">The InkStroke used to render the permanent ink</param>
        /// <param name="drawAttr">The InkDrawingAttributes used to render the permanent ink</param>
        public void AddPermaInk(InkStroke stroke, InkDrawingAttributes drawAttr)
        {
            //Use the ProduceBezierPath helper method to get a new stroke path
            Path strokePath = ProduceBezierPath(stroke, drawAttr);
            //Add the new stroke path to the target panel
            m_target.Children.Add(strokePath);
            //Store the new path in the permanent ink list
            m_permInk.Add(strokePath);
        }

        /// <summary>
        /// A public method used to clear all permanent ink instances
        /// </summary>
        public void ClearPermInk()
        {
            //Remove all permanent ink paths from the target panel
            foreach (Path p in m_permInk)
                m_target.Children.Remove(p);
            //Clear the permanent ink list
            m_permInk.Clear();
        }

        /// <summary>
        /// A private helper method used to produce BezierPaths from InkStorkes
        /// </summary>
        /// <param name="stroke">The InkStroke used to render the permanent ink</param>
        /// <param name="drawAttr">The InkDrawingAttributes used to render the permanent ink</param>
        /// <returns>A new Path built from BezierSegments</returns>
        private Path ProduceBezierPath(InkStroke stroke, InkDrawingAttributes drawAttr)
        {
            //Create a new PathFeagure instance
            PathFigure figure = new PathFigure();
            //Get all of the InkStroke's rendering segments
            IReadOnlyList<InkStrokeRenderingSegment> curveSegments = stroke.GetRenderingSegments();
            //Check if we have any segments to work with
            if (curveSegments.Count > 0)
                //Use the first segment to set the figure's start position
                figure.StartPoint = curveSegments[0].Position;
            //Iterate over the segment collection, beginning from the 2nd element
            for (int i = 1; i < curveSegments.Count; ++i)
            {
                //Get an InkStrokeRenderingSegemnt instance from the collection
                InkStrokeRenderingSegment pathSegment = curveSegments[i];
                //Create a new BezierSegment instance
                BezierSegment segment = new BezierSegment();
                //Set the segment's control points
                segment.Point1 = pathSegment.BezierControlPoint1;
                segment.Point2 = pathSegment.BezierControlPoint2;
                //Set the segment's position
                segment.Point3 = pathSegment.Position;
                //Add the new segment to the figure
                figure.Segments.Add(segment);
            }

            //Create a new Path instance
            Path path = new Path();
            //Populate the path's data with a new PathGeometry instancce
            path.Data = new PathGeometry();
            //Add the figure instance to the new PathGeometry
            (path.Data as PathGeometry).Figures.Add(figure);

            //Set the path's stroke color to the drawAttr's color
            path.Stroke = new SolidColorBrush(drawAttr.Color);
            //Set the path's stroke thickness to the drawAttr's Width
            path.StrokeThickness = drawAttr.Size.Width;
            //Set the path's stroke line join to best fit the drawAttr's pen tip
            path.StrokeLineJoin = drawAttr.PenTip == PenTipShape.Circle ? Windows.UI.Xaml.Media.PenLineJoin.Round
                                                                        : Windows.UI.Xaml.Media.PenLineJoin.Miter;
            //Set the path's stroke line cap to best fit the drawAttr's pen tip
            path.StrokeStartLineCap = drawAttr.PenTip == PenTipShape.Circle ? Windows.UI.Xaml.Media.PenLineCap.Round
                                                                            : Windows.UI.Xaml.Media.PenLineCap.Square;
            //Return the new path
            return path;
        }
    }
}
