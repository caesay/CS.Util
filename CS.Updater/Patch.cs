using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS.Util.Extensions;

namespace CS.Updater
{
    public abstract class UpdateTask
    {
        public abstract Task PrepareInstall(UpdateContext context);
        public abstract Task ExecuteInstall(UpdateContext context);

        public abstract Task PrepareUninstall();
        public abstract Task ExecuteUninstall();

        public abstract Task Rollback();
        public abstract Task Uninstall();
    }

    public class UpdateContext
    {
        public Version Current { get; }
        public bool ColdRun { get; }
        public string Directory { get; }
    }
}
