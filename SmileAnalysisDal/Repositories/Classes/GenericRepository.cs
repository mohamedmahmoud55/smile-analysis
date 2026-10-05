using Microsoft.EntityFrameworkCore;
using SmileAnalysisDal.Data.Contexts;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisDal.Repositories.Classes;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity, new()
{
    private readonly SmileAnalysisDbContext _dbContext;

    public GenericRepository(SmileAnalysisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public TEntity? GetById(int id) => _dbContext.Set<TEntity>().Find(id);

    public IEnumerable<TEntity> GetAll(Func<TEntity, bool>? condition = null)
    {
        if (condition is null)
            return _dbContext.Set<TEntity>().AsNoTracking().ToList();

        return _dbContext.Set<TEntity>().AsNoTracking().Where(condition).ToList();
    }

    public void Add(TEntity entity) => _dbContext.Set<TEntity>().Add(entity);

    public void Update(TEntity entity) => _dbContext.Set<TEntity>().Update(entity);

    public void Delete(TEntity entity) => _dbContext.Set<TEntity>().Remove(entity);
}
