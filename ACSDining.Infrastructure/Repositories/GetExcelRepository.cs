using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using Microsoft.Office.Interop.Excel;

namespace ACSDining.Infrastructure.Repositories
{
    public static class GetExcelRepository
    {
        public static string GetExcelFileFromPaimentsModel(this IRepositoryAsync<WeekOrderMenu> repository,
            List<UserWeekPaimentDto> paimentList, string[] dishCategories, WorkingWeek workingWeek)
        {

            int workDayCount = workingWeek.WorkingDays.Count;
            int catLength = dishCategories.Length;
            WeekYearDto wyDto = null;
            var userWeekPaimentDto = paimentList.FirstOrDefault();
            if (userWeekPaimentDto != null) wyDto = userWeekPaimentDto.WeekYear;
            //Цены за  каждое блюдо в меню на рабочей неделе
            double[] unitPrices =
                repository.GetRepositoryAsync<MenuForWeek>()
                    .GetWeekMenuByWeekYear(wyDto)
                    .MenuForDay.SelectMany(mfd => mfd.Dishes.Select(d => d.Price))
                    .ToArray();


            //Выделяем память для искомых данных ( +1 для хранения суммы всех ожидаемых проплат)
            double[] unitPricesTotal = new double[workDayCount*catLength + 1];

            {

                for (int i = 0; i < paimentList.Count; i++)
                {
                    UserWeekPaimentDto uwp = paimentList[i];

                    double[] userweekpaiments = uwp.UserDayPaiments.SelectMany(udp => udp.Paiments).ToArray();
                    for (int j = 0; j < workDayCount*catLength; j++)
                    {
                        unitPricesTotal[j] += userweekpaiments[j];
                    }
                }
            }
            unitPricesTotal[workDayCount*catLength + 1] = unitPricesTotal.Sum();

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
                    for (int[] j = {0}; j[0] < workDayCount; j[0]++)
                    {
                        colname = GetExcelColumnName(j[0]*4 + 3);
                        colname_2 = GetExcelColumnName(j[0]*4 + 6);
                        var elementAtOrDefault = workingWeek.WorkingDays.ElementAtOrDefault(j[0] + 1);
                        if (elementAtOrDefault != null)
                            workSheet.Cells[1, colname] = elementAtOrDefault.DayOfWeek.Name;
                        str = String.Format("{0}1:{1}1", colname, colname_2);
                        workSheet.Range[str].Merge();
                    }
                    i += workDayCount*4 + 3;
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
                            colname = GetExcelColumnName(i + j*4 + k);
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
                    for (int j = 0; j < 20; j++)
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

                        for (int k = 0; k < workDayCount; k++)
                        {
                            for (int l = 0; l < catLength; l++)
                            {
                                colname = GetExcelColumnName(k*catLength + l + 3);
                                workSheet.Cells[i + j, colname] = userpai.UserDayPaiments[k].Paiments[l];
                            }
                        }
                        colname = GetExcelColumnName(23);
                        workSheet.Cells[i + j, colname] = paimentList[j].WeekSummaryPaiment;
                        colname = GetExcelColumnName(24);
                        workSheet.Cells[i + j, colname] = paimentList[j].WeekPaid;
                        colname = GetExcelColumnName(25);
                        workSheet.Cells[i + j, colname] = paimentList[j].Balance;
                        colname = GetExcelColumnName(26);
                        workSheet.Cells[i + j, colname] = paimentList[j].Note;
                    }
                    i += paimentList.Count;
                    workSheet.Cells[i, "A"] = "Итого";
                    str = String.Format("A{0}:B{1}", i, i);
                    workSheet.Range[str].Merge();
                    for (int j = 0; j < paimentList.Count; j++)
                    {
                        colname = GetExcelColumnName(j + 3);
                        workSheet.Cells[i, colname] = unitPricesTotal[j];
                    }
                    colname = GetExcelColumnName(23);
                    workSheet.Cells[i, colname] = unitPricesTotal.Sum();
                    colname = GetExcelColumnName(24);
                    workSheet.Cells[i, colname] = paimentList.Sum(up => up.WeekPaid);
                    colname = GetExcelColumnName(25);
                    workSheet.Cells[i, colname] = paimentList.Sum(up => up.Balance);

                    // Apply some predefined styles for data to look nicely :)
                    // workSheet.Range["A1:E1"].AutoFormat();
                    //workSheet.Range["B1:B42"].Rows.AutoFit();
                    workSheet.Range["A1:Y42"].HorizontalAlignment = XlHAlign.xlHAlignRight;
                    //workSheet.Range["B1:B41"].Style.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                    workSheet.Range["B1:B41"].Columns.AutoFit();
                    // Define filename
                    string fileName = string.Format(@"{0}\ExcelData.xlsx",
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                    // Save this data as a file
                    workSheet.SaveAs(fileName);

                    return fileName;
                }
            }
            catch (Exception)
            {
                return null;
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
