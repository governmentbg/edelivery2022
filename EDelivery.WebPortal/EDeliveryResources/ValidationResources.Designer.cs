﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDeliveryResources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ValidationResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ValidationResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EDeliveryResources.ValidationResources", typeof(ValidationResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сертификатът на подписа е невалиден, отменен или изтекъл към датата на подписване!.
        /// </summary>
        public static string DCSignatureCertificateNotValid {
            get {
                return ResourceManager.GetString("DCSignatureCertificateNotValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сертификатът на подписа е валиден към датата на подписване!.
        /// </summary>
        public static string DCSignatureCertificateValid {
            get {
                return ResourceManager.GetString("DCSignatureCertificateValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Подписаното съдържание не съответства на документа! Вероятно документът е различен или е променян след поставяне на подписа..
        /// </summary>
        public static string DSIntegrityNotOK {
            get {
                return ResourceManager.GetString("DSIntegrityNotOK", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Подписаното съдържание отговаря на съдържанието на документа!.
        /// </summary>
        public static string DSIntegrityOK {
            get {
                return ResourceManager.GetString("DSIntegrityOK", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Информация за Електронния Подпис.
        /// </summary>
        public static string DSLabelCertInfo {
            get {
                return ResourceManager.GetString("DSLabelCertInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Валидност на сертификата.
        /// </summary>
        public static string DSLabelCertValidity {
            get {
                return ResourceManager.GetString("DSLabelCertValidity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Интегритет.
        /// </summary>
        public static string DSLabelIntegrity {
            get {
                return ResourceManager.GetString("DSLabelIntegrity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Заверено от.
        /// </summary>
        public static string DSLabelIssuer {
            get {
                return ResourceManager.GetString("DSLabelIssuer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Подписан на.
        /// </summary>
        public static string DSLabelSignDate {
            get {
                return ResourceManager.GetString("DSLabelSignDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Подписан от.
        /// </summary>
        public static string DSLabelSignedBy {
            get {
                return ResourceManager.GetString("DSLabelSignedBy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Информация за Времевия Печат.
        /// </summary>
        public static string DSLabelTimestampCertInfo {
            get {
                return ResourceManager.GetString("DSLabelTimestampCertInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Удостоверено време на подписване.
        /// </summary>
        public static string DSLabelTimeStampDate {
            get {
                return ResourceManager.GetString("DSLabelTimeStampDate", resourceCulture);
            }
        }
    }
}