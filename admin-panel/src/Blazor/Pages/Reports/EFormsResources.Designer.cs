﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ED.AdminPanel.Blazor.Pages.Reports {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class EFormsResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EFormsResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ED.AdminPanel.Blazor.Pages.Reports.EFormsResources", typeof(EFormsResources).Assembly);
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
        ///   Looks up a localized string similar to Експорт.
        /// </summary>
        public static string BtnExport {
            get {
                return ResourceManager.GetString("BtnExport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Търси.
        /// </summary>
        public static string BtnSearch {
            get {
                return ResourceManager.GetString("BtnSearch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Показване на електронни форми в периода: {0} - {1}.
        /// </summary>
        public static string ColumnAll {
            get {
                return ResourceManager.GetString("ColumnAll", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Брой.
        /// </summary>
        public static string ColumnCount {
            get {
                return ResourceManager.GetString("ColumnCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Наименование.
        /// </summary>
        public static string ColumnName {
            get {
                return ResourceManager.GetString("ColumnName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Получател.
        /// </summary>
        public static string ColumnReceiver {
            get {
                return ResourceManager.GetString("ColumnReceiver", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to От дата.
        /// </summary>
        public static string FormFromDate {
            get {
                return ResourceManager.GetString("FormFromDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Наименование.
        /// </summary>
        public static string FormSubject {
            get {
                return ResourceManager.GetString("FormSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to До дата.
        /// </summary>
        public static string FormToDate {
            get {
                return ResourceManager.GetString("FormToDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Полето &quot;До дата&quot; трябва да е в диапазон 2 месеца след полето &quot;От дата&quot;.
        /// </summary>
        public static string MaxDateDiapasonErrorMessage {
            get {
                return ResourceManager.GetString("MaxDateDiapasonErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Няма заявени услуги за избрания период.
        /// </summary>
        public static string NoItems {
            get {
                return ResourceManager.GetString("NoItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Справка за услуги през еФорми.
        /// </summary>
        public static string Title {
            get {
                return ResourceManager.GetString("Title", resourceCulture);
            }
        }
    }
}
