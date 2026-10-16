using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Domain.Entities
{
    public class ErrorCheckResult
    {
        public int Id { get; set; }
        public string ProgressId { get; set; }
        public string SerializedErrors { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
