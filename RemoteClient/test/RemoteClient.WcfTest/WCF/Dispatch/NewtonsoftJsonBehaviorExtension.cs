using System;
using System.ServiceModel.Configuration;

namespace RemoteClient.WcfTest.WCF.Dispatch
{
    public class NewtonsoftJsonBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(NewtonsoftJsonBehavior);

        protected override object CreateBehavior()
        {
            return new NewtonsoftJsonBehavior();
        }
    }
}