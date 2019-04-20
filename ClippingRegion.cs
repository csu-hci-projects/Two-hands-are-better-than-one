// Create a path that consists of a single polygon.
     Point[] polyPoints = {
new Point(10, 10),
new Point(150, 10), 
new Point(100, 75),
new Point(100, 150)};
     GraphicsPath path = new GraphicsPath();
     path.AddPolygon(polyPoints);

     // Construct a region based on the path.
     Region region = new Region(path);

     // Draw the outline of the region.
     Pen pen = Pens.Black;
     e.Graphics.DrawPath(pen, path);

     // Set the clipping region of the Graphics object.
     e.Graphics.SetClip(region, CombineMode.Replace);

     // Draw some clipped strings.
     FontFamily fontFamily = new FontFamily("Arial");
     Font font = new Font(
        fontFamily,
        36, FontStyle.Bold,
        GraphicsUnit.Pixel);
     SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 255, 0, 0));

     e.Graphics.DrawString(
        "A Clipping Region",
        font, solidBrush,
        new PointF(15, 25));

     e.Graphics.DrawString(
        "A Clipping Region",
        font,
        solidBrush,
        new PointF(15, 68));
