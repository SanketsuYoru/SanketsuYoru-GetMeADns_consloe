using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;

namespace GotMeAnDns_consloe
{
    class Program
    {
        static private string[] Dnslist = { "114.114.114.114", "114.114.115.115", "8.8.8.8", "8.8.4.4", "119.29.29.29", "182.254.116.116", "101.226.4.6", "218.30.118.6", "180.76.76.76", "1.2.4.8", "202.98.0.68", "208.67.222.222", "208.67.220.220", "1.1.1.1" };
        public class dnsinfo
        {
            public long ping;
            public string Adress;
        }
        public class dnsSettings
        {
            public string main;
            public string sub;
        }
        static private List<dnsinfo> getLowDelayDns()
        {
            List<dnsinfo> dnsif = new List<dnsinfo>();
            long mainPing = 999999;
            long subPing = 999999;
            var MainDns = "Error";
            var SubDns = "Error";
            bool net = false;
            foreach (var dns in Dnslist)
            {
                var CurrentPing = Getping(dns);
                if (CurrentPing != 0)
                {
                    if (CurrentPing < mainPing)
                    {
                        if (mainPing < subPing)
                        {
                            subPing = mainPing;
                            SubDns = MainDns;
                        }
                        mainPing = CurrentPing;
                        MainDns = dns;
                        net = true;
                    }
                    else if (CurrentPing < subPing)
                    {

                        subPing = CurrentPing;
                        SubDns = dns;
                        net = true;
                    }
                }
            }
            if (net == true)
            {
                dnsif.Add(new dnsinfo { ping = mainPing, Adress = MainDns });
                dnsif.Add(new dnsinfo { ping = subPing, Adress = SubDns });
                return dnsif;
            }
            else
            {
                return dnsif;
            }
        }

        static private long Getping(string dns)
        {

            // bool online = false; //是否在线
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(dns);
            if (pingReply.Status == IPStatus.Success)
            {
                // online = true;
                Console.WriteLine(dns + " Online Delay=" + pingReply.RoundtripTime + "ms");
                return pingReply.RoundtripTime;
            }
            else
            {
                Console.WriteLine(dns + " Offline");
                return 9999999;
            }


        }


        public static void displayandset()
        {
            var minDelay = getLowDelayDns();
            dnsSettings dnsSet = new dnsSettings();
            bool isSet = false;
            Console.WriteLine();
            Console.WriteLine();
            foreach (var item in minDelay)
            {
                Console.WriteLine("Dns Server :" + item.Adress);
                Console.WriteLine("Ping :" + item.ping + " ms");
            }

            //设置dns

            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            foreach (ManagementObject mo in moc)
            {
                if (minDelay.Count != 0)
                {

                    inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    inPar["DNSServerSearchOrder"] = new string[] { minDelay[0].Adress, minDelay[1].Adress }; // 1.DNS 2.备用DNS
                    outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                }

            }

            //检查是否设置成功

            NetworkInterface[] ifs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface netif in ifs)
            {
                IPInterfaceProperties properties = netif.GetIPProperties();
                IPAddressCollection Dnslist = properties.DnsAddresses;
                if (Dnslist.Count > 0)
                {

                    if (Dnslist[0].ToString() == minDelay[0].Adress.ToString() && Dnslist[0].ToString() == minDelay[0].Adress.ToString())
                    {
                        isSet = true;
                        dnsSet.main = Dnslist[0].ToString();
                        dnsSet.sub = Dnslist[1].ToString();
                    }
                }



            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("当前Dns");
            Console.WriteLine("Dns：" + dnsSet.main);
            Console.WriteLine("备用Dns：" + dnsSet.sub);
            if (isSet == true)
            {
                Console.WriteLine("设置成功！");
            }
            else
            {
                Console.WriteLine("设置失败，需要提升权限！");
            }
        }

        static void Main(string[] args)
        {

            displayandset();
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }

    }
}
