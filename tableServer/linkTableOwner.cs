using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tableServer
{
    class linkTableOwner : TableOwner
    {
        public Customer owner;
        public linkTableOwner(Customer owner)
        {
            this.owner = owner;
        }
        public override void onCreateTableRequst(byte answerCode)
        {
            
        }
    }
}
