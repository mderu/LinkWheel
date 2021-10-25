using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LinkWheel.Icons;
using LinkWheel.Utils;

namespace LinkWheel
{
    public partial class Form1 : Form
    {
        private Stopwatch Stopwatch { get; set; }

        private const double MaxSeconds = .25d;
        private const int WheelArcWidth = 100;
        private const int DeadSelectionRadius = 50;

        private double T => Math.Min(Stopwatch.Elapsed.TotalSeconds, MaxSeconds) / MaxSeconds;
        private int WheelRadius = 0;
        private Point WheelCenter { get; set; }

        private Bitmap backgroundScreenshot;
        private Screen currentScreen;
        public List<WheelElement> wheelElements = new();

        private Point WheelCenterGlobal { get; set; }

        public Form1(Point wheelCenterGlobal, List<WheelElement> wheelElements)
        {
            this.wheelElements = wheelElements;
            WheelCenterGlobal = wheelCenterGlobal;
            ShowInTaskbar = false;
            InitializeComponent();
            Activate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Empty implementation to prevent the background from being drawn.
            // Previously, this was required so we can overlay, but now we take a screenshot of the entire screen and
            // pretend it's overlaying. Windows basically does this for transparent overlays anyway (does not re-render
            // any screen real estate that is under an even slightly opaque pixel). Probably a slight performance boost
            // if this is empty? ¯\_(ツ)_/¯
        }

        private void Form1_Paint(object sender, PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.DrawImage(backgroundScreenshot, 0, 0);
            Rectangle screenRect = new(0, 0, Size.Width, Size.Height);

            // Painting with T here to fade in slightly.
            g.FillRectangle(new SolidBrush(Color.FromArgb((int)(192.0d * T), 0, 0, 0)), screenRect);

            int closestIndex = 0;
            Point[] wheelCenters = new Point[wheelElements.Count];
            wheelCenters[0] = GetWheelIconCenter(0);
            int[] sqDistances = new int[wheelElements.Count];
            Point localCursorPosition = new(
                Cursor.Position.X - Location.X,
                Cursor.Position.Y - Location.Y
            );

            sqDistances[0] = wheelCenters[0].Subtract(localCursorPosition).GetSquareMagnitude();
            for (int i = 1; i < wheelElements.Count; i++)
            {
                wheelCenters[i] = GetWheelIconCenter(i);
                sqDistances[i] = wheelCenters[i].Subtract(localCursorPosition).GetSquareMagnitude();
                if (sqDistances[closestIndex] > sqDistances[i])
                {
                    closestIndex = i;
                }
            }
            Point diff = WheelCenterGlobal.Subtract(Cursor.Position);
            float mouseAngle = (float)(Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI) + 180.0f;

            Rectangle innerCircleBounds = new(
                WheelCenter.X - WheelRadius - WheelArcWidth / 2,
                WheelCenter.Y - WheelRadius - WheelArcWidth / 2,
                WheelRadius * 2 + WheelArcWidth,
                WheelRadius * 2 + WheelArcWidth
            );
            Rectangle outerCircleBounds = new(
                WheelCenter.X - WheelRadius + WheelArcWidth / 2,
                WheelCenter.Y - WheelRadius + WheelArcWidth / 2,
                WheelRadius * 2 - WheelArcWidth,
                WheelRadius * 2 - WheelArcWidth
            );

            float sweepAngle = 360.0f / wheelElements.Count;
            for (int i = 0; i < wheelElements.Count; i++)
            {
                GraphicsPath gp = new();
                float startAngle = (i - 0.5f) / wheelElements.Count * 360.0f;
                gp.AddArc(outerCircleBounds, startAngle, sweepAngle);
                // Then, draw the inner arc in the reverse direction to allow convex fill.
                gp.AddArc(innerCircleBounds, startAngle + sweepAngle, -sweepAngle);
                Color color;
                if (MathUtils.IsBetweenAngles(mouseAngle, startAngle, startAngle + sweepAngle) 
                    && (localCursorPosition.Subtract(WheelCenter).GetSquareMagnitude() > DeadSelectionRadius * DeadSelectionRadius))
                {
                    color = Color.FromArgb(192, 255, 255, 255);
                }
                else
                {
                    float angleDiff = MathUtils.ShortestAngleDifference(mouseAngle, startAngle + sweepAngle / 2);
                    color = Color.FromArgb(
                        255 - (int)(192.0f * Math.Clamp(MathF.Abs(angleDiff) / 90.0f, 0, 1)),
                        64, 
                        64, 
                        64);
                }
                g.FillPath(new SolidBrush(color), gp);

                g.DrawImage(
                    wheelElements[i].Icon,
                    (int)(wheelCenters[i].X - IconUtils.IconSize / 2.0f),
                    (int)(wheelCenters[i].Y - IconUtils.IconSize / 2.0f),
                    IconUtils.IconSize,
                    IconUtils.IconSize);
            }
        }

        private Point GetWheelIconCenter(int wheelElementIndex)
        {
            return new Point(
                WheelCenter.X + (int)(MathF.Cos((float)wheelElementIndex / wheelElements.Count * MathF.PI * 2) * WheelRadius),
                WheelCenter.Y + (int)(MathF.Sin((float)wheelElementIndex / wheelElements.Count * MathF.PI * 2) * WheelRadius));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            currentScreen = Screen.FromPoint(WheelCenterGlobal);
            WheelRadius = (int)(Math.Min(currentScreen.Bounds.Width, currentScreen.Bounds.Height) * .61f / 2.0f);
            WheelCenter = new Point(
                Math.Clamp(WheelCenterGlobal.X - currentScreen.Bounds.Location.X, WheelRadius + WheelArcWidth / 2, currentScreen.Bounds.Width - WheelRadius - WheelArcWidth / 2),
                Math.Clamp(WheelCenterGlobal.Y - currentScreen.Bounds.Location.Y, WheelRadius + WheelArcWidth / 2, currentScreen.Bounds.Height - WheelRadius - WheelArcWidth / 2)
            );
            // Move the global center to account for the margins.
            WheelCenterGlobal = currentScreen.Bounds.Location.Add(WheelCenter);

            Location = currentScreen.WorkingArea.Location;
            Size = currentScreen.WorkingArea.Size;
            backgroundScreenshot = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(backgroundScreenshot);
            g.CopyFromScreen(Location, new Point(0, 0), backgroundScreenshot.Size);
            DoubleBuffered = true;
            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            TopMost = true;
            Show();
            Activate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Can be improved. Currently, we only want to check if escape is pressed. If the user is typing on their
            // alternate monitor, we want to allow the keypresses through. The current behavior is to only intercept
            // if the mouse re-enters this form, but ideally, we'd only intercept the escape key press.
            //
            // This ideal may change if we support hotkeys for selecting something on the wheel.
            if (e.KeyCode == Keys.Escape)
            {
                TopMost = false;
                Close();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            Point diff = WheelCenterGlobal.Subtract(Cursor.Position);
            float mouseAngle = (float)(Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI) + 180.0f;
            float sweepAngle = 360.0f / wheelElements.Count;
            for (int i = 0; i < wheelElements.Count; i++)
            {
                float startAngle = (i - 0.5f) / wheelElements.Count * 360.0f;
                if (MathUtils.IsBetweenAngles(mouseAngle, startAngle, startAngle + sweepAngle)
                    && (diff.GetSquareMagnitude() > DeadSelectionRadius * DeadSelectionRadius))
                {
                    // Hide the form to make the click feel more responsive. When you open something that takes a long
                    // time to load, like Visual Studio, the wheel stays up on the screen until the process finishes
                    // starting up.
                    Hide();

                    CliUtils.SimpleInvoke(wheelElements[i].CommandAction);
                    break;
                }
            }
            Close();
            // TODO: Might be overkill.
            Application.Exit();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Refresh();
            // If the user is holding down control while clicking on a link, the browser is opened in the background
            // (i.e., the program that had the linked remains the active window). To get around this, we activate the
            // form every frame, as long as the cursor is on the overlayed screen.
            if (currentScreen.WorkingArea.Contains(Cursor.Position))
            {
                Activate();
            }
        }
    }
}
