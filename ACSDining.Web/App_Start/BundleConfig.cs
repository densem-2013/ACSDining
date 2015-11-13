using System.Web.Optimization;

namespace ACSDining.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {


            bundles.Add(new ScriptBundle("~/bundles/MenuForWeek").Include(
                        "~/Areas/SU_Area/Content/scripts/app.su_Service.js",
                        "~/Areas/SU_Area/Content/scripts/MenuForWeekInfo.js"
                        ));
            bundles.Add(new ScriptBundle("~/bundles/EmployeeInfo").Include(
                        "~/Areas/EmployeeArea/Content/scripts/EmployeeOrderInfo.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/AccountsInfo").Include(
                        "~/Areas/AdminArea/Content/scripts/app.service.js",
                        "~/Areas/AdminArea/Content/scripts/AccountsInfo.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/WeekOrders").Include(
                        "~/Areas/SU_Area/Content/scripts/OrdersInfo.js"
                        ));


            bundles.Add(new ScriptBundle("~/bundles/Modal").Include(
                     "~/Content/worthy/modal/js/jquery-1.10.2.js",
                     "~/Content/worthy/modal/js/bootstrap.min.js",
                     "~/Content/worthy/modal/js/jquery.metisMenu.js",
                     "~/Content/worthy/modal/js/custom.js"
                     ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-2.1.4.min.js",
                        "~/Scripts/jquery-ui-1.11.4.min.js",
                        "~/Scripts/jquery-ui-cup.min.js",
                        "~/Scripts/jquery-ui-i18n.min.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/transition.js",
                        "~/Scripts/collapse.js",
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/bootstrap-datetimepicker.js",
                        "~/Scripts/knockout-3.2.0.js",
                        "~/Scripts/knockout-bootstrap.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/Site.css",
                       "~/Content/bootstrap.css",
                       "~/Content/css/fonts/font-awesome.css",
                       "~/Content/themes/Cupertino/jquery-ui.min.css",
                       "~/Content/themes/Cupertino/jquery-ui.structure.min.css",
                       "~/Content/themes/Cupertino/jquery-ui.theme.min.css",
                       "~/Content/css/bootstrap-datetimepicker.min.css",
                       "~/Content/css/style.css",
                       "~/Content/css/fonts/custom.css"));

            bundles.Add(new StyleBundle("~/Modal/css").Include(
                "~/Content/worthy/modal/css/bootstrap.css",
                "~/Content/worthy/modal/css/font-awesome.css",
                "~/Content/worthy/modal/css/custom.css"
                ));


            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));


        }
    }
}