using System.ServiceModel.Channels;

namespace RemoteClient.WcfTest.WCF.Dispatch
{
    public class NewtonsoftJsonContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Raw;
        }
    }
}