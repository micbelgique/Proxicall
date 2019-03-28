﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProxiCall.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Localization {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Localization() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ProxiCall.Resources.Localization", typeof(Localization).Assembly);
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
        ///   Looks up a localized string similar to Que puis-je faire pour vous?.
        /// </summary>
        public static string AskForRequest {
            get {
                return ResourceManager.GetString("AskForRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Souhaitez vous être mis en contact ?.
        /// </summary>
        public static string AskIfWantForwardCall {
            get {
                return ResourceManager.GetString("AskIfWantForwardCall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Souhaitez-vous réessayer?.
        /// </summary>
        public static string AskIfWantRetry {
            get {
                return ResourceManager.GetString("AskIfWantRetry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Quelle est la personne que vous recherchez ?.
        /// </summary>
        public static string AskSearchedPersonFullName {
            get {
                return ResourceManager.GetString("AskSearchedPersonFullName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Veuillez répondre par &apos;Oui&apos; ou  &apos;Non&apos; pour confirmer..
        /// </summary>
        public static string AskYesOrNo {
            get {
                return ResourceManager.GetString("AskYesOrNo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bienvenue sur ProxiCall..
        /// </summary>
        public static string Greet {
            get {
                return ResourceManager.GetString("Greet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nous transférons votre appel..
        /// </summary>
        public static string InformAboutForwardingCall {
            get {
                return ResourceManager.GetString("InformAboutForwardingCall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Je n&apos;ai pas compris ce que vous venez de dire..
        /// </summary>
        public static string NoIntentFound {
            get {
                return ResourceManager.GetString("NoIntentFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} n&apos;a pas été trouvé..
        /// </summary>
        public static string PersonNotFound {
            get {
                return ResourceManager.GetString("PersonNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Le numéro de téléphone de {0} n&apos;a pas été trouvé..
        /// </summary>
        public static string PhoneNumberNotFound {
            get {
                return ResourceManager.GetString("PhoneNumberNotFound", resourceCulture);
            }
        }
    }
}
