using System.Net;
using System.Net.Mail;

namespace ASP.Back.Libraries
{
    public class Emailer
    {
        SmtpClient smtpClient;
        IConfigurationSection smtpConfig;
        MailAddress noReply;
        IHostEnvironment _hostEnvironment;

        public Emailer(IConfiguration config, IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;

            smtpConfig = config.GetSection("SMTP");
            try
            {
                smtpClient = new SmtpClient(smtpConfig["host"], int.Parse(smtpConfig["port"]));

                noReply = new MailAddress(smtpConfig["userName"], smtpConfig["displayName"]);

                NetworkCredential neReplyCreds = new NetworkCredential(smtpConfig["userName"], smtpConfig["password"]);

                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = neReplyCreds;
                smtpClient.EnableSsl = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }



        public void SendTwoFactorEmail(string toEmail, string inviteCode)
        {
            string inviteEmailPath = Path.Combine(_hostEnvironment.ContentRootPath, "inviteCode.html");
            if (!File.Exists(inviteEmailPath)) {

                Console.WriteLine("SendTwoFactorEmail - Failed, No invite code html");
                return;            
            }
            try
            {

                StreamReader sr = new StreamReader(inviteEmailPath);
            string email = sr.ReadToEnd();
                email = ControllerHelpers.ReplaceKeyInString(email, "companyName", "Aeirosoft");
                email = ControllerHelpers.ReplaceKeyInString(email, "inviteCode", inviteCode);

            MailAddress invitedUser = new MailAddress(toEmail);
            using (MailMessage inviteMessage = new MailMessage(noReply, invitedUser))
            {
                inviteMessage.Subject = "Your Confirmation Code!";
                inviteMessage.IsBodyHtml = true;
                inviteMessage.Body = email;


                    smtpClient.Send(inviteMessage);
          
            };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
            }
        }    


    }
}
