using System;
using System.Collections.Generic;

namespace NEHGSWebApiCore.Domain.Entities
{
    public class ImportSerializedMapping
    {
        public ImportSerializedMapping()
        {
            ImportFiles = new HashSet<ImportFile>();
        }

        public int ImportSerializeId { get; set; }
        public string MappingName { get; set; }
        public string SerializedMappings { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public bool? ActiveFlag { get; set; }

        public virtual ICollection<ImportFile> ImportFiles { get; set; }
    }
}
