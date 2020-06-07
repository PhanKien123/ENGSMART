using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using EngSmart.Models;
using EngSmart.Providers;
using EngSmart.Results;
using EngSmart.Models.User;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using EngSmart.Database;
using System.Linq;
using System.Text;
using EngSmart.Models.Lessons;
using EngSmart.Models.Musics;
using EngSmart.Models.Unit;
using EngSmart.Models.Dictionaries;
using EngSmart.Models.Point;

namespace EngSmart.Controllers
{

    [RoutePrefix("api/Service")]
    public class ServiceController : ApiController
    {

        [Route("Singin")]
        public IHttpActionResult Singin(JObject data)
        {
            try
            {

                /// string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                AddUserModel item = JsonConvert.DeserializeObject<AddUserModel>(data.ToString());

                using (var con = new ENGSMARTEntities())
                {
                    var query = from user in con.Users
                                where user.Phone.Trim() == item.Phone.Trim()
                                select user;
                    if (query != null && query.Count() > 0)
                    {
                        ResultModal result = new ResultModal()
                        {
                            Status = 0,
                            Code = 500,
                            Mess = "Số điện thoại đã tồn tại trong hệ thống",
                            Data = null,
                            Token = null

                        };
                        return Ok(result);
                    }
                    else
                    {
                        var User = new User()
                        {
                            Name = item.Name,
                            CreateDate = DateTime.Now,
                            Active = 1,
                            Pass = CreateMdf5(item.Pass),
                            Phone = item.Phone,
                            Rank = 0,
                            Token = ResetTooken(),
                            Gender = item.Gender
                        };
                        con.Users.Add(User);
                        con.SaveChanges();

                        ResultModal result = new ResultModal()
                        {
                            Status = 1,
                            Code = 200,
                            Mess = "Thêm người dùng thành công",
                            Data = User,
                            Token = User.Token

                        };
                        return Ok(result);

                    }

                }
            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("Login")]
        public IHttpActionResult Login(JObject Data)
        {
            try
            {
                AddUserModel item = JsonConvert.DeserializeObject<AddUserModel>(Data.ToString());
                string StrPass = CreateMdf5(item.Pass.Trim());
                using (var con = new ENGSMARTEntities())
                {
                    var query = from data in con.Users
                                where data.Phone.Trim() == item.Phone.Trim() && data.Pass.Trim() == StrPass
                                select data;

                    if (query != null && query.Count() > 0)
                    {

                        var u = con.Users.Find(query.FirstOrDefault().Id);
                        u.Token = ResetTooken();
                        con.SaveChanges();

                        ResultModal result = new ResultModal()
                        {
                            Status = 1,
                            Code = 200,
                            Mess = "Đăng nhập thành công!",
                            Data = u
                        };
                        return Ok(result);
                    }
                    else
                    {
                        ResultModal result = new ResultModal()
                        {
                            Status = 0,
                            Code = 500,
                            Mess = "Tài khoản hoặc mật khẩu không đúng!",
                            Data = null,

                        };
                        return Ok(result);
                    }
                }
            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("GetUserInfo")]
        public IHttpActionResult GetUserInfo(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    return Ok(result);
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 1,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("GetListCousre")]
        public IHttpActionResult GetListCousre(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                var domain = Request.RequestUri.Authority; 
                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            List<LessonsItemModel> Lesson = new List<LessonsItemModel>();
                            ResultModal Lessonresult = new ResultModal();
                            var query = from data in con.Lessons

                                        select new LessonsItemModel()
                                        {
                                            ID = data.ID,
                                            Description = data.Description,
                                            Image = data.Image,
                                            Link = data.Link != null ? "http://" + domain + "/content/video/"  + data.Link : "",
                                            Order = data.Order,
                                            Title = data.Title
                                        };
                            if (query != null && query.Count() > 0)
                            {
                                Lesson = query.OrderBy(u => u.Order).ToList();
                            }

                            if (Lesson.Count <= 0)
                            {
                                Lessonresult.Mess = "Dữ liệu trống";
                            }
                            Lessonresult.Status = 1;
                            Lessonresult.Data = Lesson;
                            Lessonresult.Token = result.Token;
                            Lessonresult.Code = 200;
                            return Ok(Lessonresult);
                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("GetListMusic")]
        public IHttpActionResult GetListMusic(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            List<MusicItemModel> Musics = new List<MusicItemModel>();
                            ResultModal Musicsresult = new ResultModal();

                            var query = from data in con.Musics

                                        select new MusicItemModel
                                        {
                                            ID = data.ID,
                                            content = data.content,
                                            desription = data.desription,
                                            link = data.link,
                                            singer = data.singer
                                        };
                            if (query != null && query.Count() > 0)
                            {
                                Musics = query.ToList();
                            }

                            if (Musics.Count() <= 0)
                            {
                                Musicsresult.Mess = "Dữ liệu trống";
                            }
                            Musicsresult.Status = 1;
                            Musicsresult.Data = Musics;
                            Musicsresult.Token = result.Token;
                            Musicsresult.Code = 200;
                            return Ok(Musicsresult);

                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("GetListUnit")]
        public IHttpActionResult GetListUnit(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                List<UnitItemModel> Units = new List<UnitItemModel>();
                ResultModal Unitsresult = new ResultModal();

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            var query = from data in con.Units
                                        select new UnitItemModel
                                        {
                                            ID = data.ID,
                                            Title = data.Title
                                        };
                            if (query != null && query.Count() > 0)
                            {
                                Units = query.ToList();
                            }
                            else
                            {
                                Unitsresult.Mess = "Dữ liệu trống";
                            }
                            Unitsresult.Status = 1;
                            Unitsresult.Data = Units;
                            Unitsresult.Token = result.Token;
                            Unitsresult.Code = 200;
                            return Ok(Unitsresult);
                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("GetUnitDetail")]
        public IHttpActionResult GetUnitDetail(int UnitID)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {

                        List<UnitDetailModel> ListData = new List<UnitDetailModel>();
                        using (var con = new ENGSMARTEntities())
                        {
                            var query = from que in con.Questions
                                        where que.unitID == UnitID
                                        select que;
                            if (query != null && query.Count() > 0)
                            {
                                foreach (var que in query)
                                {
                                    var data = new UnitDetailModel()
                                    {
                                        ID = que.ID,
                                        content = que.content,
                                        unitID = que.unitID
                                    };

                                    data.ListOption = new List<ListOptionByUnitModel>();
                                    if (que.Options != null && que.Options.Count() > 0)
                                    {
                                        foreach (var op in que.Options)
                                        {
                                            ListOptionByUnitModel option = new ListOptionByUnitModel()
                                            {
                                                ID = op.ID,
                                                Content = op.Content,
                                                Order = op.Order,
                                                QuestionID = op.QuestionID,
                                                Status = op.Status
                                            };
                                            data.ListOption.Add(option);
                                        }
                                    }

                                    ListData.Add(data);
                                }
                            }

                            ResultModal Unitsresult = new ResultModal();

                            if (ListData != null && ListData.Count() > 0)
                            {
                                Unitsresult.Mess = "";
                            }
                            else
                            {
                                Unitsresult.Mess = "Dữ liệu trống";
                            }

                            Unitsresult.Status = 1;
                            Unitsresult.Data = ListData;
                            Unitsresult.Token = result.Token;
                            Unitsresult.Code = 200;
                            return Ok(Unitsresult);



                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("AddCousre")]
        public IHttpActionResult AddCousre(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                AddLessonItemModel item = JsonConvert.DeserializeObject<AddLessonItemModel>(Data.ToString());

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {

                        List<UnitDetailModel> ListData = new List<UnitDetailModel>();
                        using (var con = new ENGSMARTEntities())
                        {
                            Lesson lesson = new Lesson()
                            {
                                Description = item.Description,
                                Image = item.Image,
                                Link = item.Link,
                                Order = item.Order,
                                Title = item.Title
                            };
                            con.Lessons.Add(lesson);
                            con.SaveChanges();

                            ResultModal resultLesson = new ResultModal()
                            {
                                Status = 1,
                                Code = 200,
                                Mess = "Thêm mới bài học thành công",
                                Data = lesson
                            };
                            return Ok(resultLesson);

                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("DeleteCousre")]
        [HttpGet]
        public IHttpActionResult DeleteCousre(int ID)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        List<LessonsItemModel> ListData = new List<LessonsItemModel>();
                        using (var con = new ENGSMARTEntities())
                        {
                            var lesson = con.Lessons.Find(ID);
                            if (lesson != null)
                            {
                                con.Lessons.Remove(lesson);
                                con.SaveChanges();
                            }

                            List<LessonsItemModel> Lesson = new List<LessonsItemModel>();
                            ResultModal Lessonresult = new ResultModal();
                            var query = from data in con.Lessons

                                        select new LessonsItemModel()
                                        {
                                            ID = data.ID,
                                            Description = data.Description,
                                            Image = data.Image,
                                            Link = data.Link,
                                            Order = data.Order,
                                            Title = data.Title
                                        };
                            if (query != null && query.Count() > 0)
                            {
                                Lesson = query.OrderBy(u => u.Order).ToList();
                            }

                            if (Lesson.Count <= 0)
                            {
                                Lessonresult.Mess = "Dữ liệu trống";
                            }
                            Lessonresult.Status = 1;
                            Lessonresult.Data = Lesson;
                            Lessonresult.Token = result.Token;
                            Lessonresult.Code = 200;
                            return Ok(Lessonresult);

                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }

        }

        [Route("GetListRank")]
        public IHttpActionResult GetListRank()
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            List<UserItemModel> ListData = new List<UserItemModel>();
                            ResultModal resultUser = new ResultModal(); 

                            var query = from data in con.Users
                                        select new UserItemModel()
                                        {
                                            Id = data.Id,
                                            Active = data.Active,
                                            CreateDate = data.CreateDate,
                                            Gender = data.Gender,
                                            Name = data.Name,
                                            Pass = data.Pass,
                                            Phone = data.Phone,
                                            Rank = data.Rank,

                                        };
                            if(query != null && query.Count() > 0)
                            {
                                ListData = query.OrderByDescending(u=>u.Rank).ToList(); 
                            }
                            else
                            {
                                resultUser.Mess = "Dữ Liệu Trống"; 
                            }

                            resultUser.Code = 200;
                            resultUser.Token = result.Token;
                            resultUser.Data = ListData;
                            resultUser.Status = 1;
                            return Ok(resultUser); 

                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("Translate")]
        [HttpGet]
        public IHttpActionResult Translate(string word)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            var query = from data in con.Dictionaries
                                        where data.Word.Trim().ToUpper() == word.Trim().ToUpper()
                                        select new DictionariesModel()
                                        {
                                            Id = data.Id,
                                            Word = data.Word,
                                            Translate = data.Translate,
                                            Description = data.Description,
                                            mp3 = data.mp3

                                        }; 
                            if(query != null&& query.Count() > 0)
                            {
                                ResultModal resultDisc = new ResultModal()
                                {
                                    Status = 1,
                                    Code = 200,
                                    Mess = "",
                                    Data = query.FirstOrDefault(),
                                    Token = ""

                                };
                                return Ok(resultDisc); 
                            }
                            else
                            {
                                ResultModal resultDisc = new ResultModal()
                                {
                                    Status = 0,
                                    Code = 500,
                                    Mess = "Không tìm thây từ cần tra nghĩa",
                                    Data = null,
                                    Token = ""

                                };
                                return Ok(resultDisc);
                            }

                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("AddWord")]
        [HttpPost]
        public IHttpActionResult AddWord(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                AddDictionariesModel item = JsonConvert.DeserializeObject<AddDictionariesModel>(Data.ToString());

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            var query = from data in con.Dictionary_backup
                                        where data.Word.Trim().ToUpper() == item.Word.Trim().ToUpper()
                                        select data;
                            var tran = con.Database.BeginTransaction(); 

                            if(query != null && query.Count()>0)
                            {
                                tran.Rollback(); 
                                ResultModal resultDictionaries = new ResultModal()
                                {
                                    Status = 0,
                                    Code = 500,
                                    Mess = "Từ mới đã tồn tại trong hệ thống",
                                    Data = null,
                                    Token = ""

                                };
                                return Ok(resultDictionaries);
                            }
                            else
                            {
                                
                                Dictionary_backup data = new Dictionary_backup()
                                {
                                    Description = null,
                                    mp3 = null,
                                    Translate = item.Translate,
                                    Word = item.Word,
                                   
                                };
                                con.Dictionary_backup.Add(data);
                                con.SaveChanges();
                                tran.Commit(); 
                                ResultModal resultDictionaries = new ResultModal()
                                {
                                    Status = 1,
                                    Code = 200,
                                    Mess = "Thêm từ mới thành công",
                                    Data = data,
                                    Token = ""

                                };
                                return Ok(resultDictionaries);
                            }
                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("PushPoint")]
        [HttpPost]
        public IHttpActionResult PushPoint(JObject Data)
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();
                AddPointModels item = JsonConvert.DeserializeObject<AddPointModels>(Data.ToString());

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            var user = result.Data as User;
                            var query = from data in con.Points
                                        where data.userID == user.Id && data.unitID == item.unitID
                                        select data; 
                            if(query != null && query.Count() > 0)
                            {
                                var point = query.FirstOrDefault();
                                var pointUpdate = con.Points.Find(point.ID);
                                pointUpdate.hightestPoint =  item.point > pointUpdate.hightestPoint ? item.point : pointUpdate.hightestPoint;

                                con.SaveChanges();

                                // cập nhập điểm trung bình  cho từng user 
                                UpdatePointUser(user.Id, con); 

                                ResultModal resultPoint = new ResultModal()
                                {
                                    Status = 1,
                                    Code = 200,
                                    Mess = "Thêm mới thành công",
                                    Data = pointUpdate,
                                    Token = result.Token

                                };
                                return Ok(resultPoint);
                            }
                            else
                            {
                                Point point = new Point()
                                {
                                    fistPoint = item.point,
                                    hightestPoint = item.point,
                                    unitID = item.unitID,
                                    userID = user.Id
                                };
                                con.Points.Add(point);
                                con.SaveChanges();
                                UpdatePointUser(user.Id, con);

                                ResultModal resultPoint = new ResultModal()
                                {
                                    Status = 1,
                                    Code = 200,
                                    Mess = "Thêm mới thành công",
                                    Data = point,
                                    Token = result.Token

                                };
                                return Ok(resultPoint); 

                            }
                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }

        [Route("GetTopPointUser")]
        [HttpGet]
        public IHttpActionResult GetTopPointUser()
        {
            try
            {
                string token = Request.Headers.GetValues("token").FirstOrDefault().ToString();

                if (token != null && token.Trim() != string.Empty)
                {
                    var result = this.CheckToken(token);
                    if (result.Status == 0)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        using (var con = new ENGSMARTEntities())
                        {
                            var query = from data in con.Users
                                        select data; 
                            if(query != null&& query.Count() > 0)
                            {
                                var data = query.OrderByDescending(u => u.Rank).Take(5).ToList();
                                ResultModal resultPoint = new ResultModal()
                                {
                                    Status = 1,
                                    Code = 200,
                                    Mess = "",
                                    Data = data,
                                    Token = result.Token

                                };
                                return Ok(resultPoint); 
                            }
                            else
                            {
                                var data = query.OrderByDescending(u => u.Rank).Take(5);
                                ResultModal resultPoint = new ResultModal()
                                {
                                    Status = 1,
                                    Code = 500,
                                    Mess = "Danh sách trống",
                                    Data = new List<User>(),
                                    Token = result.Token

                                };
                                return Ok(resultPoint);
                            }

                            
                        }
                    }
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                ResultModal result = new ResultModal()
                {
                    Status = 0,
                    Code = 400,
                    Mess = ex.Message,
                    Data = null,

                };
                return Ok(result);
            }
        }


        private int UpdatePointUser(int UserId, ENGSMARTEntities con)
        {
            var querypoint = from data in con.Points
                             where data.userID == UserId
                             select data;
            if (querypoint != null && querypoint.Count() > 0)
            {
                var userupdate = con.Users.Find(UserId);
                int countUnit = querypoint.Count();
                var listPoint = querypoint.ToList();

                int fistPoint = 0;
                int hightestPoint = 0;
                foreach (var data in listPoint)
                {
                    fistPoint = fistPoint + data.fistPoint.Value;
                    hightestPoint = hightestPoint + data.hightestPoint.Value;
                }

                userupdate.Rank = hightestPoint;
                userupdate.competence = (double)fistPoint / (double)countUnit;
                con.SaveChanges();
            }
            return 1; 
        }

        private ResultModal CheckToken(string Token)
        {
            using (var con = new ENGSMARTEntities())
            {
                var query = from data in con.Users
                            where data.Token == Token
                            select data;
                if (query != null && query.Count() > 0)
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 1,
                        Code = 200,
                        Mess = "Mã token hợp lệ",
                        Data = query.FirstOrDefault(),
                        Token = Token

                    };
                    return result;
                }
                else
                {
                    ResultModal result = new ResultModal()
                    {
                        Status = 0,
                        Code = 403,
                        Mess = "Phiên đăng nhập hết hạn",
                        Data = null,
                        Token = ""

                    };
                    return result;
                }
            }
        }

        private string ResetTooken()
        {
            string date = DateTime.Now.ToString();
            return CreateMdf5(date);
        }


        public static string CreateMdf5(string input)
        {

            using (MD5 md5hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }
}