using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RgenLib.Test {
    class Person {
        #region <Gen Renderer='PropertyNameEnum' Date='2014-06-07T12:59:38.931Z' Regen='Always' />
        public static class PropertyNames {

            public const System.String Name = "Name";

            public const System.String Address = "Address";

        }

        #endregion
        string Name { get; set; }
        string Address { get; set; }
    }
}
