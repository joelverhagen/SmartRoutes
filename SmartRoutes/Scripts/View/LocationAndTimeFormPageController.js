﻿
SmartRoutes.LocationAndTimeFormPageController = function(formPageRouteMap) {
    // Private:

    var pageIDRouteMap = formPageRouteMap;
    var locationAndTimeViewModel = null;

    (function Init() {
        locationAndTimeViewModel = new SmartRoutes.LocationAndTimeViewModel();

        ko.applyBindings(locationAndTimeViewModel.dropOffDepartureViewModel.dropOffDepartureAddressViewModel,
                        $("#sr-drop-off-departure-address-container")[0]);

        ko.applyBindings(locationAndTimeViewModel.dropOffDestinationViewModel.dropOffDestinationAddressViewModel,
                         $("#sr-drop-off-destination-address-container")[0]);
        ko.applyBindings(locationAndTimeViewModel.dropOffDestinationViewModel,
                        $("#sr-drop-off-final-destination-supplemental-input")[0]);

        ko.applyBindings(locationAndTimeViewModel.pickUpDepartureViewModel,
                         $("#sr-pick-up-departure-supplemental-input")[0]);
        ko.applyBindings(locationAndTimeViewModel.pickUpDepartureViewModel.pickUpDepartureAddressViewModel,
                         $("#sr-pick-up-departure-address-container")[0]);

        ko.applyBindings(locationAndTimeViewModel.pickUpDestinationViewModel,
                         $("#sr-pick-up-destination-supplemental-input")[0]);
        ko.applyBindings(locationAndTimeViewModel.pickUpDestinationViewModel.pickUpDestinationAddressViewModel,
                         $("#sr-pick-up-destination-address-container")[0]);
    })();

    return {
        // Public: 

        GetLocationAndTimeViewModel: function() {
            return locationAndTimeViewModel;
        }
    };
};