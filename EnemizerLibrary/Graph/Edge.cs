﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnemizerLibrary
{
    public class Edge
    {
        public Node SourceNode { get; set; }
        public Node DestinationNode { get; set; }
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        bool Unlocked { get; set; } = false;
        public Edge LinkedEdge { get; set; }

        public static List<Edge> MakeEdges(Node sourceNode, Node destinationNode, string requirements, bool isTwoWay)
        {
            var ret = new List<Edge>();

            Edge srcDestEdge = new Edge(sourceNode, destinationNode, requirements);
            ret.Add(srcDestEdge);
            if (isTwoWay)
            {
                Edge destSrcEdge = new Edge(destinationNode, sourceNode, requirements);
                srcDestEdge.LinkedEdge = destSrcEdge;
                destSrcEdge.LinkedEdge = srcDestEdge;
                ret.Add(destSrcEdge);
            }

            return ret;
        }

        public Edge(Node sourceNode, Node destinationNode)
        {
            this.SourceNode = sourceNode;
            this.DestinationNode = destinationNode;
            Unlocked = true;
        }

        public Edge(Node sourceNode, Node destinationNode, string requirements)
            :this(sourceNode, destinationNode)
        {
            Unlocked = false;
            foreach (var r in requirements.Split(';'))
            {
                if (r.Length > 0)
                {
                    List<Item> items = new List<Item>();
                    foreach (var reqItem in r.Split(','))
                    {
                        Item item;
                        if(!Data.GameItems.Items.TryGetValue(reqItem, out item))
                        {
                            throw new Exception($"Edge constructor - {sourceNode.LogicalId}->{destinationNode.LogicalId} could not find item {reqItem}");
                        }
                        items.Add(item);
                    }
                    this.Requirements.Add(new Requirement(items.ToArray()));
                }
            }
        }

        public bool MeetsRequirements(List<Item> items)
        {
            if (Requirements.Count == 0)
            {
                return true;
            }

            if(this.Unlocked)
            {
                return true;
            }

            int swordCount = items.Where(x => x.Id == "Progressive Sword").Count();
            int gloveCount = items.Where(x => x.Id == "Progressive Gloves").Count();

            foreach (var r in Requirements)
            {
                int count = 0;
                foreach(var i in r.Requirements)
                {
                    if(i is ConsumableItem)
                    {
                        var c = i as ConsumableItem;
                        if(items.Contains(c) && items.Any(x => x == c && ((ConsumableItem)x).Usable))
                        {
                            count++;
                            c.Consume();
                        }
                    }
                    else if(items.Contains(i))
                    {
                        count++;
                    }
                    else if(i is ProgressiveItem)
                    {
                        var split = i.Id.Split(' ');
                        if(split.Length > 1)
                        {
                            int level = (int)char.GetNumericValue(split[0][1]);

                            switch(split[1])
                            {
                                case "Sword":
                                    if(swordCount >= level)
                                    {
                                        count++;
                                    }
                                    break;
                                case "Gloves":
                                    if(gloveCount >= level)
                                    {
                                        count++;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                if(count == r.Requirements.Count)
                {
                    this.Unlocked = true;
                    if(this.LinkedEdge != null)
                    {
                        this.LinkedEdge.Unlocked = true;
                    }
                    return true;
                }
                //if (r.Requirements.Intersect(items).Count() == r.Requirements.Count)
                //{
                //    return true;
                //}
            }
            return false;
        }

        public override string ToString()
        {
            if (this.Requirements.Count > 0)
            {
                return $"Edge (req: {String.Join("; ", this.Requirements.Select(x => String.Join(", ", x.Requirements.Select(y => y.Name))))}) {this.SourceNode.Name} -> {this.DestinationNode.Name}";
            }
            return $"Edge (no req) {this.SourceNode.Name} -> {this.DestinationNode.Name}";
        }
    }
}
