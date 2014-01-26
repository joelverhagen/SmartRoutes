﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.Extensions.Conventions;
using SmartRoutes.Graph.Node;
using SmartRoutes.Model;
using SmartRoutes.Model.Gtfs;
using SmartRoutes.Model.Srds;
using SmartRoutes.Reader.Readers;

namespace SmartRoutes.Graph
{
    struct Location : ILocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IKernel kernel = new StandardKernel(new GraphModule());

                kernel.Bind(c => c
                    .FromAssemblyContaining(typeof(GtfsCollection), typeof(IEntityCollectionDownloader<,>))
                    .SelectAllClasses()
                    .BindAllInterfaces());

                /********************************************************************************************
                ******************* Fetch the data from the database and build the graph ********************
                ********************************************************************************************/

                Console.WriteLine("Fetching the GTFS data from the web.");
                DateTime tic = DateTime.Now;
                var gtfsReader = kernel.Get<IEntityCollectionReader<GtfsArchive, GtfsCollection>>();
                var gtfsCollection = gtfsReader.Read(@"google_transit_info.zip", null).Result;
                DateTime toc = DateTime.Now;
                Console.WriteLine("GTFS data fetched in {0} milliseconds.", (toc - tic).TotalMilliseconds);
                
                Console.WriteLine("Fetching the destination data from the web.");
                tic = DateTime.Now;
                var srdsReader = kernel.Get<IEntityCollectionReader<SrdsArchive, SrdsCollection>>();
                var srdsCollection = srdsReader.Read(@"srds_odjfs.zip", null).Result;
                toc = DateTime.Now;
                Console.WriteLine("Destination data fetched in {0} milliseconds.", (toc - tic).TotalMilliseconds);

                Console.WriteLine("Creating Graph.");
                tic = DateTime.Now;
                var graphBuilder = kernel.Get<IGraphBuilder>();
                var graph = graphBuilder.BuildGraph(gtfsCollection.StopTimes, srdsCollection.Destinations, GraphBuilderSettings.Default);
                toc = DateTime.Now;
                Console.WriteLine("Graph created in {0} milliseconds.", (toc - tic).TotalMilliseconds);
                Console.WriteLine("Finding route...");
                tic = DateTime.Now;

                /********************************************************************************************
                ******************** Example usage of Dijkstras for complete route **************************
                ********************************************************************************************/
                                
                // starting at my address
                var homeLocation = new Location { Latitude = 39.122309, Longitude = -84.507639 };

                // ending at the college of engineering
                var workLocation = new Location { Latitude = 39.133292, Longitude = -84.515099 };

                // have to be at work by 10:30 am
                var atWorkBy = new DateTime(1970, 1, 1, 10, 30, 0);

                // child care selected by name, since properties are not implemented yet
                const string childCareName = "ANOINTED HANDS LEARNING CENTER";

                // search starts at work, going backwards
                var workNodes = graph.GetClosestGtfsNodes(workLocation, atWorkBy, TimeDirection.Backwards, 100);

                // since we don't have properties on our location nodes yet, let's just filter by name.
                // this returns two results (apparently there are two child cares with this name)
                Func<INode, bool> goalCheck = node =>
                {
                    var nodeAsChildCare = node as DestinationNode;
                    if (nodeAsChildCare != null)
                    {
                        return nodeAsChildCare.Name == childCareName;
                    }
                    return false;
                };

                var workToChildCareResults = ExtensionMethods.Dijkstras(workNodes, goalCheck, TimeDirection.Backwards);

                // First step, find which bus stop is closest to my house, and set that as destination
                var closeToHomeStops = graph.GetClosestGtfsStops(homeLocation, 100);

                Func<INode, bool> goalCheck2 = node =>
                {
                    var nodeAsGtfsNode = node as IGtfsNode;
                    if (nodeAsGtfsNode != null)
                    {
                        bool match = false;
                        foreach (var stop in closeToHomeStops)
                        {
                            if (nodeAsGtfsNode.StopId == stop.Id)
                            {
                                match = true;
                                break;
                            }
                        }
                        return match;
                    }
                    return false;
                };

                // now for each child care result, we find our way home.
                var finalResults = new List<NodeInfo>();
                var result = workToChildCareResults.First();
                var startNodes = graph.GetDestinationNeighbors((IDestinationNode)result.node, TimeDirection.Backwards);
                var resultList = ExtensionMethods.Dijkstras(
                    startNodes, 
                    goalCheck2, 
                    TimeDirection.Backwards);

                var result2 = resultList.First();

                // we want to stich together the two routes to make one resulting route
                var current = result2;
                while (current.parent != null) current = current.parent;
                current.parent = result;

                finalResults.Add(result2);

                toc = DateTime.Now;
                Console.WriteLine("Route found in in {0} milliseconds.",
                    (toc - tic).TotalMilliseconds);

                Console.WriteLine("Displaying Route.");
                current = finalResults.First();
                while (current != null)
                {
                    Console.WriteLine("{0} -- {1}", current.node.Name, current.node.Time);
                    current = current.parent;
                }

                Console.WriteLine("fin");



                //DateTime toc = DateTime.Now;

                //Console.WriteLine("Graph created in {0} milliseconds.", (toc - tic).TotalMilliseconds);
                //Console.WriteLine("Performing Dijkstras...");
                //tic = DateTime.Now;

                //int count = 0;

                //Func<INode, bool> GoalCheck = node =>
                //{
                //    var check = node as DestinationNode;
                //    return check != null;
                //};

                //var Results = new List<NodeInfo>();

                //var StartNodes = new INode[] { graph.GraphNodes[count] };
                //Results = ExtensionMethods.Dijkstras(StartNodes, GoalCheck, TimeDirection.Forwards);

                //toc = DateTime.Now;
                //Console.WriteLine("Dijkstra's completed in {0} milliseconds, {1} results found.", 
                //    (toc - tic).TotalMilliseconds, Results.Count());

                //List<NodeBase> UniqueChildCareBases = new List<NodeBase>();

                //foreach (var node in graph.GraphNodes)
                //{
                //    var cnode = node as DestinationNode;
                //    if (cnode == null) continue;
                //    if (!UniqueChildCareBases.Contains(cnode.BaseNode))
                //    {
                //        UniqueChildCareBases.Add(cnode.BaseNode);
                //    }
                //}

                //Console.WriteLine("{0} unique child cares found in graph.", UniqueChildCareBases.Count());

            Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }
    }
}
