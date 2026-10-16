using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.DTOs
{
    public class UploadResultDto
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public string SizeFormatted { get; set; }
    }
}
