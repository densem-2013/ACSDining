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