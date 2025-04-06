using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;

namespace ProcessList
{
    class Program
    {
        static void Main(string[] args)
        {
            
            StringBuilder sb = new StringBuilder();
            foreach (var process in Process.GetProcesses())
            {
                sb.AppendLine(string.Format("Id:{0}, Title:{1},Name:{2} ",process.Id,process.MainWindowTitle, process.ProcessName));
            }

            File.WriteAllText("process.txt",sb.ToString());
            bool exit = false;
            do
            {
                Console.WriteLine("Enter Process Name to search");
                var processName = Console.ReadLine();
                if (string.IsNullOrEmpty(processName))
                {
                    Console.WriteLine("do you want to search for no-name processes (1) or quit (2)?");
                    var response= Console.ReadLine();
                    if (string.Compare("2", response, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        exit = true;
                        break;
                    }
                    else
                    {
                        var processes = Process.GetProcessesByName(string.Empty);
                        if (processes != null)
                        {
                            var processIds = processes.Select(p => p.Id).ToArray();
                            string processIdsString = string.Join(",", processIds);
                            Console.WriteLine(string.Format("Found process Ids {0}", processIdsString));
                        }

                    }
                }
                else
                {
                    var processes = Process.GetProcessesByName(processName);
                    if (processes != null)
                    {
                        var processIds= processes.Select(p => p.Id).ToArray();
                        string processIdsString=string.Join(",", processIds);
                        Console.WriteLine(string.Format("Found process Ids {0}", processIdsString));
                    }
                }
            } while (!exit);
        }
    }
}
