using System;
using System.Collections.Generic;

namespace NEHGSWebApiCore.Domain.Entities
{
    public class ImportFile
    {
        //public ImportFile()
        //{
        //    ImportLogs = new HashSet<ImportLog>();
        //    ImportRecords = new HashSet<ImportRecord>();
        //}

        public int ImportFileId { get; set; }
        public int? ImportMappingId { get; set; }
        public int? TargetCollectionId { get; set; }
        public string SourceFilename { get; set; }
        public int? DefaultRecordTypeId { get; set; }
        public int? ImportedRows { get; set; }
        public DateTime? ImportStart { get; set; }
        public DateTime? ImportEnd { get; set; }
        public string ImportedBy { get; set; }
        public int? ImportFileStatusId { get; set; }
        public int? TransferredRows { get; set; }
        public DateTime? TransferStart { get; set; }
        public DateTime? TransferEnd { get; set; }
        public string ProcessMessage { get; set; }
        public bool? HasError { get; set; }
        public int? ImportSerializeId { get; set; }
        public string ImportName { get; set; }

        // navigation
        public virtual Collection Collection { get; set; }
        public virtual ImportSerializedMapping ImportSerializedMapping { get; set; }

        //public virtual ICollection<ImportLog> ImportLogs { get; set; }
        //public virtual ICollection<ImportRecord> ImportRecords { get; set; }
    }
}
