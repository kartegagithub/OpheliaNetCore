using Microsoft.AspNetCore.Builder;
using Ophelia.Integration.I18NService.Middlewares;
using Ophelia.Integration.I18NService.Models;
using Ophelia.Service;
using System;
using System.Collections.Generic;

namespace Ophelia.Integration.I18NService
{
    public static class Extensions
    {
        public static IApplicationBuilder UseI18NService(this IApplicationBuilder builder)
        {
            return builder.UseI18NService(null);
        }
        public static void AccessedToTranslation(string name, string projectCode = "")
        {
            I18NMiddleware.I18NClient?.AccessedToTranslation(name, projectCode);
        }
        public static List<TranslationAccess> GetAccesses()
        {
            return I18NMiddleware.I18NClient?.Accesses;
        }
        public static void Flush()
        {
            I18NMiddleware.I18NClient?.Flush();
        }
        public static async void FlushAsynch()
        {
            await I18NMiddleware.I18NClient?.FlushAsynch();
        }
        public static ServiceCollectionResult<Models.TranslationPool> GetUpdates()
        {
            return I18NMiddleware.I18NClient?.GetUpdates();
        }
        public static ServiceObjectResult<bool> ValidateTranslations(Models.TranslationPoolValidatationModel entity)
        {
            return I18NMiddleware.I18NClient?.ValidateTranslations(entity);
        }
        public static ServiceObjectResult<Models.TranslationPool> GetTranslation(string name)
        {
            return I18NMiddleware.I18NClient?.GetTranslation(name);
        }
        public static ServiceObjectResult<bool> UpdateTranslation(Models.TranslationPool entity)
        {
            return I18NMiddleware.I18NClient?.UpdateTranslation(entity);
        }
        public static IApplicationBuilder UseI18NService(this IApplicationBuilder builder, Func<Options> optionBuilder)
        {
            if (optionBuilder != null)
                return builder.UseMiddleware<I18NMiddleware>(optionBuilder());
            return builder.UseMiddleware<I18NMiddleware>();
        }
    }
}
