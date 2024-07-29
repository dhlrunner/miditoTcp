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

        static string ipAddr = "10.10.4.23";
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

        static void Main(string[] args)
        {
            //byte[] midiDataBuffer = new byte[128];
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            InputDevices = new InputDevice[inputDev.Length];
            
            for (int i = 0; i < inputDev.Length; i++)
            {
                int portIndex = i;
                InputDevices[i] = InputDevice.GetByName(inputDev[i].devName);
                InputDevices[i].EventReceived += (sender, e) => {
                    var midiDevice = (MidiDevice)sender;
                    //미디 데이터 수신할때마다 new 해주지 않으면 데이터가 이상하게 변환됨
                    MidiEventToBytesConverter x = new MidiEventToBytesConverter();
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

                };
                InputDevices[i].StartEventsListening();
                Console.WriteLine($"Opened MIDI Device {InputDevices[i].Name} ({InputDevices[i].Id}) Port: {inputDev[i].portNum}");
            }

            tcp = new TcpCommunicator(ipAddr, port);

            Console.WriteLine($"Connected to {ipAddr}:{port}");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();

            for (int i = 0; i < inputDev.Length; i++) {
                InputDevices[i].Dispose();
            }

            tcp.Dispose();
            
           
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
