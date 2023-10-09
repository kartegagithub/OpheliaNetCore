using Ophelia.Integration.I18NService.Models;
using Ophelia.Service;
using System.Collections.Generic;

namespace Ophelia.Integration.I18NService.Services
{
    public class IntegrationService : Base.Facade
    {
        protected override string Schema => "Integration";
        public ServiceCollectionResult<TranslationPool> GetUpdates()
        {
            return this.GetCollection<TranslationPool>("GetUpdates", 1, int.MaxValue, null);
        }
        public ServiceObjectResult<bool> ValidateTranslations(TranslationPoolValidatationModel entity)
        {
            return this.PostObject<TranslationPoolValidatationModel, ServiceObjectResult<bool>>("ValidateTranslations", entity, null);
        }
        public ServiceObjectResult<TranslationAccessRequest> ProcessAccesses(List<TranslationAccess> accesses)
        {
            return this.GetObject<TranslationAccessRequest>("ProcessAccesses", new TranslationAccessRequest() { Accesses = accesses });
        }
        public ServiceObjectResult<TranslationPool> GetTranslation(string name)
        {
            return this.GetObject<TranslationPool>("GetTranslation", new TranslationPool() { Name = name });
        }
        public ServiceObjectResult<bool> UpdateTranslation(TranslationPool pool)
        {
            return this.PostObject<TranslationPool, ServiceObjectResult<bool>>("UpdateTranslation", pool, null);
        }

        public IntegrationService(I18NIntegratorClient API) : base(API)
        {
        }
    }
}
