﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PatientAnalytics.Utils.Localization {
    using System;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ApiResponseLocalized_de {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ApiResponseLocalized_de() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("PatientAnalytics.Utils.Localization.ApiResponseLocalized_de", typeof(ApiResponseLocalized_de).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string RepeatedError_Email {
            get {
                return ResourceManager.GetString("RepeatedError_Email", resourceCulture);
            }
        }
        
        public static string RepeatedError_Username {
            get {
                return ResourceManager.GetString("RepeatedError_Username", resourceCulture);
            }
        }
        
        public static string HeaderError_Authorization {
            get {
                return ResourceManager.GetString("HeaderError_Authorization", resourceCulture);
            }
        }

        public static string AuthError_Unauthorized_SuperAdmin {
            get {
                return ResourceManager.GetString("AuthError_Unauthorized_SuperAdmin", resourceCulture);
            }
        }
        
        public static string AuthError_Unauthorized {
            get {
                return ResourceManager.GetString("AuthError_Unauthorized", resourceCulture);
            }
        }
        
        public static string AuthError_UserNotFound {
            get {
                return ResourceManager.GetString("AuthError_UserNotFound", resourceCulture);
            }
        }
        
        public static string AuthError_WrongPassword {
            get {
                return ResourceManager.GetString("AuthError_WrongPassword", resourceCulture);
            }
        }
        
        public static string AuthError_WeakPassword_IsHashLeaked {
            get {
                return ResourceManager.GetString("AuthError_WeakPassword_IsHashLeaked", resourceCulture);
            }
        }
        
        public static string AuthError_WeakPassword_Validation {
            get {
                return ResourceManager.GetString("AuthError_WeakPassword_Validation", resourceCulture);
            }
        }
        
        public static string AuthError_DecodeJwt_Parse {
            get {
                return ResourceManager.GetString("AuthError_DecodeJwt_Parse", resourceCulture);
            }
        }
        
        public static string AuthError_DecodeJwt_UserNotFound {
            get {
                return ResourceManager.GetString("AuthError_DecodeJwt_UserNotFound", resourceCulture);
            }
        }
        
        public static string PatientError_NotFound {
            get {
                return ResourceManager.GetString("PatientError_NotFound", resourceCulture);
            }
        }
        
        public static string PatientError_Forbidden {
            get {
                return ResourceManager.GetString("PatientError_Forbidden", resourceCulture);
            }
        }
        
        public static string PatientTemperatureError_NotFound {
            get {
                return ResourceManager.GetString("PatientTemperatureError_NotFound", resourceCulture);
            }
        }
        
        public static string PatientBloodPressureError_NotFound {
            get {
                return ResourceManager.GetString("PatientBloodPressureError_NotFound", resourceCulture);
            }
        }
        
        public static string PatientHeightError_NotFound {
            get {
                return ResourceManager.GetString("PatientHeightError_NotFound", resourceCulture);
            }
        }
        
        public static string PatientWeightError_NotFound {
            get {
                return ResourceManager.GetString("PatientWeightError_NotFound", resourceCulture);
            }
        }
    }
}
