using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.Repositories;

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

        private static string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", "") +
                                      @"App.config";

        private static readonly Configuration Config =
            WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);

        //WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
        //WebConfigurationManager.OpenWebConfiguration(_path);
        //static SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
        //static string username = smtpSection.Network.UserName;
        private static readonly MailSettingsSectionGroup Settings =
            (MailSettingsSectionGroup) Config.GetSectionGroup("system.net/mailSettings");

        static MessageService()
        {
            if (string.Equals(Settings.Smtp.Network.UserName, "robot@ia.ua"))
            {
                Client = new SmtpClient(Settings.Smtp.Network.Host, Settings.Smtp.Network.Port)
                {
                    Credentials = new NetworkCredential(Settings.Smtp.Network.UserName, Settings.Smtp.Network.Password),
                    EnableSsl = Settings.Smtp.Network.EnableSsl
                };
            }
            else
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
                        //UseDefaultCredentials = Settings.Smtp.Network.DefaultCredentials,
                        EnableSsl = Settings.Smtp.Network.EnableSsl,
                        Timeout = 60000
                    };

                }
            }
        }

        public static void SendEmailAsync(List<User> users, MessageTopic topic, string datastring = null,
            string message = null)
        {
            
            MailMessage email = new MailMessage
            {
                From =
                    new MailAddress(Settings.Smtp.Network.UserName/*"robot@ia.ua"*/, "Администрация столовой", System.Text.Encoding.UTF8)//,
                //DeliveryNotificationOptions =
                //    DeliveryNotificationOptions.OnFailure
            };
            users.ForEach(u =>
            {
                email.To.Add(new MailAddress(u.Email, u.LastName + " " + u.FirstName));
            });
            string template = EMailTemplate(topic).Result;
            email.Body = string.Format(template, datastring, message);
            email.Subject = (topic == MessageTopic.MenuChanged)
                ? "Изменения в меню"
                : topic == MessageTopic.Registration
                    ? "Регистрация"
                    : "Создано меню для заказа";

            email.IsBodyHtml = true;
            try
            {
                //Task.Run(()=>).Wait();
                Client.Send(email);
                //WatchdogMailMessage(users.Select(u => u.Email).ToList(), string.Format(template, datastring, message),
                //    "Администрация столовой");

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static async Task<string> EMailTemplate(MessageTopic topic)
        {
            string template = topic == MessageTopic.MenuChanged
                ? "DayMenuChanged"
                : topic == MessageTopic.Registration ? "RegistrationCompleted" : "MenuCreateCompleted";
            var templateFilePath = HostingEnvironment.MapPath("~/Areas/SU_Area/Views/SU_/EmailTemplates/") + template +
                                   ".html";
            //string templateFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", @"ACSDining.Web\Areas\SU_Area\Views\SU_\EmailTemplates\") + template + ".html";
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

        public static void SendUpdateDayMenuMessage(IRepositoryAsync<WeekOrderMenu> repo,MenuUpdateMessageDto mesdto)
        {
            int[] dayids = mesdto.UpdatedDayMenu.Select(upd => upd.DayMenuId).Distinct().ToArray();
           
            foreach (int id in dayids)
            {
                StringBuilder strbild = new StringBuilder();
                List<User> bookingUsers = repo.GetUsersMadeOrder(id);
                strbild.AppendLine("<table>");
                strbild.AppendLine("<thead><tr><th><th><th>Старое блюдо</th><th>Новое блюдо</th></tr></thead>");
                strbild.AppendLine("<tbody>");

                foreach (var mdch in mesdto.UpdatedDayMenu.Where(dm=>dm.DayMenuId==id).ToList())
                {
                    Dish olddish = repo.GetRepositoryAsync<Dish>().Find(mdch.OldDishId);
                    Dish newdish = repo.GetRepositoryAsync<Dish>().Find(mdch.NewDishId);
                    strbild.AppendLine("<tr>");
                    strbild.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td>", mdch.Category, olddish.Title,
                        newdish.Title);
                    strbild.AppendLine("</tr>");
                }
                strbild.AppendLine("</tbody>");
                strbild.AppendLine("</table>");
                List<User> userBooking =repo.GetUsersMadeOrder(id);
                SendEmailAsync(userBooking, MessageTopic.MenuChanged, mesdto.DateTime,
                   strbild.ToString());
            }
       }
        public static void WatchdogMailMessage(List<string> messageTo, string body, string subject)
        {

            //var watchdogSettings = new WatchdogSettings();

            var mailMessage = new MailMessage();

            try
            {

                var smtpServerName = "srv-terminal";

                using (var smtpClient = new SmtpClient(smtpServerName))
                {

                    smtpClient.Port = 25;
                    smtpClient.EnableSsl = false;
                    //smtpClient.UseDefaultCredentials = false;
                    mailMessage.From = new MailAddress("robot@ia.ua");

                    //foreach (var address in messageTo.Where(address => !string.IsNullOrEmpty(address)))
                    //{

                    //    mailMessage.To.Add(new MailAddress(address));

                    //}
                    mailMessage.To.Add(new MailAddress("pyatnarik2006@mail.ru"));
                    mailMessage.Subject =  subject;

                    mailMessage.IsBodyHtml = false;

                    mailMessage.Body = body;

                    smtpClient.Send(mailMessage);

                }

            }

            catch (Exception ex)
            {
                throw;
            }

        }
    }
}