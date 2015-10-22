using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class PoolRequestModelBinder: IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var body = actionContext.Request.Content.ReadAsStringAsync().Result;
            var pollRequest = JsonConvert.DeserializeObject<WeekMenuModel>(body);
            bindingContext.Model = pollRequest;
            return true;
        }
    }
}