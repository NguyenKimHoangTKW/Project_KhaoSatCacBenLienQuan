using System.Web;
using System.Web.Optimization;

namespace CTDT
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-{version}.js"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            // Client Side
            bundles.Add(new ScriptBundle("~/bundles/User/HeDaoTao").Include(
                        "~/Scripts/User/Hedaotao.js"));
            bundles.Add(new ScriptBundle("~/bundles/User/Phieukhaosat").Include(
                        "~/Scripts/User/Phieukhaosat.js"));

            bundles.Add(new ScriptBundle("~/script/client/js").Include(
                        "~/Style/assets/js/aos.js",
                      "~/Style/assets/js/jquery.min.js",
                      "~/Style/assets/js/jquery-migrate-3.0.1.min.js",
                      "~/Style/assets/js/popper.min.js",
                      "~/Style/assets/js/bootstrap.min.js",
                      "~/Style/assets/js/jquery.easing.1.3.js",
                      "~/Style/assets/js/jquery.waypoints.min.js",
                      "~/Style/assets/js/jquery.stellar.min.js",
                      "~/Style/assets/js/owl.carousel.min.js",
                      "~/Style/assets/js/jquery.magnific-popup.min.js",
                      "~/Style/assets/js/jquery.animateNumber.min.js",
                      "~/Style/assets/js/bootstrap-datepicker.js",
                      "~/Style/assets/js/jquery.timepicker.min.js",
                      "~/Style/assets/js/scrollax.min.js",
                      "~/Style/assets/js/main.js"
                      ));

            bundles.Add(new Bundle("~/xac_thuc/js").Include(
                        "~/Scripts/XacThuc/XacThuc.js",
                        "~/Scripts/XacThuc/save_xac_thuc.js"
                      ));

            bundles.Add(new Bundle("~/phieu_khao_sat/js").Include(
                        "~/Scripts/User/Survey/PhieuKhaoSat/LoadPhieuKhaoSat.js",
                        "~/Scripts/User/Survey/PhieuKhaoSat/Save_Phieu.js"
                      ));

            bundles.Add(new Bundle("~/answer_phieu_khao_sat/js").Include(
                        "~/Scripts/User/Survey/PhieuKhaoSat/Load_Answer_Phieu_Khao_Sat.js",
                        "~/Scripts/User/Survey/PhieuKhaoSat/Save_Phieu.js"
                      ));
            //
            //CTDT
            bundles.Add(new ScriptBundle("~/ctdt/thongketylethamgiakhaosat/js").Include(
                       "~/Areas/CTDT/Scripts/ThongKeNguoiDungKhaoSat.js"));
            bundles.Add(new ScriptBundle("~/bundles/CTDT/js").Include(
                        "~/Areas/assets/js/vendors.min.js",
                        "~/Areas/assets/vendors/chartjs/Chart.min.js",
                        "~/Areas/assets/js/app.min.js",
                        "~/Areas/assets/vendors/select2/select2.min.js",
                        "~/Areas/assets/js/sweetalert2@11.js",
                        "~/Areas/assets/js/xlsx.full.min.js",
                        "~/Areas/assets/js/exceljs.min.js",
                        "~/Areas/assets/js/FileSaver.min.js",
                        "~/Areas/assets/vendors/datatables/jquery.dataTables.min.js",
                        "~/Areas/assets/vendors/datatables/dataTables.bootstrap.min.js",
                        "~/Areas/assets/js/dataTables.buttons.min.js",
                        "~/Areas/assets/js/jszip.min.js",
                        "~/Areas/assets/js/pdfmake.min.js",
                        "~/Areas/assets/js/vfs_fonts.js",
                        "~/Areas/assets/js/buttons.html5.min.js",
                        "~/Areas/assets/js/buttons.print.min.js",
                        "~/Areas/assets/js/login-google.js"));
            //
            // Admi
            BundleTable.EnableOptimizations = true;
        }
    }
}
