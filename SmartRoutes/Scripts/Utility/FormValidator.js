﻿
SmartRoutes.FormValidator = function() {

    var fields = new Array();

    var fieldTypes = {
        text: 0,
    };

    function ValidateTextField(newValue, regexString) {
        var regex = new RegExp(regexString);
        return regex.test(newValue);
    };

    function ValidateTextFieldCallback(eventData) {
        var validationInfo = eventData.data;

        if (validationInfo && validationInfo.element && validationInfo.regex) {
            var newValue = validationInfo.element.val();
            var valid = ValidateTextField(newValue, validationInfo.regex);

            if (validationInfo.callback && (typeof validationInfo.callback === "function")) {
                validationInfo.callback(validationInfo.element, valid, validationInfo.data);
            }
        }
    };

    return {
        // Public:

        // element - jquery object for the dom element
        // validationCallback - function taking a boolean indicating true if the field
        //                      validated successfully, false otherwise
        // section - an integer identifying the section of the form for this field
        AddTextField: function(element, validationCallback, regex, data) {
            if (element && regex) {
                var validationInfo = {
                    fieldType: fieldTypes.text,
                    element: element,
                    callback: validationCallback,
                    regex: regex,
                    data: data,
                };

                fields.push(validationInfo);

                element.blur(validationInfo, ValidateTextFieldCallback);
            }
        },
    };
};