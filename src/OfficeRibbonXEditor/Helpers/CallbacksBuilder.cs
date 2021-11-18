using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace OfficeRibbonXEditor.Helpers
{
    public class CallbacksBuilder
    {
        private const string ControlString = "control As IRibbonControl";
        private const string SubString = "Sub ";
        private const string EndSubString = "\nEnd Sub";
        
        private readonly HashSet<string> _callbackList = new HashSet<string>();

        private CallbacksBuilder()
        {
        }
        
        private enum BaseCallbackType
        {
            ButtonOnAction,
            ToggleButtonOnAction,
            CommandOnAction,
            GalleryOnAction,
            ComboBoxOnChange,
            GetBoolean,
            GetString,
            GetItemString,
            GetImage,
            GetItemImage,
            GetInt,
            GetSize,
            GetStyle,
            OnShow,
            OnLoad,
            LoadImage,
            UnKnown,
            NotCallback, // Always last
        }

        /// <summary>
        /// Generates callbacks for given custom UI Xml.
        /// </summary>
        /// <param name="customUiXml">The custom UI Xml to generate callbacks for.</param>
        /// <returns>List of callbacks.</returns>
        public static StringBuilder GenerateCallback(XmlDocument customUiXml)
        {
            var builder = new CallbacksBuilder();
            var result = new StringBuilder();
            builder.GenerateCallback(customUiXml.DocumentElement, result);
            return result;
        }

        private void GenerateCallback(XmlNode? node, StringBuilder result)
        {
            if (node == null)
            {
                return;
            }

            if (node.Attributes != null)
            {
                foreach (XmlAttribute? attribute in node.Attributes)
                {
                    if (attribute == null)
                    {
                        continue;
                    }

                    var callback = GenerateCallback(attribute);
                    if (string.IsNullOrEmpty(callback))
                    {
                        continue;
                    }

                    var controlId = GetControlId(node);
                    result.Append("'Callback for ");
                    if (string.IsNullOrEmpty(controlId))
                    {
                        result.Append(node.Name + "." + attribute.Name);
                    }
                    else
                    {
                        result.Append(controlId + " " + attribute.Name);
                    }

                    result.Append("\n");
                    result.Append(callback);
                    result.Append("\n\n");
                }
            }

            if (node.HasChildNodes)
            {
                foreach (XmlNode? child in node.ChildNodes)
                {
                    if (child == null)
                    {
                        continue;
                    }

                    GenerateCallback(child, result);
                }
            }
        }

        private static string? GetControlId(XmlNode node)
        {
            if (node.NodeType != XmlNodeType.Element || node.Attributes == null)
            {
                return null;
            }
            try
            {
                foreach (XmlAttribute? attribute in node.Attributes)
                {
                    if (attribute == null)
                    {
                        continue;
                    }

                    if (attribute.Name == "id" || attribute.Name == "idMso" || attribute.Name == "idQ")
                    {
                        return attribute.Value.Substring(attribute.Value.LastIndexOf(':') + 1);
                    }
                }
            }
            catch (XmlException ex)
            {
                Debug.Assert(false, ex.Message);
            }

            return null;
        }

        private string GenerateCallback(XmlAttribute callback)
        {
            if (string.IsNullOrEmpty(callback.Value))
            {
                return string.Empty;
            }

            var callbackValue = callback.Value.Substring(callback.Value.LastIndexOf('.') + 1);

            Debug.Assert(_callbackList != null, "AttributeList is null");

            if (_callbackList?.Contains(callbackValue) ?? false)
            {
                return string.Empty;
            }

            string result;

            var callbackType = MapToBase(callback);
            switch (callbackType)
            {
                case BaseCallbackType.ButtonOnAction:
                    result = GenerateVoidCallback(callbackValue);
                    break;

                case BaseCallbackType.CommandOnAction:
                    result = GenerateVoidCommand(callbackValue);
                    break;

                case BaseCallbackType.ToggleButtonOnAction:
                    result = GenerateToggleButtonOnActionCallback(callbackValue);
                    break;

                case BaseCallbackType.GalleryOnAction:
                    result = GenerateItemVoidCallback(callbackValue);
                    break;

                case BaseCallbackType.ComboBoxOnChange:
                    result = GenerateVoidOnChangeCallback(callbackValue);
                    break;

                case BaseCallbackType.GetBoolean:
                case BaseCallbackType.GetString:
                case BaseCallbackType.GetInt:
                case BaseCallbackType.GetImage:
                case BaseCallbackType.GetSize:
                    result = GenerateReturnCallback(callbackValue);
                    break;

                case BaseCallbackType.GetItemString:
                case BaseCallbackType.GetItemImage:
                    result = GenerateItemReturnCallback(callbackValue);
                    break;

                case BaseCallbackType.OnLoad:
                    result = GenerateOnLoadCallback(callbackValue);
                    break;

                case BaseCallbackType.OnShow:
                    result = GenerateOnShowCallback(callbackValue);
                    break;

                case BaseCallbackType.LoadImage:
                    result = GenerateLoadImageCallback(callbackValue);
                    break;

                case BaseCallbackType.GetStyle:
                    result = GenerateGetSlabStyleCallback(callbackValue);
                    break;

                case BaseCallbackType.UnKnown:
                    Debug.Assert(false, "Unknown callback " + callback.OwnerDocument?.Name + "." + callback.Name);
                    return string.Empty;
                    
                default:
                    return string.Empty;
            }

            _callbackList?.Add(callbackValue);
            return result;
        }

        private static string GenerateOnLoadCallback(string callback)
        {
            return SubString + callback + "(ribbon As IRibbonUI)" + EndSubString;
        }

        private static string GenerateOnShowCallback(string callback)
        {
            return SubString + callback + "(contextObject As Object)" + EndSubString;
        }

        private static string GenerateLoadImageCallback(string callback)
        {
            return SubString + callback + "(imageID As String, ByRef returnedVal)" + EndSubString;
        }

        private static string GenerateVoidCommand(string callback)
        {
            return SubString + callback + "(" + ControlString + ", ByRef cancelDefault)" + EndSubString;
        }

        private static string GenerateVoidCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ")" + EndSubString;
        }

        private static string GenerateVoidOnChangeCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ", text As String)" + EndSubString;
        }

        private static string GenerateToggleButtonOnActionCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ", pressed As Boolean)" + EndSubString;
        }

        private static string GenerateReturnCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ", ByRef returnedVal)" + EndSubString;
        }

        private static string GenerateItemVoidCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ", id As String, index As Integer)" + EndSubString;
        }

        private static string GenerateItemReturnCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ", index As Integer, ByRef returnedVal)" + EndSubString;
        }

        private static string GenerateGetSlabStyleCallback(string callback)
        {
            return SubString + callback + "(" + ControlString + ", ByRef returnedVal)" + "\n\treturnedVal = BackstageGroupStyle.BackstageGroupStyleWarning" + EndSubString;
        }

        private static BaseCallbackType MapToBase(XmlAttribute callback)
        {
            switch (callback.Name)
            {
                case "onLoad":
                    return BaseCallbackType.OnLoad;
                case "onShow":
                case "onHide":
                    return BaseCallbackType.OnShow;
                case "loadImage":
                    return BaseCallbackType.LoadImage;
                case "onAction":
                    switch (callback.OwnerElement?.Name)
                    {
                        case "dropDown":
                        case "gallery":
                            return BaseCallbackType.GalleryOnAction;
                        case "command":
                            return BaseCallbackType.CommandOnAction;
                        case "toggleButton":
                        case "checkBox":
                            return BaseCallbackType.ToggleButtonOnAction;
                        default:
                            return BaseCallbackType.ButtonOnAction;
                    }
                case "onChange":
                    return BaseCallbackType.ComboBoxOnChange;
                case "getEnabled":
                case "getVisible":
                case "getPressed":
                case "getShowLabel":
                case "getShowImage":
                    return BaseCallbackType.GetBoolean;
                case "getLabel":
                case "getScreentip":
                case "getSupertip":
                case "getDescription":
                case "getKeytip":
                case "getSelectedItemID":
                case "getImageMso":
                case "getContent":
                case "getText":
                case "getTitle":
                case "getTarget":
                case "getHelperText":
                    return BaseCallbackType.GetString;
                case "getItemLabel":
                case "getItemTooltip":
                case "getItemID":
                    return BaseCallbackType.GetItemString;
                case "getImage":
                    return BaseCallbackType.GetImage;
                case "getItemImage":
                    return BaseCallbackType.GetItemImage;
                case "getItemCount":
                case "getItemIndex":
                case "getItemHeight":
                case "getItemWidth":
                case "getSelectedItemIndex":
                    return BaseCallbackType.GetInt;

                case "getSize":
                case "getItemSize":
                    return BaseCallbackType.GetSize;

                case "getStyle":
                    return BaseCallbackType.GetStyle;

                default:
                    if (callback.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) || callback.Name.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                    {
                        return BaseCallbackType.UnKnown;
                    }
                    else
                    {
                        return BaseCallbackType.NotCallback;
                    }
            }
        }
    }
}
