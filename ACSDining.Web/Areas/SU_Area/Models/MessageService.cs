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

        static readonly Configuration Config =
           WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
        static readonly MailSettingsSectionGroup Settings =
            (MailSettingsSectionGroup)Config.GetSectionGroup("system.net/mailSettings");
        static MessageService()
        {
            NetworkCredential crident = null;
            try
            {
                crident = new System.Net.NetworkCredential(Settings.Smtp.Network.UserName,
                    Settings.Smtp.Network.Password);
            }
            catch (Exception)
            {
                    
                throw;
            }

            if (Settings != null)
            {
                Client = new SmtpClient
                {
                    Host = Settings.Smtp.Network.Host,
                    Port = Settings.Smtp.Network.Port,
                    Credentials = crident,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = Settings.Smtp.Network.DefaultCredentials,
                    EnableSsl = Settings.Smtp.Network.EnableSsl
                };

            }
        }

        public static async Task SendEmailAsync(List<User> users, MessageTopic topic, string datastring = null,
            string message = null)
        {
            MailMessage email = new MailMessage();
            email.From = new MailAddress(Settings.Smtp.Network.UserName, "Администрация столовой");
            users.ForEach(u =>
            {
                email.To.Add(new MailAddress(u.Email, u.LastName + " " + u.FirstName));
            });
            string template = await EMailTemplate(topic);
            email.Body = String.Format(template, datastring, message);
            email.Subject = topic == MessageTopic.MenuChanged
                ? "Изменения в меню"
                : topic == MessageTopic.Registration ? "Регистрация" 
                : "Создано меню для заказа";

            email.IsBodyHtml = true;
            try
            {
                await Client.SendMailAsync(email);
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
            StreamReader objstreamreaderfile = new StreamReader(templateFilePath);
            var body = await objstreamreaderfile.ReadToEndAsync();
            objstreamreaderfile.Close();
            return body;
        }
    }
}