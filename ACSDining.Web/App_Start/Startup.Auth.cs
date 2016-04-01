using System.Web.Http;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace ACSDining.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {

            // Настройка контекста базы данных, диспетчера пользователей и диспетчера входа для использования одного экземпляра на запрос
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            // Включение использования файла cookie, в котором приложение может хранить информацию для пользователя, выполнившего вход,
            // и использование файла cookie для временного хранения информации о входах пользователя с помощью стороннего поставщика входа
            // Настройка файла cookie для входа
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                //Provider = new CookieAuthenticationProvider
                //{
                //    // Позволяет приложению проверять метку безопасности при входе пользователя.
                //    // Эта функция безопасности используется, когда вы меняете пароль или добавляете внешнее имя входа в свою учетную запись.  
                //    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, User>(
                //        validateInterval: TimeSpan.FromMinutes(30),
                //        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                //}
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }
    }
}