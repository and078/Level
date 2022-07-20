using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App3
{
    public partial class MainPage : ContentPage
    {

        AccelerometerDataTransmitter accelerometerData = new AccelerometerDataTransmitter();

        public MainPage()
        {
            InitializeComponent();
            this.BackgroundColor = Color.Black;
            DeviceDisplay.KeepScreenOn = true;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Accelerometer.IsMonitoring)
                return;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(SensorSpeed.Default);
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            accelerometerData.XValue = e.Reading.Acceleration.X * 40;
            accelerometerData.YValue = e.Reading.Acceleration.Y * 40;
            canvasView.InvalidateSurface();
        }

        SKPaint whiteFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        SKPaint grayFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.LightGray
        };

        void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float x = 0;
            float y = 0;
            float width = info.Width;
            float height = info.Height;

            DisplayClipOp(canvas, new SKRect(x, y, width, height), SKRegionOperation.XOR);
        }

        void DisplayClipOp(SKCanvas canvas, SKRect rect, SKRegionOperation regionOp)
        {
            float radius = 0.8f * Math.Min(rect.Width / 3, rect.Height / 2);
            float xCenter = rect.MidX;
            float yCenter = rect.MidY;

            SKRectI recti = new SKRectI((int)rect.Left, (int)rect.Top,
                                        (int)rect.Right, (int)rect.Bottom);

            float xVal = accelerometerData.XValue >= 0 ? accelerometerData.XValue * accelerometerData.XValue : accelerometerData.XValue * accelerometerData.XValue * -1;
            float yVal = accelerometerData.YValue >= 0 ? accelerometerData.YValue * accelerometerData.YValue : accelerometerData.YValue * accelerometerData.YValue * -1;
            

            using (SKRegion wholeRectRegion = new SKRegion())
            {
                wholeRectRegion.SetRect(recti);

                using (SKRegion textRegion = new SKRegion(wholeRectRegion))
                using (SKRegion region1 = new SKRegion(wholeRectRegion))
                using (SKRegion region2 = new SKRegion(wholeRectRegion))
                {
                    using (SKPath path1 = new SKPath())
                    {
                        path1.AddCircle(xCenter + xVal, yCenter + yVal, radius);
                        region1.SetPath(path1);
                    }

                    using (SKPath path2 = new SKPath())
                    {
                        path2.AddCircle(xCenter - xVal, yCenter - yVal, radius);
                        region2.SetPath(path2);
                    }

                    using (SKPath textPath = new SKPath())
                    {
                        textPath.AddRect(new SKRect(
                            xCenter - accelerometerData.XValue * 10,
                            yCenter - accelerometerData.YValue * 10,
                            xCenter + accelerometerData.XValue * 10,
                            yCenter + accelerometerData.YValue * 10)
                        );
                        textRegion.SetPath(textPath);
                    }

                    region1.Op(region2, regionOp);
                    region1.Op(textRegion, regionOp);

                    canvas.Save();
                    canvas.ClipRegion(region1);
                    canvas.DrawPaint(whiteFillPaint);
                    canvas.Restore();
                }
            }
        }
    }
}
