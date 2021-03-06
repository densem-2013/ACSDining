﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using Bytescout.Spreadsheet;
using Bytescout.Spreadsheet.Constants;
using Font = System.Drawing.Font;
using ForExcelDataDto = ACSDining.Infrastructure.DTO.SuperUser.Orders.ForExcelDataDto;
using PlanUserWeekOrderDto = ACSDining.Infrastructure.DTO.SuperUser.Orders.PlanUserWeekOrderDto;
using Range = Bytescout.Spreadsheet.Range;
using UserWeekPaimentDto = ACSDining.Infrastructure.DTO.SuperUser.Paiments.UserWeekPaimentDto;
using WeekPaimentDto = ACSDining.Infrastructure.DTO.SuperUser.Paiments.WeekPaimentDto;
using Worksheet = Bytescout.Spreadsheet.Worksheet;

namespace ACSDining.Infrastructure.Repositories
{
    public static class GetExcelRepository
    {
        #region Paiments


        public static string GetExcelFileFromPaimentsModel(this IRepositoryAsync<WeekOrderMenu> repository,
            ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(repository.GetRepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            WorkingWeek workWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(feDto.WeekYear);
            int workDayCount = workWeek.WorkingDays.Count(wd => wd.IsWorking);
            int catLength = repository.GetRepositoryAsync<DishType>().GetAll().Count;
            List<UserWeekPaimentDto> paimentList = dto.UserWeekPaiments;
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


            Spreadsheet document = new Spreadsheet();

            //document.Workbook.Worksheets.DeleteAll();
            // Get worksheet by name
            Worksheet workSheet = document.Workbook.Worksheets.Add("Оплаты");
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

                    string endcolname = GetExcelColumnName(dishcount + 6);
                    string allstr = string.Format("A1:{0}{1}", endcolname, paimentList.Count + 6);
                    workSheet.Range(allstr).Font = new Font("Arial", 13, FontStyle.Bold);

                    workSheet.Cell(1, 0).Value = "№";
                    workSheet.Range("A2:A5").Merge();

                    workSheet.Range("B2:B5").Merge();
                    workSheet.Cell(1, 1).MergedWithCell.Value = "Ф.И.О.";
                    workSheet.Range("B2:B5").AlignmentHorizontal = AlignmentHorizontal.Centered;

                    string titlerang = String.Format("A1:{0}1", GetExcelColumnName(dishcount + 6));
                    Range range = workSheet.Range(titlerang);
                    range.Merge();
                    workSheet.Cell("A1").MergedWithCell.Value = "Оплаты на " + feDto.DataString;
                    range.AlignmentHorizontal = AlignmentHorizontal.Centered;
                    int i = 0;
                    string str;
                    string colname;
                    string colname_2;
                    for (int[] j = {0}; j[0] < workDayCount; j[0]++)
                    {
                        colname = GetExcelColumnName(j[0]*catLength + 3);
                        colname_2 = GetExcelColumnName(j[0]*catLength + 6);
                        var elementAtOrDefault = workWeek.WorkingDays.Where(wd => wd.IsWorking).ElementAtOrDefault(j[0]);
                        if (elementAtOrDefault != null)
                            workSheet.Cell(1, j[0]*catLength + 3).Value = elementAtOrDefault.DayOfWeek.Name;
                        str = String.Format("{0}2:{1}2", colname, colname_2);
                        workSheet.Range(str).Merge();
                    }
                    i += dishcount + 2;
                    colname = GetExcelColumnName(i + 1);
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Cell(2, i).MergedWithCell.Value = "Сумма к оплате ";
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 90;
                    i++;
                    colname = GetExcelColumnName(i + 1);
                    workSheet.Cell(2, i).Value = "Оплата за неделю";
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 90;
                    i++;
                    colname = GetExcelColumnName(i + 1);
                    workSheet.Cell(2, i).Value = "Баланс";
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 80;
                    i++;
                    colname = GetExcelColumnName(i + 1);
                    workSheet.Cell(2, i).Value = "Примечание";
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 90;

                    colname = GetExcelColumnName(dishcount + 2);
                    workSheet.Cell(3, 2).Value = "Цена за одну порцию, грн";
                    str = String.Format("C4:{0}4", colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;

                    i = paimentList.Count + 5;
                    str = String.Format("A{0}:B{1}", i + 1, i + 1);
                    workSheet.Range(str).Merge();
                    workSheet.Cell(i, 0).MergedWithCell.Value = "Итого";
                    workSheet.Cell(i, 0).AlignmentHorizontal = AlignmentHorizontal.Right;


                    workSheet.Columns[i].Width = 120;

                    workSheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
                    workSheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;

                    i = 2;
                    for (int j = 0; j < workDayCount; j++)
                    {
                        for (int k = 0; k < catLength; k++)
                        {
                            colname = GetExcelColumnName(3 + j*catLength + k);
                            workSheet.Cell(2, 2 + j*catLength + k).Value = dishCategories[k];
                            workSheet.Range(colname + "3").Rotation = 90;
                            if ((k) % 4 != 0)
                            {
                                workSheet.Cell(2, 2 + j * catLength + k).LeftBorderStyle = LineStyle.Thin;
                            }
                            workSheet.Cell(2, 2 + j * catLength + k).RightBorderStyle = LineStyle.Thin;
                        }
                    }


                    double[] dishprices = unitPrices;
                    for (int j = 0; j < dishcount; j++)
                    {
                        colname = GetExcelColumnName(i + j);
                        workSheet.Cell(4, i + j).Value = dishprices[j];
                        if ((j) % 4 != 0)
                        {
                            workSheet.Cell(4, j + 2).LeftBorderStyle = LineStyle.Thin;
                        }
                        workSheet.Cell(4, j + 2).RightBorderStyle = LineStyle.Thin;
                    }

                    i = paimentList.Count + 5;

                    for (int j = 0; j < dishcount; j++)
                    {

                        colname = GetExcelColumnName(j + 3);
                        workSheet.Cell(i, j + 2).Value = unitPricesTotal[j];
                        if ((j) % 4 != 0)
                        {
                            workSheet.Cell(i, j + 2).LeftBorderStyle = LineStyle.Thin;
                        }
                        workSheet.Cell(i, j + 2).RightBorderStyle = LineStyle.Thin;
                    }
                    colname = GetExcelColumnName(dishcount + 3);
                    workSheet.Cell(i, dishcount + 2).Value = unitPricesTotal[dishcount];
                    colname = GetExcelColumnName(dishcount + 4);
                    workSheet.Cell(i, dishcount + 3).Value = paimentList.Sum(up => up.Paiment);
                    colname = GetExcelColumnName(dishcount + 5);
                    workSheet.Cell(i, dishcount + 4).Value = paimentList.Sum(up => up.Balance);


                    i = 5;
                    Color contentColor = Color.FromArgb(227, 238, 245);
                    Color nullColor = Color.FromArgb(6, 84, 156);
                    for (int j = 0; j < paimentList.Count; j++)
                    {
                        bool ev = (i + j)%2 != 0;
                        UserWeekPaimentDto userpai = paimentList[j];
                        workSheet.Cell(i + j, 0).Value = j + 1;
                        workSheet.Cell(i + j, 1).Value = userpai.UserName;
                        workSheet.Cell(i + j, 1).ShrinkToFit = true;
                        if (ev)
                        {
                            workSheet.Cell(i + j, 0).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, 0).FillPatternForeColor = contentColor;
                            workSheet.Cell(i + j, 1).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, 1).FillPatternForeColor = contentColor;

                        }
                        for (int k = 0; k < dishcount; k++)
                        {

                            colname = GetExcelColumnName(k + 2);
                            bool itsnulval = userpai.WeekPaiments[k] == 0.00;
                            if (!itsnulval)
                            {
                                workSheet.Cell(i + j, k + 2).Value = userpai.WeekPaiments[k];
                            }
                            if (ev)
                            {
                                workSheet.Cell(i + j, k + 2).FillPattern = PatternStyle.Solid;
                                workSheet.Cell(i + j, k + 2).FillPatternForeColor = contentColor;

                            }
                            if ((k) % 4 != 0 )
                            {
                                workSheet.Cell(i + j, k + 2).LeftBorderStyle = LineStyle.Thin;
                            }
                            workSheet.Cell(i + j, k + 2).RightBorderStyle = LineStyle.Thin;
                        }

                        colname = GetExcelColumnName(dishcount + 2);
                        workSheet.Cell(i + j, dishcount + 2).Value = paimentList[j].WeekPaiments[workDayCount*catLength];
                        if (ev)
                        {
                            workSheet.Cell(i + j, dishcount + 2).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, dishcount + 2).FillPatternForeColor = contentColor;

                        }
                        colname = GetExcelColumnName(dishcount + 3);
                        if (ev)
                        {
                            workSheet.Cell(i + j, dishcount + 3).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, dishcount + 3).FillPatternForeColor = contentColor;

                        }
                        colname = GetExcelColumnName(dishcount + 4);
                        workSheet.Cell(i + j, dishcount + 4).Value = paimentList[j].Balance;
                        if (ev)
                        {
                            workSheet.Cell(i + j, dishcount + 4).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, dishcount + 4).FillPatternForeColor = contentColor;

                        }
                        colname = GetExcelColumnName(dishcount + 5);
                        workSheet.Cell(i + j, dishcount + 5).Value = paimentList[j].Note;
                        if (ev)
                        {
                            workSheet.Cell(i + j, dishcount + 5).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, dishcount + 5).FillPatternForeColor = contentColor;
                        }
                        workSheet.Cell(i + j, dishcount + 5).TopBorderStyle = LineStyle.Medium;
                        workSheet.Cell(i + j, dishcount + 5).RightBorderStyle = LineStyle.Medium;
                        workSheet.Cell(i + j, dishcount + 5).BottomBorderStyle = LineStyle.Medium;
                    }
                    colname = GetExcelColumnName(dishcount + 1);
                    string headerstr = string.Format("C{0}:{2}{1}", 1, 2, colname);
                    workSheet.Range(headerstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    string headerusnamesstr = string.Format("A{0}:B{1}", 2, 5);
                    workSheet.Range(headerusnamesstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    workSheet.Range(headerusnamesstr).AlignmentVertical = AlignmentVertical.Centered;
                    string usernames = string.Format("B{0}:B{1}", 6, paimentList.Count + 5);
                    workSheet.Range(usernames).AlignmentHorizontal = AlignmentHorizontal.Left;
                    string userquantistr = string.Format("C{0}:{2}{1}", 5, paimentList.Count + 6, endcolname);
                    //workSheet.Range(userquantistr).NumberFormatString = "0.0";
                    workSheet.Range(userquantistr).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    string sumnotestart = GetExcelColumnName(dishcount + 2);
                    string sumnotend = GetExcelColumnName(dishcount + 5);
                    string sumcol = string.Format("{0}{1}:{2}{3}", sumnotestart, 5, sumnotend, paimentList.Count + 7);
                    workSheet.Range(sumcol).NumberFormatString = "#,##0.00";
                    workSheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    workSheet.Columns[0].Width = 40;
                    workSheet.Columns[1].Width = 250;
                    //worksheet.Columns[1].AutoFit();

                    for (int j = 0; j < paimentList.Count + 6; j++)
                    {
                        workSheet.Rows[j].Height = (uint)((j != 2) ? 35 : 120);
                        workSheet.Rows[j].AlignmentVertical = AlignmentVertical.Centered;
                    }

                    //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                    //               @"ACSDining.Web\ExcelFiles\Оплаты.xls";
                    string pathstr = string.Format("~/ExcelFiles/Оплаты_{0}.xls", YearWeekHelp.GetWeekTitle(repository.GetRepositoryAsync<MenuForWeek>(), dto.WeekYearDto));
                    string _path = HostingEnvironment.MapPath(pathstr);
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
            }
            catch (Exception ex)
            {
                throw;
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

            int dishcount = workDayCount*catLength;
            int orderscount = userWeekOrders.Count;
            // Create new Spreadsheet
            Spreadsheet document = new Spreadsheet();

            //document.Workbook.Worksheets.DeleteAll();
            // Get worksheet by name
            Worksheet worksheet = document.Workbook.Worksheets.Add("Заявки фактические");
            string titlerang = String.Format("A1:{0}1", GetExcelColumnName(dishcount + 3));
            Range range = worksheet.Range(titlerang);
            range.Merge();
            worksheet.Cell("A1").MergedWithCell.Value = "Заявки фактические " + feDto.DataString;
            range.AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Cell(2, 0).Value = "№";
            worksheet.Range("A2:A5").Merge();
            worksheet.Cell(2, 1).Value = "Ф.И.О.";
            worksheet.Range("B2:B5").Merge();
            worksheet.Range("B2:B5").AlignmentHorizontal = AlignmentHorizontal.Centered;

            string str;
            string colname;
            string colname_2;
            string endcolname = GetExcelColumnName(dishcount + 3);
            string allstr = string.Format("A{0}:{2}{1}", 1, userWeekOrders.Count + 6, endcolname);

            colname = GetExcelColumnName(dishcount + 2);
            worksheet.Cell(2 + 1, 3).Value = "Цена за одну порцию, грн";
            str = String.Format("C4:{0}4", colname);
            worksheet.Range(str).Merge();
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;

            int i = dishcount + 3;
            colname = GetExcelColumnName(i);
            worksheet.Cell(1, i - 1).Value = "Стоимость заказа за неделю";
            str = String.Format("{0}2:{1}5", colname, colname);
            worksheet.Range(str).Merge();
            worksheet.Range(str).Wrap = true;
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Columns[i - 1].Width = 100;
            worksheet.Cell(1, i - 1).ShrinkToFit = true;

            i = userWeekOrders.Count + 5;
            worksheet.Cell(i, 0).Value = "Всего заказано";
            str = String.Format("A{0}:B{1}", i + 1, i + 1);
            worksheet.Range("A2:B5").AlignmentHorizontal = AlignmentHorizontal.Right;
            worksheet.Range(str).Merge();

            worksheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;

            for (int j = 0; j < dishcount; j++)
            {
                worksheet.Cell(4, 2 + j).Value = weekDishPrices[j];
                if ((j) % 4 != 0)
                {
                    worksheet.Cell(4, 2 + j).LeftBorderStyle = LineStyle.Thin;
                }
                worksheet.Cell(4, 2 + j).RightBorderStyle = LineStyle.Thin;
            }


            i = 0;
            for (int[] j = {0}; j[0] < workDayCount; j[0]++)
            {
                colname = GetExcelColumnName(j[0]*catLength + 3);
                colname_2 = GetExcelColumnName(j[0]*catLength + 6);
                var elementAtOrDefault = workWeek.WorkingDays.Where(wd => wd.IsWorking).ElementAtOrDefault(j[0]);
                if (elementAtOrDefault != null)
                    worksheet.Cell(1, j[0]*catLength + 3).Value = elementAtOrDefault.DayOfWeek.Name;
                str = String.Format("{0}2:{1}2", colname, colname_2);
                worksheet.Range(str).Merge();
            }

            i = 2;
            for (int j = 0; j < workDayCount; j++)
            {
                for (int k = 0; k < catLength; k++)
                {
                    colname = GetExcelColumnName(2 + 1 + j*catLength + k);
                    worksheet.Cell(2, 2 + j*catLength + k).Value = dishCategories[k];
                    worksheet.Range(colname + "3").Rotation = 90;
                    if ((k) % 4 != 0)
                    {
                        worksheet.Cell(2, 2 + j * catLength + k).LeftBorderStyle = LineStyle.Thin;
                    }
                    worksheet.Cell(2, 2 + j * catLength + k).RightBorderStyle = LineStyle.Thin;
                }
            }
            str = string.Format("A1:{0}5", GetExcelColumnName(dishcount + 3));
            i = 5;
            Color contentColor = Color.FromArgb(224, 232, 241);
            Color nullColor = Color.FromArgb(6, 84, 156);
            for (int j = 0; j < userWeekOrders.Count; j++)
            {
                var itsevenrow = (i + j)%2 != 0;
                UserWeekOrderDto userweekorder = userWeekOrders[j];
                worksheet.Cell(i + j, 0).Value = j + 1;
                worksheet.Cell(i + j, 1).Value = userweekorder.UserName;
                if (itsevenrow)
                {
                    worksheet.Cell(i + j, 0).FillPattern = PatternStyle.Solid;
                    worksheet.Cell(i + j, 0).FillPatternForeColor = contentColor;
                    worksheet.Cell(i + j, 1).FillPattern = PatternStyle.Solid;
                    worksheet.Cell(i + j, 1).FillPatternForeColor = contentColor;
                }
                for (int k = 0; k < dishcount + 1; k++)
                {
                    var celval = userweekorder.UserWeekOrderDishes[k];
                    if (celval!=0.00)
                    {
                        worksheet.Cell(i + j, k + 2).Value = celval;
                    }
                    if (itsevenrow)
                    {
                        worksheet.Cell(i + j, k + 2).FillPattern = PatternStyle.Solid;
                        worksheet.Cell(i + j, k + 2).FillPatternForeColor = contentColor;
                    }
                    if ((k) % 4 != 0)
                    {
                        worksheet.Cell(i + j, k + 2).LeftBorderStyle = LineStyle.Thin;
                    }
                    worksheet.Cell(i + j, k + 2).RightBorderStyle = LineStyle.Thin;
                }

            }
            i = userWeekOrders.Count + 5;
            Color evcolor = Color.FromArgb(68, 240, 196);
            for (int j = 0; j < workDayCount; j++)
            {
                for (int k = 0; k < catLength; k++)
                {
                    Cell curCell = worksheet.Cell(i, j*catLength + k + 2);
                    curCell.Value = summaryDishQuantities[j*catLength + k];
                    if (j%2 == 0)
                    {
                        curCell.FillPattern = PatternStyle.Solid;
                        curCell.FillPatternForeColor = evcolor;
                    }
                    if ((k) % 4 != 0)
                    {
                        worksheet.Cell(i, j * catLength + k + 2).LeftBorderStyle = LineStyle.Thin;
                    }
                    worksheet.Cell(i, j * catLength + k + 2).RightBorderStyle = LineStyle.Thin;
                }
            }
            worksheet.Cell(i, dishcount + 2).Value = userWeekOrders.Sum(uo => uo.UserWeekOrderDishes[dishcount]);

            string headerstr = string.Format("C{0}:{2}{1}", 1, 3, endcolname);
            worksheet.Range(headerstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string headerusnamesstr = string.Format("A{0}:B{1}", 1, 5);
            worksheet.Range(headerusnamesstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Range(headerusnamesstr).AlignmentVertical = AlignmentVertical.Centered;
            string usernames = string.Format("A{0}:B{1}", 6, userWeekOrders.Count + 5);
            worksheet.Range(usernames).AlignmentHorizontal = AlignmentHorizontal.Left;
            string userquantistr = string.Format("C{0}:{2}{1}", 5, userWeekOrders.Count + 6, endcolname);
            //worksheet.Range(userquantistr).NumberFormatString = "#.#";
            worksheet.Range(userquantistr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string sumcol = string.Format("{0}{1}:{2}{3}", endcolname, 5, endcolname, userWeekOrders.Count + 6);
            worksheet.Range(sumcol).NumberFormatString = "#,##0.00";
            worksheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;

            for (int j = 0; j < userWeekOrders.Count + 6; j++)
            {
                worksheet.Rows[j].Height = (uint)((j != 2) ? 35 : 150);
                worksheet.Rows[j].AlignmentVertical = AlignmentVertical.Centered;
            }

            worksheet.Range(allstr).Font = new Font("Arial", 13, FontStyle.Bold);
            worksheet.Columns[0].Width = 40;
            worksheet.Columns[1].Width = 250;
            //worksheet.Columns[1].AutoFit();

            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //               @"ACSDining.Web\ExcelFiles\ЗаявкиФакт.xls";
            string pathstr = string.Format("~/ExcelFiles/ЗаявкиФакт_{0}.xls", YearWeekHelp.GetWeekTitle(repository.GetRepositoryAsync<MenuForWeek>(), feDto.WeekYear));
            string _path = HostingEnvironment.MapPath(pathstr);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            document.SaveAs(_path);

            // Close document
            document.Close();

            return _path;
        }

        public static string GetPlanOrdersExcelFileWeekYearDto(this IRepositoryAsync<WeekOrderMenu> repository,
            ForExcelDataDto feDto)
        {
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            List<PlannedWeekOrderMenu> weekOrderMenus =
                repository.GetRepositoryAsync<PlannedWeekOrderMenu>().OrdersMenuByWeekYear(feDto.WeekYear);
            List<PlanUserWeekOrderDto> userWeekOrders =
                weekOrderMenus.Select(woDto => PlanUserWeekOrderDto.MapDto(repository.Context, woDto)).ToList();
            string[] dayNames = repository.Context.GetDayNames(feDto.WeekYear, true).Result;
            double[] weekDishPrices = repository.Context.GetWeekDishPrices(feDto.WeekYear).Result;
            double[] summaryDishQuantities = repository.Context.GetFactSumWeekUserCounts(feDto.WeekYear).Result;

            WorkingWeek workWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(feDto.WeekYear);
            int workDayCount = workWeek.WorkingDays.Count(wd => wd.IsWorking);
            int catLength = repository.GetRepositoryAsync<DishType>().GetAll().Count;

            int dishcount = workDayCount*catLength;
            int orderscount = userWeekOrders.Count;
            // Create new Spreadsheet
            Spreadsheet document = new Spreadsheet();

            // Get worksheet by name
            Worksheet worksheet = document.Workbook.Worksheets.Add("Заявки плановые");


            string titlerang = String.Format("A1:{0}1", GetExcelColumnName(dishcount + 3));
            Range range = worksheet.Range(titlerang);
            range.Merge();
            worksheet.Cell("A1").MergedWithCell.Value = "Заявки плановые " + feDto.DataString;
            range.AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Cell(2, 0).Value = "№";
            worksheet.Range("A2:A5").Merge();
            worksheet.Cell(2, 1).Value = "Ф.И.О.";
            worksheet.Range("B2:B5").Merge();
            worksheet.Range("B2:B5").AlignmentHorizontal = AlignmentHorizontal.Centered;

            string str;
            string colname;
            string colname_2;
            string endcolname = GetExcelColumnName(dishcount + 3);
            string allstr = string.Format("A{0}:{2}{1}", 1, userWeekOrders.Count + 6, endcolname);

            colname = GetExcelColumnName(dishcount + 2);
            worksheet.Cell(2 + 1, 3).Value = "Цена за одну порцию, грн";
            str = String.Format("C4:{0}4", colname);
            worksheet.Range(str).Merge();
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;

            int i = dishcount + 3;
            colname = GetExcelColumnName(i);
            worksheet.Cell(1, i - 1).Value = "Стоимость заказа за неделю";
            str = String.Format("{0}2:{1}5", colname, colname);
            worksheet.Range(str).Merge();
            worksheet.Range(str).Wrap = true;
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Columns[i - 1].Width = 100;
            worksheet.Cell(1, i - 1).ShrinkToFit = true;

            i = userWeekOrders.Count + 5;
            worksheet.Cell(i, 0).Value = "Всего заказано";
            str = String.Format("A{0}:B{1}", i + 1, i + 1);
            worksheet.Range("B2:B5").AlignmentHorizontal = AlignmentHorizontal.Right;
            worksheet.Range(str).Merge();

            worksheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;

            for (int j = 0; j < dishcount; j++)
            {
                worksheet.Cell(4, 2 + j).Value = weekDishPrices[j];
                if ((j) % 4 != 0)
                {
                    worksheet.Cell(4, 2 + j).LeftBorderStyle = LineStyle.Thin;
                }
                worksheet.Cell(4, 2 + j).RightBorderStyle = LineStyle.Thin;
            }


            i = 0;
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

            i = 2;
            for (int j = 0; j < workDayCount; j++)
            {
                for (int k = 0; k < catLength; k++)
                {
                    colname = GetExcelColumnName(2 + 1 + j * catLength + k);
                    worksheet.Cell(2, 2 + j * catLength + k).Value = dishCategories[k];
                    worksheet.Range(colname + "3").Rotation = 90;
                    if ((k) % 4 != 0)
                    {
                        worksheet.Cell(2, 2 + j * catLength + k).LeftBorderStyle = LineStyle.Thin;
                    }
                    worksheet.Cell(2, 2 + j * catLength + k).RightBorderStyle = LineStyle.Thin;
                }
            }
            str = string.Format("A1:{0}5", GetExcelColumnName(dishcount + 3));
            i = 5;
            Color contentColor = Color.FromArgb(224, 232, 241);
            Color nullColor = Color.FromArgb(6, 84, 156);
            for (int j = 0; j < userWeekOrders.Count; j++)
            {
                var itsevenrow = (i + j) % 2 != 0;
                PlanUserWeekOrderDto userweekorder = userWeekOrders[j];
                worksheet.Cell(i + j, 0).Value = j + 1;
                worksheet.Cell(i + j, 1).Value = userweekorder.UserName;
                if (itsevenrow)
                {
                    worksheet.Cell(i + j, 0).FillPattern = PatternStyle.Solid;
                    worksheet.Cell(i + j, 0).FillPatternForeColor = contentColor;
                    worksheet.Cell(i + j, 1).FillPattern = PatternStyle.Solid;
                    worksheet.Cell(i + j, 1).FillPatternForeColor = contentColor;
                }
                worksheet.Cell(i + j, 1).ShrinkToFit = true;
                for (int k = 0; k < dishcount + 1; k++)
                {
                    var celval = userweekorder.UserWeekOrderDishes[k];
                    if (celval != 0.00)
                    {
                        worksheet.Cell(i + j, k + 2).Value = celval;
                    }
                    if (itsevenrow)
                    {
                        worksheet.Cell(i + j, k + 2).FillPattern = PatternStyle.Solid;
                        worksheet.Cell(i + j, k + 2).FillPatternForeColor = contentColor;
                    }
                    if ((k) % 4 != 0)
                    {
                        worksheet.Cell(i + j, k + 2).LeftBorderStyle = LineStyle.Thin;
                    }
                    worksheet.Cell(i + j, k + 2).RightBorderStyle = LineStyle.Thin;
                }

            }
            i = userWeekOrders.Count + 5;
            Color evcolor = Color.FromArgb(68, 240, 196);
            for (int j = 0; j < workDayCount; j++)
            {
                for (int k = 0; k < catLength; k++)
                {
                    Cell curCell = worksheet.Cell(i, j * catLength + k + 2);
                    curCell.Value = summaryDishQuantities[j * catLength + k];
                    if (j % 2 == 0)
                    {
                        curCell.FillPattern = PatternStyle.Solid;
                        curCell.FillPatternForeColor = evcolor;
                    }
                    if ((k) % 4 != 0)
                    {
                        worksheet.Cell(i, j * catLength + k + 2).LeftBorderStyle = LineStyle.Thin;
                    }
                    worksheet.Cell(i, j * catLength + k + 2).RightBorderStyle = LineStyle.Thin;
                }
            }
            worksheet.Cell(i, dishcount + 2).Value = userWeekOrders.Sum(uo => uo.UserWeekOrderDishes[dishcount]);

            string headerstr = string.Format("C{0}:{2}{1}", 1, 3, endcolname);
            worksheet.Range(headerstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string headerusnamesstr = string.Format("A{0}:B{1}", 1, 5);
            worksheet.Range(headerusnamesstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Range(headerusnamesstr).AlignmentVertical = AlignmentVertical.Centered;
            string usernames = string.Format("A{0}:B{1}", 6, userWeekOrders.Count + 5);
            worksheet.Range(usernames).AlignmentHorizontal = AlignmentHorizontal.Left;
            string userquantistr = string.Format("C{0}:{2}{1}", 5, userWeekOrders.Count + 6, endcolname);
            //worksheet.Range(userquantistr).NumberFormatString = "#.#";
            worksheet.Range(userquantistr).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string sumcol = string.Format("{0}{1}:{2}{3}", endcolname, 5, endcolname, userWeekOrders.Count + 6);
            worksheet.Range(sumcol).NumberFormatString = "#,##0.00";
            worksheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Columns[0].Width = 40;
            worksheet.Columns[1].Width = 250;
            //worksheet.Columns[1].AutoFit();
            worksheet.Range(allstr).Font = new Font("Arial", 13, FontStyle.Bold);

            for (int j = 0; j < userWeekOrders.Count + 6; j++)
            {
                worksheet.Rows[j].Height = (uint)((j != 2) ? 35 : 150);
                worksheet.Rows[j].AlignmentVertical = AlignmentVertical.Centered;
            }

            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //               @"ACSDining.Web\ExcelFiles\ЗаявкиПлан.xls";
            string pathstr = string.Format("~/ExcelFiles/ЗаявкиПлан_{0}.xls", YearWeekHelp.GetWeekTitle(repository.GetRepositoryAsync<MenuForWeek>(), feDto.WeekYear));
            string _path = HostingEnvironment.MapPath(pathstr);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            document.SaveAs(_path);

            // Close document
            document.Close();

            return _path;
        }

        #endregion

        #region Меню

        public static string GetMenuExcelFile(this IRepositoryAsync<MenuForWeek> repository, ForMenuExcelDto dto)
        {
            WeekMenuDto weekMenuDto = repository.MapWeekMenuDto(dto.WeekYear);
            WorkingWeek wweek = repository.WorkWeekByWeekYear(dto.WeekYear);
            string[] daynames = wweek.WorkingDays.Where(wd => wd.IsWorking).OrderBy(wd=>wd.DayOfWeek.Id).Select(wd => wd.DayOfWeek.Name).ToArray();
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            int daycount = weekMenuDto.WorkWeekDays.Count(d => d);
            int catLength = dishCategories.Length;
            List<MenuForDayDto> mfdays = new List<MenuForDayDto>();
            for (int i = 0; i < weekMenuDto.WorkWeekDays.Length; i++)
            {
                if (weekMenuDto.WorkWeekDays[i])
                {
                    mfdays.Add(weekMenuDto.MfdModels[i]);
                }
            }
            // Create new Spreadsheet
            Spreadsheet document = new Spreadsheet();

            // Get worksheet by name
            Worksheet worksheet = document.Workbook.Worksheets.Add("Меню");

            string endcolname = GetExcelColumnName(4);

            string allstr = string.Format("A{0}:D{1}", 1, mfdays.Count*(dishCategories.Length + 1) + 3);

            string titlerang = String.Format("A1:{0}1", GetExcelColumnName(4));
            Range range = worksheet.Range(titlerang);
            range.Merge();
            worksheet.Cell("A1").MergedWithCell.Value = "Меню " + dto.MenuTitle;
            range.AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Columns[0].Width = 180;
            worksheet.Cell(1, 1).Value = "Наименование блюд";
            worksheet.Columns[1].Width = 450;
            worksheet.Columns[2].Width = 90;
            worksheet.Columns[3].Width = 120;
            worksheet.Rows[0].Height = 50;
            worksheet.Rows[0].AlignmentVertical=AlignmentVertical.Centered;
            worksheet.Rows[1].Height = 50;
            worksheet.Rows[1].AlignmentVertical = AlignmentVertical.Centered;
            worksheet.Range("A2:D2").FillPattern = PatternStyle.Solid;
            worksheet.Range("A2:D2").FillPatternForeColor = Color.FromArgb(48, 127, 217);
            worksheet.Range("C2:D2").Merge();
            worksheet.Cell(1, 2).MergedWithCell.Value = "Цена, грн";
            worksheet.Range("A2:D2").AlignmentHorizontal = AlignmentHorizontal.Centered;
            for (int i = 0; i < daycount; i++)
            {
                MenuForDayDto mfd = mfdays[i];
                int strcount = i*catLength + 2;
                string colname = string.Format("A{0}:D{1}", strcount + 1 + i, strcount + 1 + i);
                worksheet.Range(colname).Merge();
                worksheet.Cell(strcount + i, 0).MergedWithCell.Value = daynames[i];
                worksheet.Range(colname).AlignmentHorizontal = AlignmentHorizontal.Centered;
                worksheet.Range(colname).FillPattern = PatternStyle.Solid;
                worksheet.Range(colname).FillPatternForeColor = Color.FromArgb(144, 164, 187);
                for (int j = 0; j < mfd.Dishes.Count; j++)
                {
                    worksheet.Cell(strcount + j + 1 + i, 0).Value = mfd.Dishes[j].Category;
                    worksheet.Cell(strcount + j + 1 + i, 0).Indent = 2;
                    worksheet.Cell(strcount + j + 1 + i, 1).Value = mfd.Dishes[j].Title;
                    worksheet.Cell(strcount + j + 1 + i, 1).Indent = 2;
                    bool deskexists = !string.IsNullOrEmpty(mfd.Dishes[j].Description);
                    if (deskexists)
                    {
                        worksheet.Cell(strcount + j + 1 + i, 1).Value = mfd.Dishes[j].Title + ":" +
                                                                        mfd.Dishes[j].Description;
                        worksheet.Cell(strcount + j + 1 + i, 1).Font = new Font("Arial", 12, FontStyle.Bold);
                    }
                    worksheet.Cell(strcount + j + 1 + i, 1).Wrap = true;
                    worksheet.Cell(strcount + j + 1 + i, 2).Value = mfd.Dishes[j].Price;
                    worksheet.Cell(strcount + j + 1 + i, 2).NumberFormatString = "#,##0.00";
                    worksheet.Rows[strcount + j + 1 + i].Height = (uint) (!deskexists ? 70 : 90);
                    worksheet.Rows[strcount + j + 1 + i].AlignmentVertical = AlignmentVertical.Centered;
                }
                string sumdaytotal = string.Format("D{0}:D{1}", strcount + 2 + i, strcount + 1 + i + catLength);
                worksheet.Range(sumdaytotal).Merge();
                worksheet.Range(sumdaytotal).AlignmentHorizontal = AlignmentHorizontal.Centered;
                worksheet.Range(sumdaytotal).AlignmentVertical = AlignmentVertical.Centered;
                worksheet.Cell(strcount + 1 + i, 3).Value = mfd.TotalPrice;
                worksheet.Range(sumdaytotal).NumberFormatString = "#,##0.00";
            }
            string totalstr = string.Format("A{0}:C{0}", daycount*(catLength+1) + 3);
            worksheet.Range(totalstr).Merge();
            worksheet.Cell(daycount*(catLength + 1) + 2, 0).Value = "Всего ";
            worksheet.Range(totalstr).AlignmentHorizontal = AlignmentHorizontal.Right;
            worksheet.Cell(daycount * (catLength + 1) + 2, 3).Value = weekMenuDto.SummaryPrice;
            worksheet.Cell(daycount * (catLength + 1) + 2, 3).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Cell(daycount * (catLength + 1) + 2, 3).NumberFormatString = "#,##0.00";

            worksheet.Rows[daycount * (catLength + 1) + 2].Height = 60;
            worksheet.Rows[daycount * (catLength + 1) + 2].AlignmentVertical = AlignmentVertical.Centered;
            worksheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).Font = new Font("Arial", 14, FontStyle.Bold);


            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //               @"ACSDining.Web\ExcelFiles\Menu.xls";

            string pathstr = string.Format("~/ExcelFiles/Меню_{0}.xls", YearWeekHelp.GetWeekTitle(repository, dto.WeekYear));
            string _path = HostingEnvironment.MapPath(pathstr);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            try
            {
                document.SaveAs(_path);
            }
            catch (Exception)
            {
                    
                throw;
            }

            // Close document
            document.Close();

            return _path;
        }

        #endregion

    }
}
