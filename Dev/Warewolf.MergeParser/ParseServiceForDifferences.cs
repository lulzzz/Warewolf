﻿using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Presentation;
using System.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2;

namespace Warewolf.MergeParser
{
    public class ParseServiceForDifferences
    {
        readonly ModelItem _head;
        readonly ModelItem _mergeHead;

        public ParseServiceForDifferences(ModelItem mergeHead,ModelItem head)
        {
            _mergeHead = mergeHead ?? throw new ArgumentNullException(nameof(mergeHead));
            _head = head ?? throw new ArgumentNullException(nameof(head));
            
        }

        public List<ModelItem> MergeHeadNodes { get; private set; }
        public List<ModelItem> HeadNodes { get; private set; }

        public List<(Guid uniqueId, IDev2Activity activity, bool conflict)> GetDifferences()
        {
            var conflictList = new List<(Guid uniqueId, IDev2Activity activity, bool conflict)>();
            
            MergeHeadNodes = GetNodes(_mergeHead);
            HeadNodes = GetNodes(_head);

            var mergeHeadActivities = MergeHeadNodes.Select(i => i.GetProperty<IDev2Activity>("Action"));
            var headActivities = HeadNodes.Select(i => i.GetProperty<IDev2Activity>("Action"));

            var nodesDifferentInMergeHead = mergeHeadActivities.Except(headActivities);
            var nodesDifferentInHead = headActivities.Except(mergeHeadActivities);
            var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead);

            var isInOrder = mergeHeadActivities.Zip(headActivities, (act1, act2) => (act1,act2,act1.UniqueID == act2.UniqueID));
            if (isInOrder.All(b => b.Item3))
            {
                var equalItems = mergeHeadActivities.Intersect(headActivities);

                foreach (var item in equalItems)
                {
                    var equalItem = (Guid.Parse(item.UniqueID), item, false);
                    conflictList.Add(equalItem);
                }
            }
            else
            {
                foreach(var item in isInOrder)
                {
                    if (!item.Item3)
                    {
                        var diffItem1 = (Guid.Parse(item.act1.UniqueID), item.act1, true);
                        var diffItem2 = (Guid.Parse(item.act2.UniqueID), item.act2, true);
                        conflictList.Add(diffItem1);
                        conflictList.Add(diffItem2);
                    }
                }
            }
            
                  
            foreach (var item in allDifferences)
            {
                var diffItem = (Guid.Parse(item.UniqueID), item, true);
                conflictList.Add(diffItem);
            }


            return conflictList;

        }

        private List<ModelItem> GetNodes(ModelItem modelItem)
        {
            var wd = new WorkflowDesigner();
            wd.Load(modelItem);
            var modelService = wd.Context.Services.GetService<ModelService>();
            var nodeList = modelService.Find(modelItem, typeof(FlowNode)).ToList();
            wd = null;            
            return nodeList;
        }
    }
}