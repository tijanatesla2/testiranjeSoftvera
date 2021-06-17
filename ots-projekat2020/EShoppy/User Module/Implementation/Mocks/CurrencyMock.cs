using EShoppy.Finantial_Module.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShoppy.User_Module.Implementation.Mocks
{
    public class CurrencyStub : ICurrency
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Value { get; set; }
        public CurrencyStub()
        {
            ID = Guid.NewGuid();
            Name = "stub";
            Code = "stub";
            Value = 1;
        }
    }
}
