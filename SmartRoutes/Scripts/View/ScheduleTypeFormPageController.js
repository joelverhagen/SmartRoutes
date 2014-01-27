﻿
// scheduleTypePageID - The element ID of the page
SmartRoutes.ScheduleTypeFormPageController = (function(pageID) {

    // Private: 
    var scheduleTypeFormPageID = pageID;
    var scheduleTypeViewModel = null;
    var pageValidationCallback = null;

    function ValidateScheduleTypeCallback(noSelectionNewValue) {
        if (pageValidationCallback) {
            var valid = !noSelectionNewValue;
            pageValidationCallback(valid);
        }
    };

    (function Init() {
        scheduleTypeViewModel = new SmartRoutes.ScheduleTypeViewModel();
        scheduleTypeViewModel.noScheduleTypeSelected.subscribe(ValidateScheduleTypeCallback);

        ko.applyBindings(scheduleTypeViewModel, $("#sr-schedule-type-form-page-view")[0]);

    })();

    return {
        // Public:

        RunPage: function(validationCallback) {
            pageValidationCallback = validationCallback;
            $("#" + scheduleTypePageID).fadeIn(SmartRoutes.Constants.formPageFadeInTime);
        },

        StopPage: function() {
            pageValidationCallback = null;
        },

        IsPageDataValid: function() {
            // A schedule type must be selected.
            return !scheduleTypeViewModel.noScheduleTypeSelected();
        },

        GetFormPageID: function() {
            return scheduleTypeFormPageID;
        },

        GetScheduleTypeInformation: function() {
            var scheduleType = {
                dropOffChecked: scheduleTypeViewModel.dropOffChecked(),
                pickUpChecked: scheduleTypeViewModel.pickUpChecked()
            }

            return scheduleType;
        }
    };

});
