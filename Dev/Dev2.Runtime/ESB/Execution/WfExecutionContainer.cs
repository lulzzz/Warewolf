/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class WfExecutionContainer : EsbExecutionContainer
    {
        private readonly IResourceCatalog _catalog;

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _catalog = ResourceCatalog.Instance;
        }


        public WfExecutionContainer(IResourceCatalog catalog, ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            :this(sa,dataObj,theWorkspace,esbChannel)
        {
            _catalog = catalog;
        }
        /// <summary>
        /// Executes the specified errors.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <param name="update"></param>
        /// <returns></returns>
        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();

            Guid result = GlobalConstants.NullDataListID;
            DataObject.ExecutionID = DataObject.ExecutionID ?? Guid.NewGuid();
            var user = Thread.CurrentPrincipal;
            Dev2Logger.Debug("About to execute web request [ " + DataObject.ServiceName + " ] for User [ " + user?.Identity?.Name + " : " + user?.Identity?.AuthenticationType + " : " + user?.Identity?.IsAuthenticated + " ] with DataObject Payload [ " + DataObject.RawPayload + " ]", DataObject.ExecutionID.ToString());
            Dev2Logger.Debug("Request URL [ " + DataObject.WebUrl+ " ]", DataObject.ExecutionID.ToString());

            Dev2Logger.Debug("Entered Wf Container", DataObject.ExecutionID.ToString());

            // Set Service Name
            DataObject.ServiceName = ServiceAction.ServiceName;

            if (DataObject.ServerID == Guid.Empty)
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;


            Dev2Logger.Info($"Started Execution for Service Name:{DataObject.ServiceName} Resource Id:{DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}", DataObject.ExecutionID.ToString());

            var resource = _catalog.GetResource(TheWorkspace.ID, DataObject.ResourceID);
            if (resource != null)
            {
                var environment = ExecutionEnvironmentUtils.GetXmlInputFromEnvironment(DataObject, resource.DataList.ToString(), update);
                Dev2Logger.Debug("Studio Debug inputs [ " + environment + " ]", DataObject.ExecutionID.ToString());
            }
            //Set execution origin
            if (!string.IsNullOrWhiteSpace(DataObject.ParentServiceName))
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Workflow;
                DataObject.ExecutionOriginDescription = DataObject.ParentServiceName;
            }
            else if (DataObject.IsDebug)
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Debug;
            }
            else
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.External;
            }
            var userPrinciple = Thread.CurrentPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { result = ExecuteWf(); });
            foreach (var err in DataObject.Environment.Errors)
            {
                errors.AddError(err, true);
            }
            foreach (var err in DataObject.Environment.AllErrors)
            {
                errors.AddError(err, true);
            }

            Dev2Logger.Info($"Completed Execution for Service Name:{DataObject.ServiceName} Resource Id: {DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}", DataObject.ExecutionID.ToString());
            return result;
        }

        Guid ExecuteWf()
        {
            Guid result = new Guid();
            DataObject.StartTime = DateTime.Now;
            var wfappUtils = new WfApplicationUtils();
            ErrorResultTO invokeErrors;
            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;

                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, true, false, false);
                }
                if (CanExecute(DataObject.ResourceID, DataObject, AuthorizationContext.Execute))
                {
                    Eval(DataObject.ResourceID, DataObject);
                }
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
                }
                result = DataObject.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe, DataObject.ExecutionID.ToString());
                var msg = iwe.Message;

                int start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                var errorMessage = start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message;
                DataObject.Environment.AddError(errorMessage);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, DataObject.ExecutionID.ToString());
                DataObject.Environment.AddError(ex.Message);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
            }
            return result;
        }

        public override bool CanExecute(Guid resourceID, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            var isAuthorized = ServerAuthorizationService.Instance.IsAuthorized(authorizationContext, resourceID.ToString());
            if (!isAuthorized)
            {
                dataObject.Environment.AddError(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToExecuteException);
            }
            return isAuthorized;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Eval(DynamicActivity flowchartProcess, IDSFDataObject dsfDataObject, int update)
        {
            IDev2Activity resource = new ActivityParser().Parse(flowchartProcess);

            EvalInner(dsfDataObject, resource, update);
        }

        private void Eval(Guid resourceID, IDSFDataObject dataObject)
        {
            Dev2Logger.Debug("Getting Resource to Execute", dataObject.ExecutionID.ToString());
            IDev2Activity resource = _catalog.Parse(TheWorkspace.ID, resourceID, dataObject.ExecutionID.ToString());
            Dev2Logger.Debug("Got Resource to Execute", dataObject.ExecutionID.ToString());
            EvalInner(dataObject, resource, dataObject.ForEachUpdateValue);

        }


        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return null;
        }

        static void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            if (resource == null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
            WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;
            var next = resource.Execute(dsfDataObject, update);
            while (next != null)
            {
                if (!dsfDataObject.StopExecution)
                {
                    next = next.Execute(dsfDataObject, update);
                    if (dsfDataObject.Environment.Errors.Count > 0)
                    {
                        foreach (var e in dsfDataObject.Environment.Errors)
                        {
                            dsfDataObject.Environment.AllErrors.Add(e);
                        }

                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
