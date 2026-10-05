using SmileAnalysisDal.Data.Contexts;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisDal.Repositories.Classes;

public class UnitOfWork : IUnitOfWork
{
    private readonly SmileAnalysisDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(SmileAnalysisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new()
    {
        var entityType = typeof(TEntity);
        if (_repositories.TryGetValue(entityType, out var repo))
            return (IGenericRepository<TEntity>)repo;

        var newRepo = new GenericRepository<TEntity>(_dbContext);
        _repositories[entityType] = newRepo;
        return newRepo;
    }

    public int SaveChanges() => _dbContext.SaveChanges();
}
