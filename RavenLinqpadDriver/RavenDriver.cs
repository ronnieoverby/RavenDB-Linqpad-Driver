using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;

namespace RavenLinqpadDriver
{
    public class Dummy { }

    public class RavenDriver : StaticDataContextDriver
    {
        public override string Author
        {
            get { return "Ronnie Overby"; }
        }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var connInfo = RavenConnectionInfo.Load(cxInfo);
            return string.Format("{0} (RavenDB)", connInfo.Name);
        }

        public override string Name
        {
            get { return "RavenDB Driver"; }
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            RavenConnectionInfo conn;
            conn = isNewConnection
                ? new RavenConnectionInfo { CxInfo = cxInfo }
                : RavenConnectionInfo.Load(cxInfo);
            
            var win = new RavenConectionDialog(conn);
            var result = win.ShowDialog() == true;

            if (result)
            {
                conn.Save();
                cxInfo.CustomTypeInfo.CustomAssemblyPath = Assembly.GetAssembly(typeof(RavenContext)).Location;
                cxInfo.CustomTypeInfo.CustomTypeName = "RavenLinqpadDriver.RavenContext";
            }

            return result;
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[] { new ParameterDescriptor("connInfo", "RavenLinqpadDriver.RavenConnectionInfo") };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            RavenConnectionInfo connInfo = RavenConnectionInfo.Load(cxInfo);
            return new[] { connInfo };
        }

        public override IEnumerable<string> GetAssembliesToAdd()
        {
            return new[] { 
                "Newtonsoft.Json.Net35.dll",
                "NLog.dll",
                "Raven.Abstractions-3.5.dll"
            };
        }

        public override IEnumerable<string> GetNamespacesToRemove()
        {
            // linqpad uses System.Data.Linq by default, which isn't needed
            return new[] { "System.Data.Linq" };
        }

        public override IEnumerable<string> GetNamespacesToAdd()
        {
            return base.GetNamespacesToAdd()
                .Union(new[] {
                "Raven.Client",
                "Raven.Client.Document",
                "Raven.Abstractions.Data",
                "Raven.Client.Linq"
            });
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            return new List<ExplorerItem>();
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            var rc = context as RavenContext;
            rc.LogWriter = executionManager.SqlTranslationWriter;
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            base.TearDownContext(cxInfo, context, executionManager, constructorArguments);
            var rc = context as RavenContext;
            if (rc != null)
                rc.Dispose();
        }
    }
}
