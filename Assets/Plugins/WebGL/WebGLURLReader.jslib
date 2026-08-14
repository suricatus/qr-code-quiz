mergeInto(LibraryManager.library, {
    GetURLParameters: function () {
        var queryString = window.location.search;

        if (!queryString && document.referrer) {
            var idx = document.referrer.indexOf('?');
            if (idx !== -1) {
                queryString = document.referrer.substring(idx);
            }
        }

        var bufferSize = lengthBytesUTF8(queryString) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(queryString, buffer, bufferSize);
        return buffer;
    }
});
