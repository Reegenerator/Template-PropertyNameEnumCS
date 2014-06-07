using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RgenLib.Test {
    class Person {
        #region <Gen Renderer='PropertyNameEnum' Date='06/06/2014 16:40:50' />
        public static class PropertyNames {

            public const System.String Name = "Name";

            public const System.String Address = "Address";

        }

        #endregion

    
        string Name { get; set; }
        string Address { get; set; }
    }
}
