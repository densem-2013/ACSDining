using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class PoolRequestModelBinder: IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var body = actionContext.Request.Content.ReadAsStringAsync().Result;
            //var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;// OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var pollRequest = JsonConvert.DeserializeObject<WeekMenuModel>(body/*, jsonFormatter.SerializerSettings*/);
            // string strRequest=body.Split('/');
            //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //WeekMenuModel pollRequest = serializer.Deserialize<WeekMenuModel>(body);
            bindingContext.Model = pollRequest;
            return true;
        }
    }
}