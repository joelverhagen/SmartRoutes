﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ninject;
using Ninject.Extensions.Conventions;
using PolyGeocoder.Geocoders;
using PolyGeocoder.Support;
using SmartRoutes.Demo.OdjfsDatabase;
using SmartRoutes.Demo.OdjfsDatabase.Model;
using SmartRoutes.Graph;
using SmartRoutes.Model;
using SmartRoutes.Model.Gtfs;
using SmartRoutes.Model.Srds;
using SmartRoutes.Models;
using SmartRoutes.Models.Itinerary;
using SmartRoutes.Models.Payloads;
using SmartRoutes.Reader.Readers;
using SmartRoutes.Support;
using Location = PolyGeocoder.Support.Location;

namespace SmartRoutes.Controllers
{
    /// <summary>
    ///     Controller that handles requests from the guided search page.
    /// </summary>
    public class GuidedSearchPageController : Controller
    {
        private static IGraph _graph;

        public static IGraph Graph
        {
            get { return _graph ?? (_graph = BuildGraph()); }
        }

        private static IGraph BuildGraph()
        {
            IKernel kernel = new StandardKernel(new GraphModule());

            kernel.Bind(c => c
                .FromAssemblyContaining(typeof (GtfsCollection), typeof (IEntityCollectionDownloader<,>))
                .SelectAllClasses()
                .BindAllInterfaces());

            // get Metro models
            var gtfsFetcher = kernel.Get<IEntityCollectionDownloader<GtfsArchive, GtfsCollection>>();
            GtfsCollection gtfsCollection =
                gtfsFetcher.Download(new Uri("http://www.go-metro.com/uploads/GTFS/google_transit_info.zip"), null)
                    .Result;

            // get child care models
            var odjfsDatabase = new OdjfsDatabase("OdjfsDatabase");
            IEnumerable<ChildCare> childCares = odjfsDatabase.GetChildCares().Result;

            // build the graph
            var graphBuilder = kernel.Get<IGraphBuilder>();
            return graphBuilder.BuildGraph(gtfsCollection.StopTimes, childCares, GraphBuilderSettings.Default);
        }

        //
        // GET: /GuidedSearchPage/

        /// <summary>
        ///     Retrieves information about the available accreditations in JSON format.
        /// </summary>
        /// <returns>The JSON data.</returns>
        public JsonResult Accreditations()
        {
            return Json(ResourceModels.AccreditationModels, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Returns information about the child care service types in JSON format.
        /// </summary>
        /// <returns>JSON information.</returns>
        public JsonResult ServiceTypes()
        {
            return Json(ResourceModels.ServiceTypeModels, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Returns the raw HTML for the accreditation view.
        /// </summary>
        /// <returns>HTML string for the accreditation view.</returns>
        public ActionResult AccreditationView()
        {
            return PartialView("~/Views/Search/_AccreditationView.cshtml");
        }

        /// <summary>
        ///     Returns the raw HTML for the service type view.
        /// </summary>
        /// <returns>HTML string for the service type view.</returns>
        public ActionResult ServiceTypeView()
        {
            return PartialView("~/Views/Search/_ServiceTypeView.cshtml");
        }

        private static Destination Geocode(ISimpleGeocoder geocoder, IDictionary<string, Destination> destinations, AddressPayload addressPayload)
        {
            // convert the request to a string
            string request = string.Join(", ", new[]
            {
                addressPayload.Address,
                addressPayload.AddressLine2,
                addressPayload.City,
                addressPayload.State,
                addressPayload.ZipCode
            });

            // try to get a previous response
            Destination destination;
            if (destinations.TryGetValue(request, out destination))
            {
                return destination;
            }

            // geocode the address and save the result to the dictionary
            Response response = geocoder.GeocodeAsync(request).Result;
            Location location = response.Locations.FirstOrDefault();
            if (location != null)
            {
                destination = new Destination
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Name = location.Name
                };
            }
            else
            {
                throw new GeocoderException(
                    string.Format("The provivided address '{0}' could not be geocoded using geocode '{1}'.",
                        request,
                        geocoder.GetType().FullName));
            }
            destinations[request] = destination;

            return destination;
        }

        private static Func<IDestination, bool> CreateCriterion(ChildCareSearchQueryPayload searchQuery, ChildInformationPayload childInformation)
        {
            return destination =>
            {
                var childCare = destination as ChildCare;
                var detailedChildCare = destination as DetailedChildCare;
                if (childCare == null)
                {
                    return false;
                }

                if (detailedChildCare != null)
                {
                    // check the age group
                    if (ResourceModels.AgeGroupValidators.ContainsKey(childInformation.AgeGroup) && // is the age group valid?
                        ResourceModels.AgeGroupValidators.Values.Any(validate => validate(detailedChildCare)) && // are any age groups reported?
                        !ResourceModels.AgeGroupValidators[childInformation.AgeGroup](detailedChildCare)) // is the age group supported?
                    {
                        return false;
                    }

                    // check the accrediations
                    var checkedAccreditations = searchQuery
                        .Accreditations
                        .Where(a => a.Checked && ResourceModels.AccreditationValidators.ContainsKey(a.Name))
                        .ToArray();
                    if (checkedAccreditations.Any() &&
                        !checkedAccreditations.Any(a => ResourceModels.AccreditationValidators[a.Name](detailedChildCare)))
                    {
                        return false;
                    }
                }

                // check the service type
                if (!ResourceModels.ServiceTypeValidators.Values.Any(validator => validator(childCare)))
                {
                    return false;
                }

                return true;
            };
        }

        private static ChildCareSearchResultsModel GetSpoofedSearchResults()
        {
            var results = new ChildCareSearchResultsModel();

            results.AddChildCare(new ChildCareModel
            {
                Address = "47 CORRY BOULEVARD, CINCINNATI, OH, 45221",
                ChildCareName = "ARLITT CHILD DEVELOPMENT CENTER",
                Hours = new BusinessHoursModel[0],
                Link = null,
                PhoneNumber = "513-556-3802",
                ReviewLink = null
            });

            /*
            results.AddChildCare(new ChildCareModel
            {
                Address = "6601 HAMILTON, CINCINNATI, OH, 45224",
                ChildCareName = "ANGELS OF JOY CHILDREN LEARNING CENTER",
                Hours = new[]
                {
                    new BusinessHoursModel {Day = BusinessHoursModel.WeekDay.Monday, OpeningTime = "6:00 AM", ClosingTime = "11:55 PM"},
                    new BusinessHoursModel {Day = BusinessHoursModel.WeekDay.Tuesday, OpeningTime = "6:00 AM", ClosingTime = "11:55 PM"},
                    new BusinessHoursModel {Day = BusinessHoursModel.WeekDay.Wednesday, OpeningTime = "6:00 AM", ClosingTime = "11:55 PM"},
                    new BusinessHoursModel {Day = BusinessHoursModel.WeekDay.Thursday, OpeningTime = "6:00 AM", ClosingTime = "11:55 PM"},
                    new BusinessHoursModel {Day = BusinessHoursModel.WeekDay.Friday, OpeningTime = "6:00 AM", ClosingTime = "11:55 PM"}
                },
                Link = null,
                PhoneNumber = null,
                ReviewLink = null
            });
            results.AddChildCare(new ChildCareModel
            {
                Address = "1655 CHASE AVENUE, CINCINNATI, OH, 45223",
                ChildCareName = "AMICUS CHILDREN LEARNING CENTER MCKIE",
                Hours = new BusinessHoursModel[0],
                Link = null,
                PhoneNumber = "513-541-5300",
                ReviewLink = null
            });
            */

            var dropOff = new DropOffItineraryModel();
            dropOff.AddAction(new DepartAction("2300 Stratford Ave, Cincinnati, OH 45219"));
            dropOff.AddAction(new BoardBusAction("31", new DateTime(1970, 1, 1, 8, 13, 00), "Mcmillan St & Chickasaw St"));
            dropOff.AddAction(new ExitBusAction(new DateTime(1970, 1, 1, 8, 25, 0), "Mcmillan St & Scioto St"));
            dropOff.AddAction(new DropOffAction(new[] { 0 }, "ARLITT CHILD DEVELOPMENT CENTER"));
            dropOff.AddAction(new BoardBusAction("31", new DateTime(1970, 1, 1, 8, 47, 0), "Mcmillan St & Scioto St"));
            dropOff.AddAction(new ExitBusAction(new DateTime(1970, 1, 1, 9, 5, 0), "Mcmillan St & Symmes St"));
            dropOff.AddAction(new ArriveAction("499 E McMillan St, Cincinnati, OH 45206"));
            dropOff.Routes = new[] { "31", "31" };

            var pickUp = new PickUpItineraryModel();
            pickUp.AddAction(new DepartAction("499 E McMillan St, Cincinnati, OH 45206"));
            pickUp.AddAction(new BoardBusAction("31", new DateTime(1970, 1, 1, 17, 3, 00), "Mcmillan St & Symmes St"));
            pickUp.AddAction(new ExitBusAction(new DateTime(1970, 1, 1, 17, 20, 0), "Mcmillan St & Scioto St"));
            pickUp.AddAction(new PickUpAction(new[] { 0 }, "ARLITT CHILD DEVELOPMENT CENTER"));
            pickUp.AddAction(new BoardBusAction("31", new DateTime(1970, 1, 1, 17, 49, 0), "Mcmillan St & Scioto St"));
            pickUp.AddAction(new ExitBusAction(new DateTime(1970, 1, 1, 18, 2, 0), "Mcmillan St & Chickasaw St"));
            pickUp.AddAction(new ArriveAction("2300 Stratford Ave, Cincinnati, OH 45219"));

            results.AddChildCareRoute(new ChildCareRouteModel
            {
                ResultPriority = 0,
                ChildCareIndices = new[] { 0 },
                DropOffPlan = dropOff,
                PickUpPlan = pickUp
            });

            return results;
        }

        /// <summary>
        ///     Performs the child care search for the supplied query and returns
        ///     the results.
        /// </summary>
        /// <param name="searchQuery">The query for the search.</param>
        /// <returns>The results of the search.</returns>
        public JsonResult PerformChildCareSearch(ChildCareSearchQueryPayload searchQuery)
        {
            var geocoder = new OpenStreetMapGeocoder(new Client(), OpenStreetMapGeocoder.MapQuestEndpoint);
            var responses = new Dictionary<string, Destination>();

            IEnumerable<NodeInfo> dropOffResults = null;
            IEnumerable<NodeInfo> pickUpResults = null;

            if (searchQuery.ScheduleType.DropOffChecked)
            {
                Destination startLocation = Geocode(geocoder, responses, searchQuery.LocationsAndTimes.DropOffDepartureAddress);
                Destination endLocation = Geocode(geocoder, responses, searchQuery.LocationsAndTimes.DropOffDestinationAddress);

                Func<IDestination, bool>[] criteria = searchQuery
                    .ChildInformation
                    .Select(childInformation => CreateCriterion(searchQuery, childInformation))
                    .ToArray();

                dropOffResults = Graph.Search(
                    startLocation,
                    endLocation,
                    searchQuery.LocationsAndTimes.DropOffLatestArrivalTime,
                    TimeDirection.Backwards,
                    criteria);
            }

            if (searchQuery.ScheduleType.PickUpChecked)
            {
                Destination startLocation = Geocode(geocoder, responses, searchQuery.LocationsAndTimes.PickUpDepartureAddress);
                Destination endLocation = Geocode(geocoder, responses, searchQuery.LocationsAndTimes.PickUpDestinationAddress);

                Func<IDestination, bool>[] criteria = searchQuery
                    .ChildInformation
                    .Select(childInformation => CreateCriterion(searchQuery, childInformation))
                    .ToArray();

                pickUpResults = Graph.Search(
                    startLocation,
                    endLocation,
                    searchQuery.LocationsAndTimes.PickUpDepartureTime,
                    TimeDirection.Forwards,
                    criteria);
            }

            return Json(GetSpoofedSearchResults(), JsonRequestBehavior.AllowGet);
        }
    }
}