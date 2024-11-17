BrowserHelper = {

    JS_GetBaseUrl: function () {
        var url = location.origin;
        var bufferSize = lengthBytesUTF8(url) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(url, buffer, bufferSize);
        return buffer;
    },
    JS_GetCurrentUrl: function () {
        var url = location.href;
        var bufferSize = lengthBytesUTF8(url) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(url, buffer, bufferSize);
        return buffer;
    },
    JS_GetUrlPath: function () {
        var urlPath = location.pathname;
        var bufferSize = lengthBytesUTF8(urlPath) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(urlPath, buffer, bufferSize);
        return buffer;
    },

    JS_GetUrlParams: function () {
        var urlParams = location.search;
        var bufferSize = lengthBytesUTF8(urlParams) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(urlParams, buffer, bufferSize);
        return buffer;
    },

    JS_Redirect: function (urlPtr) {
        url = UTF8ToString(urlPtr);
        location.href = url;
        return true;
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
    },

    JS_TextFile_CreateObject: function (initialTextPtr) {
        if (initialTextPtr) {
            initialText = UTF8ToString(initialTextPtr);
        } else {
            initialText = "\n";
        }

        myInstance.textData = {
            initialContent: initialText +"\n",
            appendedContent: []
        };

        myInstance.textData.initialContent = initialText;
    },
    JS_TextFile_Append: function (textToAppendPtr) {
        myInstance.textData.appendedContent.push(UTF8ToString(textToAppendPtr));
    },
    JS_TextFile_CreateBlob: function (filenamePtr) {

        const blob = new Blob([myInstance.textData.initialContent + myInstance.textData.appendedContent.join('\n')], { type: 'text/plain' });

        // Create an Object URL for the Blob
        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = UTF8ToString(filenamePtr) + '.txt';

        // Trigger the download by simulating a click
        link.click();

        // Clean up the Object URL after download
        URL.revokeObjectURL(url);

    }



}
mergeInto(LibraryManager.library, BrowserHelper);