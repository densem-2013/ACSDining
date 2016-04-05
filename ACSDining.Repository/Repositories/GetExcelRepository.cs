using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.DTO.SuperUser;
using Microsoft.Office.Interop.Excel;

namespace ACSDining.Repository.Repositories
{
    public static class GetExcelRepository
    {
        public static string GetExcelFileFromPaimentsModel(this IRepositoryAsync<OrderMenu> repository, PaimentsDTO model)
        {
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
                workSheet.Cells[1, "A"] = "№";
                workSheet.Range["A1:A4"].Merge();
                workSheet.Cells[1, "B"] = "Ф.И.О.";
                workSheet.Range["B1:B4"].Merge();
                var workingWeek =
                    repository.GetRepository<WorkingWeek>()
                        .Queryable()
                        .Include("WorkingDays")
                        .FirstOrDefault(
                            ww => ww.WeekNumber == model.WeekNumber && ww.Year.YearNumber == model.YearNumber);
                if (workingWeek != null)
                {
                    int count = workingWeek.WorkingDays.Count();
                    int i = 0;
                    string str;
                    string colname;
                    string colname_2;
                    for (int[] j = { 0 }; j[0] < count; j[0]++)
                    {
                        colname = GetExcelColumnName(j[0] * 4 + 3);
                        colname_2 = GetExcelColumnName(j[0] * 4 + 6);
                        var elementAtOrDefault = workingWeek.WorkingDays.ElementAtOrDefault( j[0] + 1);
                        if (elementAtOrDefault != null)
                            workSheet.Cells[1, colname] = elementAtOrDefault.DayOfWeek.Name;
                        str = String.Format("{0}1:{1}1", colname, colname_2);
                        workSheet.Range[str].Merge();
                    }
                    i += count * 4 + 3;
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
                    List<DishType> dishTypes = repository.GetRepository<DishType>().Queryable().ToList();
                    i = 3;
                    for (int j = 0; j < 5; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            colname = GetExcelColumnName(i + j * 4 + k);
                            workSheet.Cells[2, colname] = dishTypes.ElementAt(k).Category;
                            workSheet.Range[colname + "2"].Orientation = 90;
                        }
                    }
                    colname = GetExcelColumnName(22);
                    workSheet.Cells[3, "C"] = "Цена за одну порцию, грн";
                    str = String.Format("C3:{0}3", colname);
                    workSheet.Range[str].Merge();
                    workSheet.Range[str].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    int orderid = repository.OrderMenuByWeekYear(model.WeekNumber, model.YearNumber).Id;
                    double[] dishprices = model.UnitPrices;
                    for (int j = 0; j < 20; j++)
                    {
                        colname = GetExcelColumnName(i + j);
                        workSheet.Cells[4, colname] = dishprices[j];
                    }
                    i = 5; //i==row;
                    for (int j = 0; j < model.UserPaiments.Count; j++)
                    {
                        workSheet.Cells[i + j, "A"] = j + 1;
                        workSheet.Cells[i + j, "B"] = model.UserPaiments.ElementAt(j).UserName;
                        double[] userpais = repository.GetUserWeekOrderPaiments(orderid);
                        for (int k = 0; k < 20; k++)
                        {
                            colname = GetExcelColumnName(k + 3);
                            workSheet.Cells[i + j, colname] = userpais[k];
                        }
                        colname = GetExcelColumnName(23);
                        workSheet.Cells[i + j, colname] = model.UserPaiments.ElementAt(j).SummaryPrice;
                        colname = GetExcelColumnName(24);
                        workSheet.Cells[i + j, colname] = model.UserPaiments.ElementAt(j).WeekPaid;
                        colname = GetExcelColumnName(25);
                        workSheet.Cells[i + j, colname] = model.UserPaiments.ElementAt(j).Balance;
                        colname = GetExcelColumnName(26);
                        workSheet.Cells[i + j, colname] = model.UserPaiments.ElementAt(j).Note;
                    }
                    i += model.UserPaiments.Count;
                    workSheet.Cells[i, "A"] = "Итого";
                    str = String.Format("A{0}:B{1}", i, i);
                    workSheet.Range[str].Merge();
                    for (int j = 0; j < 20; j++)
                    {
                        colname = GetExcelColumnName(j + 3);
                        workSheet.Cells[i, colname] = model.UnitPricesTotal[j];
                    }
                    colname = GetExcelColumnName(23);
                    workSheet.Cells[i, colname] = model.UnitPricesTotal.Sum();
                    colname = GetExcelColumnName(24);
                    workSheet.Cells[i, colname] = model.UserPaiments.Sum(up => up.WeekPaid);
                    colname = GetExcelColumnName(25);
                    workSheet.Cells[i, colname] = model.UserPaiments.Sum(up => up.Balance);
                }

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
            catch (Exception exception)
            {
                return null;
            }
            finally
            {
                // Quit Excel application
                excel.Quit();

                // Release COM objects (very important!)
                if (excel != null)
                    Marshal.ReleaseComObject(excel);

                if (workSheet != null)
                    Marshal.ReleaseComObject(workSheet);

                // Empty variables
                excel = null;
                workSheet = null;

                // Force garbage collector cleaning
                GC.Collect();
            }

        }
        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}
