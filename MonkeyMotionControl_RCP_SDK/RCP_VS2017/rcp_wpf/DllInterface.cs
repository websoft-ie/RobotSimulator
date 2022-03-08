using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rcp_wpf
{
    public struct CamInfo
    {
        public string ip_address;
        public string id;
        public string camInterface;
        public string pin;
    }

    class DllInterface
    {
        [DllImport("rcp_dll.dll", EntryPoint = "Add", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Add(int a, int b);


        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitMutex();

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteCameraConnection();

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RCPGetList(int id);

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string RCPGetLabel(int id);

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string RCPGet(int id);

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RCPDiscoveryStep();

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RCPDiscoveryGetList(out int length);

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RCPDiscoveryFreeList();

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RCPDiscoveryEnd();

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RCPDiscoveryProcessData(string data, int len, string from_ipv4);

        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RCPProcessData(string data, int len);



    }
}
