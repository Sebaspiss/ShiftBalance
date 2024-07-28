using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ShiftBalance.MVC.Excel
{
    public static class CustomStyleSheet
    {
        /// <summary>
        /// Returns a spreadsheet stylesheet to use to define a cell format, FormatId = 0 means no fill, from 1 to 9 gets different colors
        /// </summary>
        public static Stylesheet Build()
        {
            var stylesheet = new Stylesheet
            {
                Fills = new Fills(),
            };

            // Append FONTS, FILLS, BORDERS & CellFormats objects to stylesheet
            stylesheet.AddChild(GetFonts());
            stylesheet.AddChild(GetFills());
            stylesheet.AddChild(GetBorders());
            stylesheet.AddChild(GetCellFormats());

            return stylesheet;
        }

        // Create a Fonts object
        private static Fonts GetFonts()
        {
            Fonts fonts = new(
                new Font( // Index 0 - default font.
                    new FontSize() { Val = 11 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" }),
            new Font(
                new Bold(),
                new FontSize() { Val = 11 },
                new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                new FontName() { Val = "Calibri" }));
            return fonts;
        }

        // Create a Fills object
        private static Fills GetFills()
        {
            Fills fills = new();
            fills.AppendChild(new Fill() { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // Index 0 - default fill (RESERVED BY EXCEL)
            fills.AppendChild(new Fill() { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // Index 1 - Grey fill (RESERVED BY EXCEL)
            fills.AppendChild(new Fill()
            {
                PatternFill = new PatternFill
                {
                    PatternType = PatternValues.Solid,
                    BackgroundColor = new BackgroundColor { Indexed = 64 },
                    ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("D2042D") }//2- Red
                }
            });

            return fills;
        }

        // Create a Borders object
        private static Borders GetBorders()
        {
            Borders borders = new(
                new Border(), // Index 0 - default border.
                new Border( // Index 1 - applies a border to the left and right sides of a cell.
                    new LeftBorder() { Color = new Color() { Auto = true }, Style = BorderStyleValues.Thin },
                    new RightBorder() { Color = new Color() { Auto = true }, Style = BorderStyleValues.Thin },
                    new TopBorder() { Color = new Color() { Auto = true }, Style = BorderStyleValues.Thin },
                    new BottomBorder() { Color = new Color() { Auto = true }, Style = BorderStyleValues.Thin }
                    ));
            return borders;
        }

        private static CellFormats GetCellFormats()
        {
            // Create a cell format objects and add them to the stylesheet
            CellFormats cellFormats = new(
                new CellFormat(), // Default style : Mandatory, reserved by Excel.
                new CellFormat(), // Grey125 : Mandatory, reserved by Excel.
                new CellFormat { FontId = 1, FillId = 2, BorderId = 1, ApplyBorder = true }, // Style index 2
            new CellFormat { FontId = 1, FillId = 0, BorderId = 1, ApplyFill = true }); // Style index 3

            foreach (CellFormat cellFormat in cellFormats.Cast<CellFormat>())
            {
                cellFormat.AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
            }
            return cellFormats;
        }
    }
}