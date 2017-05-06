using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;
using System.Timers;
using System.Linq;
using Realms;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Gateway
{
    [Activity(Label = "Gateway", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ListAdapter adapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Render UI
            SetContentView(Resource.Layout.Main);

            // Get previous messages
            var res = Realm.GetInstance().All<Message>().ToList();
            
            ListView listView = FindViewById<ListView>(Resource.Id.listView);
            adapter = new ListAdapter(this, res);
            listView.Adapter = adapter;
            listView.ItemClick += ListView_Click;

            SMSBroadcastReceiver receiver = new SMSBroadcastReceiver();
            receiver.OnNewMessage += Receiver_NewMessage;

            RegisterReceiver(receiver, new IntentFilter("android.provider.Telephony.SMS_RECEIVED"));
            
            //Realm.GetInstance().Write(() =>
            //{
            //    Realm.GetInstance().Add(new Message() { Text = "5", Date = DateTimeOffset.Now, Sender = "12356" });
            //});
        }

        private void ListView_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            Message item = adapter[e.Position];
            
            AlertDialog alert = new AlertDialog.Builder(this).Create();

            alert.SetTitle(item.Sender);
            alert.SetMessage(item.Text + "\n\n" + item.Date + "\n" + item.RequestStatusCode);

            alert.SetButton("OK", (senderAlert, args) => {
                alert.Dismiss(); 
            });
            
            alert.Show();
        }

        private void Receiver_NewMessage(object sender, NewMessageEventArgs e)
        {
            adapter.AddTop(e.Message);
            
            var request = HttpWebRequest.Create("http://localhost:8080/messages");
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Timeout = 5000;
            
            try
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(JsonConvert.SerializeObject(e.Message));

                request.ContentLength = bytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Realm.GetInstance().Write(() =>
                {
                    e.Message.RequestStatusCode = response.StatusCode.ToString();
                });
            }
            catch (Exception ex)
            {
                Realm.GetInstance().Write(() =>
                {
                    e.Message.RequestStatusCode = ex.Message;
                });
            }
        }

        void ReadSMS()
        {
            // public static final String INBOX = "content://sms/inbox";
            // public static final String SENT = "content://sms/sent";
            // public static final String DRAFT = "content://sms/draft";

            var cursor = ContentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);

            if (cursor.MoveToFirst())
            { // must check the result to prevent exception
                do
                {
                    if (cursor.GetString(cursor.GetColumnIndex("sub_id")) == "2")
                    {
                        Console.WriteLine(cursor.GetString(cursor.GetColumnIndex("body")));
                    }
                    //String msgData = "";
                    //for (int idx = 0; idx < cursor.ColumnCount; idx++)
                    //{
                    //    msgData += " " + cursor.GetColumnName(idx) + ":" + cursor.GetString(idx) + "\n";
                    //}
                    //Console.WriteLine(msgData);
                } while (cursor.MoveToNext());
            }
            else
            {
                // empty box, no SMS
            }
        }
    }
}
