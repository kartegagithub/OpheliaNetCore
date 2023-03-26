using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Service
{
    public static class ServiceExtension
    {
        public static ServiceObjectResult<TEntity> Try<TEntity>(this ServiceResult service, Func<ServiceObjectResult<TEntity>> handler, Action onErrorAction = null)
        {
            var result = new ServiceObjectResult<TEntity>();
            try
            {
                result = handler();
            }
            catch (Exception ex)
            {
                onErrorAction?.Invoke();
                result.Fail(ex);
            }
            return result;
        }

        public static  async Task<ServiceObjectResult<TEntity>> TryAsync<TEntity>(this ServiceResult service, Func<Task<ServiceObjectResult<TEntity>>> handler, Action onErrorAction = null)
        {
            var result = new ServiceObjectResult<TEntity>();
            try
            {
                result = await handler();
            }
            catch (Exception ex)
            {
                onErrorAction?.Invoke();
                result.Fail(ex);
            }
            return result;
        }

        public static ServiceCollectionResult<TEntity> Try<TEntity>(this ServiceResult service, Func<ServiceCollectionResult<TEntity>> handler, Action onErrorAction = null)
        {
            var result = new ServiceCollectionResult<TEntity>();
            try
            {
                result = handler();
            }
            catch (Exception ex)
            {
                onErrorAction?.Invoke();
                result.Fail(ex);
            }
            return result;
        }

        public static async Task<ServiceCollectionResult<TEntity>> TryAsync<TEntity>(this ServiceResult service, Func<Task<ServiceCollectionResult<TEntity>>> handler, Action onErrorAction = null)
        {
            var result = new ServiceCollectionResult<TEntity>();
            try
            {
                result = await handler();
            }
            catch (Exception ex)
            {
                onErrorAction?.Invoke();
                result.Fail(ex);
            }
            return result;
        }
    }
}
