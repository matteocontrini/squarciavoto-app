using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using Android.Telephony;
using Realms;

namespace Gateway
{
    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver")]
    public class SMSBroadcastReceiver : BroadcastReceiver
    {
        private Realm realm = Realm.GetInstance();
        public event EventHandler<NewMessageEventArgs> OnNewMessage;

        public override void OnReceive(Context context, Intent intent)
        {
            if (Telephony.Sms.Intents.SmsReceivedAction == intent.Action)
            {
                foreach (SmsMessage smsMessage in Telephony.Sms.Intents.GetMessagesFromIntent(intent))
                {
                    Message sms = new Message();
                    sms.Text = smsMessage.MessageBody;
                    sms.Sender = smsMessage.OriginatingAddress;
                    sms.Date = DateTimeOffset.Now;

                    realm.Write(() =>
                    {
                        realm.Add(sms);
                    });


                    OnNewMessage(this, new NewMessageEventArgs(sms));
                }
            }
            
        }
    }
}