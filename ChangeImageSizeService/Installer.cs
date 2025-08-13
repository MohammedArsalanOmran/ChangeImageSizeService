using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ChangeImageSizeSerivice
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public Installer()
        {
            InitializeComponent();
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            // Service configuration
            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "ImageSizeService",
                DisplayName = "Change Image Size",
                StartType = ServiceStartMode.Automatic,
                Description = "this service changes Image Size.",
            };

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);

        }
    }
}
