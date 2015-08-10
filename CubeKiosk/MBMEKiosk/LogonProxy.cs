﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MBMEKiosk.LogonProxy
{
    using System.Runtime.Serialization;
    using System;


    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "RequestBase", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(MBMEKiosk.LogonProxy.LogOffKioskRequest))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(MBMEKiosk.LogonProxy.RegisterKioskRequest))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(MBMEKiosk.LogonProxy.LogOnKioskRequest))]
    public partial class RequestBase : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
    {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        private int KioskIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string KioskRefNumField;

        private System.DateTime StatusUpdatedField;

        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true)]
        public int KioskId
        {
            get
            {
                return this.KioskIdField;
            }
            set
            {
                if ((this.KioskIdField.Equals(value) != true))
                {
                    this.KioskIdField = value;
                    this.RaisePropertyChanged("KioskId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string KioskRefNum
        {
            get
            {
                return this.KioskRefNumField;
            }
            set
            {
                if ((object.ReferenceEquals(this.KioskRefNumField, value) != true))
                {
                    this.KioskRefNumField = value;
                    this.RaisePropertyChanged("KioskRefNum");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true)]
        public System.DateTime StatusUpdated
        {
            get
            {
                return this.StatusUpdatedField;
            }
            set
            {
                if ((this.StatusUpdatedField.Equals(value) != true))
                {
                    this.StatusUpdatedField = value;
                    this.RaisePropertyChanged("StatusUpdated");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "LogOffKioskRequest", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    public partial class LogOffKioskRequest : MBMEKiosk.LogonProxy.RequestBase
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "RegisterKioskRequest", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    public partial class RegisterKioskRequest : MBMEKiosk.LogonProxy.RequestBase
    {

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool RegisterField;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Register
        {
            get
            {
                return this.RegisterField;
            }
            set
            {
                if ((this.RegisterField.Equals(value) != true))
                {
                    this.RegisterField = value;
                    this.RaisePropertyChanged("Register");
                }
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "LogOnKioskRequest", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    public partial class LogOnKioskRequest : MBMEKiosk.LogonProxy.RequestBase
    {

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ReleaseVersionField;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ReleaseVersion
        {
            get
            {
                return this.ReleaseVersionField;
            }
            set
            {
                if ((object.ReferenceEquals(this.ReleaseVersionField, value) != true))
                {
                    this.ReleaseVersionField = value;
                    this.RaisePropertyChanged("ReleaseVersion");
                }
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "ResponseBase", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(MBMEKiosk.LogonProxy.LogOffKioskResponse))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(MBMEKiosk.LogonProxy.RegisterKioskResponse))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(MBMEKiosk.LogonProxy.LogOnKioskResponse))]
    public partial class ResponseBase : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
    {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime StatusUpdatedField;

        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime StatusUpdated
        {
            get
            {
                return this.StatusUpdatedField;
            }
            set
            {
                if ((this.StatusUpdatedField.Equals(value) != true))
                {
                    this.StatusUpdatedField = value;
                    this.RaisePropertyChanged("StatusUpdated");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "LogOffKioskResponse", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    public partial class LogOffKioskResponse : MBMEKiosk.LogonProxy.ResponseBase
    {

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool isLoggedOffField;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool isLoggedOff
        {
            get
            {
                return this.isLoggedOffField;
            }
            set
            {
                if ((this.isLoggedOffField.Equals(value) != true))
                {
                    this.isLoggedOffField = value;
                    this.RaisePropertyChanged("isLoggedOff");
                }
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "RegisterKioskResponse", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    public partial class RegisterKioskResponse : MBMEKiosk.LogonProxy.ResponseBase
    {

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsRegisteredField;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsRegistered
        {
            get
            {
                return this.IsRegisteredField;
            }
            set
            {
                if ((this.IsRegisteredField.Equals(value) != true))
                {
                    this.IsRegisteredField = value;
                    this.RaisePropertyChanged("IsRegistered");
                }
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "LogOnKioskResponse", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Contracts")]
    [System.SerializableAttribute()]
    public partial class LogOnKioskResponse : MBMEKiosk.LogonProxy.ResponseBase
    {

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private MBMEKiosk.LogonProxy.KioskService[] BillerServiceListField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private MBMEKiosk.LogonProxy.KioskDevice[] DeviceListField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string KioskLocationField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool isLoggedOnField;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public MBMEKiosk.LogonProxy.KioskService[] BillerServiceList
        {
            get
            {
                return this.BillerServiceListField;
            }
            set
            {
                if ((object.ReferenceEquals(this.BillerServiceListField, value) != true))
                {
                    this.BillerServiceListField = value;
                    this.RaisePropertyChanged("BillerServiceList");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public MBMEKiosk.LogonProxy.KioskDevice[] DeviceList
        {
            get
            {
                return this.DeviceListField;
            }
            set
            {
                if ((object.ReferenceEquals(this.DeviceListField, value) != true))
                {
                    this.DeviceListField = value;
                    this.RaisePropertyChanged("DeviceList");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string KioskLocation
        {
            get
            {
                return this.KioskLocationField;
            }
            set
            {
                if ((object.ReferenceEquals(this.KioskLocationField, value) != true))
                {
                    this.KioskLocationField = value;
                    this.RaisePropertyChanged("KioskLocation");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool isLoggedOn
        {
            get
            {
                return this.isLoggedOnField;
            }
            set
            {
                if ((this.isLoggedOnField.Equals(value) != true))
                {
                    this.isLoggedOnField = value;
                    this.RaisePropertyChanged("isLoggedOn");
                }
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "KioskService", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Models")]
    [System.SerializableAttribute()]
    public partial class KioskService : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
    {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime ActivationDateField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool AvailableField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int BillerServiceIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string BillerServiceNameField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int KioskIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string KioskRefNumField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ServiceKeyField;

        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime ActivationDate
        {
            get
            {
                return this.ActivationDateField;
            }
            set
            {
                if ((this.ActivationDateField.Equals(value) != true))
                {
                    this.ActivationDateField = value;
                    this.RaisePropertyChanged("ActivationDate");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Available
        {
            get
            {
                return this.AvailableField;
            }
            set
            {
                if ((this.AvailableField.Equals(value) != true))
                {
                    this.AvailableField = value;
                    this.RaisePropertyChanged("Available");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int BillerServiceId
        {
            get
            {
                return this.BillerServiceIdField;
            }
            set
            {
                if ((this.BillerServiceIdField.Equals(value) != true))
                {
                    this.BillerServiceIdField = value;
                    this.RaisePropertyChanged("BillerServiceId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string BillerServiceName
        {
            get
            {
                return this.BillerServiceNameField;
            }
            set
            {
                if ((object.ReferenceEquals(this.BillerServiceNameField, value) != true))
                {
                    this.BillerServiceNameField = value;
                    this.RaisePropertyChanged("BillerServiceName");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int KioskId
        {
            get
            {
                return this.KioskIdField;
            }
            set
            {
                if ((this.KioskIdField.Equals(value) != true))
                {
                    this.KioskIdField = value;
                    this.RaisePropertyChanged("KioskId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string KioskRefNum
        {
            get
            {
                return this.KioskRefNumField;
            }
            set
            {
                if ((object.ReferenceEquals(this.KioskRefNumField, value) != true))
                {
                    this.KioskRefNumField = value;
                    this.RaisePropertyChanged("KioskRefNum");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ServiceKey
        {
            get
            {
                return this.ServiceKeyField;
            }
            set
            {
                if ((object.ReferenceEquals(this.ServiceKeyField, value) != true))
                {
                    this.ServiceKeyField = value;
                    this.RaisePropertyChanged("ServiceKey");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "KioskDevice", Namespace = "http://schemas.datacontract.org/2004/07/MBMEServices.LogonService.Models")]
    [System.SerializableAttribute()]
    public partial class KioskDevice : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
    {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool DeviceEnabledField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int DeviceIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DeviceKeyField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DeviceNameField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int KioskIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string KioskRefNumField;

        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool DeviceEnabled
        {
            get
            {
                return this.DeviceEnabledField;
            }
            set
            {
                if ((this.DeviceEnabledField.Equals(value) != true))
                {
                    this.DeviceEnabledField = value;
                    this.RaisePropertyChanged("DeviceEnabled");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int DeviceId
        {
            get
            {
                return this.DeviceIdField;
            }
            set
            {
                if ((this.DeviceIdField.Equals(value) != true))
                {
                    this.DeviceIdField = value;
                    this.RaisePropertyChanged("DeviceId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DeviceKey
        {
            get
            {
                return this.DeviceKeyField;
            }
            set
            {
                if ((object.ReferenceEquals(this.DeviceKeyField, value) != true))
                {
                    this.DeviceKeyField = value;
                    this.RaisePropertyChanged("DeviceKey");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DeviceName
        {
            get
            {
                return this.DeviceNameField;
            }
            set
            {
                if ((object.ReferenceEquals(this.DeviceNameField, value) != true))
                {
                    this.DeviceNameField = value;
                    this.RaisePropertyChanged("DeviceName");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int KioskId
        {
            get
            {
                return this.KioskIdField;
            }
            set
            {
                if ((this.KioskIdField.Equals(value) != true))
                {
                    this.KioskIdField = value;
                    this.RaisePropertyChanged("KioskId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string KioskRefNum
        {
            get
            {
                return this.KioskRefNumField;
            }
            set
            {
                if ((object.ReferenceEquals(this.KioskRefNumField, value) != true))
                {
                    this.KioskRefNumField = value;
                    this.RaisePropertyChanged("KioskRefNum");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "LogonProxy.ILogonService")]
    public interface ILogonService
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ILogonService/LogOnKiosk", ReplyAction = "http://tempuri.org/ILogonService/LogOnKioskResponse")]
        MBMEKiosk.LogonProxy.LogOnKioskResponse LogOnKiosk(MBMEKiosk.LogonProxy.LogOnKioskRequest request);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ILogonService/LogOffKiosk", ReplyAction = "http://tempuri.org/ILogonService/LogOffKioskResponse")]
        MBMEKiosk.LogonProxy.LogOffKioskResponse LogOffKiosk(MBMEKiosk.LogonProxy.LogOffKioskRequest request);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ILogonService/RegisterKiosk", ReplyAction = "http://tempuri.org/ILogonService/RegisterKioskResponse")]
        MBMEKiosk.LogonProxy.RegisterKioskResponse RegisterKiosk(MBMEKiosk.LogonProxy.RegisterKioskRequest request);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILogonServiceChannel : MBMEKiosk.LogonProxy.ILogonService, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class LogonServiceClient : System.ServiceModel.ClientBase<MBMEKiosk.LogonProxy.ILogonService>, MBMEKiosk.LogonProxy.ILogonService
    {

        public LogonServiceClient()
        {
        }

        public LogonServiceClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public LogonServiceClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public LogonServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public LogonServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public MBMEKiosk.LogonProxy.LogOnKioskResponse LogOnKiosk(MBMEKiosk.LogonProxy.LogOnKioskRequest request)
        {
            return base.Channel.LogOnKiosk(request);
        }

        public MBMEKiosk.LogonProxy.LogOffKioskResponse LogOffKiosk(MBMEKiosk.LogonProxy.LogOffKioskRequest request)
        {
            return base.Channel.LogOffKiosk(request);
        }

        public MBMEKiosk.LogonProxy.RegisterKioskResponse RegisterKiosk(MBMEKiosk.LogonProxy.RegisterKioskRequest request)
        {
            return base.Channel.RegisterKiosk(request);
        }
    }
}