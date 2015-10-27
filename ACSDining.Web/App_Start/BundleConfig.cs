using System.Web.Optimization;

namespace ACSDining.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/common").Include(
                        "~/Scripts/app/common.js"));


            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                        "~/Scripts/knockout-3.2.0.js"///,
                        //"~/Scripts/knockout.simpleGrid.3.0.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/MenuForWeek").Include(
                        "~/Areas/SU_Area/Content/scripts/MenuForWeekInfo.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/WeekOrders").Include(
                        "~/Areas/SU_Area/Content/scripts/OrdersInfo.js"
                        ));

            bundles.Add(new ScriptBundle("~/EmployeePanel/bundles/jquery").Include(
                        "~/Areas/EmployeeArea/Content/assets/js/jquery.js",
                        "~/Areas/EmployeeArea/Content/assets/js/bootstrap.min.js",
                        "~/Areas/EmployeeArea/Content/assets/js/jquery.easing-1.3.min.js",
                        "~/Areas/EmployeeArea/Content/assets/js/jquery.scrollTo-1.4.3.1-min.js",
                        "~/Areas/EmployeeArea/Content/assets/js/shop.js"
                        ));

            bundles.Add(new ScriptBundle("~/SU_Panel/bundles/jquery").Include(
                        "~/Scripts/jquery-2.1.4.min.js",
                        "~/Content/worthy/bootstrap/js/bootstrap.min.js",
                        "~/Content/worthy/plugins/modernizr.js",
                        "~/Content/worthy/plugins/isotope/isotope.pkgd.min.js",
                        "~/Content/worthy/plugins/jquery.backstretch.min.js",
                        "~/Content/worthy/plugins/jquery.appear.js",
                        "~/Areas/SU_Area/Content/scripts/template.js",
                        "~/Content/worthy/js/custom.js",
                        "~/Content/worthy/bootstrap/js/bootstrap-datepicker.js"//,
                        //"~/Content/worthy/js/dataTables/jquery.dataTables.js",
                        //"~/Content/worthy/js/dataTables/dataTables.bootstrap.js"
                        ));

            bundles.Add(new ScriptBundle("~/EmployeePanel/bundles/jquery").Include(
                        "~/Content/worthy/plugins/jquery.min.js",
                        "~/Content/worthy/bootstrap/js/bootstrap.min.js",
                        "~/Content/worthy/plugins/modernizr.js",
                        "~/Content/worthy/plugins/isotope/isotope.pkgd.min.js",
                        "~/Content/worthy/plugins/jquery.backstretch.min.js",
                        "~/Content/worthy/plugins/jquery.appear.js",
                        "~/Areas/EmployeeArea/Content/scripts/template.js",
                        "~/Content/worthy/js/custom.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/worthy").Include(
                        "~/Content/worthy/plugins/jquery.min.js",
                        "~/Content/worthy/bootstrap/js/bootstrap.min.js",
                        "~/Content/worthy/plugins/modernizr.js",
                        "~/Content/worthy/plugins/isotope/isotope.pkgd.min.js",
                        "~/Content/worthy/plugins/jquery.backstretch.min.js",
                        "~/Content/worthy/plugins/jquery.appear.js",
                        "~/Content/worthy/js/template.js",
                        "~/Content/worthy/js/custom.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/Modal").Include(
                     "~/Content/worthy/modal/js/jquery-1.10.2.js",
                     "~/Content/worthy/modal/js/bootstrap.min.js",
                     "~/Content/worthy/modal/js/jquery.metisMenu.js",
                     "~/Content/worthy/modal/js/custom.js"
                     ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

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

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Modal/css").Include(
                "~/Content/worthy/modal/css/bootstrap.css",
                "~/Content/worthy/modal/css/font-awesome.css",
                "~/Content/worthy/modal/css/custom.css"
                ));

            bundles.Add(new StyleBundle("~/Content/worthy").Include(
                "~/Content/worthy/bootstrap/css/bootstrap.css",
                "~/Content/worthy/fonts/font-awesome/css/font-awesome.css",
                "~/Content/worthy/css/animations.css",
                "~/Content/worthy/css/style.css",
                "~/Content/worthy/css/custom.css"
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

            bundles.Add(new StyleBundle("~/AdminPanel/css").Include(
                       "~/Content/bootstrap.css",
                       "~/Areas/AdminArea/Content/assets/css/font-awesome.css",
                       "~/Areas/AdminArea/Content/assets/js/morris/morris-0.4.3.min.css",
                       "~/Areas/AdminArea/Content/assets/css/custom.css"
                       ));

            bundles.Add(new StyleBundle("~/SU_Panel/worthy/css").Include(
                        "~/Content/worthy/bootstrap/css/bootstrap.css",
                        //"~/Content/worthy/bootstrap/css/jquery.dataTables.min.css",
                        "~/Content/worthy/fonts/font-awesome/css/font-awesome.css",
                        "~/Content/worthy/css/animations.css",
                        "~/Content/worthy/css/style.css",
                        "~/Content/worthy/css/custom.css",
                        "~/Content/worthy/bootstrap/css/bootstrap-datepicker3.css//",
                        "~/Content/worthy/js/dataTables/dataTables.bootstrap.css"
                       ));

            bundles.Add(new StyleBundle("~/EmployeePanel/css").Include(
                       "~/Areas/EmployeeArea/Content/assets/css/bootstrap.css",
                       "~/Areas/EmployeeArea/Content/style.css",
                       "~/Areas/EmployeeArea/Content/assets/font-awesome/css/font-awesome.css"
                       ));

        }
    }
}