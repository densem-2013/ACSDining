using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
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

         
        public static string GetExcelFileFromPaimentsModel(this IRepositoryAsync<WeekOrderMenu> repository,
            ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(repository.GetRepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string[] dishCategories = MapHelper.GetCategoriesStrings(repository.Context);
            WorkingWeek workWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(feDto.WeekYear);
            int workDayCount = workWeek.WorkingDays.Count(wd => wd.IsWorking);
            int catLength = repository.GetRepositoryAsync<DishType>().GetAll().Count;
            List<UserWeekPaimentDto> paimentList = dto.UserWeekPaiments;
            //WeekYearDto wyDto = dto.WeekYearDto;
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

            document.Workbook.Worksheets.DeleteAll();
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
                    //Color bordercolor = Color.FromArgb(179, 15, 83);
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
                            workSheet.Cell(1, j[0] * catLength + 3).Value = elementAtOrDefault.DayOfWeek.Name;
                        str = String.Format("{0}2:{1}2", colname, colname_2);
                        workSheet.Range(str).Merge();
                        //workSheet.Cell(1, j[0] * catLength + 3).RightBorderStyle = LineStyle.Double;
                        //workSheet.Cell(1, j[0] * catLength + 3).LeftBorderStyle = LineStyle.Double;
                    }
                    i += dishcount + 2;
                    colname = GetExcelColumnName(i+1);
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Cell(2, i).MergedWithCell.Value = "Сумма к оплате ";
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 90;
                    i++;
                    colname = GetExcelColumnName(i+1);
                    workSheet.Cell(2, i).Value = "Оплата за неделю";
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 90;
                    i++;
                    colname = GetExcelColumnName(i+1);
                    workSheet.Cell(2, i).Value = "Баланс";
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 80;
                    i++;
                    colname = GetExcelColumnName(i+1);
                    workSheet.Cell(2, i).Value = "Примечание";
                    str = String.Format("{0}2:{1}5", colname, colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).Rotation = 90;
                    workSheet.Columns[i].Width = 120;

                    i = 2;
                    for (int j = 0; j < workDayCount; j++)
                    {
                        for (int k = 0; k < catLength; k++)
                        {
                            colname = GetExcelColumnName(3 + j * catLength + k);
                            workSheet.Cell(2, 2 + j * catLength + k).Value = dishCategories[k];
                            workSheet.Range(colname + "3").Rotation = 90;
                        }
                    }
                    colname = GetExcelColumnName(dishcount + 2);
                    workSheet.Cell(3, 2).Value = "Цена за одну порцию, грн";
                    str = String.Format("C4:{0}4", colname);
                    workSheet.Range(str).Merge();
                    workSheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    double[] dishprices = unitPrices;
                    for (int j = 0; j < dishcount; j++)
                    {
                        colname = GetExcelColumnName(i + j);
                        workSheet.Cell(4, i+j).Value = dishprices[j];
                    }
                    i = 5;
                    Color contentColor = Color.FromArgb(192, 225, 245);
                    Color nullColor = Color.FromArgb(6, 84, 156);
                    for (int j = 0; j < paimentList.Count; j++)
                    {
                        bool ev=(i + j) % 2 != 0;
                        UserWeekPaimentDto userpai = paimentList[j];
                        workSheet.Cell(i + j, 0).Value = j + 1;
                        workSheet.Cell(i + j, 1).Value = userpai.UserName;
                        workSheet.Cell(i + j, 1).ShrinkToFit = true;
                        //workSheet.Cell(i + j, 1).Font = new Font("Arial", 15, FontStyle.Bold);
                        if (ev)
                        {
                            workSheet.Cell(i + j, 0).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, 0).FillPatternForeColor = contentColor;
                            workSheet.Cell(i + j, 1).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, 1).FillPatternForeColor = contentColor;
                            
                        }
                        for (int k = 0; k < dishcount; k++)
                        {

                            colname = GetExcelColumnName(k  + 2);
                            workSheet.Cell(i + j, k + 2).Value = userpai.WeekPaiments[k]; 
                            if (ev)
                            {
                                workSheet.Cell(i + j, k + 2).FillPattern = PatternStyle.Solid;
                                workSheet.Cell(i + j, k + 2).FillPatternForeColor = contentColor;

                            }
                            bool itsnulval = userpai.WeekPaiments[k] == 0.00;
                            if (itsnulval)
                            {
                                workSheet.Cell(i + j, k + 2).FillPattern = PatternStyle.Solid;
                                workSheet.Cell(i + j, k + 2).FillPatternForeColor = nullColor;
                            }
                        }

                        colname = GetExcelColumnName(dishcount + 2);
                        workSheet.Cell(i + j, dishcount + 2).Value = paimentList[j].WeekPaiments[workDayCount * catLength];
                        if (ev)
                        {
                            workSheet.Cell(i + j, dishcount + 2).FillPattern = PatternStyle.Solid;
                            workSheet.Cell(i + j, dishcount + 2).FillPatternForeColor = contentColor;

                        }
                        colname = GetExcelColumnName(dishcount + 3);
                        //workSheet.Cell(i + j, dishcount + 3).Value = paimentList[j].Paiment;
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

                        //if ((i + j)%2 == 0) continue;
                        //var striprangestr = string.Format("A{0}:{2}{1}", i + j, i + j, endcolname);
                        //workSheet.Range(striprangestr).FillPattern = PatternStyle.Solid;
                        //workSheet.Range(striprangestr).FillPatternBackColor = contentColor;
                    }
                    i = 5;
                    i += paimentList.Count;
                    str = String.Format("A{0}:B{1}", i + 1, i + 1);
                    workSheet.Range(str).Merge();
                    workSheet.Cell(i, 0).MergedWithCell.Value = "Итого";
                    workSheet.Cell(i, 0).AlignmentHorizontal = AlignmentHorizontal.Right;
                    for (int j = 0; j < dishcount; j++)
                    {
                        colname = GetExcelColumnName(j + 3);
                        workSheet.Cell(i, j+2).Value = unitPricesTotal[j];
                    }
                    colname = GetExcelColumnName(dishcount+3);
                    workSheet.Cell(i, dishcount + 2).Value = unitPricesTotal[dishcount];
                    colname = GetExcelColumnName(dishcount + 4);
                    workSheet.Cell(i, dishcount + 3).Value = paimentList.Sum(up => up.Paiment);
                    colname = GetExcelColumnName(dishcount + 5);
                    workSheet.Cell(i, dishcount + 4).Value = paimentList.Sum(up => up.Balance);


                    string endcolname = GetExcelColumnName(dishcount + 6);
                    string allstr = string.Format("A0:{0}{1}", endcolname, paimentList.Count + 6);

                    string headerstr = string.Format("C{0}:{2}{1}", 1, 2, endcolname);
                    workSheet.Range(headerstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    string headerusnamesstr = string.Format("A{0}:B{1}", 2, 5);
                    workSheet.Range(headerusnamesstr).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    workSheet.Range(headerusnamesstr).AlignmentVertical = AlignmentVertical.Centered;
                    string usernames = string.Format("B{0}:B{1}", 6, paimentList.Count + 5);
                    workSheet.Range(usernames).AlignmentHorizontal = AlignmentHorizontal.Left;
                    //string userpaistr = string.Format("C{0}:{2}{1}", 4, paimentList.Count + 5, endcolname);
                    //workSheet.Range(userpaistr).NumberFormatString = "#,##0.00";
                    //workSheet.Range(userpaistr).AlignmentHorizontal = AlignmentHorizontal.Centered;

                    //workSheet.Range(allstr).Columns.AutoFit();
                    //string headerstr = string.Format("C{0}:{2}{1}", 1, 3, endcolname);
                    string userquantistr = string.Format("C{0}:{2}{1}", 5, paimentList.Count + 6, endcolname);
                    workSheet.Range(userquantistr).NumberFormatString = "0.0";
                    workSheet.Range(userquantistr).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    string sumnotestart = GetExcelColumnName(dishcount + 2);
                    string sumnotend = GetExcelColumnName(dishcount + 5);
                    string sumcol = string.Format("{0}{1}:{2}{3}", sumnotestart, 5, sumnotend, paimentList.Count + 7);
                    workSheet.Range(sumcol).NumberFormatString = "#,##0.00";
                    workSheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;
                    //string allstr = string.Format("A{0}:{2}{1}", 1, userWeekOrders.Count + 6, endcolname);
                    workSheet.Columns[0].Width = 30;
                    workSheet.Columns[1].Width = 180;
                    workSheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
                    workSheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;
                    workSheet.Range(allstr).Font = new Font("Arial", 12, FontStyle.Bold); 
                    // Define filename
                    //string fileName = string.Format(@"{0}\ExcelData.xlsx",
                    //    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                    string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                                              @"ACSDining.Web\ExcelFiles\Paiments.xls";

                    //string _path = HostingEnvironment.MapPath("~/ExcelFiles/Paiments.xls");
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

            int dishcount = workDayCount * catLength;
            int orderscount = userWeekOrders.Count;
            // Create new Spreadsheet
            Spreadsheet document = new Spreadsheet();

            document.Workbook.Worksheets.DeleteAll();
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
            for (int j = 0; j < userWeekOrders.Count; j++)
            {
                var itsevenrow = (i + j)%2 == 0;
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
            worksheet.Range(sumcol).NumberFormatString = "#,##0.00";
            worksheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;
            string allstr = string.Format("A{0}:{2}{1}", 1, userWeekOrders.Count + 6, endcolname);
            worksheet.Columns[0].Width = 30;
            worksheet.Columns[1].Width = 150;
            worksheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).Font = new Font("Arial", 12, FontStyle.Bold);
             string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                                      @"ACSDining.Web\ExcelFiles\Paiments.xls";
           // string _path = HostingEnvironment.MapPath("~/ExcelFiles/Orders.xls");
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
            List<PlannedWeekOrderMenu> weekOrderMenus = repository.GetRepositoryAsync<PlannedWeekOrderMenu>().OrdersMenuByWeekYear(feDto.WeekYear);
            List<PlanUserWeekOrderDto> userWeekOrders =
                weekOrderMenus.Select(woDto => PlanUserWeekOrderDto.MapDto(repository.Context, woDto)).ToList();
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

            document.Workbook.Worksheets.DeleteAll();
            Worksheet worksheet = document.Workbook.Worksheets.Add("Заявки плановые");

            string endcolname = GetExcelColumnName(dishcount + 3);
            string allstr = string.Format("A{0}:{2}{1}", 1, userWeekOrders.Count + 6, endcolname);
            worksheet.Range(allstr).OuterBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).InnerBorderStyle = LineStyle.Medium;
            worksheet.Range(allstr).Font = new Font("Arial", 12, FontStyle.Bold);

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
            worksheet.Cell(2, i - 1).Value = "Стоимость заказа за неделю";
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
                    colname = GetExcelColumnName(i + 1 + j * catLength + k);
                    worksheet.Cell(2, i + j * catLength + k).Value = dishCategories[k];
                    worksheet.Range(colname + "3").Rotation = 90;
                }
            }
            colname = GetExcelColumnName(dishcount + 2);
            worksheet.Cell(i + 1, 3).Value = "Цена за одну порцию, грн";
            str = String.Format("C3:{0}3", colname);
            worksheet.Range(str).Merge();
            worksheet.Range(str).AlignmentHorizontal = AlignmentHorizontal.Centered;
            for (int j = 0; j < dishcount; j++)
            {
                worksheet.Cell(4, i + j).Value = weekDishPrices[j];
            }
            str = string.Format("A1:{0}5", GetExcelColumnName(dishcount + 3));
            // worksheet.Range(str).Font=new Font("Arial",12,FontStyle.Bold);
            i = 5;
            Color contentColor = Color.FromArgb(24, 107, 208);
            for (int j = 0; j < userWeekOrders.Count; j++)
            {
                var itsevenrow = (i + j) % 2 == 0;
                PlanUserWeekOrderDto userweekorder = userWeekOrders[j];
                worksheet.Cell(i + j, 0).Value = j + 1;
                worksheet.Cell(i + j, 1).Value = userweekorder.UserName;
                if (itsevenrow) worksheet.Cell(i + j, 1).FillPatternBackColor = contentColor;
                worksheet.Cell(i + j, 1).ShrinkToFit = true;
                for (int k = 0; k < dishcount + 1; k++)
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
            str = String.Format("A{0}:B{1}", i + 1, i + 1);
            worksheet.Range(str).Merge();
            for (int j = 0; j < dishcount; j++)
            {
                worksheet.Cell(i, j + 2).Value = summaryDishQuantities[j];
            }
            worksheet.Cell(i, dishcount + 2).Value = userWeekOrders.Sum(uo => uo.UserWeekOrderDishes[dishcount]);
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
            worksheet.Range(sumcol).NumberFormatString = "#,##0.00";
            worksheet.Range(sumcol).AlignmentHorizontal = AlignmentHorizontal.Centered;
            worksheet.Columns[0].Width = 30;
            worksheet.Columns[1].Width = 180;

            string _path = HostingEnvironment.MapPath("~/ExcelFiles/PlanOrders.xls");
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
    }
}
