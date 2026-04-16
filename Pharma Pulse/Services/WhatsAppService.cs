using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class WhatsAppService
{
    public WhatsAppService()
    {
        TwilioClient.Init("YOUR_ACCOUNT_SID", "YOUR_AUTH_TOKEN");
    }

    public void SendWhatsApp(string to, string message)
    {
        MessageResource.Create(
            from: new PhoneNumber("whatsapp:+14155238886"), // Twilio sandbox
            to: new PhoneNumber("whatsapp:" + to),
            body: message
        );
    }
}