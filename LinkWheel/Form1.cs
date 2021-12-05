using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CoreAPI.Config;
using CoreAPI.Icons;
using CoreAPI.Models;
using CoreAPI.Utils;
using LinkWheel.Properties;

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
        public List<IdelAction> actions = new();

        private int lastHoveredIndex = -1;
        private double lastHoverStartTime = 0;

        private Point WheelCenterGlobal { get; set; }

        public Form1(Point wheelCenterGlobal, List<IdelAction> actions)
        {
            this.actions = actions;
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

            Point localCursorPosition = new(
                Cursor.Position.X - Location.X,
                Cursor.Position.Y - Location.Y
            );

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

            float sweepAngle = 360.0f / actions.Count;
            int selectedAction = -1;
            for (int i = 0; i < actions.Count; i++)
            {
                GraphicsPath gp = new();
                float startAngle = (i - 0.5f) / actions.Count * 360.0f;
                Color color;
                Rectangle inner = innerCircleBounds;
                Rectangle outer = outerCircleBounds;
                int radius = WheelRadius;
                if (MathUtils.IsBetweenAngles(mouseAngle, startAngle, startAngle + sweepAngle) 
                    && (localCursorPosition.Subtract(WheelCenter).GetSquareMagnitude() > DeadSelectionRadius * DeadSelectionRadius))
                {
                    selectedAction = i;
                    
                    if (lastHoveredIndex != selectedAction)
                    {
                        lastHoverStartTime = Stopwatch.Elapsed.TotalSeconds;
                    }

                    color = Color.FromArgb(192, 0, 192, 255);

                    radius += (int)Math.Clamp(Math.Sqrt(Stopwatch.Elapsed.TotalSeconds - lastHoverStartTime) * 30, 0, 10);
                    inner = new(
                        WheelCenter.X - radius - WheelArcWidth / 2,
                        WheelCenter.Y - radius - WheelArcWidth / 2,
                        radius * 2 + WheelArcWidth,
                        radius * 2 + WheelArcWidth
                    );
                    outer = new(
                        WheelCenter.X - radius + WheelArcWidth / 2,
                        WheelCenter.Y - radius + WheelArcWidth / 2,
                        radius * 2 - WheelArcWidth,
                        radius * 2 - WheelArcWidth
                    );
                }
                else
                {
                    float angleDiff = MathUtils.ShortestAngleDifference(mouseAngle, startAngle + sweepAngle / 2);
                    color = Color.FromArgb(
                        255 - (int)(192.0f * Math.Clamp(MathF.Abs(angleDiff) / 90, 0, 1)),
                        96, 
                        96, 
                        96);
                }
                gp.AddArc(outer, startAngle, sweepAngle);
                // Then, draw the inner arc in the reverse direction to allow convex fill.
                gp.AddArc(inner, startAngle + sweepAngle, -sweepAngle);
                g.FillPath(new SolidBrush(color), gp);

                Bitmap icon = actions[i].Icon ?? Resources.MissingIcon;

                Point center = GetWheelIconCenter((float)i / actions.Count, radius);

                if (actions[i].IconSecondary is null)
                {
                    g.DrawImage(
                        icon,
                        (int)(center.X - IconUtils.IconSize / 2.0f),
                        (int)(center.Y - IconUtils.IconSize / 2.0f),
                        IconUtils.IconSize,
                        IconUtils.IconSize);
                }
                else
                {
                    g.DrawImage(
                        IconUtils.Compose(IconUtils.RoundCorners(icon), actions[i].IconSecondary),
                        (int)(center.X - IconUtils.IconSize / 2.0f),
                        (int)(center.Y - IconUtils.IconSize / 2.0f),
                        IconUtils.IconSize,
                        IconUtils.IconSize);
                }
            }

            if (selectedAction != -1)
            {
                titleLabel.Text = actions[selectedAction].Title;
                descriptionLabel.Text = actions[selectedAction].Description;
            }
            lastHoveredIndex = selectedAction;
        }

        private Point GetWheelIconCenter(float percentile, float radius)
        {
            return new Point(
                WheelCenter.X + (int)(MathF.Cos(percentile * MathF.PI * 2) * radius),
                WheelCenter.Y + (int)(MathF.Sin(percentile * MathF.PI * 2) * radius));
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

            titleLabel.Location = new(WheelCenter.X - titleLabel.Size.Width / 2, WheelCenter.Y - titleLabel.Size.Height - titleLabel.Margin.Bottom / 2);
            descriptionLabel.Location = new(WheelCenter.X - descriptionLabel.Size.Width / 2, WheelCenter.Y + descriptionLabel.Margin.Top / 2 - descriptionLabel.Margin.Bottom / 2);

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
            // Close if the user right clicked, middle clicked, or clicked some other way.
            if (e.Button != MouseButtons.Left)
            {
                TopMost = false;
                Close();
            }
            Point diff = WheelCenterGlobal.Subtract(Cursor.Position);
            float mouseAngle = (float)(Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI) + 180.0f;
            float sweepAngle = 360.0f / actions.Count;
            for (int i = 0; i < actions.Count; i++)
            {
                float startAngle = (i - 0.5f) / actions.Count * 360.0f;
                if (MathUtils.IsBetweenAngles(mouseAngle, startAngle, startAngle + sweepAngle)
                    && (diff.GetSquareMagnitude() > DeadSelectionRadius * DeadSelectionRadius))
                {
                    // Hide the form to make the click feel more responsive. When you open something that takes a long
                    // time to load, like Visual Studio, the wheel stays up on the screen until the process finishes
                    // starting up.
                    Hide();

                    CliUtils.SimpleInvoke(actions[i].Command, actions[i].CommandWorkingDirectory);
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
