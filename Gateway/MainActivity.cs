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
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway
{
    [Activity(Label = "Gateway", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ListAdapter adapter;
        private HttpClient client;
        
        public MainActivity()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://192.168.0.11:8080/");
            client.Timeout = TimeSpan.FromSeconds(5);
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Render UI
            SetContentView(Resource.Layout.Main);

            Realm.GetInstance().Write(() =>
            {
                Realm.GetInstance().Add(new Message() { Text = "Boot", Date = DateTimeOffset.Now, Sender = "boot" });
            });

            // Get previous messages
            var res = Realm.GetInstance().All<Message>().OrderByDescending((x) => x.Date).ToList();
            
            ListView listView = FindViewById<ListView>(Resource.Id.listView);
            adapter = new ListAdapter(this, res);
            listView.Adapter = adapter;
            listView.ItemClick += ListView_Click;

            SMSBroadcastReceiver receiver = new SMSBroadcastReceiver();
            receiver.OnNewMessage += Receiver_NewMessage;

            RegisterReceiver(receiver, new IntentFilter("android.provider.Telephony.SMS_RECEIVED"));
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
            
            alert.SetButton2("RETRY", (senderAlert, args) => {
                alert.Dismiss();
                Forward(item);
            });

            alert.Show();
        }

        private void Receiver_NewMessage(object sender, NewMessageEventArgs e)
        {
            adapter.AddTop(e.Message);

            Forward(e.Message);
        }

        private async void Forward(Message message)
        {
            string body = JsonConvert.SerializeObject(message);
            StringContent content = new StringContent(body, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage res = await client.PostAsync("messages", content);

                Realm.GetInstance().Write(() =>
                {
                    message.RequestStatusCode = res.StatusCode.ToString();
                });
            }
            catch (Exception ex)
            {
                string reason = ex.Message;

                if (ex is TaskCanceledException)
                {
                    reason = "TIMEOUT";
                }

                Realm.GetInstance().Write(() =>
                {
                    message.RequestStatusCode = reason;
                });
            }

            RunOnUiThread(() =>
            {
                adapter.NotifyDataSetChanged();
            });
        }


        // unused
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
