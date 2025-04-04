using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace JSBrowserUtilities
{
    public class BrowserHelper
    {
        [DllImport("__Internal")]
        public static extern string JS_GetBaseUrl();
        [DllImport("__Internal")]
        public static extern string JS_GetCurrentUrl();
        [DllImport("__Internal")]
        public static extern string JS_GetUrlPath();
        [DllImport("__Internal")]
        public static extern string JS_GetUrlParams();
        [DllImport("__Internal")]
        public static extern bool JS_Redirect(string url);
        [DllImport("__Internal")]
        public static extern bool JS_SetCookie(string cookieName, string cookieValue, int expInDays);
        [DllImport("__Internal")]
        public static extern string JS_GetCookie(string cookieName);
        [DllImport("__Internal")]
        public static extern bool JS_DeleteCookie(string cookieName);

         [DllImport("__Internal")]
        public static extern bool JS_TextFile_CreateObject(string initialText = null);
        [DllImport("__Internal")]
        public static extern bool JS_TextFile_Append(string textToAppend);
        [DllImport("__Internal")]
        public static extern bool JS_TextFile_CreateBlob(string filename);
        [DllImport("__Internal")]
        public static extern bool JS_TextFile_Download();
    }
}
