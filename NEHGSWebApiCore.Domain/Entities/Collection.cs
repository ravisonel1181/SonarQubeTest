using System;
using System.Collections.Generic;

namespace NEHGSWebApiCore.Domain.Entities
{
    public class Collection
    {
        public Collection()
        {
            Attributes = new HashSet<object>();
            Records = new HashSet<object>();
            CollectionVolumes = new HashSet<object>();
            Records1 = new HashSet<object>();
            CollectionColumns = new HashSet<object>();
            ImportFiles = new HashSet<ImportFile>();
            IndexLogs = new HashSet<object>();
            Locations = new HashSet<object>();
        }

        public int CollectionId { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public string SearchTips { get; set; }
        public short? StartYear { get; set; }
        public short? EndYear { get; set; }
        public bool HasVolumes { get; set; }
        public bool HasPages { get; set; }
        public bool ResultsAsPages { get; set; }
        public string Citations { get; set; }
        public string Description { get; set; }
        public bool IsOffline { get; set; }
        public bool IsHTML { get; set; }
        public string ResultsTemplate { get; set; }
        public byte AccessLevel { get; set; }
        public bool ShowVolumeName { get; set; }
        public bool ShowPageName { get; set; }
        public bool IsFamilySearch { get; set; }
        public int? ImageId1 { get; set; }
        public int? ImageId2 { get; set; }
        public int? ImageId3 { get; set; }
        public string Caption1 { get; set; }
        public string Caption2 { get; set; }
        public string Caption3 { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public string ImageTargetLink { get; set; }
        public bool ErrorReportEnabled { get; set; }
        public string ErrorReportMessage { get; set; }
        public bool Exclusive { get; set; }
        public int? DefaultImage { get; set; }

        public virtual ICollection<object> Attributes { get; set; }
        public virtual object CollectionCategory { get; set; }
        public virtual ICollection<object> Records { get; set; }
        public virtual ICollection<object> CollectionVolumes { get; set; }
        public virtual ICollection<object> Records1 { get; set; }
        public virtual ICollection<object> CollectionColumns { get; set; }
        public virtual ICollection<ImportFile> ImportFiles { get; set; }
        public virtual ICollection<object> IndexLogs { get; set; }
        public virtual ICollection<object> Locations { get; set; }
    }
}
