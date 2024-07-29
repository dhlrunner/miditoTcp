using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using System.Net;

namespace miditoTcp
{
    class Program
    {
        static InputDevice[] InputDevices = null;
        //static InputDevice inputDeviceA = null;
        //static InputDevice inputDeviceB = null;

        static string ipAddr = "172.27.11.136";
        static int port = 12345;

        static TcpClient tcpClient = null;
        static NetworkStream stream = null;

        static TcpCommunicator tcp = null;

        static (string devName, byte portNum)[] inputDev = new (string, byte)[]{ 
            ("SAM5916 A", 1),
            ("SAM5916 B", 2),
            ("MMT-TG Ctrl", 1),
            ("MMT-TG A", 2),
            ("MMT-TG B", 3),
            ("MMT-TG C", 4),
            ("MMT-TG D", 5)
        };

        //static byte PortOffset = 1;
        static void Main(string[] args)
        {
            //byte[] midiDataBuffer = new byte[128];
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            InputDevices = new InputDevice[inputDev.Length];
            MidiEventToBytesConverter x = new MidiEventToBytesConverter();
            for (int i = 0; i < inputDev.Length; i++)
            {
                int portIndex = i;
                InputDevices[i] = InputDevice.GetByName(inputDev[i].devName);
                InputDevices[i].EventReceived += (sender, e) => {
                    var midiDevice = (MidiDevice)sender;

                    //byte[] data = x.Convert(e.Event);
                    List<byte> data = x.Convert(e.Event).ToList();
                    if(data.Count > 0)
                    {
                        if (data[0] == 0xf0)
                        {
                            //byte[] newdata = new byte[(data.Count - 1) + 3];

                            //newdata[0] = (byte)((data.Count - 1) + 2); //length
                            //newdata[1] = 0xF5;
                            //newdata[2] = (byte)(inputDev[portIndex].portNum);
                            //newdata[3] = 0xF0;
                            //for (int j = 2; j < data.Count; j++)
                            //{
                            //    newdata[j + 2] = data[j];
                            //}
                            data.RemoveAt(1);
                            data.InsertRange(0, new byte[] { 0xFF, (byte)(data.Count + 2), 0xF5, inputDev[portIndex].portNum });
                            
                        }
                        else
                        {
                            //byte[] newdata = new byte[(data.Count) + 3];
                            //newdata[0] = (byte)((data.Count) + 2); //length
                            //newdata[1] = 0xF5;
                            //newdata[2] = (byte)(inputDev[portIndex].portNum);
                            //for (int j = 0; j < data.Count; j++)
                            //{
                            //    newdata[j + 3] = data[j];
                            //}
                            data.InsertRange(0, new byte[] { 0xFF, (byte)(data.Count + 2), 0xF5, inputDev[portIndex].portNum });
                            //tcp.Send(data.ToArray());
                        }

                        try
                        {
                            tcp.Send(data.ToArray());
                        }
                        catch(InvalidOperationException)
                        {
                            Console.WriteLine("Error!");
                            tcp = new TcpCommunicator(ipAddr, port);
                        }
                        catch (System.IO.IOException)
                        {
                            Console.WriteLine("Error!");
                            tcp = new TcpCommunicator(ipAddr, port);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Empty data");
                    }

                    //data.InsertRange(0, new byte[] { 0xF5, (byte)(portIndex + 1 + PortOffset) });
                    //data.Insert(0, (byte)data.Count);
                    //tcp.Send(data.ToArray());
                };
                InputDevices[i].StartEventsListening();
                Console.WriteLine($"Opened MIDI Device {InputDevices[i].Name} ({InputDevices[i].Id}) Port: {inputDev[i].portNum}");
            }

            //inputDeviceA = InputDevice.GetByName("SAM5916 A");
            //inputDeviceA.EventReceived += OnEventReceived1;
            //inputDeviceA.StartEventsListening();

            //inputDeviceB = InputDevice.GetByName("SAM5916 B");
            //inputDeviceB.EventReceived += OnEventReceived2;
            //inputDeviceB.StartEventsListening();

            //tcpClient = new TcpClient(ipAddr, port);
            //tcpClient.NoDelay = true;

            //stream = tcpClient.GetStream();

            tcp = new TcpCommunicator(ipAddr, port);

            Console.WriteLine($"Connected to {ipAddr}:{port}");
            Console.WriteLine("Press enter to exit");
            //TcpListener server = new TcpListener(IPAddress.Any, 9999);
            //server.Start();
            Console.ReadLine();
            //inputDeviceA.Dispose();
            //inputDeviceB.Dispose();

            for (int i = 0; i < inputDev.Length; i++) {
                InputDevices[i].Dispose();
            }

            tcp.Dispose();
            
           
        }
        private static void OnEventReceived1(object sender, MidiEventReceivedEventArgs e)
        {
            var midiDevice = (MidiDevice)sender;
            
            MidiEventToBytesConverter x = new MidiEventToBytesConverter();
            List<byte> data = x.Convert(e.Event).ToList();
            //Console.WriteLine("Midi A data in ");
            //foreach(byte a in data)
            //{
            //    Console.Write("{0:X2} ", a);
            //}
            //Console.WriteLine();
            
            if (data[0] == 0xf0)
            {
                data.RemoveAt(1);              
            }

            data.InsertRange(0, new byte[] {0xF5,0x02 });
            tcp.Send(data.ToArray());
            //stream.Write(data.ToArray(), 0, data.Count);
            

            //SendTcp(socket, data, 0, data.Length, 10000);

        }
        private static void OnEventReceived2(object sender, MidiEventReceivedEventArgs e)
        {
            var midiDevice = (MidiDevice)sender;

            MidiEventToBytesConverter x = new MidiEventToBytesConverter();
            List<byte> data = x.Convert(e.Event).ToList();
            //Console.WriteLine("Midi B data in ");
            //foreach (byte a in data)
            //{
            //    Console.Write("{0:X2} ", a);
            //}
            //Console.WriteLine();

            if (data[0] == 0xf0)
            {
                data.RemoveAt(1);
            }

            data.InsertRange(0, new byte[] { 0xF5, 0x03 });
            tcp.Send(data.ToArray());
            //stream.Write(data.ToArray(), 0, data.Count);
            //stream.Close();
            //tcpClient.Close();

            //SendTcp(socket, data, 0, data.Length, 10000);

        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //inputDeviceA.Dispose();    
            //inputDeviceB.Dispose();
            tcp.Dispose();
            //stream.Close();
            //tcpClient.Close();
            Console.WriteLine("exit");
        }
    }
}
