using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReegeneratorCollection.Test {
    class Person {
        public static class PropertyNames {

            public const System.String Name = "Name";

            public System.String Address = "Address";

        }
        string Name { get; set; }
        string Address { get; set; }
    }
}
