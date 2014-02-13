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

    [Obsolete]
    public sealed class Singleton
    {
        /*
         * http://msdn.microsoft.com/en-us/library/ms998558.aspx
         */
        private static volatile Singleton _instance;
        private static object _syncRoot = new Object();

        private Singleton() { }

        public static Singleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Singleton();
                    }
                }

                return _instance;
            }
        }
    }
    
    [Obsolete]
    public sealed class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _syncRoot = new object();

        public static T Instance
        {
            get
            {
                lock (_syncRoot)
                {

                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
                return _instance;
            }
        }

        private Singleton(){}
    }
    
    [Obsolete]
    public static class RegisteredSingleton<T> where T : class
    {
        /*
         * http://stackoverflow.com/questions/1928264/object-that-is-needed-throughout-the-application
         */

        static T _instance;
        static readonly object _syncRoot = new object();
        static bool _registered = false;

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }


        public static void Register(Func<T> constructor)
        {
            lock (_syncRoot)
            {
                if (!_registered)
                {
                    _instance = constructor();
                    _registered = true;
                }
            }
        }
    }
}