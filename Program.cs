using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace faster_resp
{

    class Program
    {
        const byte CR = 13;
        const byte LF = 10;
        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(address, 6379);
            listener.Start();
            while (true) // Add your exit flag here
            {
                var client = listener.AcceptTcpClient();
                Task.Run(() => RespListener(client));
            }
        }
        private static void RespListener(TcpClient client)
        {
            Console.WriteLine("Client connected. Waiting for data.");
            string message = "";
            while (client.Connected)
            {
                var st = client.GetStream();
                while (st.DataAvailable)
                {
                    string line;
                    line = System.Text.Encoding.ASCII.GetString(new[] { (byte)st.ReadByte() });
                    switch (line)
                    {
                        //+ simple strings
                        case "+":
                            Console.WriteLine("+ simple strings");
                            string stringContent = readline(st);
                            Console.WriteLine(stringContent);
                            break;
                        //- Errors 45
                        case "-":
                            Console.WriteLine("- Errors");
                            string errorContent = readline(st);
                            Console.WriteLine(errorContent);
                            break;
                        //: Integers 58
                        case ":":
                            Console.WriteLine(": Integers");
                            string intContent = readline(st);
                            Console.WriteLine(int.Parse(intContent));
                            break;
                        //$ Bulk Strings 36
                        case "$":
                            Console.WriteLine("$ Bulk Strings");
                            string bulkContent = readline(st);
                            Console.WriteLine(bulkContent);
                            break;
                        //* Array 42
                        case "*":
                            Console.WriteLine("* Array");
                            string arrayContent = readline(st);
                            Console.WriteLine(arrayContent);
                            break;
                        default:
                            Console.WriteLine(readline(st));
                            break;
                    }
                }
                byte[] data = Encoding.ASCII.GetBytes("+OK");
                client.GetStream().Write(data, 0, data.Length);
                client.GetStream().Dispose();
            }
            Console.WriteLine("Closing connection.");
            client.GetStream().Dispose();
        }


        private static string readline(NetworkStream st)
        {
            List<byte> content = new List<byte>();
            byte a;
            do
            {
                a = (byte)st.ReadByte();
                if (a != 10 && a != 13)
                {
                    content.Add(a);
                }
            }
            while (a != 10 && a != 13);
            if (st.DataAvailable && a == 10)
            {
                st.ReadByte();
            }
            return Encoding.UTF8.GetString(content.ToArray());
        }
        private static object[] ReadArray(Span<byte> content, int start, out int position)
        {
            start++;
            var length = content[start];
            start = start + 2;
            object[] contents = new object[length];
            for (int i = start; i < content.Length; i++)
            {
                var signal = content[i];
                switch (signal)
                {
                    //+ simple strings
                    case 43:
                        Console.WriteLine("+ simple strings");
                        int stringPosition;
                        var resultString = ReadSimpleString(content, i, out stringPosition);
                        i = stringPosition;
                        Console.WriteLine(resultString);
                        break;
                    //- Errors
                    case 45:
                        Console.WriteLine("- Errors");
                        break;
                    //: Integers
                    case 58:
                        Console.WriteLine(": Integers");
                        break;
                    //$ Bulk Strings
                    case 36:
                        Console.WriteLine("$ Bulk Strings");
                        int bulkStringPosition;
                        var resultBulkString = ReadSimpleString(content, i, out bulkStringPosition);
                        i = bulkStringPosition;
                        Console.WriteLine(resultBulkString);
                        break;
                    //* Array
                    case 42:
                        Console.WriteLine("* Array");
                        int arrayPosition;
                        var result = ReadArray(content, i, out arrayPosition);
                        i = arrayPosition;
                        break;
                    default:
                        Console.WriteLine(signal);
                        break;
                }
            }
            position = start;
            return contents;
        }
        private static string ReadSimpleString(Span<byte> content, int start, out int position)
        {
            start++;

            var length = content[start];
            start = start + 2;
            string result = Encoding.UTF8.GetString(content.ToArray(), start, length);
            position = start + length + 2;
            return result;
        }
    }
}
