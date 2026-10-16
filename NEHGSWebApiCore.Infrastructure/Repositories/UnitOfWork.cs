using NEHGSWebApiCore.Application.Interfaces.Repositories;
using NEHGSWebApiCore.Domain.Entities;
using NEHGSWebApiCore.Infrastructure.Data;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<ImportFile> _importFiles;
        private IRepository<ImportSerializedMapping> _importSerializedMappings;
        private IRepository<Collection> _collections;
        private IRepository<RecordRole> _recordRoles;
        private IRepository<CollectionAttribute> _attributes;
        private IRepository<ExtendedAttribute> _extendedAttributes;
        private IRepository<ErrorCheckResult> _errorCheckResults;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<ImportFile> ImportFiles => _importFiles ??= new Repository<ImportFile>(_context);
        public IRepository<ImportSerializedMapping> ImportSerializedMappings => _importSerializedMappings ??= new Repository<ImportSerializedMapping>(_context);
        public IRepository<Collection> Collections => _collections ??= new Repository<Collection>(_context);
        public IRepository<RecordRole> RecordRoles => _recordRoles ??= new Repository<RecordRole>(_context);
        public IRepository<CollectionAttribute> Attributes => _attributes ??= new Repository<CollectionAttribute>(_context);
        public IRepository<ExtendedAttribute> ExtendedAttributes => _extendedAttributes ??= new Repository<ExtendedAttribute>(_context);
        public IRepository<ErrorCheckResult> ErrorCheckResults => _errorCheckResults ??= new Repository<ErrorCheckResult>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
