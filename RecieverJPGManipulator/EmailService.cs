using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RecieverJPGManipulator
{
    public class EmailService
    {
        public void Send(Stream pictureStream, string toEmail)
        {
            SmtpClient smtpClient = new SmtpClient("smtp-mail.outlook.com"); //standard Microsoft smtp server
            var mail = new MailMessage();
            mail.From = new MailAddress("anpe53920@edu.ucl.dk");
            mail.To.Add(toEmail);
            mail.Subject = "Digi image";

            string cid = "image";
            mail.Body = "<html><body>" +
                "<p>Hey 👋,</p>" +
                "<p>This should be the requested manipulated image:</p>" +
                "<p><img src='cid:" + cid + "'/></p>" +
                "<p> Muahah - made your image worse but stylish</p>" +
                "</body></html>";

            // Add image as inline attachment, and.
            pictureStream.Position = 0;
           mail.Attachments.Add(new Attachment(pictureStream,"image.png","png/jpg") { ContentId = cid });

            // Add file as attachment.
            //mail.Attachments.Add(new Attachment("image.png"));
            mail.IsBodyHtml = true;
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential("anpe53920@edu.ucl.dk", "Norregaard1!"); //replace password
            smtpClient.EnableSsl = true;
            smtpClient.Send(mail);

        }
    }
}
