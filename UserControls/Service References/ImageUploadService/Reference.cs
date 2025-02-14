﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UserControls.ImageUploadService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ImageDto", Namespace="http://schemas.datacontract.org/2004/07/Realtor.DTO")]
    [System.SerializableAttribute()]
    public partial class ImageDto : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] ImageContentField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ImageTypeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string streamKeyField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] ImageContent {
            get {
                return this.ImageContentField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageContentField, value) != true)) {
                    this.ImageContentField = value;
                    this.RaisePropertyChanged("ImageContent");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ImageType {
            get {
                return this.ImageTypeField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageTypeField, value) != true)) {
                    this.ImageTypeField = value;
                    this.RaisePropertyChanged("ImageType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string streamKey {
            get {
                return this.streamKeyField;
            }
            set {
                if ((object.ReferenceEquals(this.streamKeyField, value) != true)) {
                    this.streamKeyField = value;
                    this.RaisePropertyChanged("streamKey");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ImageUploadService.IImageUpload")]
    public interface IImageUpload {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamService/CreateStream", ReplyAction="http://tempuri.org/IStreamService/CreateStreamResponse")]
        string CreateStream();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamService/GetStreamLength", ReplyAction="http://tempuri.org/IStreamService/GetStreamLengthResponse")]
        long GetStreamLength(string key);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamService/ReadFromStream", ReplyAction="http://tempuri.org/IStreamService/ReadFromStreamResponse")]
        byte[] ReadFromStream(out int readedLength, string key, int count);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamService/WriteToStream", ReplyAction="http://tempuri.org/IStreamService/WriteToStreamResponse")]
        long WriteToStream(string key, byte[] buffer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamService/CloseStream", ReplyAction="http://tempuri.org/IStreamService/CloseStreamResponse")]
        void CloseStream(string key);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamService/DisposeStream", ReplyAction="http://tempuri.org/IStreamService/DisposeStreamResponse")]
        void DisposeStream(string key);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IImageUpload/ReadBlob", ReplyAction="http://tempuri.org/IImageUpload/ReadBlobResponse")]
        UserControls.ImageUploadService.ImageDto ReadBlob(long image);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IImageUpload/WriteBlob", ReplyAction="http://tempuri.org/IImageUpload/WriteBlobResponse")]
        int WriteBlob(UserControls.ImageUploadService.ImageDto image, long estateID);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IImageUploadChannel : UserControls.ImageUploadService.IImageUpload, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ImageUploadClient : System.ServiceModel.ClientBase<UserControls.ImageUploadService.IImageUpload>, UserControls.ImageUploadService.IImageUpload {
        
        public ImageUploadClient() {
        }
        
        public ImageUploadClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ImageUploadClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ImageUploadClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ImageUploadClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string CreateStream() {
            return base.Channel.CreateStream();
        }
        
        public long GetStreamLength(string key) {
            return base.Channel.GetStreamLength(key);
        }
        
        public byte[] ReadFromStream(out int readedLength, string key, int count) {
            return base.Channel.ReadFromStream(out readedLength, key, count);
        }
        
        public long WriteToStream(string key, byte[] buffer) {
            return base.Channel.WriteToStream(key, buffer);
        }
        
        public void CloseStream(string key) {
            base.Channel.CloseStream(key);
        }
        
        public void DisposeStream(string key) {
            base.Channel.DisposeStream(key);
        }
        
        public UserControls.ImageUploadService.ImageDto ReadBlob(long image) {
            return base.Channel.ReadBlob(image);
        }
        
        public int WriteBlob(UserControls.ImageUploadService.ImageDto image, long estateID) {
            return base.Channel.WriteBlob(image, estateID);
        }
    }
}
