﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.19408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LocalizationResources.WebService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WebService.DateTimeServiceSoap")]
    public interface DateTimeServiceSoap {
        
        // CODEGEN: Generating message contract since element name GetCurrentDateResult from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrentDate", ReplyAction="*")]
        LocalizationResources.WebService.GetCurrentDateResponse GetCurrentDate(LocalizationResources.WebService.GetCurrentDateRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetCurrentDateRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetCurrentDate", Namespace="http://tempuri.org/", Order=0)]
        public LocalizationResources.WebService.GetCurrentDateRequestBody Body;
        
        public GetCurrentDateRequest() {
        }
        
        public GetCurrentDateRequest(LocalizationResources.WebService.GetCurrentDateRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute()]
    public partial class GetCurrentDateRequestBody {
        
        public GetCurrentDateRequestBody() {
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetCurrentDateResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetCurrentDateResponse", Namespace="http://tempuri.org/", Order=0)]
        public LocalizationResources.WebService.GetCurrentDateResponseBody Body;
        
        public GetCurrentDateResponse() {
        }
        
        public GetCurrentDateResponse(LocalizationResources.WebService.GetCurrentDateResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class GetCurrentDateResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string GetCurrentDateResult;
        
        public GetCurrentDateResponseBody() {
        }
        
        public GetCurrentDateResponseBody(string GetCurrentDateResult) {
            this.GetCurrentDateResult = GetCurrentDateResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface DateTimeServiceSoapChannel : LocalizationResources.WebService.DateTimeServiceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DateTimeServiceSoapClient : System.ServiceModel.ClientBase<LocalizationResources.WebService.DateTimeServiceSoap>, LocalizationResources.WebService.DateTimeServiceSoap {
        
        public DateTimeServiceSoapClient() {
        }
        
        public DateTimeServiceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DateTimeServiceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DateTimeServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DateTimeServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        LocalizationResources.WebService.GetCurrentDateResponse LocalizationResources.WebService.DateTimeServiceSoap.GetCurrentDate(LocalizationResources.WebService.GetCurrentDateRequest request) {
            return base.Channel.GetCurrentDate(request);
        }
        
        public string GetCurrentDate() {
            LocalizationResources.WebService.GetCurrentDateRequest inValue = new LocalizationResources.WebService.GetCurrentDateRequest();
            inValue.Body = new LocalizationResources.WebService.GetCurrentDateRequestBody();
            LocalizationResources.WebService.GetCurrentDateResponse retVal = ((LocalizationResources.WebService.DateTimeServiceSoap)(this)).GetCurrentDate(inValue);
            return retVal.Body.GetCurrentDateResult;
        }
    }
}