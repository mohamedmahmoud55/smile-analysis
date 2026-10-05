using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Repositories.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new();

    int SaveChanges();
}
