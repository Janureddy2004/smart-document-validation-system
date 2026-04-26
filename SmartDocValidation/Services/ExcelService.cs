using OfficeOpenXml;

namespace SmartDocValidation.Services
{
    public class ExcelService
    {
        public List<Dictionary<string, string>> ReadExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var result = new List<Dictionary<string, string>>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[0];

                int rowCount = sheet.Dimension.Rows;
                int colCount = sheet.Dimension.Columns;

                var headers = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    headers.Add(sheet.Cells[1, col].Text);
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    var rowData = new Dictionary<string, string>();

                    for (int col = 1; col <= colCount; col++)
                    {
                        rowData[headers[col - 1]] = sheet.Cells[row, col].Text;
                    }

                    result.Add(rowData);
                }
            }

            return result;
        }
    }
}