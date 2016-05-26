using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;

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
        /// <returns></returns>
        public static void DayFactToPlan(this ApplicationDbContext context)
        {
            context.Database.ExecuteSqlCommand("exec DayFactToPlan");
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
        public static void UpdateWeekPaiment(this ApplicationDbContext context, UpdateWeekPaimentDto userweekpaiDto)
        {
            var paiidParameter = new SqlParameter("@Paid", userweekpaiDto.Id);
            var paivalueParameter = new SqlParameter("@PaiValue", userweekpaiDto.Paiment);

            context.Database.ExecuteSqlCommand("exec UpdateWeekPaiment @Paid,@PaiValue",
                paiidParameter, paivalueParameter);
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
