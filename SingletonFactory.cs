using System;

namespace CollectionJsonExtended.Core
{
   
    public interface ISingletonFactory<out T>
        where T : class, new()
    {
        T GetInstance();        
    }

    public class SingletonFactory<T>: ISingletonFactory<T>
        where T : class, new()
    {
        private static volatile T _instance;
        private static readonly object _syncRoot = new object();

        private SingletonFactory()
        {
        }

        public SingletonFactory(Func<T> constructor)
        {
            lock (_syncRoot)
            {
                _instance = constructor();
            }
        }


        public virtual T GetInstance()
        {
            return Instance;
        }

        public static T Instance
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                        _instance = new T();
                }
                return _instance;
            }
        }
    }

}