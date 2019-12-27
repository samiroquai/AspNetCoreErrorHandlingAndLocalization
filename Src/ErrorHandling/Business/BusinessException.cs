using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Business
{
    public class BusinessException:Exception
    {
        public BusinessExceptionCode Code { get; }

        public BusinessException(BusinessExceptionCode code)
        {
            Code = code;
        }
    }
}
