using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace OnlineMeasurement
{
    /// <summary>Shared dark blue-gray palette for the inspection application.</summary>
    internal static class AppTheme
    {
        internal static readonly Color Window = Color.FromArgb(31, 37, 47);
        internal static readonly Color Surface = Color.FromArgb(39, 49, 63);
        internal static readonly Color RaisedSurface = Color.FromArgb(49, 61, 77);
        internal static readonly Color InputSurface = Color.FromArgb(54, 66, 83);
        internal static readonly Color Text = Color.FromArgb(239, 243, 248);
        internal static readonly Color Accent = Color.FromArgb(88, 175, 255);

        private static readonly Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
        private static readonly ConditionalWeakTable<Control, ThemeHook> HookedControls = new ConditionalWeakTable<Control, ThemeHook>();
        private static readonly ConditionalWeakTable<DataGridView, ThemeHook> HookedGrids = new ConditionalWeakTable<DataGridView, ThemeHook>();
        private static readonly ToolStripRenderer DarkRenderer = new ToolStripProfessionalRenderer(new DarkColorTable());

        internal static void Apply(Form form)
        {
            if (form == null) return;
            form.SuspendLayout();
            form.BackColor = Window;
            form.ForeColor = Text;
            form.Font = GetFont(9F, FontStyle.Regular);
            if (form.ContextMenuStrip != null) ApplyToolStrip(form.ContextMenuStrip);
            foreach (Control control in form.Controls) ApplyControl(control);
            form.ResumeLayout(true);
        }

        internal static Font GetFont(float size, FontStyle style)
        {
            string key = size.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ":" + (int)style;
            Font font;
            if (!Fonts.TryGetValue(key, out font))
            {
                font = new Font("Microsoft YaHei UI", size, style, GraphicsUnit.Point);
                Fonts.Add(key, font);
            }
            return font;
        }

        internal static void ApplyToolStrip(ToolStrip strip)
        {
            if (strip == null) return;
            strip.BackColor = Surface;
            strip.ForeColor = Text;
            strip.Renderer = DarkRenderer;
            foreach (ToolStripItem item in strip.Items)
            {
                item.BackColor = Surface;
                item.ForeColor = Text;
                ToolStripDropDownItem dropDownItem = item as ToolStripDropDownItem;
                if (dropDownItem != null && dropDownItem.DropDown != null) ApplyToolStrip(dropDownItem.DropDown);
            }
        }

        private static void ApplyControl(Control control)
        {
            if (control == null) return;
            Font oldFont = control.Font;
            float size = oldFont == null ? 9F : oldFont.SizeInPoints;
            FontStyle style = oldFont == null ? FontStyle.Regular : oldFont.Style;
            control.Font = GetFont(size, style);
            if (control.ContextMenuStrip != null) ApplyToolStrip(control.ContextMenuStrip);

            LinkLabel linkLabel = control as LinkLabel;
            Label label = control as Label;
            if (linkLabel != null)
            {
                linkLabel.LinkColor = Accent;
                linkLabel.ActiveLinkColor = Color.FromArgb(80, 178, 255);
                linkLabel.VisitedLinkColor = Color.FromArgb(145, 174, 210);
            }
            else if (label != null) ApplyLabel(label);
            else if (control is DataGridView) ApplyGrid((DataGridView)control);
            else if (control is Button) ApplyButton((Button)control);
            else if (control is ComboBox)
            {
                ComboBox comboBox = (ComboBox)control;
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.BackColor = InputSurface;
                comboBox.ForeColor = Text;
            }
            else if (control is TextBoxBase || control is NumericUpDown || control is ListBox || control is ListView || control is TreeView || control is PropertyGrid)
            {
                control.BackColor = InputSurface;
                control.ForeColor = Text;
            }
            else if (control is CheckBox || control is RadioButton)
            {
                control.BackColor = Surface;
                control.ForeColor = Text;
                if (control is CheckBox) ((CheckBox)control).UseVisualStyleBackColor = false;
                if (control is RadioButton) ((RadioButton)control).UseVisualStyleBackColor = false;
            }
            else if (control is GroupBox || control is TabPage || control is Panel || control is FlowLayoutPanel || control is TableLayoutPanel || control is SplitContainer)
            {
                control.BackColor = Surface;
                control.ForeColor = Text;
            }
            else if (!(control is PictureBox)) control.ForeColor = Text;

            ToolStrip toolStrip = control as ToolStrip;
            if (toolStrip != null) ApplyToolStrip(toolStrip);
            // Also theme controls added later by the camera and PLC setup screens.
            ThemeHook hook;
            if (!HookedControls.TryGetValue(control, out hook))
            {
                HookedControls.Add(control, new ThemeHook());
                control.ControlAdded += Control_ControlAdded;
            }
            foreach (Control child in control.Controls) ApplyControl(child);
        }

        private static void Control_ControlAdded(object sender, ControlEventArgs e) { ApplyControl(e.Control); }

        private static void ApplyLabel(Label label)
        {
            Color original = label.BackColor;
            if (original == Color.Transparent || original.A == 0 || original == SystemColors.Control)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = Text;
            }
            else if (IsSemanticColor(original)) label.ForeColor = ContrastingText(original);
            else if (IsLight(original)) label.ForeColor = Color.FromArgb(31, 37, 47);
            else
            {
                label.BackColor = Surface;
                label.ForeColor = Text;
            }
        }

        private static void ApplyButton(Button button)
        {
            Color original = button.BackColor;
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(79, 96, 117);
            button.FlatAppearance.BorderSize = 1;
            if (IsSemanticColor(original))
            {
                button.BackColor = original;
                button.ForeColor = ContrastingText(original);
            }
            else
            {
                button.BackColor = RaisedSurface;
                button.ForeColor = Text;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(63, 79, 99);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(66, 83, 103);
            }
        }

        private static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Window;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Color.FromArgb(63, 75, 92);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = RaisedSurface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = RaisedSurface;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.Font = grid.Font;
            grid.RowHeadersDefaultCellStyle.BackColor = RaisedSurface;
            grid.RowHeadersDefaultCellStyle.ForeColor = Text;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = RaisedSurface;
            grid.RowHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.RowHeadersDefaultCellStyle.Font = grid.Font;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.Font = grid.Font;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(64, 85, 111);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 56, 72);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Text;
            grid.AlternatingRowsDefaultCellStyle.Font = grid.Font;
            grid.RowsDefaultCellStyle.BackColor = Surface;
            grid.RowsDefaultCellStyle.ForeColor = Text;
            grid.RowsDefaultCellStyle.Font = grid.Font;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.DefaultCellStyle.Font = grid.Font;
                column.HeaderCell.Style.Font = grid.Font;
            }
            ThemeHook hook;
            if (!HookedGrids.TryGetValue(grid, out hook))
            {
                HookedGrids.Add(grid, new ThemeHook());
                grid.CellFormatting += Grid_CellFormatting;
            }
        }

        private static void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            Color background = e.CellStyle.BackColor;
            if (background.IsEmpty)
                background = e.RowIndex >= 0 && e.RowIndex % 2 == 1 ? grid.AlternatingRowsDefaultCellStyle.BackColor : grid.DefaultCellStyle.BackColor;
            e.CellStyle.ForeColor = ContrastingText(background);
        }

        private static bool IsLight(Color color) { return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) >= 155; }
        private static Color ContrastingText(Color background) { return IsLight(background) ? Color.FromArgb(31, 37, 47) : Text; }
        private sealed class ThemeHook { }

        private static bool IsSemanticColor(Color color)
        {
            if (color.IsEmpty || color.A == 0) return false;
            return (color.R > 155 && color.G < 115 && color.B < 115)
                || (color.G > 115 && color.R < 125 && color.B < 125)
                || (color.R > 185 && color.G > 150 && color.B < 100);
        }

        private sealed class DarkColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Surface; } }
            public override Color ImageMarginGradientBegin { get { return Surface; } }
            public override Color ImageMarginGradientMiddle { get { return Surface; } }
            public override Color ImageMarginGradientEnd { get { return Surface; } }
            public override Color MenuItemSelected { get { return RaisedSurface; } }
            public override Color MenuItemBorder { get { return Color.FromArgb(82, 99, 119); } }
            public override Color MenuBorder { get { return Color.FromArgb(82, 99, 119); } }
            public override Color ToolStripBorder { get { return Color.FromArgb(63, 75, 92); } }
            public override Color SeparatorDark { get { return Color.FromArgb(67, 80, 97); } }
            public override Color SeparatorLight { get { return Surface; } }
        }
    }
}