using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using WcfRestClient.Helpers;
using WcfRestClient.ServiceHoster.Extensions;

namespace WcfRestClient.ServiceHoster.Dispatch
{
    public class NewtonsoftJsonDispatchFormatter : IDispatchMessageFormatter
    {
        private readonly OperationDescription _operation;
        private readonly Dictionary<string, int> _bodyParameterIndexes;
        private readonly Dictionary<string, int> _uriParameterIndexes;

        public NewtonsoftJsonDispatchFormatter(OperationDescription operation, bool isRequest)
        {
            _operation = operation;
            if (!isRequest)
                return;
            int operationParameterCount = operation.Messages[0].Body.Parts.Count;
            _bodyParameterIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _uriParameterIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < operationParameterCount; i++)
            {
                string name = _operation.GetParameter(i).Name;
                bool isUriParameter = operation.IsUriParameter(name);
                var dictToAddParameter = isUriParameter ? _uriParameterIndexes : _bodyParameterIndexes;
                dictToAddParameter.Add(name, i);
            }
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            // ReSharper disable once PossibleNullReferenceException
            var uriParams = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BoundVariables;
            foreach (string parameterName in uriParams)
            {
                int parameterIndex = _uriParameterIndexes[parameterName];
                var type = _operation.GetParameter(parameterIndex).Type;
                parameters[parameterIndex] = NullableQueryStringConverter.Instance.ConvertStringToValue(uriParams[parameterName], type);
            }
            if (_bodyParameterIndexes.Count == 0)
            {
                return;
            }
            object bodyFormatProperty;
            if (!message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out bodyFormatProperty) ||
                ((WebBodyFormatMessageProperty)bodyFormatProperty).Format != WebContentFormat.Raw)
            {
                throw new InvalidOperationException("Incoming messages must have a body format of Raw. Is a ContentTypeMapper set on the WebHttpBinding?");
            }

            var bodyReader = message.GetReaderAtBodyContents();
            bodyReader.ReadStartElement("Binary");
            byte[] rawBody = bodyReader.ReadContentAsBase64();

            if (_bodyParameterIndexes.Count == 1)
            {
                // single parameter, assuming bare
                int parameterIndex = _bodyParameterIndexes.Values.First();
                parameters[parameterIndex] = NewtonsoftInterop.DeserializeFromBytes(rawBody, _operation.GetParameter(parameterIndex).Type);
            }
            else
            {
                // multiple parameter, needs to be wrapped
                using (var ms = new MemoryStream(rawBody, false))
                using (var sr = new StreamReader(ms, NewtonsoftInterop.JsonEncoding))
                using (var reader = new JsonTextReader(sr))
                {
                    reader.Read();
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        throw new InvalidOperationException("Input needs to be wrapped in an object");
                    }

                    reader.Read();
                    while (reader.TokenType == JsonToken.PropertyName)
                    {
                        var parameterName = (string)reader.Value;
                        reader.Read();
                        if (_bodyParameterIndexes.ContainsKey(parameterName))
                        {
                            int parameterIndex = _bodyParameterIndexes[parameterName];
                            parameters[parameterIndex] = NewtonsoftInterop.DeserializeFromReader(reader, _operation.GetParameter(parameterIndex).Type);
                        }
                        else
                        {
                            reader.Skip();
                        }

                        reader.Read();
                    }

                    reader.Close();
                }
            }
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            byte[] body = NewtonsoftInterop.SerializeToBytes(result);
            Message replyMessage = Message.CreateMessage(messageVersion, _operation.Messages[1].Action, new RawBodyWriter(body));
            replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            var respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] = "application/json";
            replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);
            return replyMessage;
        }
    }
}
