using System;
using System.Collections.Generic;
using System.Reflection;
using LINQPad.Extensibility.DataContext;

namespace RavenLinqpadDriver
{
    public class RavenDriver : StaticDataContextDriver
    {
        RavenConnectionDialogViewModel _connInfo;

        public override string Author
        {
            get { return "Ronnie Overby"; }
        }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            _connInfo = RavenConnectionDialogViewModel.Load(cxInfo);
            return string.Format("RavenDB: {0}", _connInfo.Name);
        }

        public override string Name
        {
            get
            {
                return "RavenDB Driver";
            }
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            _connInfo = isNewConnection
                ? new RavenConnectionDialogViewModel { CxInfo = cxInfo }
                : RavenConnectionDialogViewModel.Load(cxInfo);

            var win = new RavenConectionDialog(_connInfo);
            var result = win.ShowDialog() == true;

            if (result)
            {
                _connInfo.Save();
                cxInfo.CustomTypeInfo.CustomAssemblyPath = Assembly.GetAssembly(typeof(RavenContext)).Location;
                cxInfo.CustomTypeInfo.CustomTypeName = "RavenLinqpadDriver.RavenContext";
            }

            return result;
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            _connInfo = RavenConnectionDialogViewModel.Load(cxInfo);

            return new[] { new ParameterDescriptor("connInfo", "RavenLinqpadDriver.RavenConnectionDialogViewModel") };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            _connInfo = RavenConnectionDialogViewModel.Load(cxInfo);
// ReSharper disable CoVariantArrayConversion
            return new[] { _connInfo };
// ReSharper restore CoVariantArrayConversion
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            // load user's assemblies
            return _connInfo.AssemblyPaths;
        }

        public override IEnumerable<string> GetNamespacesToRemove(IConnectionInfo cxInfo)
        {
            return new[] {"System.Data.Linq", "System.Data.Linq.SqlClient"};
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            var namespaces = new List<String>(base.GetNamespacesToAdd(cxInfo));

            namespaces.AddRange(new[] 
            {
                "Raven.Client",
                "Raven.Client.Document",
                "Raven.Abstractions.Data",              
                "Raven.Client.Linq",
                "RavenLinqpadDriver.Common"
            });

            if (_connInfo != null)
                namespaces.AddRange(_connInfo.GetNamespaces());

            return namespaces;
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            return new List<ExplorerItem>();
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            _connInfo = RavenConnectionDialogViewModel.Load(cxInfo);

            var rc = (RavenContext) context;
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
