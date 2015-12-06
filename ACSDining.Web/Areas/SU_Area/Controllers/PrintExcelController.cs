using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO.SuperUser;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    public class PrintExcelController : ApiController
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<OrderMenu> _orderRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;
        private readonly IRepository<DishType> _dishtypeRepository;
        private readonly IRepository<DayOfWeek> _dayRepository;

        public PrintExcelController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = _unitOfWork.Repository<OrderMenu>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
            _dishtypeRepository = _unitOfWork.Repository<DishType>();
            _dayRepository = _unitOfWork.Repository<DayOfWeek>();
        }

        public void ExportToExcel(PaimentsDTO paimodel)
        {
            // Load Excel application
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();

            // Create empty workbook
            excel.Workbooks.Add();

            // Create Worksheet from active sheet
            Microsoft.Office.Interop.Excel._Worksheet workSheet = excel.ActiveSheet;

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
                int count = _dayRepository.GetAll().Count();
                int i = 0;
                string str = String.Empty;
                string colname = String.Empty;
                string colname_2 = String.Empty;
                for (int j = 0; j < count; j++)
                {
                    colname = GetExcelColumnName(j*4 + 3);
                    colname_2 = GetExcelColumnName(j*4 + 6);
                    workSheet.Cells[1, colname] = _dayRepository.Find(d => d.ID == j + 1).Name;
                    str = String.Format("{0}1:{1}1", colname, colname_2);
                    workSheet.Range[str].Merge();
                }
                i += count*4 + 3;
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
                List<DishType> dishTypes = _dishtypeRepository.GetAll().ToList();
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
                workSheet.Range[str].Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                int orderid =
                    _orderRepository.Find(
                        ord =>
                            ord.MenuForWeek.WeekNumber == paimodel.WeekNumber &&
                            ord.MenuForWeek.Year.YearNumber == paimodel.YearNumber).Id;
                double[] dishprices = paimodel.UnitPrices;
                for (int j = 0; j < 20; j++)
                {
                    colname = GetExcelColumnName(i+j);
                    workSheet.Cells[4, colname] = dishprices[j];
                }
                i = 5; //i==row;
                for (int j = 0; j < paimodel.UserPaiments.Count; j++)
                {
                    workSheet.Cells[i + j, "A"] = j + 1;
                    workSheet.Cells[i + j, "B"] = paimodel.UserPaiments.ElementAt(j).UserName;
                    double[] userpais = _unitOfWork.GetUserWeekOrderPaiments(orderid);
                    for (int k = 0; k < 20; k++)
                    {
                        colname = GetExcelColumnName(k+3);
                        workSheet.Cells[i + j, colname] = userpais[k];
                    }
                    colname = GetExcelColumnName(23);
                    workSheet.Cells[i + j, colname] = paimodel.UserPaiments.ElementAt(j).SummaryPrice;
                    colname = GetExcelColumnName(24);
                    workSheet.Cells[i + j, colname] = paimodel.UserPaiments.ElementAt(j).WeekPaid;
                    colname = GetExcelColumnName(25);
                    workSheet.Cells[i + j, colname] = paimodel.UserPaiments.ElementAt(j).Balance;
                    colname = GetExcelColumnName(26);
                    workSheet.Cells[i + j, colname] = paimodel.UserPaiments.ElementAt(j).Note;
                }
                i += paimodel.UserPaiments.Count;
                workSheet.Cells[i, "A"] = "Итого";
                str = String.Format("A{0}:B{1}", i, i);
                workSheet.Range[str].Merge();
                for (int j = 0; j < 20; j++)
                {
                    colname = GetExcelColumnName(j+3);
                    workSheet.Cells[i, colname] = paimodel.UnitPricesTotal[j];
                }
                colname = GetExcelColumnName(23);
                workSheet.Cells[i, colname] = paimodel.UnitPricesTotal.Sum();
                colname = GetExcelColumnName(24);
                workSheet.Cells[i, colname] = paimodel.UserPaiments.Sum(up => up.WeekPaid);
                colname = GetExcelColumnName(25);
                workSheet.Cells[i, colname] = paimodel.UserPaiments.Sum(up => up.Balance);

                // Apply some predefined styles for data to look nicely :)
                workSheet.Range["A1:E1"].AutoFormat();

                // Define filename
                string fileName = string.Format(@"{0}\ExcelData.xlsx",
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                // Save this data as a file
                workSheet.SaveAs(fileName);

            }
            catch (Exception exception)
            {

            }
            finally
            {
                // Quit Excel application
                excel.Quit();

                // Release COM objects (very important!)
                if (excel != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);

                if (workSheet != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workSheet);

                // Empty variables
                excel = null;
                workSheet = null;

                // Force garbage collector cleaning
                GC.Collect();
            }
        }
        private string GetExcelColumnName(int columnNumber)
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
