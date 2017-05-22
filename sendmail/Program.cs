using AegisImplicitMail;
using Fclp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sendmail
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMail(SendMailConfig.LoadFromAppConfig(), SendMailContext.LoadFromArgs(args));
        }

        static void SendMail(SendMailConfig config, SendMailContext context)
        {
            var client = new SmtpSocketClient(config.Smtp, config.Port, config.From, config.Password);
            client.SslType = config.SslEnabled ? SslMode.Ssl : SslMode.Auto;
            var mail = new MimeMailMessage();
            mail.From = new System.Net.Mail.MailAddress(context.DisplayName ?? config.From);
            mail.Sender = new System.Net.Mail.MailAddress(config.From);
            mail.To.Add(context.To);
            mail.IsBodyHtml = context.IsBodyHtml;
            mail.Subject = context.Subject;
            mail.Body = context.Body;
            mail.BodyEncoding = Encoding.UTF8;
            foreach (var attachment in context.Attachments)
            {
                mail.Attachments.Add(new MimeAttachment(attachment));
            }

            client.SendMail(mail);
        }
    }

    public class SendMailConfig
    {
        public string Smtp { get; set; }

        public short Port { get; set; }

        public bool SslEnabled { get; set; }

        public string From { get; set; }

        public string Password { get; set; }

        public static SendMailConfig LoadFromAppConfig()
        {
            return new SendMailConfig
            {
                Smtp = ConfigurationManager.AppSettings["smtp"], 
                From = ConfigurationManager.AppSettings["from"], 
                Password = ConfigurationManager.AppSettings["password"], 
                Port = short.Parse(ConfigurationManager.AppSettings["smtp-port"]),
                SslEnabled = bool.Parse(ConfigurationManager.AppSettings["ssl-enabled"])
            };
        }
    }

    public class SendMailContext
    {
        public string DisplayName { get; set; }

        public string To { get; set; }

        public bool IsBodyHtml { get; set; }

        public List<string> Attachments { get; set; } = new List<string>();

        public string Subject { get; set; }

        public string Body { get; set; }

        public static SendMailContext LoadFromArgs(string[] args)
        {
            var parser = new FluentCommandLineParser<SendMailContext>();

            parser.Setup(x => x.DisplayName)
                .As("from");
            parser.Setup(x => x.To)
                .As("to")
                .Required();
            parser.Setup(x => x.IsBodyHtml)
                .As("html");
            parser.Setup(x => x.Attachments)
                .As("attachments");
            parser.Setup(x => x.Subject)
                .As("subject")
                .Required();
            parser.Setup(x => x.Body)
                .As("body");

            parser.SetupHelp("?")
                .Callback(x =>
                {
                    Console.WriteLine(x);
                    Environment.Exit(1);
                });

            var result = parser.Parse(args);
            if (result.HasErrors)
            {
                Console.WriteLine(result.ErrorText);
                Environment.Exit(2);
            }

            return parser.Object;
        }
    }
}
