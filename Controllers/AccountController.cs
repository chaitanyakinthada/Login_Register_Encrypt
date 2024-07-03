using LogInAndRegister.Data;
using LogInAndRegister.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.IO;
using MimeKit;
using MailKit.Net.Smtp;
using LogInAndRegister.Helper;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
//using System.Net.Mail;
//using System.Net;
//using System.Net.Mail;
//using MailKit.Net.Smtp;
//using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace LogInAndRegister.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        //public static string apiUrl = "https://localhost:44300/";
        
        public static string apiUrl = "https://run.mocky.io/v3/your-mock-id";
        static List<Account> Accounts = new List<Account>();

        AccountAPI api = new AccountAPI();

        public AccountController(AppDbContext _db, IHttpContextAccessor httpContextAccessor)
        {
            db = _db;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List(Account account)
        {


            string apiUrl = "https://localhost:44300/api/account";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var table = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(data);

                }


            }


            //List<Account> account = new List<Account>();
            //HttpClient client = api.Initial();
            //HttpResponseMessage res = await client.PostAsync("api/AccountAPI");
            //if(res.IsSuccessStatusCode)
            //{
            //    var result = res.Content.ReadAsStringAsync().Result;
            //    account = JsonConvert.DeserializeObject<List<Account>>(result);
            //}

            return View();
        }
        private const string SecurityKey = "CompleKeyHere_12121";

        public static string EncryptText(string Pwd)
        {
            byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(Pwd);
            MD5CryptoServiceProvider objMDCryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = objMDCryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMDCryptoService.Clear();

            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCryptoTransform = objTripleDESCryptoService.CreateEncryptor();
            byte[] resultArray = objCryptoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        //public string DecryptText(string Data)
        //{
        //try
        //{
        //    System.Text.UTF8Encoding oUTF8Encoding = new System.Text.UTF8Encoding();
        //    System.Text.Decoder oDecoder = oUTF8Encoding.GetDecoder();
        //    byte[] oByte = Convert.FromBase64String(Data);
        //    int CharCount = oDecoder.GetCharCount(oByte, 0, oByte.Length);
        //    char[] DecodedChar = new char[CharCount];
        //    oDecoder.GetChars(oByte, 0, oByte.Length, DecodedChar, 0);
        //    string DecodedData = new String(DecodedChar);
        //    return DecodedData;
        //}
        //catch (Exception ex)
        //{
        //    throw new Exception(ex.Message);
        //}
        //}
        public static string DecryptText(string Pwd)
        {
            byte[] toEncryptArray = Convert.FromBase64String(Pwd);
            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();


            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();

            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();

            objTripleDESCryptoService.Key = securityKeyArray;

            objTripleDESCryptoService.Mode = CipherMode.ECB;

            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();

            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            objTripleDESCryptoService.Clear();


            return UTF8Encoding.UTF8.GetString(resultArray);
        }


        public async Task<ActionResult> Login(LoginViewModel model)
        {
            
            //var decrypt = DecryptText(db.Accounts.Password);
            
            if (ModelState.IsValid)
            {
                var userdetails = await db.Accounts
                .Where(m => m.Email == model.Email).FirstOrDefaultAsync();
                if (userdetails == null)
                {
                    ModelState.AddModelError("Password", "Invalid login attempt.");
                    return View("Index");
                }
                else
                {
                    var userpass = userdetails.Password;
                    var decrypt = DecryptText(userpass);
                    if (decrypt == model.Password)
                    {
                        HttpContext.Session.SetString("userId", userdetails.Name);
                        return RedirectToAction("Dashboard", new { Email = model.Email });
                    }
                    return View("Index");
                }
                
                
            }
            else
            {
                return View("Index");
            }
            
        }

        public IActionResult Details(string Email)
        {
            var info = db.Accounts.Where(x => x.Email == Email).FirstOrDefault();
            return View(info);

        }

        //public IActionResult Details()
        //{
        //    var userid = db.GetUserId(HttpContext.User);

        //    if(userid == null)
        //    {
        //        return RedirectToAction("Index", "Account");
        //    }
        //    else
        //    {
        //        AppDbContext user = db.FindByIdAsync(userid).Result();
        //        return View(user);
        //    }

            
            
        //}
    

        public IActionResult Dashboard()
        {

            return View();
            
        }





        [HttpPost]
        public async Task<ActionResult> Register(RegistrationViewModel model)
        {
            var encryptedPass = EncryptText(model.Password);
            //try
            //{ 
            if (ModelState.IsValid)
            {

                string CustObj = JsonConvert.SerializeObject(model); //needs to convert to HTTP Content
                
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //client.BaseAddress = new Uri(apiUrl);

                    //Account user = new Account
                    //{
                    //    Name = model.Name,
                    //    Email = model.Email,
                    //    Password = encryptedPass,
                    //    Mobile = model.Mobile

                    //};
                    //db.Add(user);
                    var CustContent = new StringContent(CustObj, UnicodeEncoding.UTF8, "application/json");// to create we need to build string content and pass in response.
                    HttpResponseMessage httpResponse = await client.PostAsync("/Api/AccountAPI", CustContent);
                    //HttpResponseMessage responseMessage = null;
                    //try
                    //{
                    //    responseMessage = await client.PostAsync("/Api/AccountAPI", CustContent);
                    //}
                    //catch (Exception ex)
                    //{
                    //    if (responseMessage == null)
                    //    {
                    //        responseMessage = new HttpResponseMessage();
                    //    }
                    //    responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                    //    responseMessage.ReasonPhrase = string.Format("RestHttpClient.SendRequest failed: {0}", ex);
                    //}

                    //if (httpResponse.IsSuccessStatusCode)
                    //{
                    //    //return RedirectToAction("Index");
                    //}
                    Account a = new Account
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Mobile = model.Mobile,
                        Password = encryptedPass

                    };

                    db.Accounts.Add(a);
                    db.SaveChanges();

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Chaitanya chinmaya sai", "chaitanya.kinthada3@gmail.com"));
                    message.To.Add(new MailboxAddress(model.Name, model.Email));
                    message.Subject = $"Hi {model.Name}";
                    message.Body = new TextPart("plain")
                    {
                        Text = "You have been successfully registered!"
                    };
                    using (var client0 = new SmtpClient())
                    {
                        client0.Connect("smtp.gmail.com", 587, false);
                        client0.Authenticate("chaitanya.kinthada3@gmail.com", "qfnvvqqbvuzxgkiv");
                        client0.Send(message);
                        client0.Disconnect(true);
                    }
                }

                
            }
            else
            {
                return View("Registration");
            }
            //}
            //catch
            //{

            //    return View();
            //}
            //using SmtpClient email = new SmtpClient
            //{
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    EnableSsl = true,
            //    Host = "smtp.gmail.com",
            //    Port = 587,
            //    Credentials = new NetworkCredential("chaitanyachinmayasai@gmail.com","@Omsairam@3", "smtp.gmail.com")

            //};
            //string subject = "Success";
            //string body = "You have been registered successfully";
            //email.Send("chaitanyachinmayasai@gmail.com", model.Email, subject, body);

            //using (MailMessage mail = new MailMessage())
            //{
            //    mail.From = new MailAddress("chaitanyakinthada@yahoo.com");
            //    mail.To.Add(model.Email);
            //    mail.Subject = "Hello World";
            //    mail.Body = "<h1>Hello</h1>";
            //    mail.IsBodyHtml = true;
            //    //mail.Attachments.Add(new Attachment("C:\\file.zip"));

            //    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            //    {
            //        smtp.Credentials = new NetworkCredential("chaitanyakinthada@yahoo.com", "Chaitu@3");
            //        smtp.EnableSsl = true;
            //        smtp.Send(mail);
            //    }
            //}
            //Account a = new Account
            //{
            //    Password = encryptedPass
            //};

            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress("Chaitanya chinmaya sai", "chaitanya.kinthada3@gmail.com"));
            //message.To.Add(new MailboxAddress(model.Name, model.Email));
            //message.Subject = $"Hi {model.Name}";
            //message.Body = new TextPart("plain")
            //{
            //    Text = "You have been successfully registered!"
            //};
            //using (var client = new SmtpClient())
            //{
            //    client.Connect("smtp.gmail.com", 587, false);
            //    client.Authenticate("chaitanya.kinthada3@gmail.com", "qfnvvqqbvuzxgkiv");
            //    client.Send(message);
            //    client.Disconnect(true);
            //}

            //await db.SaveChangesAsync();
            return RedirectToAction("Index", "Account");
        }

        // registration Page load
        public IActionResult Registration()
        {
            ViewData["Message"] = "Registration Page";

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }


        //public void ValidationMessage(string key, string alert, string value)
        //{
        //    try
        //    {
        //        TempData.Remove(key);
        //        TempData.Add(key, value);
        //        TempData.Add("alertType", alert);
        //    }
        //    catch
        //    {
        //        Debug.WriteLine("TempDataMessage Error");
        //    }

        //}

    }
}
