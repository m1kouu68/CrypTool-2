﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Trivium.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Trivium.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Initialization rounds.
        /// </summary>
        internal static string InitRoundsCaption {
            get {
                return ResourceManager.GetString("InitRoundsCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How many initialization rounds should be performed? Default is 1152.
        /// </summary>
        internal static string InitRoundsTooltip {
            get {
                return ResourceManager.GetString("InitRoundsTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input data.
        /// </summary>
        internal static string InputDataCaption {
            get {
                return ResourceManager.GetString("InputDataCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data to be encrypted/decrypted.
        /// </summary>
        internal static string InputDataTooltip {
            get {
                return ResourceManager.GetString("InputDataTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Initialization vector.
        /// </summary>
        internal static string InputIVCaption {
            get {
                return ResourceManager.GetString("InputIVCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Up to 10 bytes (80 bit) long.
        /// </summary>
        internal static string InputIVTooltip {
            get {
                return ResourceManager.GetString("InputIVTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key.
        /// </summary>
        internal static string InputKeyCaption {
            get {
                return ResourceManager.GetString("InputKeyCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 10 bytes (80 bits) long (in hex)..
        /// </summary>
        internal static string InputKeyTooltip {
            get {
                return ResourceManager.GetString("InputKeyTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output data.
        /// </summary>
        internal static string OutputDataCaption {
            get {
                return ResourceManager.GetString("OutputDataCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Encrypted or decrypted output.
        /// </summary>
        internal static string OutputDataTooltip {
            get {
                return ResourceManager.GetString("OutputDataTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Trivium.
        /// </summary>
        internal static string PluginCaption {
            get {
                return ResourceManager.GetString("PluginCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current cipher from the eSTREAM project.
        /// </summary>
        internal static string PluginTooltip {
            get {
                return ResourceManager.GetString("PluginTooltip", resourceCulture);
            }
        }
    }
}
