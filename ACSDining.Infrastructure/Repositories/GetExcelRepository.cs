using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web.Hosting;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using Microsoft.Office.Interop.Excel;

namespace ACSDining.Infrastructure.Repositories
{
    public static class GetExcelRepository
    {
        public static string/*async Task<FileStream>*/ GetExcelFileFromPaimentsModel(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekPaimentDto wpDto)
        {
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            WorkingWeek workWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(wpDto.WeekYearDto);
            int workDayCount = workWeek.WorkingDays.Count(wd => wd.IsWorking);
            int catLength = repository.GetRepositoryAsync<DishType>().GetAll().Count;
            List<UserWeekPaimentDto> paimentList = wpDto.UserWeekPaiments;
            WeekYearDto wyDto = wyDto = wpDto.WeekYearDto;
            //Цены за  каждое блюдо в меню на рабочей неделе
            double[] unitPrices = wpDto.WeekDishPrices;
            int dishcount = workDayCount*catLength;
            //Выделяем память для искомых данных ( +1 для хранения суммы всех ожидаемых проплат)
            double[] unitPricesTotal =
                new double[dishcount + 1];

            for (int i = 0; i < dishcount; i++)
            {
                unitPricesTotal[i] = wpDto.SummaryDishPaiments[i];
            }

            unitPricesTotal[dishcount] = wpDto.SummaryDishPaiments.Sum();

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
                    i += workDayCount * 4 + 3;
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
                    for (int j = 0; j < 5; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            colname = GetExcelColumnName(i + j * 4 + k);
                            workSheet.Cells[2, colname] = dishCategories[k];
                            workSheet.Range[colname + "2"].Orientation = 90;
                        }
                    }
                    colname = GetExcelColumnName(22);
                    workSheet.Cells[3, "C"] = "Цена за одну порцию, грн";
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
                        var striprangestr = string.Format("A{0}:Z{1}", i + j, i + j);
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


                    string headerstr = string.Format("C{0}:Z{1}", 1, 2);
                    workSheet.Range[headerstr].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    string headerusnamesstr = string.Format("A{0}:B{1}", 1, 4);
                    workSheet.Range[headerusnamesstr].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    workSheet.Range[headerusnamesstr].VerticalAlignment = XlVAlign.xlVAlignCenter;
                    string usernames = string.Format("A{0}:B{1}", 5, paimentList.Count + 5);
                    workSheet.Range[usernames].HorizontalAlignment = XlHAlign.xlHAlignRight;
                    string userpaistr = string.Format("C{0}:Z{1}", 4, paimentList.Count + 5);
                    workSheet.Range[userpaistr].NumberFormat = "#,##0.00";
                    workSheet.Range[userpaistr].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    string allstr = string.Format("A{0}:Z{1}", 1, paimentList.Count + 5);
                    workSheet.Range[allstr].Columns.AutoFit();
                    // Define filename
                    //string fileName = string.Format(@"{0}\ExcelData.xlsx",
                    //    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                    //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                    //                          @"ACSDining.Web\App_Data\DBinitial\Paiments.xlsx";

                    string _path = HostingEnvironment.MapPath("~/ExcelFiles/Paiments.xlsx");
                    // Save this data as a file
                    excel.DisplayAlerts = false;
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
    }
}
