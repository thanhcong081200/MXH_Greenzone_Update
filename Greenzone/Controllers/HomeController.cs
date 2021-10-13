using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Greenzone.Models;
using System.Dynamic;

namespace Greenzone.Controllers
{
    public class HomeController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext();
        [HttpGet]
        public ActionResult Index()
        {

            ViewBag.Title = "Trang chủ - Social Network Greenzone";
            //Cho 2 hàm mượn vào đây
            dynamic dy = new ExpandoObject();
            dy.user = getUser();
            dy.baiviet = getBaiViet();
            return View(dy);
        }

        //Mượn hàm USERR
        public List<User> getUser()
        {
            List<User> user = db.Users.ToList();
            return user;
        }

        //Mượn hàm BAIVIET
        public List<BaiViet> getBaiViet()
        {
            List<BaiViet> lst = new List<BaiViet>();
            User iduser = Session["IDuser"] as User;
            if (iduser != null)
            {
                string id = iduser.idUser;
                lst = db.BaiViets.Where(bv => bv.idUser == id.ToString()).OrderByDescending(p => p.idBaiViet).ToList();
            }
            return lst;
        }

        //Gán 2 hàm mượn đó vào đây
        public ActionResult XemBaiViet(string id)
        {
            ViewBag.Title ="Xem Bài Viết " + id;
            dynamic dy = new ExpandoObject();
            dy.baiviet = BaiVietComment(id);
            dy.cmt = CommentBaiViet(id);
            return View(dy);
        }

        //Hàm mượn bài viết click vào cmt
        public List<BaiViet> BaiVietComment(string id)
        {
            List<BaiViet> lst = new List<BaiViet>();
            lst = db.BaiViets.Where(bv => bv.idBaiViet == id).ToList();
            return lst;
        }
        
        //Hàm mượn Comment bài viết
        public List<Comment> CommentBaiViet(string id)
        {
            List<Comment> lst = new List<Comment>();
            lst = db.Comments.Where(cmt => cmt.idBaiViet == id).OrderByDescending(p => p.idCmt).ToList();
            return lst;
        }

        //Xử lí đăng nhập
        public ActionResult Logout()
        {
            return RedirectToAction("DangNhapVaDangKy");
        }

        //Trang cá nhân
        public ActionResult TrangCaNhan()
        {
            ViewBag.Title = "Trang cá nhân - Social Network Greenzone";
            dynamic dy = new ExpandoObject();
            dy.user = getUser();
            dy.baiviet = getBaiViet();
            return View(dy);
        }

        //Thông báo
        public ActionResult ThongBao()
        {
            return View();
        }

        //Trang đăng nhập và đăng ký
        public ActionResult DangNhapVaDangKy()
        {
            return View();
        }

        //Xử lý Đăng nhập
        [HttpPost]
        public ActionResult DangNhap(FormCollection fc)
        {
            var tendn = fc["Username"];
            var matkhau = fc["Password"];
            if (string.IsNullOrEmpty(tendn))
            {
                ViewData["Loi"] = "Phải nhập tên đăng nhập";
            }
            else if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi"] = "Phải nhập mật khẩu";
            }

            else
            {
                User user = db.Users.FirstOrDefault(n => n.SDT == tendn && n.Password == matkhau);
                if (user != null)
                {
                    Session["IDuser"] = user;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["Loi"] = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return View("DangNhapVaDangKy");
                }
            }
            return View("DangNhapVaDangKy");
        }

        //Xử lý Đăng ký
        [HttpPost]
        public ActionResult DangKy(FormCollection fc, User user)
        {
            user.idUser = "";
            user.FullName = fc["fullname"];
            user.SDT = fc["sdt"];
            user.Email = fc["email"];
            user.HinhAnh = "user.png";
            user.Password = fc["password"];
            user.GioiTinh = fc["gioitinh"];
            user.isOnline = true;     //Fix
            user.QueQuan = fc["quequan"];
            user.AnhBia = "bg.jpg";
            user.NgaySinh = DateTime.Parse(fc["ngaysinh"]);
            user.SoBanBe = 1;           //Fix
            db.Users.InsertOnSubmit(user);
            db.SubmitChanges();
            return View("DangNhapVaDangKy");
        }
        public ActionResult Chat()
        {
            return View();
        }

        //-------------------------------------------------------XỬ LÝ------------------------------------------------------
        //Đăng bài viết
        [HttpPost]
        public JsonResult DangBaiViet(string noidung, BaiViet baiviet, string chedo)
        {
            User user = Session["IDuser"] as User;
            baiviet.idBaiViet = "";
            baiviet.idUser = user.idUser;
            baiviet.Noidung = noidung;
            baiviet.NgayDang = DateTime.Now;
            baiviet.CheDo = chedo;
            try
            {
                db.BaiViets.InsertOnSubmit(baiviet);
                db.SubmitChanges();
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //Comment bài viết
        [HttpPost]
        public JsonResult CommentBaiViet(string noidung , Comment cmt , string idbaiviet )
        {
            User user = Session["IDuser"] as User;
            cmt.idBaiViet = idbaiviet;
            cmt.idCmt = "";
            cmt.idUser = user.idUser;
            cmt.NoiDung = noidung;
            try
            {
                db.Comments.InsertOnSubmit(cmt);
                db.SubmitChanges();
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //Like bài viết
        [HttpPost]
        public JsonResult LikeBaiViet(BaiViet bv, Like like,int luotlike,string idbaiviet)
        {
            User user = Session["IDuser"] as User;
            like.idBaiViet = idbaiviet;
            bv.LuotLike = luotlike;
            like.idUser = user.idUser;
            try
            {
                db.Likes.InsertOnSubmit(like);
                db.SubmitChanges();
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //Share bài viết
        [HttpGet]
        public JsonResult ShareBaiViet(string id)
        {
            BaiViet baiv = new BaiViet();
            BaiViet baiviet = db.BaiViets.Where(bv => bv.idBaiViet == id).FirstOrDefault();
            baiv.Noidung = baiviet.Noidung;
            baiv.CheDo = baiviet.CheDo;
            baiv.NgayDang = baiviet.NgayDang.Value;
            baiv.idBaiViet = baiviet.idBaiViet;
            return Json(baiv, JsonRequestBehavior.AllowGet);
        }

        //Xem Comment bài viết

        [HttpGet]
        public JsonResult LayCMTTheoBaiViet(string id)
        {
            Comment comment = new Comment();
            Comment cmt = db.Comments.Where(c => c.idBaiViet == id).FirstOrDefault();
            comment.NoiDung = cmt.NoiDung;
            comment.idBaiViet = cmt.idBaiViet;
            return Json(comment, JsonRequestBehavior.AllowGet);
        }

        //Share Bài viết

        [HttpPost]
        public JsonResult PostShareBaiViet(string noidung, BaiViet baiviet, string chedo,string idbaiviet)
        {
            User user = Session["IDuser"] as User;
            baiviet.idBaiViet = "";
            baiviet.idUser = user.idUser;
            baiviet.Noidung = noidung;
            baiviet.NgayDang = DateTime.Now;
            baiviet.CheDo = chedo;
            baiviet.idBaiVietShare = idbaiviet;
            try
            {
                db.BaiViets.InsertOnSubmit(baiviet);
                db.SubmitChanges();
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //Chỉnh sửa thông tin
        [HttpPost]
        public JsonResult EditUser(DateTime ngaysinh, User user, string gioitinh, string fullname, string sdt, string email, string quequan,string idUser)
        {
            try
            {
                user = db.Users.FirstOrDefault(i=>i.idUser == idUser); 
                    user.FullName = fullname;
                    user.SDT = sdt;
                    user.Email = email;
                    user.GioiTinh = gioitinh;
                    user.NgaySinh = ngaysinh;
                    user.QueQuan = quequan;
                    db.SubmitChanges();
                    Session["IDuser"] = user;
                return Json(1, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
    }
}