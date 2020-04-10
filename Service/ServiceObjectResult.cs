using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServiceObjectResult<TEntity> : ServiceResult
    {
        [DataMember]
        public TEntity Data { get; set; }

        public virtual void SetData(TEntity entity)
        {
            this.Data = entity;
        }
    }
}
