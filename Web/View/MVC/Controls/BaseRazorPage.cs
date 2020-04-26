using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using System;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Controls
{
    public class BaseRazorPage<TModel> : RazorPage<TModel>
    {
        [RazorInject]
        public Client Client
        {
            get
            {
                return Client.Current;
            }
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
