using System;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels.UndoRedo;
using UniRx;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class UndoRedoManager : IDisposable
    {
        //private readonly CommandManager _commandManager;
        private List<IReactiveUndoRedoController> _reactivePropertyControllers = new List<IReactiveUndoRedoController>();
        private CompositeDisposable _disposables = new CompositeDisposable();
        private Dictionary<Type, MethodInfo> methodCacheCollection = new();

        // public UndoRedoManager(CommandManager commandManager)
        // {
        //     //_commandManager = commandManager;
        // }

        public void SetupUndoRedo(ReactivePropertyManager reactivePropertyManager)
        {
            Dictionary<string, IDisposable> rps = reactivePropertyManager.GetAllReactiveProperties(true);
            foreach (var rpKV in rps)
            {
                Type type = rpKV.Value.GetType();
                if (!type.IsGenericType)
                    continue;
                
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType != typeof(ReactiveProperty<>))
                    continue;
                
                Type[] genericArguments = type.GetGenericArguments();
                Type genericArgument = genericArguments[0];
                Type reactivePropertyControllerType = typeof(ReactivePropertyController<>).MakeGenericType(genericArgument);
                IReactiveUndoRedoController reactiveUndoRedoController = (IReactiveUndoRedoController)Activator.CreateInstance(reactivePropertyControllerType, rpKV.Value);
                _reactivePropertyControllers.Add(reactiveUndoRedoController);
                _disposables.Add(reactiveUndoRedoController);
                
                if (!genericArgument.IsGenericType)
                    continue;
                
                var genericArgumentType = genericArgument.GetGenericTypeDefinition();
                if (genericArgumentType != typeof(ReactiveCollection<>))
                    continue;
                
                Type[] innerGenericArguments = genericArgument.GetGenericArguments();
                Type innerGenericArgument = innerGenericArguments[0];
                if (!methodCacheCollection.TryGetValue(innerGenericArgument, out var genericHelperMethod))
                {
                    var helperMethod = GetType().GetMethod(nameof(SubscribeHelperCollection),
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    genericHelperMethod = helperMethod.MakeGenericMethod(genericArgument, innerGenericArgument);
                    methodCacheCollection[innerGenericArgument] = genericHelperMethod;
                }
                genericHelperMethod.Invoke(this, new object[] { rpKV.Value });
            }
        }

        
        private void SubscribeHelperCollection<T, TInner>(IDisposable property) where T : ReactiveCollection<TInner>
        {
            var typedProperty = (ReactiveProperty<T>)property;
            typedProperty.DoWithLast(last =>
                {
                    //TODO check for memory leaks (disposing of subscriptions)
                })
                .Where(x=>x != null).Subscribe(next =>
                {
                    IReactiveUndoRedoController redoController = new ReactiveCollectionController<TInner>(next);
                    _reactivePropertyControllers.Add(redoController);
                    _disposables.Add(redoController);
                }).AddTo(_disposables);
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}