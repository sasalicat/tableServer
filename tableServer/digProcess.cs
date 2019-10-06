using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tableServer
{
    interface state
    {
        void action();
        void toNext(bool result);
    }
    class state_End : state
    {
        public state_End(digProcess process) {

        }
        public bool Complete = true;
        public void action()
        {
            
        }

        public void toNext(bool result)
        {
            return;
        }
    }
    class state_tryConncet : state
    {
        Customer node;
        Customer traget;
        state next1;
        state next2;
        digProcess process;
        public state_tryConncet(Customer node,Customer traget,state next1,state next2,digProcess process)
        {
            this.node = node;
            this.traget = traget;
            this.next1 = next1;
            this.next2 = next2;
            this.process = process;
        }


        public void action()
        {
            process.nowState = this;
            if (node != null)
            {
                ((linkCustomer)node).activeProcess = process;
                ((linkCustomer)node).tryConnect((linkCustomer)traget);
            }
        }
        public void toNext(bool result)
        {
            ((linkCustomer)node).activeProcess = null;

            if (result) {
                next1.action();
            }
            else
            {
                next2.action();
            }
        }
    }
    class digProcess
    {
        public state nowState;
        List<Customer> owner;
        public digProcess(Customer host,Customer client)
        {
            state_End se = new state_End(this);
            state_tryConncet s1 = new state_tryConncet(client,host,se,se,this);
            state_tryConncet s0 = new state_tryConncet(host, client, s1, s1, this);
            nowState = s0;
            ((linkCustomer)host).activeProcess = this;
            nowState.action();
        }
        public void end()
        {
            foreach(linkCustomer c in owner)
            {
                c.activeProcess = null;
            }
        }
        public void next(bool result)
        {
            nowState.toNext(result);
        }
    }
}
