using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tableServer
{
    class linkTable : Table
    {
        public string name = null;
        public string getCustomerInf() {
            string inf = "";
            inf += ((linkCustomer)((linkTableOwner)owner).owner).name;
            foreach(linkCustomer c in insides)
            {
                if (c != null)
                {
                    inf += '/';
                    inf += c.name;
                }
                
            }
            
            return inf;
        }
        public linkTable(Customer o,Dictionary<byte,object> args)
        {
            owner = new linkTableOwner(o);
            insides = new Customer[MAX_NUM-1];
            name = (string)args[0];
        }
        public override int MAX_NUM
        {
            get
            {
                return 2;
            }
        }
        public int numInside
        {
            get
            {
                int num = 1;
                foreach(Customer c in insides)
                {
                    if(c!=null)
                        num++;
                }
                return num;
            }
        }
        public TableOwner Owner {
            get {
                return owner;
            }
        }
    }
}
