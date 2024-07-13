using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public class OverlayForm : Form
    {
        private Panel[] panels;
        private PictureBox[] pictureBoxes;

        private int panelHight = 3;
        private int panelWidth = 3;
        private int panelAll = 9;

        public OverlayForm(int currentDesktop)
        {
            // Initialize the overlay form
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.Opacity = 0.3; // Set opacity to make the form semi-transparent
            this.Width = 800;
            this.Height = 800;
            this.TopMost = true;

            panelHight = VWinMover.Properties.Settings.Default.window_height;
            panelWidth = VWinMover.Properties.Settings.Default.window_width;
            panelAll = panelHight * panelWidth;

            // Create and initialize the grid
            InitializeGrid();

            // Highlight the current desktop
            HighlightCurrentDesktop(currentDesktop);
        }

        private void InitializeGrid()
        {
            // Create the TableLayoutPanel
            var tableLayoutPanel = new TableLayoutPanel
            {
                RowCount = VWinMover.Properties.Settings.Default.window_height,
                ColumnCount = VWinMover.Properties.Settings.Default.window_width,
                Dock = DockStyle.Fill
            };

            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));

            panels = new Panel[panelAll];
            pictureBoxes = new PictureBox[panelAll];

            // Create PictureBox controls and set images from resources
            for (int i = 0; i < pictureBoxes.Length; i++)
            {
                string resourceName = $"_{i + 1}img";
                pictureBoxes[i] = 
                    CreatePictureBox((Image)VWinMover.Properties.Resources.ResourceManager
                        .GetObject(resourceName));
            }

            for (int i = 0; i < panelAll; i++)
            {
                panels[i] = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10) // Optional: Add padding for better visibility
                };
                panels[i].Controls.Add(pictureBoxes[i]);
            }

            // Add Panel controls to the TableLayoutPanel
            int winTotal = 0;
            for (int row = 0; row < panelHight; row++)
            {
                for (int column = 0; column < panelWidth; column++)
                {
                    tableLayoutPanel.Controls.Add(panels[winTotal++], column, row);
                }
            }

            // Add the TableLayoutPanel to the form
            this.Controls.Add(tableLayoutPanel);
        }

        private PictureBox CreatePictureBox(Image image)
        {
            var pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = image
            };
            return pictureBox;
        }

        public void HighlightCurrentDesktop(int nextId)
        {
            
            // Ensure the index is within the valid range
            if (nextId < 0 || nextId > 8) return;

            // Highlight the selected desktop by changing the panel's background color
            for (int i = 0; i < panels.Length; i++)
            {
                if (i == nextId)
                {
                    panels[i].BackColor = Color.Yellow; // Highlight color
                }
                else
                {
                    panels[i].BackColor = Color.Transparent; // Default color
                }
            }
        }
    }
}
