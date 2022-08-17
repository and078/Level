using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App3
{
    public partial class MainPage : ContentPage
    {

        AccelerometerDataTransmitter accelerometerData = new AccelerometerDataTransmitter();

        List<float> listX = new List<float>(5) { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f };
        List<float> listY = new List<float>(5) { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f };

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
            Accelerometer.Start(SensorSpeed.Fastest);
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            ShiftList(accelerometerData.XValue, ref listX);
            ShiftList(accelerometerData.YValue, ref listY);

            accelerometerData.XValue = MidValue(e.Reading.Acceleration.X, listX);
            accelerometerData.YValue = MidValue(e.Reading.Acceleration.Y, listY);

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

            canvas.DrawText($"X:{accelerometerData.XValue}  Y:{accelerometerData.YValue}", 10.0f, 10.0f, grayFillPaint);

            DisplayClipOp(canvas, new SKRect(x, y, width, height), SKRegionOperation.XOR);
        }

        void DisplayClipOp(SKCanvas canvas, SKRect rect, SKRegionOperation regionOp)
        {
            float radius = 0.8f * Math.Min(rect.Width / 3, rect.Height / 2);
            float xCenter = rect.MidX;
            float yCenter = rect.MidY;

            SKRectI recti = new SKRectI((int)rect.Left, (int)rect.Top,
                                        (int)rect.Right, (int)rect.Bottom);

            float xMult = accelerometerData.XValue * 30;
            float yMult = accelerometerData.YValue * 40;

            float xVal = xMult >= 0 ? xMult * xMult : xMult * xMult * -1;
            float yVal = yMult >= 0 ? yMult * yMult : yMult * yMult * -1;

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
                    using (SKPaint paint = new SKPaint())
                    {
                        paint.TextSize = 10;

                        using (SKPath textPath = paint.GetTextPath("CODE", 0, 0))
                        {
                            SKRect bounds;
                            textPath.GetTightBounds(out bounds);
                            textRegion.SetPath(textPath);
                        }
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

        float MidValue (float accelVal, List<float> list)
        {
            float sum = accelVal;

            for (int i = 0; i < (list.Capacity - 1); i++)
            {
                sum += list[i];
            }
            return sum / list.Capacity;
        }

        void ShiftList (float accelVal, ref List<float> list)
        {
            for (int i = 0; i < list.Capacity; i++)
            {
                if(i < list.Capacity - 1)
                {
                    list[i] = list[i + 1];
                }
                else
                {
                    list[i] = accelVal;
                }
            }
        }
    }
}
