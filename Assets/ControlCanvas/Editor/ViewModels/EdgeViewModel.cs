﻿using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Serialization;

namespace ControlCanvas.Editor.ViewModels
{
    public class EdgeViewModel : BaseViewModel<EdgeData>
    {
        //public ReactiveProperty<EdgeData> edgeData { get; private set; } = new ReactiveProperty<EdgeData>();

        public string Guid => DataProperty.Value.Guid;
        public string StartNodeGuid => DataProperty.Value.StartNodeGuid;
        public string EndNodeGuid => DataProperty.Value.EndNodeGuid;

        public EdgeViewModel(NodeViewModel from, NodeViewModel to) : base()
        {
            //edgeData.Value.Guid = System.Guid.NewGuid().ToString();
            DataProperty.Value.StartNodeGuid = from.DataProperty.Value.guid;
            DataProperty.Value.EndNodeGuid = to.DataProperty.Value.guid;
        }

        public EdgeViewModel(EdgeData data, bool autobind) : base(data, autobind)
        {
        }

        protected override EdgeData CreateData()
        {
            return CreateEdgeData();
        }
        
        public static EdgeData CreateEdgeData()
        {
            EdgeData newData = new();
            newData.Guid = System.Guid.NewGuid().ToString();
            return newData;
        }
        
        public static EdgeData CreateEdgeData(string start, string end, string startPortName = null, string endPortName = null)
        {
            EdgeData newData = CreateEdgeData();
            newData.StartNodeGuid = start;
            newData.EndNodeGuid = end;
            //if(startPortName != "portOut")
                newData.StartPortName = startPortName;
            //if(endPortName != "portIn")
                newData.EndPortName = endPortName;
            return newData;
        }
    }
}