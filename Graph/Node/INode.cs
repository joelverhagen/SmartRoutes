﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartRoutes.Model;
using SmartRoutes.Model.Sorta;

namespace SmartRoutes.Graph.Node
{
    public interface INode : ILocation
    {
        ISet<INode> UpwindNeighbors { get; set; }
        ISet<INode> DownwindNeighbors { get; set; }
        DateTime Time { get; }
        string Name { get; }
    }
}
