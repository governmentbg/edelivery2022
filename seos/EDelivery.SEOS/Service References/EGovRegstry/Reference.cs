﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDelivery.SEOS.EGovRegstry {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="uri:egovmsg", ConfigurationName="EGovRegstry.egovmsgPortType")]
    public interface egovmsgPortType {
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetAllRecords", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true, Use=System.ServiceModel.OperationFormatUse.Encoded)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Service))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Entity))]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        EDelivery.SEOS.EGovRegstry.EGovMessageDir GetAllRecords();
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetAllRecords", ReplyAction="*")]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetAllRecordsAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetNewRecords", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true, Use=System.ServiceModel.OperationFormatUse.Encoded)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Service))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Entity))]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        EDelivery.SEOS.EGovRegstry.EGovMessageDir GetNewRecords(System.DateTime date);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetNewRecords", ReplyAction="*")]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetNewRecordsAsync(System.DateTime date);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetByEntityIdentifier", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true, Use=System.ServiceModel.OperationFormatUse.Encoded)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Service))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Entity))]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        EDelivery.SEOS.EGovRegstry.EGovMessageDir GetByEntityIdentifier(string EntityIdentifier);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetByEntityIdentifier", ReplyAction="*")]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetByEntityIdentifierAsync(string EntityIdentifier);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetByGuid", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true, Use=System.ServiceModel.OperationFormatUse.Encoded)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Service))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Entity))]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        EDelivery.SEOS.EGovRegstry.EGovMessageDir GetByGuid(string guid);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetByGuid", ReplyAction="*")]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetByGuidAsync(string guid);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetByCertificateSN", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true, Use=System.ServiceModel.OperationFormatUse.Encoded)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Service))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Entity))]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        EDelivery.SEOS.EGovRegstry.EGovMessageDir GetByCertificateSN(string CertificateSN);
        
        [System.ServiceModel.OperationContractAttribute(Action="uri:egovmsg/GetByCertificateSN", ReplyAction="*")]
        [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
        System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetByCertificateSNAsync(string CertificateSN);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3190.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.SoapTypeAttribute(Namespace="uri:egovmsg")]
    public partial class EGovMessageDir : object, System.ComponentModel.INotifyPropertyChanged {
        
        private Entity[] entitiesField;
        
        private System.DateTime lastChangeField;
        
        /// <remarks/>
        public Entity[] Entities {
            get {
                return this.entitiesField;
            }
            set {
                this.entitiesField = value;
                this.RaisePropertyChanged("Entities");
            }
        }
        
        /// <remarks/>
        public System.DateTime LastChange {
            get {
                return this.lastChangeField;
            }
            set {
                this.lastChangeField = value;
                this.RaisePropertyChanged("LastChange");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3190.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.SoapTypeAttribute(Namespace="uri:egovmsg")]
    public partial class Entity : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string entityIdentifierField;
        
        private string guidField;
        
        private string parentGuidField;
        
        private string administrativeBodyNameField;
        
        private Contact contactField;
        
        private string certificateSNField;
        
        private Service[] servicesField;
        
        private System.DateTime lastChangeField;
        
        private string statusField;
        
        /// <remarks/>
        public string EntityIdentifier {
            get {
                return this.entityIdentifierField;
            }
            set {
                this.entityIdentifierField = value;
                this.RaisePropertyChanged("EntityIdentifier");
            }
        }
        
        /// <remarks/>
        public string Guid {
            get {
                return this.guidField;
            }
            set {
                this.guidField = value;
                this.RaisePropertyChanged("Guid");
            }
        }
        
        /// <remarks/>
        public string ParentGuid {
            get {
                return this.parentGuidField;
            }
            set {
                this.parentGuidField = value;
                this.RaisePropertyChanged("ParentGuid");
            }
        }
        
        /// <remarks/>
        public string AdministrativeBodyName {
            get {
                return this.administrativeBodyNameField;
            }
            set {
                this.administrativeBodyNameField = value;
                this.RaisePropertyChanged("AdministrativeBodyName");
            }
        }
        
        /// <remarks/>
        public Contact Contact {
            get {
                return this.contactField;
            }
            set {
                this.contactField = value;
                this.RaisePropertyChanged("Contact");
            }
        }
        
        /// <remarks/>
        public string CertificateSN {
            get {
                return this.certificateSNField;
            }
            set {
                this.certificateSNField = value;
                this.RaisePropertyChanged("CertificateSN");
            }
        }
        
        /// <remarks/>
        public Service[] Services {
            get {
                return this.servicesField;
            }
            set {
                this.servicesField = value;
                this.RaisePropertyChanged("Services");
            }
        }
        
        /// <remarks/>
        public System.DateTime LastChange {
            get {
                return this.lastChangeField;
            }
            set {
                this.lastChangeField = value;
                this.RaisePropertyChanged("LastChange");
            }
        }
        
        /// <remarks/>
        public string Status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
                this.RaisePropertyChanged("Status");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3190.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.SoapTypeAttribute(Namespace="uri:egovmsg")]
    public partial class Contact : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string phoneField;
        
        private string faxField;
        
        private string emailAddressField;
        
        /// <remarks/>
        public string Phone {
            get {
                return this.phoneField;
            }
            set {
                this.phoneField = value;
                this.RaisePropertyChanged("Phone");
            }
        }
        
        /// <remarks/>
        public string Fax {
            get {
                return this.faxField;
            }
            set {
                this.faxField = value;
                this.RaisePropertyChanged("Fax");
            }
        }
        
        /// <remarks/>
        public string EmailAddress {
            get {
                return this.emailAddressField;
            }
            set {
                this.emailAddressField = value;
                this.RaisePropertyChanged("EmailAddress");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3190.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.SoapTypeAttribute(Namespace="uri:egovmsg")]
    public partial class Service : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string guidField;
        
        private string nameField;
        
        private string uRIField;
        
        private string statusField;
        
        private string typeField;
        
        private string versionField;
        
        /// <remarks/>
        public string Guid {
            get {
                return this.guidField;
            }
            set {
                this.guidField = value;
                this.RaisePropertyChanged("Guid");
            }
        }
        
        /// <remarks/>
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
                this.RaisePropertyChanged("Name");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.SoapElementAttribute(DataType="anyURI")]
        public string URI {
            get {
                return this.uRIField;
            }
            set {
                this.uRIField = value;
                this.RaisePropertyChanged("URI");
            }
        }
        
        /// <remarks/>
        public string Status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
                this.RaisePropertyChanged("Status");
            }
        }
        
        /// <remarks/>
        public string Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
                this.RaisePropertyChanged("Type");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.SoapElementAttribute(DataType="integer")]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
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
    public interface egovmsgPortTypeChannel : EDelivery.SEOS.EGovRegstry.egovmsgPortType, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class egovmsgPortTypeClient : System.ServiceModel.ClientBase<EDelivery.SEOS.EGovRegstry.egovmsgPortType>, EDelivery.SEOS.EGovRegstry.egovmsgPortType {
        
        public egovmsgPortTypeClient() {
        }
        
        public egovmsgPortTypeClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public egovmsgPortTypeClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public egovmsgPortTypeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public egovmsgPortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public EDelivery.SEOS.EGovRegstry.EGovMessageDir GetAllRecords() {
            return base.Channel.GetAllRecords();
        }
        
        public System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetAllRecordsAsync() {
            return base.Channel.GetAllRecordsAsync();
        }
        
        public EDelivery.SEOS.EGovRegstry.EGovMessageDir GetNewRecords(System.DateTime date) {
            return base.Channel.GetNewRecords(date);
        }
        
        public System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetNewRecordsAsync(System.DateTime date) {
            return base.Channel.GetNewRecordsAsync(date);
        }
        
        public EDelivery.SEOS.EGovRegstry.EGovMessageDir GetByEntityIdentifier(string EntityIdentifier) {
            return base.Channel.GetByEntityIdentifier(EntityIdentifier);
        }
        
        public System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetByEntityIdentifierAsync(string EntityIdentifier) {
            return base.Channel.GetByEntityIdentifierAsync(EntityIdentifier);
        }
        
        public EDelivery.SEOS.EGovRegstry.EGovMessageDir GetByGuid(string guid) {
            return base.Channel.GetByGuid(guid);
        }
        
        public System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetByGuidAsync(string guid) {
            return base.Channel.GetByGuidAsync(guid);
        }
        
        public EDelivery.SEOS.EGovRegstry.EGovMessageDir GetByCertificateSN(string CertificateSN) {
            return base.Channel.GetByCertificateSN(CertificateSN);
        }
        
        public System.Threading.Tasks.Task<EDelivery.SEOS.EGovRegstry.EGovMessageDir> GetByCertificateSNAsync(string CertificateSN) {
            return base.Channel.GetByCertificateSNAsync(CertificateSN);
        }
    }
}