using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace RemoteClient.WcfTest.WCF.Dispatch
{
    public class NewtonsoftJsonBehavior : WebHttpBehavior
    {
        public override void Validate(ServiceEndpoint endpoint)
        {
            base.Validate(endpoint);

            var elements = endpoint.Binding.CreateBindingElements();
            var webEncoder = elements.Find<WebMessageEncodingBindingElement>();
            if (webEncoder == null)
            {
                throw new InvalidOperationException("This behavior must be used in an endpoint with the WebHttpBinding (or a custom binding with the WebMessageEncodingBindingElement).");
            }

            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                ValidateOperation(operation);
            }
        }

        protected override IDispatchMessageFormatter GetRequestDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            return new NewtonsoftJsonDispatchFormatter(operationDescription, true);
        }

        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (operationDescription.Messages.Count == 1 || operationDescription.Messages[1].Body.ReturnValue.Type == typeof(void))
            {
                return base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }
            else
            {
                return new NewtonsoftJsonDispatchFormatter(operationDescription, false);
            }
        }

        private void ValidateOperation(OperationDescription operation)
        {
            if (operation.Messages.Count > 1)
            {
                if (operation.Messages[1].Body.Parts.Count > 0)
                {
                    throw new InvalidOperationException("Operations cannot have out/ref parameters.");
                }
            }

            WebMessageBodyStyle bodyStyle = operation.GetBodyStyle() ?? DefaultBodyStyle;
            int inputParameterCount = operation.Messages[0].Body.Parts.Count;
            if (!operation.IsGetOrDeleteOperation())
            {
                var wrappedRequest = bodyStyle == WebMessageBodyStyle.Wrapped || bodyStyle == WebMessageBodyStyle.WrappedRequest;
                if (inputParameterCount == 1 && wrappedRequest)
                {
                    throw new InvalidOperationException("Wrapped body style for single parameters not implemented in this behavior.");
                }
            }

            var wrappedResponse = bodyStyle == WebMessageBodyStyle.Wrapped || bodyStyle == WebMessageBodyStyle.WrappedResponse;
            var isVoidReturn = operation.Messages.Count == 1 || operation.Messages[1].Body.ReturnValue.Type == typeof(void);
            if (!isVoidReturn && wrappedResponse)
            {
                throw new InvalidOperationException("Wrapped response not implemented in this behavior.");
            }
        }

    }
}