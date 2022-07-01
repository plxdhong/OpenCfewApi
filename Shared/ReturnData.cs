using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CFEW.Shared
{
    public class ReturnData<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        public T Data {  get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
