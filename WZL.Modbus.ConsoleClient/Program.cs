﻿using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WZL.Modbus.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {

            ConsoleKeyInfo response;

            do
            {
                Console.WriteLine();
                Console.WriteLine("Wybierz rodzaj pomiaru:");
                Console.WriteLine("(T)emperature");
                Console.WriteLine("(V)oltage");
                Console.WriteLine("(B)inary");
                Console.WriteLine("(A)nalog");
                Console.WriteLine("(N)etwork");
                Console.WriteLine("(S)erial");

                response = Console.ReadKey(true);

                switch (response.KeyChar)
                {
                    case 'T': GetTemperatureTest(); break;
                    case 'V': GetVoltageTest(); break;
                    case 'B': SetBinaryTest(); break;
                    case 'A': SetAnalogTest(); break;
                    case 'N': GetACVoltageTest(); break;
                    case 'S': SerialPortTest(); break;
                }
            }
            while (response.Key != ConsoleKey.Escape);


        }




        private static void SerialPortTest()
        {
            var slaveId = byte.Parse(ConfigurationManager.AppSettings["SM4"]);
            var serialPort = ConfigurationManager.AppSettings["serialPort"];
            var baudRate = int.Parse(ConfigurationManager.AppSettings["baudRate"]);
            var dataBits = int.Parse(ConfigurationManager.AppSettings["dataBits"]);
            var parity = (Parity) Enum.Parse(typeof(Parity), ConfigurationManager.AppSettings["parity"]);
            var stopBits = (StopBits) Enum.Parse(typeof(StopBits), ConfigurationManager.AppSettings["stopBits"]);

            ushort startAddress = 2200;
            
            using (var port = new SerialPort(serialPort, baudRate, parity, dataBits, stopBits))
            using (var master = ModbusSerialMaster.CreateRtu(port))
            {
                port.Open();

                Console.WriteLine("Connected.");

                // Przykładowe wartości
                bool[] outputs = { true, false, false, true };

                // Zapis wielu rejestrów
                master.WriteMultipleCoils(slaveId, startAddress, outputs);

                // Odczyt wielu rejestrów
                bool[] inputs = master.ReadCoils(slaveId, startAddress, 4);

                // Wyświetlenie
                Console.WriteLine(String.Join(", ", inputs));

            }
        }

        private static void GetACVoltageTest()
        {
            var hostname = ConfigurationManager.AppSettings["hostname"];
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var slaveId = byte.Parse(ConfigurationManager.AppSettings["N10"]);

            // Napięcie zmienne L1 (2 rejestry 16-bitowe)
            ushort startAddress = 7000;
            ushort numRegisters = 60;

            Console.WriteLine($"Connecting to {hostname}:{port}");

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var master = ModbusIpMaster.CreateIp(client))
                {
                    Console.WriteLine("Connected.");

                    while (!Console.KeyAvailable)
                    {
                        ushort[] inputs = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

                        var voltageL1 = Converter.ConvertToFloat(inputs);
                        var ampL1 = Converter.ConvertToFloat(inputs, 2);
                        var activepowerL1 = Converter.ConvertToFloat(inputs, 4);
                        var reactivepowerL1 = Converter.ConvertToFloat(inputs, 6);
                        var apparentpowerL1 = Converter.ConvertToFloat(inputs, 8);
                        var powerfactorL1 = Converter.ConvertToFloat(inputs, 10);
                        var phasefactorL1 = Converter.ConvertToFloat(inputs, 12);
                        var frequency = Converter.ConvertToFloat(inputs, 56);
                        if (apparentpowerL1 == 0) powerfactorL1 = 0;
                        if (activepowerL1 == 0) phasefactorL1 = 0;

                        Console.WriteLine($"L1. AC Voltage: {voltageL1} V; Amp: {ampL1} A; Active power: {activepowerL1} W; Reactive power: {reactivepowerL1} VAR; Apparent power: {apparentpowerL1} VA; Power factor: {powerfactorL1}; Phase factor: {phasefactorL1}; Frequency: {frequency} Hz");

                        Thread.Sleep(1000);

                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Błąd połączenia. Sprawdź okablowanie oraz konfigurację.");
            }



        }

        private static void SetAnalogTest()
        {
            var hostname = ConfigurationManager.AppSettings["hostname"];
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var slaveId = byte.Parse(ConfigurationManager.AppSettings["S4AO"]);

            // Wyjście analogowe nr 1
            ushort startAddress = 4100;

            // Zadana wartość
            float voltage = 3.7f;

            // Konwersja
            ushort output = (ushort) (voltage * 100);

            Console.WriteLine($"Connecting to {hostname}:{port}");

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var master = ModbusIpMaster.CreateIp(client))
                {
                    Console.WriteLine("Connected.");

                    master.WriteSingleRegister(slaveId, startAddress, output);

                    var input = master.ReadInputRegisters(slaveId, startAddress, 1);

                    // Konwersja
                    float volt = input[0] / 100f;

                    Console.WriteLine($"Napięcie: {volt}V");


                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Błąd połączenia. Sprawdź okablowanie oraz konfigurację.");
            }




        }

        private static void SetBinaryTest()
        {
            var hostname = ConfigurationManager.AppSettings["hostname"];
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var slaveId = byte.Parse(ConfigurationManager.AppSettings["SM4"]);

            ushort startAddress = 2200;

            Console.WriteLine($"Connecting to {hostname}:{port}");

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var master = ModbusIpMaster.CreateIp(client))
                {
                    Console.WriteLine("Connected.");

                    // Przykładowe wartości
                    bool[] outputs = { true, false, true, true };


                    // startAddress = (ushort)(startAddress + 2);
                    // Zapis pojedynczego rejestru 
                    // master.WriteSingleCoil(slaveId, startAddress, true);
                    // Uwaga: nie działa

                    // Zapis wielu rejestrów
                    master.WriteMultipleCoils(slaveId, startAddress, outputs);

                    // Odczyt wielu rejestrów
                    bool[] inputs = master.ReadCoils(slaveId, startAddress, 4);

                    // Wyświetlenie
                    Console.WriteLine(String.Join(", ", inputs));

                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("Błąd połączenia. Sprawdź okablowanie oraz konfigurację.");
            }
        }

        private static void GetVoltageTest()
        {
            var hostname = ConfigurationManager.AppSettings["hostname"];
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var slaveId = byte.Parse(ConfigurationManager.AppSettings["N30H_VoltDC"]);

            ushort startAddress = 7010;
            ushort numRegisters = 2;

            Console.WriteLine($"Connecting to {hostname}:{port}");

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var master = ModbusIpMaster.CreateIp(client))
                {
                    Console.WriteLine("Connected.");

                    Console.WriteLine("Press any key to exit.");

                    while (!Console.KeyAvailable)
                    {
                        ushort[] inputs = master.ReadInputRegisters(slaveId, startAddress, numRegisters);

                        float voltage = Converter.ConvertToFloat(inputs);

                        Console.WriteLine($"Voltage : {voltage} V");

                        Thread.Sleep(1000);
                    }
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("Błąd połączenia. Sprawdź okablowanie oraz konfigurację.");
            }

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

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var master = ModbusIpMaster.CreateIp(client))
                {
                    Console.WriteLine("Connected.");

                    Console.WriteLine("Press any key to exit.");

                    while (!Console.KeyAvailable)
                    {
                        ushort[] inputs = master.ReadInputRegisters(slaveId, startAddress, numRegisters);

                        float temperature = Converter.ConvertToFloat(inputs);

                        Console.WriteLine($"Temperature: {temperature} °C");

                        Thread.Sleep(1000);
                    }
                }

            }
            catch(SocketException e)
            {
                Console.WriteLine("Błąd połączenia. Sprawdź okablowanie oraz konfigurację.");
            }



        }
    }
}
