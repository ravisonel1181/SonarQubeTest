using NEHGSWebApiCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<ImportFile> ImportFiles { get; }
        IRepository<ImportSerializedMapping> ImportSerializedMappings { get; }
        IRepository<Collection> Collections { get; }
        IRepository<RecordRole> RecordRoles { get; }
        IRepository<CollectionAttribute> Attributes { get; }
        IRepository<ExtendedAttribute> ExtendedAttributes { get; }
        IRepository<ErrorCheckResult> ErrorCheckResults { get; }

        Task<int> SaveChangesAsync();
    }
}
