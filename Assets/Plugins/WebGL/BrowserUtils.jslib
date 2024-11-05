BrowserHelper = {

    JS_GetCurrentUrl: function () {
        var url = window.location.href;
        var bufferSize = lengthBytesUTF8(url) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(url, buffer, bufferSize);
        return buffer;
    },
    JS_GetUrlPath: function () {
        var urlPath = window.location.pathname;
        var bufferSize = lengthBytesUTF8(urlPath) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(urlPath, buffer, bufferSize);
        return buffer;
    },

    JS_GetUrlParams: function () {
        var urlParams = window.location.search;
        var bufferSize = lengthBytesUTF8(urlParams) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(urlParams, buffer, bufferSize);
        return buffer;
    },

    JS_SetCookie: function (cookieNamePtr, cookieValuePtr, expInDays) {
        cookieName = UTF8ToString(cookieNamePtr);
        cookieValue = UTF8ToString(cookieValuePtr);
        const d = new Date();
        d.setTime(d.getTime() + (expInDays * 24 * 60 * 60 * 1000));
        let expires = "expires=" + d.toUTCString();
        document.cookie = cookieName + "=" + cookieValue + ";" + expires + ";path=/";
        return true;
    },

    JS_GetCookie: function (cookieNamePtr) {
        cookieName = UTF8ToString(cookieNamePtr);
        let name = cookieName + "=";
        let cookieArray = document.cookie.split(';');
        for (let i = 0; i < cookieArray.length; i++) {
            let cookie = cookieArray[i];
            while (cookie.charAt(0) == ' ') {
                cookie = cookie.substring(1);
            }
            if (cookie.indexOf(name) == 0) {
                var cookieValue = cookie.substring(name.length, cookie.length)
                var bufferSize = lengthBytesUTF8(cookieValue) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(cookieValue, buffer, bufferSize);
                return buffer();
            }
        }
        return null;
    },

    JS_DeleteCookie: function (cookieNamePtr) {
        cookieName = UTF8ToString(cookieNamePtr);
        document.cookie = cookieName + '=; Max-Age=-99999999;';
        return true;
    }

}
mergeInto(LibraryManager.library, BrowserHelper);