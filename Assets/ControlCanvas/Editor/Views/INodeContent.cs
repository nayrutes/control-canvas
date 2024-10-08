﻿using ControlCanvas.Runtime;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public interface INodeContent
    {
        public VisualElement CreateView(IControl control);
    }
}