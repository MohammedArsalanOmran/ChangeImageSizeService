using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ChangeImageSizeSerivice
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                // Run as a console application
                ImageSizeService service = new ImageSizeService();
                service.StartInConsole();

            }
            else
            {
                // Run as a Windows Service
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new ImageSizeService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
