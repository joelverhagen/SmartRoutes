﻿/* requires(SmartRoutes.js) */

SmartRoutes.GuidedSearchViewController = (function() {

    // Private:
    
    var formPageSammyApp = null;
    var activePageElement = null;

    var pageIDs = {
        childInformationPageID: "sr-child-information-form-page-view",
        scheduleTypePageID: "sr-schedule-type-form-page-view",
        locationAndTimePageID: "sr-location-time-form-page-view",
        accreditationPageID: "sr-accreditation-form-page-view",
        serviceTypePageID: "sr-service-type-form-page-view"
    };
    
    var pageIDRouteMap = {
        "sr-child-information-form-page-view": "#/search/childinformation",
        "sr-schedule-type-form-page-view": "#/search/scheduletype",
        "sr-location-time-form-page-view": "#/search/locationsandtimes",
        "sr-accreditation-form-page-view": "#/search/accreditation",
        "sr-service-type-form-page-view": "#/sereach/servicetype"
    }
    var childInformationFormPageController = null;
    var scheduleTypeFormPageController = null;
    var locationAndTimeFormPageController = null;
    var accreditationFormPageController = null;
    var serviceTypeFormPageController = null;

    var InitPageSubroutes = function() {
        formPageSammyApp = $.sammy(function() {
            // TODO: Validation should be handled when the Next button is clicked.
            // However, we also need to prevent navigating directly to a URL.
            // If the navigation wasn't done with the Next button, it might make sense
            // to simply dump the user back on the first search form page?

            this.get(pageIDRouteMap[pageIDs.childInformationPageID], function() {
                $(".sr-form-page").hide();
                childInformationFormPageController.RunPage();
            });

            this.get(pageIDRouteMap[pageIDs.scheduleTypePageID], function() {
                $(".sr-form-page").hide();
                scheduleTypeFormPageController.RunPage();
            });

            this.get(pageIDRouteMap[pageIDs.locationAndTimePageID], function() {
                $(".sr-form-page").hide();
                $("#" + pageIDs.locationAndTimePageID).show();
            });

            this.get(pageIDRouteMap[pageIDs.accreditationPageID], function() {
                $(".sr-form-page").hide();
                $("#" + pageIDs.accreditationPageID).show();
            });

            this.get(pageIDRouteMap[pageIDs.serviceTypePageID], function() {
                $(".sr-form-page").hide();
                $("#" + pageIDs.serviceTypePageID).show();
            });
        });
    };

    var InitChildInfoPage = function() {
        childInformationFormPageController = new SmartRoutes.ChildInformationFormPageController(pageIDs.childInformationPageID);
    };

    var InitScheduleTypePage = function() {
        scheduleTypeFormPageController = new SmartRoutes.ScheduleTypeFormPageController(pageIDs.scheduleTypePageID);
    };

    var InitLocationAndTimePage = function() {
        locationAndTimeFormPageController = new SmartRoutes.LocationAndTimeFormPageController(pageIDRouteMap);
    };

    var InitAccreditationPage = function() {
        accreditationFormPageController = new SmartRoutes.AccreditationFormPageController();
    };

    var InitServiceTypeFormPage = function() {
        serviceTypeFormPageController = new SmartRoutes.ServiceTypeFormPageController();
    };

    (function Init() {
        InitPageSubroutes();
        InitChildInfoPage();
        InitScheduleTypePage();
        InitLocationAndTimePage();
        InitAccreditationPage();
        InitServiceTypeFormPage();
    })();

    // Event handlers

    $("#sr-guided-search-button-previous").click(function() {
        var previousPage = $(activePageElement).prev(".sr-form-page");

        if (previousPage.length > 0) {
            // Change the route, this will also change the page.
            var previousPageID = previousPage.attr("id");
            formPageSammyApp.setLocation(pageIDRouteMap[previousPageID]);

            activePageElement = previousPage;
        }
    });

    $("#sr-guided-search-button-next").click(function() {
        var nextPage = $(activePageElement).next(".sr-form-page");

        if (nextPage.length > 0) {
            var nextPageID = nextPage.attr("id");
            formPageSammyApp.setLocation(pageIDRouteMap[nextPageID]);

            // TODO: should do page validation here since the user
            // has indicated that they want to move to the next page.
            activePageElement = nextPage;
        }
    });


    return {
        // Public:

        RunPage: function() {
            // Anywhere else just needs to navigate to #/search.
            // This controller will navigate to the correct sub-route.
            formPageSammyApp.setLocation(pageIDRouteMap[pageIDs.childInformationPageID]);
            activePageElement = $("#" + pageIDs.childInformationPageID);
        }
    };
});