using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.Compression.CAB.Device.Test
{
    public class NOPInstaller : WinCEInstallerFile
    {
        public NOPInstaller(string cabFileName)
            : base(cabFileName)
        {
        }

        public override void OnInstallShortcut(ref ShortcutInstallInfo shortcutInfo)
        {
        }

        public override void OnInstallFile(ref FileInstallInfo fileInfo, out bool skipped)
        {
            skipped = true;
        }

        public override void OnInstallRegValue(ref RegistryInstallInfo registryInfo)
        {
        }

        public override void OnOSCheck()
        {
        }

        public override bool OnRunInstallerDllInit()
        {
            return true;
        }

        public override bool OnRunInstallerDllExit()
        {
            return true;
        }

        public override void RegisterInstallation()
        {
        }
    }
}
