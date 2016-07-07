using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.HelpClasses;
using UpdateUserOrderDto = ACSDining.Infrastructure.DTO.SuperUser.Orders.UpdateUserOrderDto;
using UpdateWeekPaimentDto = ACSDining.Infrastructure.DTO.SuperUser.Paiments.UpdateWeekPaimentDto;
using WeekUserOrder = ACSDining.Infrastructure.DTO.SuperUser.Orders.WeekUserOrder;

namespace ACSDining.Infrastructure.Identity
{
    public static class ContextStoredFunctions
    {
        #region Общие

        /// <summary>
        /// Цены на на каждое блюдо в меню на рабочую неделю
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <param name="witoutnowork"></param>
        /// <returns></returns>
        public static async Task<string[]> GetDayNames(this ApplicationDbContext context, WeekYearDto wyDto, bool witoutnowork=false)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            var withoutNoworking = new SqlParameter("@Wonw", witoutnowork);
            var res = context.Database.SqlQuery<string>("Select DayName From GetDayNames(@Week, @Year,@Wonw)",
                weekParameter, yearParameter, withoutNoworking)
                .ToArrayAsync().Result;
            return res;
        }

        /// <summary>
        /// Переводит все существующие фактические заявки на меню текущего дня в плановые
        /// и закрывает возможность редактирования заказа на этот день для клиентов
        /// </summary>
        /// <param name="context"></param>
        /// <param name="workdayid"></param>
        /// <returns></returns>
        public static void DayFactToPlan(this ApplicationDbContext context, int? workdayid = null)
        {
            var workdayidParametr = new SqlParameter("@WDayId", workdayid);
            if (workdayid==null)
            {
                workdayidParametr.Value = DBNull.Value;
            }
            context.Database.ExecuteSqlCommand("exec DayFactToPlan @WDayId", workdayidParametr);
        }

        /// <summary>
        /// Переводит все существующие фактические заявки на меню текущего дня в плановые
        /// и закрывает возможность редактирования заказа на этот день для клиентов
        /// </summary>
        /// <param name="context"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public static void UpdateBalanceByWeekOrderId(this ApplicationDbContext context, int orderid)
        {
            var orderidParametr = new SqlParameter("@orderid", orderid);
            context.Database.ExecuteSqlCommand("exec UpdateBalanceByWeekOrderId @orderid", orderidParametr);
        }

        /// <summary>
        /// Обновляем баланс по тем пользователям, которые 
        /// заказали ненулевое количество изменяемого блюда в данном дневном меню
        /// </summary>
        /// <param name="context"></param>
        /// <param name="daymenuid"></param>
        /// <param name="dishtypeid"></param>
        /// <returns></returns>
        public static void UpdateBalanceByDayMenuDishTypeId(this ApplicationDbContext context, int daymenuid,int dishtypeid)
        {
            var daymenuidParametr = new SqlParameter("@daymenuid", daymenuid);
            var dishtypeidParametr = new SqlParameter("@dishtypeid", dishtypeid);
            context.Database.ExecuteSqlCommand("exec UpdateBalanceByDayMenuDishTypeId @daymenuid,@dishtypeid", daymenuidParametr, dishtypeidParametr);
        }

        #endregion

        #region Заявки

        /// <summary>
        /// Получает список сущностей, содержащих id недельных заказов по каждому пользователю и их полные имена
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <returns></returns>
        public static async Task<List<WeekUserOrder>> GetWeekOrdersByWeekYear(this ApplicationDbContext context,
            WeekYearDto wyDto)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            return context.Database.SqlQuery<WeekUserOrder>("Select * From WeekOrdersByWeekYear(@Week,@Year)",
                weekParameter, yearParameter)
                .ToListAsync().Result;
        }

        /// <summary>
        /// Цены на на каждое блюдо в меню на рабочую неделю
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <returns></returns>
        public static async Task<double[]> GetWeekDishPrices(this ApplicationDbContext context, WeekYearDto wyDto)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            var res= context.Database.SqlQuery<double>("Select Price From GetWeekDishPrices(@Week, @Year)",
                weekParameter, yearParameter)
                .ToArrayAsync().Result;
            return res;
        }

        /// <summary>
        /// Получает суммарные фактические количества по каждому заказанному блюду на неделе всеми пользователями
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <returns></returns>
        public static async Task<double[]> GetPlanSumWeekUserCounts(this ApplicationDbContext context, WeekYearDto wyDto)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            return context.Database.SqlQuery<double>("Select SumByDishes From GetPlanSumWeekUserCounts(@Week,@Year)",
                weekParameter, yearParameter)
                .ToArrayAsync().Result;
        }


        /// <summary>
        /// Получает суммарные фактические количества по каждому заказанному блюду на неделе всеми пользователями
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <returns></returns>
        public static async Task<double[]> GetFactSumWeekUserCounts(this ApplicationDbContext context, WeekYearDto wyDto)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            var res= context.Database.SqlQuery<double>("Select SumByDishes From GetFactSumWeekUserCounts(@Week,@Year)",
                weekParameter, yearParameter)
                .ToArrayAsync().Result;
            return res;
        }

        /// <summary>
        /// Возвращает значения количеств заказанных блюд для указанного недельного заказа
        /// </summary>
        /// <param name="context"></param>
        /// <param name="weekuserorderid">id недельного заказа пользователя</param>
        /// <returns></returns>
        public static async Task<double[]> FactDishQuantByWeekOrderId(this ApplicationDbContext context, int weekuserorderid)
        {
            double[] res=null;
            try
            {
                var weekorderParameter = new SqlParameter("@Weekuserorderid", weekuserorderid);
                res = context.Database.SqlQuery<double>("Select DishQuant From dbo.[FactDishQuantByWeekOrderId](@Weekuserorderid)",
                    weekorderParameter)
                    .ToArrayAsync().Result;
            }
            catch (Exception)
            {
                    
                throw;
            }
            return res;
        }

        /// <summary>
        /// Возвращает значения количеств заказанных блюд для указанного недельного заказа
        /// </summary>
        /// <param name="context"></param>
        /// <param name="planweekuserorderid">id плановой недельной заявки пользователя</param>
        /// <returns></returns>
        public static async Task<double[]> PlanDishQuantByWeekOrderId(this ApplicationDbContext context, int planweekuserorderid)
        {
            double[] res = null;
            try
            {
                var planWeekOrdParameter = new SqlParameter("@Planword", planweekuserorderid);
                res = context.Database.SqlQuery<double>("Select DishQuant From dbo.[PlanDishQuantByWeekOrderId](@Planword)",
                    planWeekOrdParameter)
                    .ToArrayAsync().Result;
            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }

        /// <summary>
        /// Обновляет значение заказа на блюдо а также сумму заказа на день и рабочую неделю
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userOrderDto"></param>
        /// <returns></returns>
        public static void UpdateDishQuantity(this ApplicationDbContext context, UpdateUserOrderDto userOrderDto)
        {
            var dayorderidParameter = new SqlParameter("@Dayorderid", userOrderDto.DayOrderId);
            var dishtypeidParameter = new SqlParameter("@Dishtypeid", userOrderDto.CategoryId);
            var quantityParameter = new SqlParameter("@Quantity", userOrderDto.Quantity);

            context.Database.ExecuteSqlCommand("exec UpdateDishQuantity @Dayorderid,@Dishtypeid,@Quantity",
                dayorderidParameter, dishtypeidParameter, quantityParameter);
        }

        /// <summary>
        /// Обновляет пользовательский заказ по id недельного заказа используя значения 
        ///Id дневных заказов и массив значений количеств заказанных блюд
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userWeekOrderDto"></param>
        /// <returns></returns>
        public static void UpdateAllQuantitiesOnWeekOrder(this ApplicationDbContext context,
            UpdateAllWeekOrderDto userWeekOrderDto)
        {
                var dayorderarrayidParameter = new SqlParameter("@DayOrderIdArray",
                    MapHelper.CreateDataTable(userWeekOrderDto.DayOrdIds, "dayord"))
                {
                    TypeName = "dbo.DayOrdArray",
                    SqlDbType = SqlDbType.Structured
                };
                var weekordidParameter = new SqlParameter("@weekordid", userWeekOrderDto.WeekOrdId);
                var quantitiesParameter = new SqlParameter("@quantities", MapHelper.CreateDataTable(userWeekOrderDto.QuantArray, "quant"))
                {
                    TypeName = "dbo.QuantArray",
                    SqlDbType = SqlDbType.Structured
                };
                context.Database.ExecuteSqlCommand(
                    "exec UpdateAllQuantitiesOnWeekOrder @DayOrderIdArray, @weekordid, @quantities", dayorderarrayidParameter, weekordidParameter, quantitiesParameter);
            
        }

        /// <summary>
        /// Обновляет пользовательский заказ со стороны SU - только факт
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userOrderDto"></param>
        /// <returns></returns>
        public static void UpdateDishQuantityBySu(this ApplicationDbContext context, UpdateUserOrderDto userOrderDto)
        {
            var dayorderidParameter = new SqlParameter("@Dayorderid", userOrderDto.DayOrderId);
            var dishtypeidParameter = new SqlParameter("@Dishtypeid", userOrderDto.CategoryId);
            var quantityParameter = new SqlParameter("@Quantity", userOrderDto.Quantity);

            context.Database.ExecuteSqlCommand("exec UpdateDishQuantityBySU @Dayorderid,@Dishtypeid,@Quantity",
                dayorderidParameter, dishtypeidParameter, quantityParameter);
        }

        /// <summary>
        /// Обновляет пользовательский заказ со стороны SU - только факт
        /// </summary>
        /// <param name="context"></param>
        /// <param name="weekorderid"></param>
        /// <returns></returns>
        public static void OrderAllByOne(this ApplicationDbContext context, int weekorderid)
        {
            var weekorderidParameter = new SqlParameter("@WeekOrdId", weekorderid);

            context.Database.ExecuteSqlCommand("exec OrderAllByOne @WeekOrdId", weekorderidParameter);
        }

        /// <summary>
        /// Создаёт такой же заказ на данной неделе 
        ///  как и на предыдущей для данного пользователя
        /// </summary>
        /// <param name="context"></param>
        /// <param name="weekorderid"></param>
        /// <returns></returns>
        public static void OrderAsPrewWeek(this ApplicationDbContext context, int weekorderid)
        {
            var weekorderidParameter = new SqlParameter("@WeekOrdId", weekorderid);

            context.Database.ExecuteSqlCommand("exec OrderAsPrevWeek @WeekOrdId", weekorderidParameter);
        }

        #endregion

        #region Оплаты

        /// <summary>
        /// Возвращает значения оплаты за каждую позицию блюда для указанного недельного заказа
        /// </summary>
        /// <param name="context"></param>
        /// <param name="weekordid">id недельного заказа пользователя</param>
        /// <returns></returns>
        public static async Task<double[]> WeekPaimentByOrderId(this ApplicationDbContext context, int weekordid)
        {
            var weekordidParameter = new SqlParameter("@Weekordid", weekordid);
            return context.Database.SqlQuery<double>("SELECT DishPaiment FROM WeekPaimentByOrderId(@Weekordid)", weekordidParameter)
                .ToArrayAsync().Result;
        }

        /// <summary>
        /// Суммы по планируемым оплатам на заказанное количество каждого блюда в меню на неделю
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <returns></returns>
        public static async Task<double[]> SumWeekPaimentsByDishes(this ApplicationDbContext context, WeekYearDto wyDto)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            return context.Database.SqlQuery<double>("SELECT SumByDishes FROM SumWeekPaimentsByDishes(@Week,@Year)",
                weekParameter, yearParameter)
                .ToArrayAsync().Result;
        }

        /// <summary>
        /// Обновляет значение заказа на блюдо а также сумму заказа на день и рабочую неделю
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userweekpaiDto"></param>
        /// <returns></returns>
        public static double UpdateWeekPaiment(this ApplicationDbContext context, UpdateWeekPaimentDto userweekpaiDto)
        {
            var paiidParameter = new SqlParameter("@Paid", userweekpaiDto.Id);
            var paivalueParameter = new SqlParameter("@PaiValue", userweekpaiDto.Paiment);
            //var curBalanceParameter = new SqlParameter
            //{
            //    ParameterName = "@CurBalance",
            //    DbType = DbType.Double,
            //    Direction = ParameterDirection.Output
            //};

            return context.Database.SqlQuery<double>("exec UpdateWeekPaiment @Paid,@PaiValue",
                paiidParameter, paivalueParameter).FirstOrDefaultAsync().Result;
             //Convert.ToDouble(curBalanceParameter.Value);
        }
    #endregion

        #region Меню
        /// <summary>
        /// Создаёт новое меню на неделю и заполняет его пустыми блюдами
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wyDto">Объект, содержащий номера недели и года для запроса</param>
        /// <returns></returns>
        public static void CreateNewWeekMenu(this ApplicationDbContext context, WeekYearDto wyDto)
        {
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);

            context.Database.ExecuteSqlCommand("exec CreateNewWeekMenu @Week,@Year",
                weekParameter, yearParameter);
        }
        #endregion
    }
}
