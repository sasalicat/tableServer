using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
namespace tableServer
{
    class Server : tableServer
    {
        private Socket SocketWatch;
        //private Dictionary<string, Socket> ClientConnectionItems;
        private Dictionary<int,Table> tableList;
        private Customer[] customerList;
        private List<Customer> subCustomers;
        private static Server main;
        public static Server Main {//這樣是為了顯示Main為已讀,防止其他類別寫Server.Main = null
            get {
                return main;
            }
        }
        public int MAX_CONNECT_NUM
        {
            get
            {
                return 200;
            }
        }

        public int MAX_TABLE_NUM
        {
            get
            {
                return 100;
            }
        }

        public Dictionary<int, Table>.Enumerator getTableList()
        {
            return tableList.GetEnumerator();
        }
        public bool createTable(Customer asker, Dictionary<byte, object> args)
        {
            if (tableList.Count<=MAX_TABLE_NUM && !tableList.ContainsKey(asker.Id) && ((linkCustomer)asker).linkedReady)
            {
                linkTable temp = new linkTable(asker,args);
                tableList[asker.Id] = temp;
                return true;
            }
            return false;
        }


        public void updateTable(Customer asker, Dictionary<byte, object> args)
        {
            throw new NotImplementedException();
        }
        void WatchConnecting()
        {
            Console.WriteLine("開啟監聽......");


            //持續不斷監聽客戶端發來的請求     
            while (true)
            {
                Customer newCustomer = null;
                // string cID;
                try
                {
                    Socket connection = SocketWatch.Accept();
                    
                    bool find = false;
                    for(int id = 0; id < MAX_CONNECT_NUM; id++)
                    {
                        if (customerList[id]==null)
                        {
                            newCustomer = new linkCustomer(id,connection);
                            //Console.WriteLine("找到id:"+id+"newCustomer:"+ newCustomer);
                            find = true;
                            customerList[id] = newCustomer;
                            Thread thread = new Thread(recv);
                            //設定為後臺執行緒，隨著主執行緒退出而退出 
                            thread.IsBackground = true;
                            //啟動執行緒
                            Console.WriteLine("new customer ip:" + ((IPEndPoint)connection.RemoteEndPoint).Address+" port:" + ((IPEndPoint)connection.RemoteEndPoint).Port);
                            thread.Start(newCustomer);
                            break;
                        }

                    }
                    if (!find)
                    {
                        Console.WriteLine("connect close");
                        connection.Shutdown(SocketShutdown.Both);
                        connection.Close();
                    }

                }
                catch (Exception ex)
                {
                    //提示套接字監聽異常     
                    Console.WriteLine(ex.Message);
                    break;
                }
                //byte[] idrec = new byte[1024 * 1024];
                //int length = connection.Receive(idrec);
                //var cID = Encoding.UTF8.GetString(idrec, 0, length);
                //ClientConnectionItems.Add(cID, connection);
                //顯示與客戶端連線情況
                //Console.WriteLine("\r\n[客戶端\"" + cID + "\"建立連線成功！ 客戶端數量：" + ClientConnectionItems.Count + "]");
                //獲取客戶端的IP和埠號  
                //IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                //int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;
                //string sendmsg = "[" + "本端：" + cID + " 連線服務端成功！]";
                //byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                //connection.Send(arrSendMsg);
                //建立一個通訊執行緒      

            }
        }
        public Server(IPEndPoint ipe)
        {
            if (main == null)
            {
                SocketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //監聽繫結的網路節點  
                SocketWatch.Bind(ipe);
                //將套接字的監聽佇列長度限制為20  
                SocketWatch.Listen(20);
                main = this;
            }
            else {
                Console.WriteLine("錯誤!已經有一個Server物件存在");
            }
        }
        void recv(object socketclientpara)
        {
            linkCustomer customer = (linkCustomer)socketclientpara;
            Console.WriteLine("customer:"+customer);
            while (true)
            {
                try
                {
                    customer.listening();
                    //customer.encodePacket
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception:" + e.Message);
                    Console.WriteLine("customer" + customer.Id + "斷開連接");
                    customer.CloseSocket();
                    customerList[customer.Id] = null;
                    if (tableList.ContainsKey(customer.Id))
                    {
                        tableList[customer.Id] = null;
                    }
                    break;
                }
            }
            /*
            Socket socketServer = socketclientpara as Socket;

            while (true)
            {
                try
                {
                    byte[] arrServerRecMsg = new byte[1024 * 1024];
                    int length = socketServer.Receive(arrServerRecMsg);
                    //將機器接受到的位元組陣列轉換為人可以讀懂的字串     
                    string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                    string ccID = strSRecMsg.Substring(0, 4);
                    int len = strSRecMsg.Length;
                    string hc = ccID + ":" + strSRecMsg.Substring(4, len - 4);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]\r\n" + strSRecMsg);
                    //判斷是否包含這個客戶端
                    bool contains = ClientConnectionItems.ContainsKey(ccID);
                    if (contains)
                    {
                        ClientConnectionItems[ccID].Send(Encoding.UTF8.GetBytes(hc));
                    }
                    else
                    {
                        Console.WriteLine("輸入有誤，不予轉發\r\n");
                    }
                    arrServerRecMsg.DefaultIfEmpty();
                }
                catch (Exception)
                {
                    string temp = ClientConnectionItems.First().Key;
                    //提示套接字監聽異常  
                    Console.WriteLine("\r\n[客戶端\"" + socketServer.RemoteEndPoint + "\"已經中斷連線！ 客戶端數量：" + ClientConnectionItems.Count + "]");
                    ClientConnectionItems.Remove(ClientConnectionItems.First().Key);
                    Console.WriteLine("\r\n[客戶端\"" + temp + "\"已經中斷連線！ 客戶端數量：" + ClientConnectionItems.Count + "]");
                    break;
                }
            }*/
        }
        public void start()
        {
            customerList = new Customer[MAX_CONNECT_NUM];
            subCustomers = new List<Customer>();
            tableList = new Dictionary<int, Table>();
            //測試code--------------------------------------
            linkCustomer c1 = new linkCustomer(94,null);
            c1.name = "喵喵";
            tableList[94] = new linkTable(c1, new Dictionary<byte, object>() { {0,"房間1"} });
            linkCustomer c2 = new linkCustomer(87, null);
            c2.name = "吼!一起上幫老e拔毛";
            tableList[87] = new linkTable(c2, new Dictionary<byte, object>() { { 0, "房間2" } });
            //----------------------------------------------
            Thread threadwatch = new Thread(WatchConnecting);
            //將窗體執行緒設定為與後臺同步，隨著主執行緒結束而結束  
            threadwatch.IsBackground = true;
            //啟動執行緒     
            threadwatch.Start();

            Console.WriteLine("點選輸入任意資料回車退出程式......");
            Console.ReadKey();
            SocketWatch.Close();
        }

        public bool linked(int index, string pwd, Customer sub)
        {
            if (customerList[index] == null) {
                return false;
            }
            return ((linkCustomer)customerList[index]).setLinked(pwd, sub);
        }
        public void decompositionCustomer(Customer customer)
        {
            customerList[customer.Id] = null;
            ((linkCustomer)customer).releseAbuseData();
            subCustomers.Add(customer);
        }

        public void tryConnectTable(int ownerId, Customer asker)
        {
            linkTable traget = (linkTable)tableList[ownerId];
            if (traget.numInside < traget.MAX_NUM)
            {
                var host = ((linkTableOwner)traget.Owner).owner;
                new digProcess(host, asker);
            }
        }
    }
    class Program
    {
        //private Socket SocketWatch;
        //Dictionary<string, Socket> ClientConnectionItems;
        public static bool log2Screen = true;

        /*
        testServer(IPEndPoint ipe)
        {
            SocketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //監聽繫結的網路節點  
            SocketWatch.Bind(ipe);
            //將套接字的監聽佇列長度限制為20  
            SocketWatch.Listen(20);
        }
        void WatchConnecting()
        {
            Console.WriteLine("開啟監聽......");

            Socket connection = null;
            //持續不斷監聽客戶端發來的請求     
            while (true)
            {
                // string cID;
                try
                {
                    connection = SocketWatch.Accept();
                }
                catch (Exception ex)
                {
                    //提示套接字監聽異常     
                    Console.WriteLine(ex.Message);
                    break;
                }
                byte[] idrec = new byte[1024 * 1024];
                int length = connection.Receive(idrec);
                var cID = Encoding.UTF8.GetString(idrec, 0, length);
                ClientConnectionItems.Add(cID, connection);
                //顯示與客戶端連線情況
                Console.WriteLine("\r\n[客戶端\"" + cID + "\"建立連線成功！ 客戶端數量：" + ClientConnectionItems.Count + "]");
                //獲取客戶端的IP和埠號  
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;
                string sendmsg = "[" + "本端：" + cID + " 連線服務端成功！]";
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                connection.Send(arrSendMsg);
                //建立一個通訊執行緒      
                Thread thread = new Thread(recv);
                //設定為後臺執行緒，隨著主執行緒退出而退出 
                thread.IsBackground = true;
                //啟動執行緒     
                thread.Start(connection);
            }
        }
        void recv(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;

            while (true)
            {
                try
                {
                    byte[] arrServerRecMsg = new byte[1024 * 1024];
                    int length = socketServer.Receive(arrServerRecMsg);
                    //將機器接受到的位元組陣列轉換為人可以讀懂的字串     
                    string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                    string ccID = strSRecMsg.Substring(0, 4);
                    int len = strSRecMsg.Length;
                    string hc = ccID + ":" + strSRecMsg.Substring(4, len - 4);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]\r\n" + strSRecMsg);
                    //判斷是否包含這個客戶端
                    bool contains = ClientConnectionItems.ContainsKey(ccID);
                    if (contains)
                    {
                        ClientConnectionItems[ccID].Send(Encoding.UTF8.GetBytes(hc));
                    }
                    else
                    {
                        Console.WriteLine("輸入有誤，不予轉發\r\n");
                    }
                    arrServerRecMsg.DefaultIfEmpty();
                }
                catch (Exception)
                {
                    string temp = ClientConnectionItems.First().Key;
                    //提示套接字監聽異常  
                    Console.WriteLine("\r\n[客戶端\"" + socketServer.RemoteEndPoint + "\"已經中斷連線！ 客戶端數量：" + ClientConnectionItems.Count + "]");
                    ClientConnectionItems.Remove(ClientConnectionItems.First().Key);
                    Console.WriteLine("\r\n[客戶端\"" + temp + "\"已經中斷連線！ 客戶端數量：" + ClientConnectionItems.Count + "]");
                    break;
                }
            }
        }
        public void start()
        {
            ClientConnectionItems = new Dictionary<string, Socket>();
            Thread threadwatch = new Thread(WatchConnecting);
            //將窗體執行緒設定為與後臺同步，隨著主執行緒結束而結束  
            threadwatch.IsBackground = true;
            //啟動執行緒     
            threadwatch.Start();

            Console.WriteLine("點選輸入任意資料回車退出程式......");
            Console.ReadKey();
            SocketWatch.Close();
        }*/

        static void Main(string[] args)
        {
            //埠號（用來監聽的）
            int port = 6000;
            IPAddress ip = IPAddress.Any;
            //將IP地址和埠號繫結到網路節點point上  
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Server server = new Server(ipe);
            //testServer server = new testServer(ipe);
            server.start();

        }
        public static void Log(string msg) {
            if (log2Screen)
                Console.WriteLine(msg);
        }
    }
}
