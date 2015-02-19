using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Interaction logic for ImageEditor.xaml
    /// </summary>
    public partial class ImageEditor : UserControl
    {

        public enum DrawingModes 
        {
            Line,
            Ellipse,
            Rectangle,
            Free
        }

        /// <summary>
        /// Dependencyproperty referente à figura que está sendo desenhada
        /// </summary>
        public static readonly DependencyProperty DrawingModeProperty = DependencyProperty.Register("DrawingMode", typeof(DrawingModes), typeof(ImageEditor), new UIPropertyMetadata(DrawingModes.Free, CurrentFigureChanged));

        /// <summary>
        /// Dependencyproperty da espessura do desenho
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(int), typeof(ImageEditor), new UIPropertyMetadata(3, StrokeThicknessChanged));

        /// <summary>
        /// Dependencyproperty referente a cor do desenho
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ImageEditor), new UIPropertyMetadata(Colors.Black, ColorChanged));

        /// <summary>
        /// Dependecy property referente ao zoom da edição
        /// </summary>
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ImageEditor));

        static void CurrentFigureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageEditor typedSender = sender as ImageEditor;
            if (typedSender != null)
            {
                if ((DrawingModes)e.NewValue != DrawingModes.Free)
                {
                    typedSender.canvas.EditingMode = InkCanvasEditingMode.None;
                }
                else
                {
                    typedSender.canvas.EditingMode = InkCanvasEditingMode.Ink;
                }
            }
        }

        /// <summary>
        /// Chamada sempre que a espessura do pincel muda
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void StrokeThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageEditor typedSender = sender as ImageEditor;
            if (typedSender != null)
            {
                typedSender.canvas.DefaultDrawingAttributes.Height = (int)e.NewValue;
                typedSender.canvas.DefaultDrawingAttributes.Width = (int)e.NewValue;
            }
        }

        /// <summary>
        /// Chamada sempre que a cor de desenho muda
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageEditor typedSender = sender as ImageEditor;
            if (typedSender != null)
            {
                typedSender.canvas.DefaultDrawingAttributes.Color = (Color)e.NewValue;
            }
        }

        private Rectangle rectangle;
        private Ellipse ellipse;
        private Line line;
        private Point startPoint;

        public DrawingModes DrawingMode
        {
            get { return (DrawingModes)GetValue(DrawingModeProperty); }
            set { SetValue(DrawingModeProperty, value); }
        }

        /// <summary>
        /// Propriedade referente a espessura do pincel
        /// </summary>
        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>
        /// Propriedade referente a cor do desenho
        /// </summary>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Propriedade referente ao zoom da área de edição
        /// </summary>
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public ImageEditor()
        {
            InitializeComponent();
        }

        public void SetImage(string path)
        {
            try
            {
                Image image = new System.Windows.Controls.Image();

                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bmp.EndInit();
                image.Source = bmp;

                
                SetImage(image);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        public void SetImage(Image img)
        {
            canvas.Children.RemoveRange(0, canvas.Children.Count);
            try
            {
                canvas.Width = img.Source.Width;
                canvas.Height = img.Source.Height;
                canvas.Children.Add(img);
            }
            catch(NullReferenceException exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        public void Clean()
        {
            canvas.Strokes.Clear();
            canvas.Children.RemoveRange(0, canvas.Children.Count);
        }

        public void Save(string path)
        {
            // salva a transformação atual da área de edição (inkcanvas)
            Transform transform = canvas.LayoutTransform;
            // reset a atual transformação da área de edição
            canvas.LayoutTransform = null;

            var size = new Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Margin = new Thickness(0, 0, 0, 0);

            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            FileStream fs = File.Open(@path, FileMode.Create);
            encoder.Save(fs);
            canvas.LayoutTransform = transform;
            fs.Close();
        }

        private void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);

            SolidColorBrush brushColor = new SolidColorBrush();
            brushColor.Color = Color;

            switch (DrawingMode)
            {
                case DrawingModes.Rectangle:

                    rectangle = new Rectangle
                    {
                        Stroke = brushColor,
                        StrokeThickness = StrokeThickness
                    };

                    InkCanvas.SetLeft(rectangle, startPoint.X);
                    InkCanvas.SetTop(rectangle, startPoint.Y);
                    canvas.Children.Add(rectangle);

                    break;

                case DrawingModes.Ellipse:

                    ellipse = new Ellipse
                    {
                        Stroke = brushColor,
                        StrokeThickness = StrokeThickness
                    };

                    InkCanvas.SetLeft(ellipse, startPoint.X);
                    InkCanvas.SetTop(ellipse, startPoint.Y);
                    canvas.Children.Add(ellipse);

                    break;

                case DrawingModes.Line:

                    line = new Line
                    {
                        Stroke = brushColor,
                        StrokeThickness = StrokeThickness,
                        X1 = startPoint.X,
                        Y1 = startPoint.Y,
                    };

                    canvas.Children.Add(line);

                    break;

                default:
                    break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                switch (DrawingMode)
                {
                    case DrawingModes.Rectangle:

                        if (rectangle != null)
                        {
                            var pos = e.GetPosition(canvas);

                            var x = Math.Min(pos.X, startPoint.X);
                            var y = Math.Min(pos.Y, startPoint.Y);

                            var w = Math.Max(pos.X, startPoint.X) - x;
                            var h = Math.Max(pos.Y, startPoint.Y) - y;

                            rectangle.Width = w;
                            rectangle.Height = h;

                            InkCanvas.SetLeft(rectangle, x);
                            InkCanvas.SetTop(rectangle, y);
                        }

                        break;

                    case DrawingModes.Ellipse:

                        if (ellipse != null)
                        {
                            var pos = e.GetPosition(canvas);

                            var x = Math.Min(pos.X, startPoint.X);
                            var y = Math.Min(pos.Y, startPoint.Y);

                            var w = Math.Max(pos.X, startPoint.X) - x;
                            var h = Math.Max(pos.Y, startPoint.Y) - y;

                            ellipse.Width = w;
                            ellipse.Height = h;

                            InkCanvas.SetLeft(ellipse, x);
                            InkCanvas.SetTop(ellipse, y);

                        }

                        break;

                    case DrawingModes.Line:

                        if (line != null)
                        {
                            var pos = e.GetPosition(canvas);
                            line.X2 = pos.X;
                            line.Y2 = pos.Y;
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rectangle = null;
            ellipse = null;
            line = null;
        }
    }
}
