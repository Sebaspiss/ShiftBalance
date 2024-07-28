using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.Excel
{
    public class ShiftWorksheet
    {
        private List<Employee> _workers;
        private MergeCells _cellsToMerge;
        private ShiftMatrix _availability;
        private ShiftMatrix _openings;
        private ShiftMatrix _closeings;
        private Dictionary<int, DateTime> _calendarMap;

        protected uint styleIndex = 3;
        protected uint styleRedIndex = 2;

        private const int ROW_MONTHS_INDEX = 1;
        private const int ROW_WEEKDAYS_INDEX = 2;
        private const int ROW_NUMBERS_INDEX = 3;
        private const int ROW_START_INDEX = 4;

        private const int CLM_WORKERS_INDEX = 1;
        private const int CLM_START_INDEX = 2;

        public ShiftWorksheet(List<Employee> workers, ShiftMatrix openings, ShiftMatrix closeings, ShiftMatrix availability, Dictionary<int, DateTime> calendar)
        {
            _workers = workers;
            _calendarMap = calendar;
            _availability = availability;
            _openings = openings;
            _closeings = closeings;
            _cellsToMerge = new MergeCells();
        }

        public void Generate(string fileFullname)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(fileFullname, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new();
                worksheetPart.Worksheet = new Worksheet(sheetData);
                workbookpart.AddNewPart<WorkbookStylesPart>();
                workbookpart.WorkbookStylesPart.Stylesheet = CustomStyleSheet.Build();
                workbookpart.WorkbookStylesPart.Stylesheet.Save();

                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Plan"
                };
                sheetData.Append(GetColumns());
                sheetData.Append(GetRows());
                sheets.Append(sheet);

                if (_cellsToMerge.ChildElements.Count > 0)
                {
                    worksheetPart.Worksheet.InsertAfter(_cellsToMerge, worksheetPart.Worksheet.Elements<SheetData>().First());
                }
                worksheetPart.Worksheet.Save();
                workbookpart.Workbook.Save();
                spreadsheetDocument.Dispose();
            }
        }

        // Returns three spreadsheet rows, one for months, one for weekDays and the other for daysNumber
        private List<OpenXmlElement> GetColumns()
        {
            List<OpenXmlElement> columns = new List<OpenXmlElement>();

            Row rowMonth = new() { RowIndex = ROW_MONTHS_INDEX };
            int startClmIndex = CLM_START_INDEX;
            int clmMonthIndex = CLM_START_INDEX;
            uint styleIndex = 0;
            string cellAppRef;

            // Months
            for (int i = 0; i < _calendarMap.Count; i++)
            {
                cellAppRef = ColumnMapper.GetColumnNameFromNumber(clmMonthIndex) + rowMonth.RowIndex;
                rowMonth.Append(GetCell(cellAppRef, _calendarMap[i].ToString("MMMM"), styleIndex));

                if (i == _calendarMap.Count -1 || _calendarMap[i].Month != _calendarMap[i + 1].Month)
                {
                    AddMerge(startClmIndex, clmMonthIndex, ROW_MONTHS_INDEX, ROW_MONTHS_INDEX);
                    startClmIndex = clmMonthIndex + 1;

                }
                clmMonthIndex++;
            }
            columns.Add(rowMonth);

            clmMonthIndex = CLM_START_INDEX;
            Row rowWeekday = new() { RowIndex = ROW_WEEKDAYS_INDEX };

            // WeekDays
            for (int j = 0; j < _calendarMap.Count; j++)
            {
                cellAppRef = ColumnMapper.GetColumnNameFromNumber(clmMonthIndex) + rowWeekday.RowIndex;
                styleIndex = GetStyleIndex(_calendarMap[j].DayOfWeek);
                rowWeekday.Append(GetCell(cellAppRef, _calendarMap[j].ToString("ddd"), styleIndex));
                clmMonthIndex++;
            }
            columns.Add(rowWeekday);

            clmMonthIndex = CLM_START_INDEX;
            Row rowDay = new() { RowIndex = ROW_NUMBERS_INDEX };

            // Day
            for (int j = 0; j < _calendarMap.Count; j++)
            {
                cellAppRef = ColumnMapper.GetColumnNameFromNumber(clmMonthIndex) + rowDay.RowIndex;
                styleIndex = GetStyleIndex(_calendarMap[j].DayOfWeek);
                rowDay.Append(GetCell(cellAppRef, _calendarMap[j].ToString("dd"), styleIndex));
                clmMonthIndex++;
            }
            columns.Add(rowDay);

            return columns;
        }

        // Returns all workers schedule
        private List<OpenXmlElement> GetRows()
        {
            List<OpenXmlElement> rows = new List<OpenXmlElement>();
            string cellReference;
            string cellValue;
            int clmDayIndex = CLM_START_INDEX;

            for (int i = 0; i < _availability.NumberOfEmployees; i++)
            {
                Row employeeRow = new() { RowIndex = ROW_START_INDEX + (uint)i };

                cellReference = ColumnMapper.GetColumnNameFromNumber(CLM_WORKERS_INDEX) + employeeRow.RowIndex;
                string name = string.Format("{0} {1}", _workers[i].Name, _workers[i].Surname);
                Cell employeeName = GetCell(cellReference, name, 0);
                employeeRow.Append(employeeName);

                for (int j = CLM_START_INDEX; j < CLM_START_INDEX + _calendarMap.Count; j++)
                {
                    Cell employeeDay;
                    
                    cellReference = ColumnMapper.GetColumnNameFromNumber(clmDayIndex) + employeeRow.RowIndex;
                    styleIndex = GetStyleIndex(_calendarMap[j-CLM_START_INDEX].DayOfWeek);

                    if (styleIndex == 3)
                    {
                        if (_openings.Matrix[i, j - CLM_START_INDEX] == 1)
                        {
                            cellValue = "A";
                        }
                        else if (_closeings.Matrix[i, j - CLM_START_INDEX] == 1)
                        {
                            cellValue = "C";
                        }
                        else
                        {
                            cellValue = "";
                        }
                        employeeDay = GetCell(cellReference, cellValue, styleIndex);
                    }
                    else
                    {
                        if (_availability.Matrix[i, j - CLM_START_INDEX] == 1)
                        {
                            cellValue = "R";
                        }
                        else
                        {
                            cellValue = "";
                        }
                        employeeDay = GetCell(cellReference, cellValue, styleIndex);
                    }
                    employeeRow.Append(employeeDay);
                    clmDayIndex++;
                }
                clmDayIndex = CLM_START_INDEX;
                rows.Add(employeeRow);
            }
            return rows;
        }

        // Returns a spreadsheet cell at a specified index (example: B4), the value that has to contain and the styleIndex that calls a specific format in the stylesheet
        protected static Cell GetCell(string reference, string value, uint styleIndex)
        {
            return new Cell()
            {
                CellReference = reference,
                DataType = CellValues.String,
                CellValue = new CellValue(value),
                StyleIndex = styleIndex
            };
        }

        // Merge cells
        protected void AddMerge(int startClmIndex, int endClmIndex, int startRowIndex, int endRowIndex)
        {
            string startCell = $"{ColumnMapper.GetColumnNameFromNumber(startClmIndex)}{startRowIndex}";
            string endCell = $"{ColumnMapper.GetColumnNameFromNumber(endClmIndex)}{endRowIndex}";
            _cellsToMerge.Append(new MergeCell()
            {
                Reference = new StringValue($"{startCell}:{endCell}")
            });
        }

        // gets a number based of DayOfWeek
        protected uint GetStyleIndex(DayOfWeek dayOfWeek)
        {
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                styleIndex = 2;
            }
            else
            {
                styleIndex = 3;
            }
            return styleIndex;
        }
    }
}
