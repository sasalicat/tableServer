using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
namespace tableServer
{
    abstract class Customer
    {
        private Socket socket;
        public Socket Socket
        {
            get
            {
                return socket;
            }
        }
        private int id;
        public int Id
        {
            get
            {
                return id;
            }

        }
        public Customer(int id,Socket socket)
        {
            this.id = id;
            this.socket = socket;
        }
        public abstract void encodePacket(byte[] packet,int length);
    }
    abstract class TableOwner
    {
        protected Customer inf;
        public abstract void onCreateTableRequst(byte answerCode);
    }
    abstract class Table
    {
        public abstract int MAX_NUM //用於表示房間多人數
        {
            get;
        }
        protected TableOwner owner;
        protected Customer[] insides;

    }
    interface tableServer
    {
        int MAX_TABLE_NUM
        {
            get;
        }
        int MAX_CONNECT_NUM
        {
            get;
        }
        bool linked(int index, string pwd, Customer sub);
        bool createTable(Customer asker, Dictionary<byte,object> args);
        Dictionary<int, Table>.Enumerator getTableList();//改成由customer呼叫,藉由此function customer得知目前的table情況如何
        void updateTable(Customer asker, Dictionary<byte, object> args);
        void tryConnectTable(int ownerId, Customer asker);
    }
}
