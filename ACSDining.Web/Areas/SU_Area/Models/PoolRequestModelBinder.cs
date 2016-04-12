using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using ACSDining.Infrastructure.DTO.SuperUser;
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

            var pollRequest = JsonConvert.DeserializeObject<WeekMenuDto>(body/*, jsonFormatter.SerializerSettings*/);
            // string strRequest=body.Split('/');
            //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //WeekMenuDto pollRequest = serializer.Deserialize<WeekMenuDto>(body);
            bindingContext.Model = pollRequest;
            return true;
        }
    }
}