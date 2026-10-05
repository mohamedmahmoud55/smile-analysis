namespace SmileAnalysisDal.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity>
    {

        IEnumerable<TEntity> GetAll(Func<TEntity, bool>? condition = null);

        TEntity? GetById(int id);

        void Add(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

    }
}
