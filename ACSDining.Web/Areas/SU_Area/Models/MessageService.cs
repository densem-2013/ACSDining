using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using ACSDining.Core.Domains;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public enum MessageTopic
    {
        MenuChanged,
        Registration,
        MenuCreated
    };
    public static class MessageService
    {
        private static readonly SmtpClient Client;
        static string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", "") +
                                      @"App.config";
        static readonly Configuration Config =
            WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            
           //WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
           //WebConfigurationManager.OpenWebConfiguration(_path);
        //static SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
        //static string username = smtpSection.Network.UserName;
        static readonly MailSettingsSectionGroup Settings =
            (MailSettingsSectionGroup)Config.GetSectionGroup("system.net/mailSettings");
        static MessageService()
        {
            NetworkCredential crident = null;
            try
            {
                crident = new System.Net.NetworkCredential(Settings.Smtp.Network.UserName,
                    Settings.Smtp.Network.Password);
                //crident = new System.Net.NetworkCredential(smtpSection.Network.UserName,
                //    smtpSection.Network.Password);
            }
            catch (Exception)
            {
                    
                throw;
            }

            if (Settings/*smtpSection*/ != null)
            {
                Client = new SmtpClient
                {
                    Host = Settings.Smtp.Network.Host,
                    Port = Settings.Smtp.Network.Port,
                    Credentials = crident,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    //UseDefaultCredentials = Settings.Smtp.Network.DefaultCredentials,
                    EnableSsl = Settings.Smtp.Network.EnableSsl,
                    //Host = smtpSection.Network.Host,
                    //Port = smtpSection.Network.Port,
                    //Credentials = crident,
                    //DeliveryMethod = SmtpDeliveryMethod.Network,
                    //UseDefaultCredentials = smtpSection.Network.DefaultCredentials,
                    //EnableSsl = smtpSection.Network.EnableSsl,
                    Timeout = 60000
                };

            }
        }

        public static void SendEmailAsync(List<User> users, MessageTopic topic, string datastring = null,
            string message = null)
        {
           MailMessage email = new MailMessage
            {
                From = new MailAddress(Settings.Smtp.Network.UserName, "Администрация столовой", System.Text.Encoding.UTF8),
                DeliveryNotificationOptions =
                    DeliveryNotificationOptions.OnFailure
            };
            users.ForEach(u =>
            {
                email.To.Add(new MailAddress(u.Email, u.LastName + " " + u.FirstName));
            });
            string template = EMailTemplate(topic).Result;
            email.Body = string.Format(template, datastring, message);
            email.Subject = (topic == MessageTopic.MenuChanged)
                ? "Изменения в меню"
                : topic == MessageTopic.Registration ? "Регистрация" 
                : "Создано меню для заказа";

            email.IsBodyHtml = true;
            try
            {
                //Task.Run(()=>).Wait();
               Client.Send(email);
            }
            catch (Exception)
            {
                    
                throw;
            }
        }

        public static async Task<string> EMailTemplate(MessageTopic topic)
        {
            string template = topic == MessageTopic.MenuChanged ? "DayMenuChanged" : topic == MessageTopic.Registration ? "RegistrationCompleted" : "MenuCreateCompleted";
            var templateFilePath = HostingEnvironment.MapPath("~/Areas/SU_Area/Views/SU_/EmailTemplates/") + template + ".html";
            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //                          @"ACSDining.Web\Areas\SU_Area\Views\SU_\EmailTemplates\";
            //var templateFilePath = _path + template + ".html";
            string body;
            try
            {
                StreamReader objstreamreaderfile = new StreamReader(templateFilePath);
                body = objstreamreaderfile.ReadToEndAsync().Result;
                objstreamreaderfile.Close();
            }
            catch (Exception)
            {
                    
                throw;
            }
            return await Task.FromResult(body);
        }
    }
}