using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.UndoRedo;
using UniRx;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class UndoRedoManager : IDisposable
    {
        private readonly CommandManager _commandManager;
        private List<IReactivePropertyController> _reactivePropertyControllers = new List<IReactivePropertyController>();
        private CompositeDisposable _disposables = new CompositeDisposable();
        public UndoRedoManager(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public void SetupUndoRedo(ReactivePropertyManager reactivePropertyManager)
        {
            Dictionary<string, IDisposable> rps = reactivePropertyManager.GetAllReactiveProperties();
            foreach (var rp in rps)
            {
                Type type = rp.Value.GetType();
                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(ReactiveProperty<>))
                    {
                        Type[] genericArguments = type.GetGenericArguments();
                        Type genericArgument = genericArguments[0];
                        Type reactivePropertyControllerType = typeof(ReactivePropertyController<>).MakeGenericType(genericArgument);
                        IReactivePropertyController reactivePropertyController = (IReactivePropertyController)Activator.CreateInstance(reactivePropertyControllerType, rp.Value, _commandManager);
                        _reactivePropertyControllers.Add(reactivePropertyController);
                        _disposables.Add(reactivePropertyController);
                    }
                }
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}