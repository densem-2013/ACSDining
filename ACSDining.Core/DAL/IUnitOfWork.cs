namespace ACSDining.Core.DAL
{
    public interface IUnitOfWork
    {
        void Dispose();
        void SubmitChanges();
        void Dispose(bool disposing);
        IRepository<T> Repository<T>() where T : class;
    }
}
