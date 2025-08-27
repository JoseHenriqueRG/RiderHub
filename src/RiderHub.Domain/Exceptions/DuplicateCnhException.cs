using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiderHub.Domain.Exceptions
{
    public class DuplicateCnhException : Exception
    {
        public DuplicateCnhException() : base("A CNH with this number already exists.")
        {
        }
    }
}
