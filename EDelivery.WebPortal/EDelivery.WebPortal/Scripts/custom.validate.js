//require true
$.validator.addMethod("requiretrue", function (value, element, params) {
    var el = $(element);
    if (el.attr('type') == 'checkbox') {
        return el.is(':checked');
    }

    return (value == true);
});

$.validator.unobtrusive.adapters.add("requiretrue", ["propertyname"], function (options) {
    options.rules['requiretrue'] = options.params;
    options.messages["requiretrue"] = options.message;
});


$.validator.unobtrusive.adapters.add("requiredif", ["propertyname", "comparename", "comparevalue"], function (options) {
    options.rules['requiredif'] = options.params;
    if (options.message) {
        options.messages["requiredif"] = options.message;
    }
});

$.validator.addMethod("requiredif", function (value, element, params) {
    var el = $(element);
    if (el.val() != undefined && el.val() != '' && el.val() != null) {
        return true;
    }
    var otherItem = $('#' + params["comparename"]);
    if (otherItem.attr('type') == 'checkbox') {
        var val = otherItem.prop('checked');
        var compval = (params["comparevalue"] == 'True');
        return !(val == compval);
    }

    var val1 = otherItem.val();
    return !(val1 == params["comparevalue"]);
});

//personalidentifiervalidation
$.validator.unobtrusive.adapters.add("personalidentifiervalidation", ["propertyname"], function (options) {
    options.rules['personalidentifiervalidation'] = options.params;
    if (options.message) {
        options.messages["personalidentifiervalidation"] = options.message;
    }
});

$.validator.addMethod("personalidentifiervalidation", function (value, element, params) {
    var el = $(element);
    var egnOrLnch = value;
    //check the length
    if (egnOrLnch.length < 10) {
        return false;
    }
    //check if contains only digits
    if (!/^\d+$/.test(egnOrLnch)) {
        return false;
    }

    //check if the provided identifer is EGN
    var egnOrLnchArray = egnOrLnch.split('');
    var weights = [2, 4, 8, 5, 10, 9, 7, 3, 6];
    var sum = 0;
    try {
        for (var i = 0; i < 9; i++) {
            sum += weights[i] * parseInt(egnOrLnchArray[i]);
        }
        var controlPart = sum % 11;
        if (controlPart == 10) controlPart = 0;
        if (controlPart == parseInt(egnOrLnchArray[9])){
            //valid EGN
            return true;
        }
    }
    catch (ex) {
        console.log(ex);
    }

    //if we are here, the egn check hasn't succeeded. Check for lnch
    weights = [ 21, 19, 17, 13, 11, 9, 7, 3, 1 ];
    sum = 0;
    try {
        for (var i = 0; i < 9; i++) {
            sum += weights[i] * parseInt(egnOrLnchArray[i]);
        }
        if (sum % 10 == parseInt(egnOrLnchArray[9])) {
            //valid LNCH
            return true;
        }
    }
    catch (ex) {
        console.log(ex);
    }

    return false;
});

//file size
$.validator.unobtrusive.adapters.add('filesize', ['maxsize'], function (options) {
    options.rules['filesize'] = options.params;
    if (options.message) {
        options.messages['filesize'] = options.message;
    }
}
);

$.validator.addMethod('filesize', function (value, element, params) {
    if (element.files.length < 1) {
        // No files selected
        return true;
    }

    if (!element.files || !element.files[0].size) {
        // This browser doesn't support the HTML5 API
        return true;
    }

    return element.files[0].size < params.maxsize;
});

//file type
$.validator.unobtrusive.adapters.add('filetype', ['allow', 'deny'], function (options) {
    options.rules['filetype'] = options.params;
    if (options.message) {
        options.messages['filetype'] = options.message;
    }
}
);

$.validator.addMethod('filetype', function (value, element, params) {
    if (element.files.length < 1) {
        // No files selected
        return true;
    }

    if (!element.files || !element.files[0].size) {
        // This browser doesn't support the HTML5 API
        return true;
    }

    var fileExt = value.split('.').pop();
    if (params['allow'].length > 0) {
        return params['allow'].indexOf(fileExt) > -1;
    }
    if (params['deny'].length > 0) {
        return !(params['deny'].indexOf(fileExt) > -1);
    }
    return true;
});