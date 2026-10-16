using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Domain.Entities
{
    public class CollectionAttribute
    {
        public int AttributeId { get; set; }
        public int CollectionId { get; set; }
        public int ExtendedAttributeId { get; set; }
        public ExtendedAttribute ExtendedAttribute { get; set; }
    }
}
