namespace ACSDining.Core.DAL
{
    public interface IUnitOfWork
    {
        void Dispose();
        void SubmitChanges();
        void Dispose(bool disposing);
        IRepository<T> Repository<T>() where T : class;
        double[] GetUserWeekOrderDishes(int orderid);
        double[] GetUserWeekOrderPaiments(int orderid);
        double[] GetUnitWeekPrices(int menuforweekid);
        int GetNextWeekYear();
    }
}
