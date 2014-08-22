using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Windows.Forms;

namespace MyVMK_Pal
{
    class PluginManager
    {
        /*
         * Hey look! The plugin manager I never finished.
         * If someone finishes this, PLEASE send me the build c:
         * kolya.venturi@enx3s.com
         */ 

        private const string _DIR = "Plugins";
        private Stack<Plugin> _plugins = new Stack<Plugin>();

        public PluginManager(ToolStrip toolStrip)
        {
            if (Directory.Exists(_DIR))
            {
                // Get dll files
                string[] fileNames = Directory.GetFiles(_DIR, "*.dll");

                foreach (string name in fileNames)
                {
                    _plugins.Push(new Plugin(name, toolStrip));
                }
            }
        }

        public Plugin[] Plugins
        {
            get { return _plugins.ToArray(); }
        }
    }

    public class Plugin : MarshalByRefObject
    {
        public ToolStripButton MenuItem { get; private set; }
        public Exception Exception { get; private set; }
        public string PluginName { get; private set; }

        public Plugin() { } 

        public Plugin(string fileName, ToolStrip toolStrip)
        {
            MenuItem = new ToolStripButton();

            try
            {
                PluginName = Path.GetFileName(fileName);
                /* Assembly _assembly = Assembly.LoadFrom(fileName);
                Type type = _assembly.GetType("PalPlugin.Plugin");
                Type[] args = new Type[] { typeof(ToolStrip) }; 

			    type.GetMethod("Entry", args).Invoke(null, new object[] { toolStrip }); */

                //Setting the AppDomainSetup
                AppDomainSetup adSetup = new AppDomainSetup();
                adSetup.ApplicationBase = Path.GetFullPath(fileName);

                //Setting the permissions for the AppDomain
                PermissionSet permSet = new PermissionSet(PermissionState.None);
                permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Assembly.GetExecutingAssembly().Location));

                //Create appDomain
                AppDomain newDomain = AppDomain.CreateDomain("MyVMKPal", null, adSetup, permSet, null);

                //Load instance into AppDomain
                ObjectHandle handle = Activator.CreateInstanceFrom(
                    newDomain, typeof(Plugin).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(Plugin).FullName);

                //Unwrap the new domain instance into a reference in this domain and use it to execute the code
                Plugin newDomainInstance = (Plugin)handle.Unwrap();
                newDomainInstance.ExecuteCode(Path.GetFullPath(fileName), "PalPlugin.Plugin", "Entry", new object[] { toolStrip });

            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        /// <summary>
        /// Gets whether the plugin has been successfully loaded
        /// </summary>
        public bool Loaded
        {
            get { return Exception == null; }
        }

        public void ExecuteCode(string assemblyName, string typeName, string entryPoint, Object[] parameters)
        {
            //Load the MethodInfo for a method in the new Assembly
            MethodInfo target = Assembly.LoadFile(assemblyName).GetType(typeName).GetMethod(entryPoint);
            try
            {
                //Now invoke the method.
                target.Invoke(null, parameters);
            }
            catch (Exception ex)
            {
                //Print exception to console
                //(new PermissionSet(PermissionState.Unrestricted)).Assert();
                Console.WriteLine("SecurityException caught:\n{0}", ex.ToString());
                //CodeAccessPermission.RevertAssert();
                Console.ReadLine();
            }
        }
    }
}
