using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;

namespace ShopProject_Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);

        }
        public async Task Execute(string email, string subject, string body)
        {
            MailjetClient client = new MailjetClient("****************************1234", "****************************abcd")
            {
                Version = ApiVersion.V3_1,

            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
             .Property(Send.Messages, new JArray {
     new JObject {
      {
       "From",
       new JObject {
        {"Email", "robert.martirosyan9000@gmail.com"},
        {"Name", "Art"}
       }
      }, {
       "To",
       new JArray {
        new JObject {
         {
          "Email",
          email
         }, {
          "Name",
          "Art"
         }
        }
       }
      }, {
       "Subject",
       subject
      },
       {
       "HTMLPart",
       body
      },
     }
             });
            MailjetResponse response = await client.PostAsync(request);
        }
    }
}
