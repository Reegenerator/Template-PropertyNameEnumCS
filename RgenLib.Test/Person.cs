using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RgenLib.Test {
    class Person {
        #region Reegenerator:{Template:"PropertyNameEnum",Regen:Always,V:1.0.0,Date:"2014-06-09T10:47:44.7338212+08:00"}
        public static class PropertyNames {

            public const System.String Name = "Name";

            public const System.String Address = "Address";

        }

        #endregion

 



 

        string Name { get; set; }
        string Address { get; set; }
    }
}
