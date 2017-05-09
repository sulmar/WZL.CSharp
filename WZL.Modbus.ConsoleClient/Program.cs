﻿using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WZL.Modbus.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {

            GetTemperatureTest();
        }

        private static void GetTemperatureTest()
        {
            var hostname = ConfigurationManager.AppSettings["hostname"];
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var slaveId = byte.Parse(ConfigurationManager.AppSettings["N30U_Temp"]);

            ushort startAddress = 7010;
            ushort numRegisters = 2;

            Console.WriteLine($"Connecting to {hostname}:{port}");

            // Określa zakres działania obiektu i utomatycznie wywołanie metodę Dispose
            // UWAGA: using można użyć tylko dla obiektów, które implementują interfejs IDisposable

            using (var client = new TcpClient(hostname, port))
            using (var master = ModbusIpMaster.CreateIp(client))
            {
                ushort[] inputs = master.ReadInputRegisters(slaveId, startAddress, numRegisters);

                float temperature = Converter.ConvertToFloat(inputs[0], inputs[1]);

                Console.WriteLine($"Temperature: {temperature} °C");
            }



        }
    }
}