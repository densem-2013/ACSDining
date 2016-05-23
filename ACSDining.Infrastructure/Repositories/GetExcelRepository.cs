using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using Microsoft.Office.Interop.Excel;
using Bytescout.Spreadsheet;
using Bytescout.Spreadsheet.Constants;
using Font = System.Drawing.Font;
using Range = Bytescout.Spreadsheet.Range;
using Worksheet = Bytescout.Spreadsheet.Worksheet;

namespace ACSDining.Infrastructure.Repositories
{
    public static class GetExcelRepository
    {
        #region Paiments

         
        public static string/*async Task<FileStream>*/ GetExcelFileFromPaimentsModel(this IRepositoryAsync<WeekOrderMenu> repository,
            ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(repository.GetRepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            WorkingWeek workWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(feDto.WeekYear);
            int workDayCount = workWeek.WorkingDays.Count(wd => wd.IsWorking);
            int catLength = repository.GetRepositoryAsync<DishType>().GetAll().Count;
            List<UserWeekPaimentDto> paimentList = dto.UserWeekPaiments;
            WeekYearDto wyDto = dto.WeekYearDto;
            //Цены за  каждое блюдо в меню на рабочей неделе
            double[] unitPrices = dto.WeekDishPrices;
            int dishcount = workDayCount*catLength;
            //Выделяем память для искомых данных ( +1 для хранения суммы всех ожидаемых проплат)
            double[] unitPricesTotal =
                new double[dishcount + 1];

            for (int i = 0; i < dishcount; i++)
            {
                unitPricesTotal[i] = dto.SummaryDishPaiments[i];
            }

            unitPricesTotal[dishcount] = dto.SummaryDishPaiments.Sum();

            // Load Excel application
            Application excel = new Application();

            // Create empty workbook
            excel.Workbooks.Add();

            // Create Worksheet from active sheet
            _Worksheet workSheet = excel.ActiveSheet as _Worksheet;

            // I created Application and Worksheet objects before try/catch,
            // so that i can close them in finnaly block.
            // It's IMPORTANT to release these COM objects!!
            try
            {
                // ------------------------------------------------
                // Creation of header cells
                // ------------------------------------------------
                if (workSheet != null)
                {
                    workSheet.Cells[1, "A"] = "№";
                    workSheet.Range["A1:A4"].Merge();
                    workSheet.Cells[1, "B"] = "Ф.И.О.";
                    workSheet.Range["B1:B4"].Merge();
                    int i = 0;
                    string str;
                    string colname;
                    string colname_2;
                    for (int[] j = { 0 }; j[0] < workDayCount; j[0]++)
                    {
                        colname = GetExcelColumnName(j[0] * catLength + 3);
                        colname_2 = GetExcelColumnName(j[0] * catLength + 6);
                        var elementAtOrDefault = workWeek.WorkingDays.Where(wd=>wd.IsWorking).ElementAtOrDefault(j[0]);
                        if (elementAtOrDefault != null)
                            workSheet.Cells[1, colname] = elementAtOrDefault.DayOfWeek.Name;
                        str = String.Format("{0}1:{1}1", colname, colname_2);
                        workSheet.Range[str].Merge();
                    }
                    i += dishcount + 3;
                    colname = GetExcelColumnName(i);
                    workSheet.Cells[1, colname] = "Сумма за неделю";
                    str = String.Format("{0}1:{1}4", colname, colname);
                    workSheet.Range[str].Merge();
                    workSheet.Range[str].Orientation = 90;
                    i++;
                    colname = GetExcelColumnName(i);
                    workSheet.Cells[1, colname] = "Оплата за неделю";
                    str = String.Format("{0}1:{1}4", colname, colname);
                    workSheet.Range[str].Merge();
                    workSheet.Range[str].Orientation = 90;
                    i++;
                    colname = GetExcelColumnName(i);
                    workSheet.Cells[1, colname] = "Баланс";
                    str = String.Format("{0}1:{1}4", colname, colname);
                    workSheet.Range[str].Merge();
                    workSheet.Range[str].Orientation = 90;
                    i++;
                    colname = GetExcelColumnName(i);
                    workSheet.Cells[1, colname] = "Примечание";
                    str = String.Format("{0}1:{1}4", colname, colname);
                    workSheet.Range[str].Merge();
                    workSheet.Range[str].Orientation = 90;

                    i = 3;
                    for (int j = 0; j < workDayCount; j++)
                    {
                        for (int k = 0; k < catLength; k++)
                        {
                            colname = GetExcelColumnName(i + j * catLength + k);
                            workSheet.Cells[2, colname] = dishCategories[k];
                            workSheet.Range[colname + "2"].Orientation = 90;
                        }
                    }
                    colname = GetExcelColumnName(dishcount + 2);
                    workSheet.Cells[i, "C"] = "Цена за одну порцию, грн";
                    str = String.Format("C3:{0}3", colname);
                    workSheet.Range[str].Merge();
                    workSheet.Range[str].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    double[] dishprices = unitPrices;
                    for (int j = 0; j < dishcount; j++)
                    {
                        colname = GetExcelColumnName(i + j);
                        workSheet.Cells[4, colname] = dishprices[j];
                    }
                    i = 5;
                    string endcolname = GetExcelColumnName(dishcount + 6);
                    for (int j = 0; j < paimentList.Count; j++)
                    {
                        UserWeekPaimentDto userpai = paimentList[j];
                        workSheet.Cells[i + j, "A"] = j + 1;
                        workSheet.Cells[i + j, "B"] = userpai.UserName;

                        for (int k = 0; k < workDayCount * catLength; k++)
                        {

                            colname = GetExcelColumnName(k  + 3);
                            workSheet.Cells[i + j, colname] = userpai.WeekPaiments[k];
                        }

                        colname = GetExcelColumnName(dishcount + 3);
                        workSheet.Cells[i + j, colname] = paimentList[j].WeekPaiments[workDayCount * catLength];
                        colname = GetExcelColumnName(dishcount + 4);
                        workSheet.Cells[i + j, colname] = paimentList[j].Paiment;
                        colname = GetExcelColumnName(dishcount + 5);
                        workSheet.Cells[i + j, colname] = paimentList[j].Balance;
                        colname = GetExcelColumnName(dishcount + 6);
                        workSheet.Cells[i + j, colname] = paimentList[j].Note;

                        if ((i + j)%2 == 0) continue;
                        var striprangestr = string.Format("A{0}:{2}{1}", i + j, i + j, endcolname);
                        workSheet.Range[striprangestr].Interior.Color = XlRgbColor.rgbLightSteelBlue;
                    }
                    i += paimentList.Count;
                    workSheet.Cells[i, "A"] = "Итого";
                    str = String.Format("A{0}:B{1}", i, i);
                    workSheet.Range[str].Merge();
                    for (int j = 0; j < dishcount; j++)
                    {
                        colname = GetExcelColumnName(j + 3);
                        workSheet.Cells[i, colname] = unitPricesTotal[j];
                    }
                    colname = GetExcelColumnName(dishcount+3);
                    workSheet.Cells[i, colname] = unitPricesTotal[dishcount];
                    colname = GetExcelColumnName(dishcount + 4);
                    workSheet.Cells[i, colname] = paimentList.Sum(up => up.Paiment);
                    colname = GetExcelColumnName(dishcount + 5);
                    workSheet.Cells[i, colname] = paimentList.Sum(up => up.Balance);

                    string headerstr = string.Format("C{0}:{2}{1}", 1, 2, endcolname);
                    workSheet.Range[headerstr].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    string headerusnamesstr = string.Format("A{0}:B{1}", 1, 4);
                    workSheet.Range[headerusnamesstr].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    workSheet.Range[headerusnamesstr].VerticalAlignment = XlVAlign.xlVAlignCenter;
                    string usernames = string.Format("A{0}:B{1}", 5, paimentList.Count + 5);
                    workSheet.Range[usernames].HorizontalAlignment = XlHAlign.xlHAlignRight;
                    string userpaistr = string.Format("C{0}:{2}{1}", 4, paimentList.Count + 5, endcolname);
                    workSheet.Range[userpaistr].NumberFormat = "#,##0.00";
                    workSheet.Range[userpaistr].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    string allstr = string.Format("A{0}:{2}{1}", 1, paimentList.Count + 5, endcolname);
                    workSheet.Range[allstr].Columns.AutoFit();
                    // Define filename
                    //string fileName = string.Format(@"{0}\ExcelData.xlsx",
                    //    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                    //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                    //                          @"ACSDining.Web\App_Data\DBinitial\Paiments.xlsx";

                    string _path = HostingEnvironment.MapPath("~/ExcelFiles/Paiments.xlsx");
                    // Save this data as a file
                    excel.DisplayAlerts = false;
                    // delete output file if exists already
                    if (File.Exists(_path)){

                        File.Delete(_path);
                    }

                    workSheet.SaveAs(_path);
                    //Task savetask = Task.Run(() =>
                    //{
                    //    workSheet.SaveAs(_path);
                    //    // Quit Excel application
                    //    excel.Quit();

                    //    //    // Release COM objects (very important!)
                    //    Marshal.ReleaseComObject(excel);

                    //    //    if (workSheet != null)
                    //    Marshal.ReleaseComObject(workSheet);

                    //    //    // Empty variables

                    //    //    // Force garbage collector cleaning
                    //    GC.Collect();
                    //    //foreach (var process in Process.GetProcessesByName("EXCEL.EXE"))
                    //    //{
                    //    //    process.Kill();
                    //    //}
                    //});
                    //await savetask;
                    //FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    //return await Task.FromResult(fs);
                    return _path;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                // Quit Excel application
                excel.Quit();

                // Release COM objects (very important!)
                Marshal.ReleaseComObject(excel);

                if (workSheet != null)
                    Marshal.ReleaseComObject(workSheet);

                // Empty variables

                // Force garbage collector cleaning
                GC.Collect();
            }
            return null;
        }
    

    private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1)%26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo)/26;
            }

            return columnName;
        }
        #endregion

        #region Orders

        public static string GetFactOrdersExcelFileWeekYearDto(this IRepositoryAsync<WeekOrderMenu> repository,
            ForExcelDataDto feDto)
    {
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            List<WeekOrderMenu> weekOrderMenus = repository.OrdersMenuByWeekYear(feDto.WeekYear);
            List<UserWeekOrderDto> userWeekOrders =
                weekOrderMenus.Select(woDto => UserWeekOrderDto.MapDto(repository.Context, woDto)).ToList();
            string[] dayNames = repository.Context.GetDayNames(feDto.WeekYear, true).Result;
            double[] weekDishPrices = repository.Context.GetWeekDishPrices(feDto.WeekYear).Result;
            double[] summaryDishQuantities = repository.Context.GetFactSumWeekUserCounts(feDto.WeekYear).Result;

            WorkingWeek workWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(feDto.WeekYear);
            int workDayCount = workWeek.WorkingDays.Count(wd => wd.IsWorking);
            int catLength = repository.GetRepositoryAsync<DishType>().GetAll().Count;

            int dishcount = workDayCount * catLength;
            int orderscount = userWeekOrders.Count;
            // Create new Spreadsheet
            Spreadsheet document = new Spreadsheet();

            // Get worksheet by name
            Worksheet worksheet = document.Workbook.Worksheets.Add("Заявки фактические");
            string titlerang=String.Format("A1:{0}1",GetExcelColumnName(dishcount+3));
            Range range = worksheet.Range(titlerang);
            range.Merge();
            worksheet.Cell("A1").MergedWithCell.Value = "Заявки фактические " + feDto.DataString;
            range.AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Cell(2, 0).Value = "№";
            worksheet.Range("A2:A5").Merge();
            worksheet.Cell(2, 1).Value = "Ф.И.О.";
            worksheet.Range("B2:B5").Merge();
            int i = 0;
            string str;
            string colname;
            string colname_2;
            for (int[] j = { 0 }; j[0] < workDayCount; j[0]++)
            {
                colname = GetExcelColumnName(j[0] * catLength + 3);
                colname_2 = GetExcelColumnName(j[0] * catLength + 6);
                var elementAtOrDefault = workWeek.WorkingDays.Where(wd => wd.IsWorking).ElementAtOrDefault(j[0]);
                if (elementAtOrDefault != null)
                    worksheet.Cell(1, j[0] * catLength + 3).Value = elementAtOrDefault.DayOfWeek.Name;
                str = String.Format("{0}2:{1}2", colname, colname_2);
                worksheet.Range(str).Merge();
            }
            i += dishcount + 3; 
            colname = GetExcelColumnName(i);
            worksheet.Cell(1, i-1).Value = "Стоимость заказа за неделю";
            str = String.Format("{0}2:{1}5", colname, colname);
            worksheet.Range(str).Merge();
            //worksheet.Range(str).Rotation = 90;
            worksheet.Range(str).Wrap = true;
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Columns[i - 1].Width = 100;
            worksheet.Cell(1, i - 1).ShrinkToFit = true;

            i = 2;
            for (int j = 0; j < workDayCount; j++)
            {
                for (int k = 0; k < catLength; k++)
                {
                    colname = GetExcelColumnName(i+1 + j * catLength + k);
                    worksheet.Cell(2, i + j * catLength + k).Value = dishCategories[k];
                    worksheet.Range(colname + "3").Rotation = 90;
                }
            }
            colname = GetExcelColumnName(dishcount + 2);
            worksheet.Cell(i +1, 3).Value = "Цена за одну порцию, грн";
            str = String.Format("C4:{0}4", colname);
            worksheet.Range(str).Merge();
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;
            for (int j = 0; j < dishcount; j++)
            {
                worksheet.Cell(4, i + j).Value = weekDishPrices[j];
            }
            str = string.Format("A1:{0}5", GetExcelColumnName(dishcount + 3));
           // worksheet.Range(str).Font=new Font("Arial",12,FontStyle.Bold);
            i = 5;
            string endcolname = GetExcelColumnName(dishcount + 3);
            Color contentColor = Color.FromArgb(24, 107, 208);
            bool itsevenrow = false;
            for (int j = 0; j < userWeekOrders.Count; j++)
            {
                itsevenrow = (i + j)%2 == 0;
                UserWeekOrderDto userweekorder = userWeekOrders[j];
                worksheet.Cell(i + j, 0).Value = j + 1;
                worksheet.Cell(i + j, 1).Value = userweekorder.UserName;
                if (itsevenrow) worksheet.Cell(i + j, 1).FillPatternBackColor = contentColor;
                worksheet.Cell(i + j, 1).ShrinkToFit = true;
                for (int k = 0; k < dishcount+1; k++)
                {
                    worksheet.Cell(i + j, k + 2).Value = userweekorder.UserWeekOrderDishes[k];
                    if (itsevenrow) worksheet.Cell(i + j, k + 2).FillPatternBackColor = contentColor;
                }
                
                //if ((i + j) % 2 == 0) continue;
                //var striprangestr = string.Format("A{0}:{2}{1}", i + j, i + j, endcolname);
                //worksheet.Range(striprangestr).FillPatternForeColor = contentColor;
                //worksheet.Rows[i + j].FillPatternBackColor = contentColor;
            }
            i += userWeekOrders.Count;
            worksheet.Cell(i, 0).Value = "Всего заказано";
            str = String.Format("A{0}:B{1}", i+1, i+1);
            worksheet.Range(str).Merge();
            for (int j = 0; j < dishcount; j++)
            {
                worksheet.Cell(i, j+2).Value = summaryDishQuantities[j];
            }
            worksheet.Cell(i, dishcount + 2).Value = userWeekOrders.Sum(uo=>uo.UserWeekOrderDishes[dishcount]);
            //str = String.Format("{2}{0}:{2}{1}", i + 1, userWeekOrders.Count + 6, endcolname);
            //worksheet.Range(str).
           // worksheet.Range(str).Font = new Font("Arial", 12, FontStyle.Bold);

            string headerstr = string.Format("C{0}:{2}{1}", 1, 3, endcolname);
            worksheet.Range(headerstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string headerusnamesstr = string.Format("A{0}:B{1}", 1, 5);
            worksheet.Range(headerusnamesstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Range(headerusnamesstr).AlignmentVertical = AlignmentVertical.Centered;
            string usernames = string.Format("A{0}:B{1}", 5, userWeekOrders.Count + 6);
            worksheet.Range(usernames).AlignmentHorizontal = AlignmentHorizontal.Left;
            string userquantistr = string.Format("C{0}:{2}{1}", 5, userWeekOrders.Count + 6, endcolname);
            worksheet.Range(userquantistr).NumberFormatString = "0.0";
            worksheet.Range(userquantistr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string sumcol = string.Format("{0}{1}:{2}{3}", endcolname, 5, endcolname, userWeekOrders.Count + 6);
            worksheet.Range(sumcol).NumberFormatString = "#,###.00";
            worksheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string allstr = string.Format("A{0}:{2}{1}", 1, userWeekOrders.Count + 6, endcolname);
            worksheet.Columns[0].Width = 30;
            worksheet.Columns[1].Width = 150;
            worksheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).Font = new Font("Arial", 12, FontStyle.Bold);

           // worksheet.AutoFitRows();
            string _path = HostingEnvironment.MapPath("~/ExcelFiles/Orders.xlsx");
            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //                          @"ACSDining.Web\ExcelFiles\Orders.xlsx";
            // Save this data as a file
            //worksheet.DisplayAlerts = false;
            // delete output file if exists already
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            document.SaveAs(_path);

            // Close document
            document.Close();

            return _path;
        }

        //public static string GetTitleString(string title, string datastring)
        //{
        //    StringBuilder bilder=new StringBuilder();
        //    bilder.Append(title);
        //    var  firs = wweek.WorkingDays.OrderBy(wd => wd.DayOfWeek.Id).FirstOrDefault(wd => wd.IsWorking);
        //    string firstworkday=null;
        //    if ( firs != null)
        //    {
        //        firstworkday =
        //             firs.DayOfWeek.Name;
        //    }
        //    string lastworkday = null;
        //    var last= wweek.WorkingDays.OrderBy(wd => wd.DayOfWeek.Id).LastOrDefault(wd => wd.IsWorking);
        //    if (last!=null)
        //    {
        //        lastworkday = last.DayOfWeek.Name;
        //    }
        //    bilder.AppendFormat(" {0} - {1}{2}", firstworkday, lastworkday, wweek.Year.YearNumber);
        //    return bilder.ToString();
        //}
        #endregion
    }
}
