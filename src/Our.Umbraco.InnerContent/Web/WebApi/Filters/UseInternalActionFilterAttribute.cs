using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Our.Umbraco.InnerContent.Web.WebApi.Filters
{
    /// <summary>
    /// Uses reflection to enable the use of an action filter that has been marked as internal by a 3rd party assembly.
    /// </summary>
    /// <remarks>
    /// The reason for developing this attribute was that we wanted to use Umbraco's `OutgoingEditorModelEvent` action filter.
    /// The irony of this is that we've marked this attribute class as internal ourselves.
    /// </remarks>
    internal class UseInternalActionFilterAttribute : ActionFilterAttribute
    {
        private readonly string _fullyQualifiedTypeName;
        private Type _internalType;
        private object _internalInstance;
        private readonly bool _runOnActionExecuting;
        private readonly bool _runOnActionExecuted;

        public UseInternalActionFilterAttribute(
            string fullyQualifiedTypeName,
            bool onActionExecuting = false,
            bool onActionExecuted = false)
        {
            _fullyQualifiedTypeName = fullyQualifiedTypeName;
            _runOnActionExecuting = onActionExecuting;
            _runOnActionExecuted = onActionExecuted;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (_runOnActionExecuting)
            {
                InvokeInternalAction("OnActionExecuting", new[] { actionContext });
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (_runOnActionExecuted)
            {
                InvokeInternalAction("OnActionExecuted", new[] { actionExecutedContext });
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        private void InvokeInternalAction(string methodName, params object[] parameters)
        {
            try
            {
                if (_internalType == null && string.IsNullOrWhiteSpace(_fullyQualifiedTypeName) == false)
                {
                    _internalType = TypeFinder.GetTypeByName(_fullyQualifiedTypeName);
                }

                if (_internalType != null)
                {
                    var method = _internalType.GetMethod(methodName);
                    if (method != null)
                    {
                        if (_internalInstance == null)
                        {
                            _internalInstance = Activator.CreateInstance(_internalType);
                        }

                        if (_internalInstance != null)
                        {
                            method.Invoke(_internalInstance, parameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Current.Logger.Error<UseInternalActionFilterAttribute>(methodName, ex);
            }
        }
    }
}