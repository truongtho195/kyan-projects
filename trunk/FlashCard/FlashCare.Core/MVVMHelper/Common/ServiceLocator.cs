using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMHelper.Common
{
    /// <summary>
    /// Implements a simple service locator
    /// </summary>
    public class ServiceLocator : IServiceProvider, IDisposable
    {
        Dictionary<Type, object> services = new Dictionary<Type, object>();

        #region IServiceProvider Members
        
        /// <summary>
        /// Gets a service from the service locator
        /// </summary>
        /// <typeparam name="T">The type of service you want to get</typeparam>
        /// <returns>Returns the instance of the service</returns>
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// Registers a service to the service locator
        /// </summary>
        /// <param name="serviceType">The type of service to register. This is used so that you can register the service by an interface that the object implements</param>
        /// <param name="service">The service to add</param>
        /// <param name="overwriteIfExists">Passing true will replace any existing service</param>
        /// <returns>Returns true if the service was successfully registered</returns>
        /// <remarks>
        ///     <para>This generics based implementation ensures that the service must at least inherit from the service type.</para>
        ///     <para>NOTE: the MSDN documentation on IServiceProvidor states that the GetService method returns an object of type servieProvider</para>
        /// </remarks>
        public bool RegisterService<T>( T service, bool overwriteIfExists)
        {
            lock (services)
            {
                if (!services.ContainsKey(typeof(T)))
                {
                    services.Add(typeof(T), service);
                    return true;
                }
                else if (overwriteIfExists)
                {
                    services[typeof(T)] = service;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Registers a service to the service locator. This will overwrite any registered services with the same registration type
        /// </summary>
        /// <param name="serviceType">The type of service to register. This is used so that you can register the service by an interface that the object implements</param>
        /// <param name="service">The service to add</param>
        /// <returns>Returns true if the service was successfully registered</returns>
        /// <remarks>
        ///     <para>This generics based implementation ensures that the service must at least inherit from the service type.</para>
        ///     <para>NOTE: the MSDN documentation on IServiceProvidor states that the GetService method returns an object of type servieProvider</para>
        /// </remarks>
        public bool RegisterService<T>(T service)
        {
            return RegisterService<T>(service, true);
        }

        /// <summary>
        /// Gets a service from the service locator
        /// </summary>
        /// <param name="serviceType">The type of service you want to get</param>
        /// <returns>Returns the instance of the service</returns>
        /// <remarks>This implements IServiceProvider</remarks>
        public object GetService(Type serviceType)
        {
            lock (services)
            {
                if (services.ContainsKey(serviceType))
                    return services[serviceType];
            }
            return null;
        }

        #endregion

        private bool disposed = false;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!this.disposed)
            {
                // Note disposing has been done.
                disposed = true;
            }

            if(services != null)
                services.Clear();
            services = null;
        }

        ~ServiceLocator()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
    }
}
