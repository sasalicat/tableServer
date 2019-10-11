using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace tableServer
{
    class linkCustomer : Customer
    {
        public delegate void action(string args);

        public const int BUFFER_SIZE = 10240;
        protected byte[] buffer;
        protected int orderCounting=0;
        protected List<action> actionList;
        protected IPEndPoint linkedEndPoint =null;
        //protected Socket linkedSocket=null;
        protected string passward = null;
        public digProcess activeProcess = null;
        public string name;
        public bool linkedReady {
            get
            {
                if (linkedEndPoint != null)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        protected virtual void action0_normalGreeting(string args)
        {
            Program.Log("custom "+Id+":[action 0]"+args);
            name = args;
            string passward = "1q2w3e4r";
            this.passward = passward;
            string packet = "1~" + Id + "," + passward + "|";
            Socket.Send(Encoding.UTF8.GetBytes(packet));
        }
        protected virtual void action1_linkedGreeting(string args)
        {
            Program.Log("custom " + Id + ":[action 1]" + args);
            string[] parts = args.Split(',');
            int tragetId;
            if (!int.TryParse(parts[0],out tragetId))
            {
                Program.Log("格式錯誤:Id部分"+parts[0]+"不是數字");
                return;
            }
            if (!Server.Main.linked(tragetId, parts[1], this))
            {
                Program.Log("連接失敗");
                return;
            }
            Server.Main.decompositionCustomer(this);//把自身從customerList中remove,節省列表的空間


        }
        protected virtual void action2_requestTableList(string args)
        {
            Program.Log("custom " + Id + ":[action 2]" + args);
            string inf = "0~";
            var enums= Server.Main.getTableList();
            while (enums.MoveNext()) {
                linkTable table = (linkTable)enums.Current.Value;
                inf += enums.Current.Key;
                inf += ';';
                inf += (table.numInside);
                inf += ';';
                inf += table.name;
                inf += ';';
                inf += table.getCustomerInf();
                inf += ':';
            }
            inf += '|';
            Socket.Send(Encoding.UTF8.GetBytes(inf));
        }
        protected virtual void aftCreateTable(byte code,linkTable room)
        {
            string packet = "3~";
            packet += code + ";";
            packet += room.getCustomerInf() + "|";
            Socket.Send(Encoding.UTF8.GetBytes(packet));
        }
        protected virtual void action3_createTable(string args)
        {
            Program.Log("custom " + Id + ":[action 3]" + args);
            Dictionary<byte, object> tableArg = new Dictionary<byte, object>();
            tableArg[0] = args;
            byte code= Server.Main.createTable(this, tableArg);
            

        }
        protected virtual void action4_connectRequest(string args)
        {
            Program.Log("custom " + Id + ":[action 4]" + args);
            int tableId;
            if (!int.TryParse(args, out tableId))
            {
                Program.Log("連接請求錯誤,id:" + args + "不是數字");
                return;
            }
            else {
                Server.Main.tryConnectTable(tableId, this);
            }
        }
        protected virtual void action5_processResult(string args){
            Program.Log("custom " + Id + ":[action 5]" + args);
            int resultCode;
            if (int.TryParse(args, out resultCode)) {
                if (resultCode == 0)
                    activeProcess.next(false);
                else if (resultCode == 1)
                    activeProcess.next(true);
                else {
                    activeProcess.end();
                }

             }
        }
        protected virtual void action6_rename(string arg)
        {
            name = arg;
        }
        protected virtual void initActionList() {
            actionList = new List<action>();
            actionList.Add(action0_normalGreeting);//action 0,普通的greeting
            actionList.Add(action1_linkedGreeting);
            actionList.Add(action2_requestTableList);
            actionList.Add(action3_createTable);
            actionList.Add(action4_connectRequest);
            actionList.Add(action5_processResult);
            actionList.Add(action6_rename);
        }
        public linkCustomer(int id, Socket socket) : base(id, socket)
        {
            buffer = new byte[BUFFER_SIZE];
            initActionList();
        }

        public virtual void listening()
        {
            if (buffer != null)
            {
                int length = Socket.Receive(buffer);
                if(length == 0)
                {
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                    throw new SocketException();
                }
                encodePacket(buffer, length);
            }
            
        }
        public virtual bool setLinked(string passward, IPEndPoint traget)
        {
            
            if (passward == this.passward)
            {
                linkedEndPoint = traget;
                //linkedSocket = traget.Socket;
                Program.Log("連接成功! customer" + Id + "->ip" + traget.Address+" port:"+traget.Port);
                return true;
            }
            else
            {
                Program.Log("連接失敗密碼不符!密碼是:"+this.passward+",輸入密碼是:"+passward);
                return false;
            }
        }
        public override void encodePacket(byte[] packet,int length)
        {
            string message = Encoding.UTF8.GetString(packet, 0, length);
            Console.WriteLine("id"+Id+":"+message);
            string[] orders = message.Split('|');//使用|切分每一個指令
            foreach(string order  in orders)
            {
                if (order != "")//split會切出""
                {
                    orderCounting++;
                    string[] part = order.Split('~');//使用~切分action code 和 action arg
                    if (part.Length != 2)
                    {
                        Program.Log("custom " + Id + "第" + orderCounting + "條order:" + order + " 格式錯誤,不予處理");
                        continue;
                    }
                    int code;
                    if (int.TryParse(part[0], out code))
                    {
                        actionList[code](part[1]);
                    }
                    else
                    {
                        Program.Log("custom " + Id + "第" + orderCounting + "條order:" + order + " order code不是數字,不予處理");
                        continue;
                    }
                }
            }
        }
        public virtual void tryConnect(linkCustomer other)
        {
            string ip = other.linkedEndPoint.Address.ToString();//((IPEndPoint)other.linkedSocket.RemoteEndPoint).Address.ToString();
            int port = other.linkedEndPoint.Port;//((IPEndPoint)other.linkedSocket.RemoteEndPoint).Port;
            string packet = "2~"+ip+","+port+"|";
            Socket.Send(Encoding.UTF8.GetBytes(packet));
        }
        public void releseAbuseData() {
            buffer = null;
            actionList = null;
            passward = null;
        }
        public void CloseSocket()
        {
            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
        }

    }
}
