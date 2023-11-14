using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronWebSocketClient;
using Crestron.SimplSharp.Cryptography;
using Newtonsoft.Json;
using Crestron.SimplSharp.Net.Http;

namespace OspaSS
{
    public class Ospa
    {
        public delegate void StatusToSPlusHandler(int arg);
        public StatusToSPlusHandler OnError { set; get; }
        public StatusToSPlusHandler OnConnectChange { set; get; }
        public StatusToSPlusHandler OnGrantedChange { set; get; }

        public delegate void ValueToSPlusHandler(int arg, int val);
        public ValueToSPlusHandler OnValueReported { set; get; }

        public delegate void DebugLogHandler(SimplSharpString data);
        public DebugLogHandler OnLog { set; get; }
        public DebugLogHandler OnOspaMsg { set; get; }

        private string m_ErrURL;
        private string m_password;
        private string m_accessKey;
        private string m_accessKeyBack;
        private bool m_allowed;
        private CTimer keepAliveTimer;
        internal WebSocketClient wsc;

        public Ospa() { }

        private void init()
        {
            m_allowed = false;
            if (OnGrantedChange != null)
                OnGrantedChange( m_allowed?1:0 );
            m_accessKey = null;
            m_accessKeyBack = null;
        }

        private bool m_debug = false;
        public int Debug
        {
            set { m_debug = value != 0; }
            get { return m_debug ? 1 : 0; }
        }

        private const int maxChunkSize = 250;
        public void Log(string message, params object[] arg)
        {
            if (m_debug)
            {
                if (OnLog != null)
                {
                    string sdata = string.Format(message, arg) + "\n";

                    for (int i = 0; i < sdata.Length; i += maxChunkSize)
                    {
                        string schunk = sdata.Substring(i, Math.Min(maxChunkSize, sdata.Length - i));
                        OnLog(new SimplSharpString(schunk));
                    }
                }
            }
        }

        private WebSocketClient.WEBSOCKET_RESULT_CODES ws_send(string resp)
        {
            Log("->| {0}", resp);
            byte[] bresp = Encoding.ASCII.GetBytes(resp);
            return wsc.Send(bresp, (uint)bresp.Length, WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME);
        }

        private WebSocketClient.WEBSOCKET_RESULT_CODES ws_json_send(object ws_obj)
        {
            string s = JsonConvert.SerializeObject(ws_obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return ws_send(s);
        }

        private const string m_heartbeat = "{\"m_TimeoutHandler\":\"test\"}";
        private void OnTimer(Object o)
        {
            if (wsc.Connected)
            {
                try
                {
                    ws_send(m_heartbeat);
                    keepAliveTimer.Reset(180000);
                }
                catch (Exception e)
                {
                    Log("Exception: {0}", e.Message);
                }
            }
        }

        private int onDisconnect(WebSocketClient.WEBSOCKET_RESULT_CODES error, object obj)
        {
            Log("Disconnected {0}", DateTime.Now.ToString());
            if (OnConnectChange != null)
                OnConnectChange(0);
            if (OnError != null)
                OnError((int) error);
            init();
            return (int) error;
        }

        private int onConnect(WebSocketClient.WEBSOCKET_RESULT_CODES error)
        {
            Log("Connected {0}", DateTime.Now.ToString());
            wsc.ReceiveAsync();
            if (OnConnectChange != null)
                OnConnectChange(1);
            if (OnError != null)
                OnError((int)error);
            return (int)error;
        }

        private int int10(float x)
        {
            return (int) ((x + 0.01) * 10.0);
        }

        private int int100(double x)
        {
            return (int)((x + 0.001) * 100.0);
        }

        internal void reportValue(int num, bool? val)
        {
            if (OnValueReported != null)
                if (val != null)
                    OnValueReported(num,(val ?? false)?1:0);
        }

        internal void reportValue(int num, int? val)
        {
            if (OnValueReported != null)
                if (val != null)
                    OnValueReported(num, val ?? 0);
        }

        internal void reportValue(int num, float? val)
        {
            if (OnValueReported != null)
                if (val != null)
                    OnValueReported(num, int10( val ?? 0) );
        }

        // 'smart' use of double vs float:)))
        internal void reportValue(int num, double? val)
        {
            if (OnValueReported != null)
                if (val != null)
                    OnValueReported(num, int100(val ?? 0));
        }

        public void checkMessage()
        {
            if (OnOspaMsg == null)
                return;

            HttpClient client = new HttpClient();
            HttpClientRequest req = new HttpClientRequest();

            try
            {
                client.KeepAlive = false;
                req.Url.Parse(m_ErrURL);
            }
            catch (Exception e)
            {
                Log("Exception: {0}", e.Message);
                return;
            }

            client.DispatchAsync(req, (resp, e) => 
            {
                try 
                {
                    if (resp.Code != 200)
                    {
                        Log("ERROR: {0}", resp.Code);
                        return;
                    }

                    string sdata = resp.ContentString;
                    for (int i = 0; i < sdata.Length; i += maxChunkSize)
                    {
                        string schunk = sdata.Substring(i, Math.Min(maxChunkSize, sdata.Length - i));
                        OnOspaMsg(new SimplSharpString(schunk));
                    }
                }
                catch (Exception e2)
                {
                    Log("Exception: {0}", e2.Message);
                }
            });
        }

        public void Send(int arg, int val)
        {
            OspaJson.wsMessage msg = new OspaJson.wsMessage();
            switch (arg)
            {
                case 1:
                    msg.m_TempIst_1_2 = val / 10f;
                    break;
                case 2:
                    msg.m_RedoxIst = val;
                    break;
                case 3:
                    msg.m_pHIst = val / 100d;
                    break;
                case 4:
                    msg.m_ChlorIst = val / 10f;
                    break;
                case 5:
                    msg.m_TempSoll_1 = val / 10f;
                    break;
                case 6:
                    msg.m_TempSoll_2 = val / 10f;
                    break;
                case 7:
                    msg.m_RaumTempIst = val / 10f; ;
                    break;
                case 8:
                    msg.m_RaumTempSoll = val / 10f;
                    break;
                case 9:
                    msg.m_RaumFeuchteIst = val / 10f;
                    break;
                case 10:
                    msg.m_RaumFeuchteSoll = val / 10f;
                    break;
                case 11:
                    msg.m_WasserwerteGueltig = val != 0;
                    break;
                case 12:
                    msg.m_AttrBetrieb1 = val != 0;
                    break;
                case 13:
                    msg.m_AttrBetrieb2 = val != 0;
                    break;
                case 14:
                    msg.m_AttrBetrieb3 = val != 0;
                    break;
                case 15:
                    msg.m_AttrBetrieb4 = val != 0;
                    break;
                case 16:
                    msg.m_AttrBetrieb5 = val != 0;
                    break;
                case 17:
                    msg.m_AttrBetrieb6 = val != 0;
                    break;
                case 18:
                    msg.m_AttrBetrieb7 = val != 0;
                    break;
                case 19:
                    msg.m_AttrBetrieb8 = val != 0;
                    break;
                case 20:
                    msg.m_AttrBetrieb9 = val != 0;
                    break;
                case 21:
                    msg.m_AttrBetrieb10 = val != 0;
                    break;
                case 22:
                    msg.m_Uws1SbEin = val != 0;
                    break;
                case 23:
                    msg.m_Uws1WpEin = val != 0;
                    break;
                case 24:
                    msg.m_WpEin = val != 0;
                    break;
                case 25:
                    msg.m_SbEin = val != 0;
                    break;
                case 26:
                    msg.m_RinneEin = val != 0;
                    break;
                case 27:
                    msg.m_BodenreinigerEin = val != 0;
                    break;
                case 28:
                    msg.m_FilteranlageBetrieb = val != 0;
                    break;
                case 29:
                    msg.m_StoerungWasserwerte = val != 0;
                    break;
                case 30:
                    msg.m_Sammelstoerung = val != 0;
                    break;
                case 31:
                    msg.m_LichtSzene = val;
                    break;
                case 32:
                    msg.m_FilterBetriebsart = val;
                    break;
                case 33:
                    msg.m_KlimaBetrieb = val != 0;
                    break;
                case 34:
                    msg.m_KlimaBadebetrieb = val != 0;
                    break;
                case 35:
                    msg.m_KlimaFrischluftbetrieb = val != 0;
                    break;
                case 36:
                    msg.m_AlarmeAnstehend = val != 0;
                    break;
                case 37:
                    msg.m_FilterpumpeVoruebergehendSchalten = val;
                    break;
                case 38:
                    msg.m_TempRegler1_Aktiv = val != 0;
                    break;
                case 39:
                    msg.m_RedoxSoll = val;
                    break;
                case 40:
                    msg.m_pHSoll = val / 100d;
                    break;
                case 41:
                    msg.m_ChlorSoll = val / 10f;
                    break;
                case 42:
                    msg.m_KlimaFrischluftschaltung = val != 0;
                    break;
                case 43:
                    msg.m_AbschaltenWegenDurchflussmangel = val != 0;
                    break;
                case 44:
                    msg.m_AlarmMaxFrischwasserNachspeisung = val != 0;
                    break;
                case 45:
                    msg.m_AlarmLeckageErkannt = val != 0;
                    break;
                case 46:
                    msg.m_ErrTestAbsperrhahn1 = val != 0;
                    break;
                case 47:
                    msg.m_ErrTestAbsperrhahn2 = val != 0;
                    break;
            }
            if (m_allowed)
                ws_json_send(msg);
        }

        private void processMessage(string rcvString)
        {
            try
            {
                OspaJson.wsMessage msg = JsonConvert.DeserializeObject<OspaJson.wsMessage>(rcvString);
                reportValue(1, msg.m_TempIst_1_2);
                reportValue(2, msg.m_RedoxIst);
                reportValue(3, msg.m_pHIst);
                reportValue(4, msg.m_ChlorIst);
                reportValue(5, msg.m_TempSoll_1);
                reportValue(6, msg.m_TempSoll_2);
                reportValue(7, msg.m_RaumTempIst);
                reportValue(8, msg.m_RaumTempSoll);
                reportValue(9, msg.m_RaumFeuchteIst);
                reportValue(10, msg.m_RaumFeuchteSoll);
                reportValue(11, msg.m_WasserwerteGueltig);
                reportValue(12, msg.m_AttrBetrieb1);
                reportValue(13, msg.m_AttrBetrieb2);
                reportValue(14, msg.m_AttrBetrieb3);
                reportValue(15, msg.m_AttrBetrieb4);
                reportValue(16, msg.m_AttrBetrieb5);
                reportValue(17, msg.m_AttrBetrieb6);
                reportValue(18, msg.m_AttrBetrieb7);
                reportValue(19, msg.m_AttrBetrieb8);
                reportValue(20, msg.m_AttrBetrieb9);
                reportValue(21, msg.m_AttrBetrieb10);
                reportValue(22, msg.m_Uws1SbEin);
                reportValue(23, msg.m_Uws1WpEin);
                reportValue(24, msg.m_WpEin);
                reportValue(25, msg.m_SbEin);
                reportValue(26, msg.m_RinneEin);
                reportValue(27, msg.m_BodenreinigerEin);
                reportValue(28, msg.m_FilteranlageBetrieb);
                reportValue(29, msg.m_StoerungWasserwerte);
                reportValue(30, msg.m_Sammelstoerung);
                reportValue(31, msg.m_LichtSzene);
                reportValue(32, msg.m_FilterBetriebsart);
                reportValue(33, msg.m_KlimaBetrieb);
                reportValue(34, msg.m_KlimaBadebetrieb);
                reportValue(35, msg.m_KlimaFrischluftbetrieb);
                reportValue(36, msg.m_AlarmeAnstehend);
                reportValue(37, msg.m_FilterpumpeVoruebergehendSchalten);
                reportValue(38, msg.m_TempRegler1_Aktiv);
                reportValue(39, msg.m_RedoxSoll);
                reportValue(40, msg.m_pHSoll);
                reportValue(41, msg.m_ChlorSoll);
                reportValue(42, msg.m_KlimaFrischluftschaltung);
                reportValue(43, msg.m_AbschaltenWegenDurchflussmangel);
                reportValue(44, msg.m_AlarmMaxFrischwasserNachspeisung);
                reportValue(45, msg.m_AlarmLeckageErkannt);
                reportValue(46, msg.m_ErrTestAbsperrhahn1);
                reportValue(47, msg.m_ErrTestAbsperrhahn2);
            }

            catch (Exception e)
            {
                Log("processMessage: {0}",e.Message);
            }
        }

        private void processAllowed(string rcvString)
        {
            try
            {
                OspaJson.AccessAllowed msg = JsonConvert.DeserializeObject<OspaJson.AccessAllowed>(rcvString);
                m_allowed = msg.accessAllowed;
                if (OnGrantedChange != null)
                    OnGrantedChange(m_allowed ? 1 : 0);
            }
            catch (Exception e)
            {
                Log("processAllowed: {0}", e.Message);
            }
        }

        private void logon(string rcvString)
        {
            try
            {
                OspaJson.AccessKey key = JsonConvert.DeserializeObject<OspaJson.AccessKey>(rcvString);
                m_accessKey = key.accessKey;

                SHA1Managed sh1 = new SHA1Managed();
                byte[] accessKeyBack = sh1.ComputeHash(Encoding.ASCII.GetBytes(m_accessKey + m_password));
                m_accessKeyBack = BitConverter.ToString(accessKeyBack).Replace("-", string.Empty).ToLower();

                OspaJson.AccessKeyBack keyBack = new OspaJson.AccessKeyBack();
                keyBack.accessKeyBack = m_accessKeyBack;
                ws_json_send(keyBack);
                keepAliveTimer.Reset(180000);
            }
            catch (Exception e)
            {
                m_accessKey = null;
                Log("logon: {0}", e.Message);
            }
        }

        private int onReceive(byte[] data, uint datalen, WebSocketClient.WEBSOCKET_PACKET_TYPES opcode, WebSocketClient.WEBSOCKET_RESULT_CODES error)
        {
            if (error != WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
            {
                wsc.DisconnectAsync(this);
                return (int)error;
            }

            string rcvString = Encoding.ASCII.GetString(data, 0, data.Length);
            Log("<-| {0}", rcvString);

            if (string.IsNullOrEmpty(m_accessKey))
            {
                logon(rcvString);
            }
            else
            {
                if( m_allowed )
                    processMessage(rcvString);
                else
                    processAllowed(rcvString);
            }

            wsc.ReceiveAsync();
            return (int)error;
        }

        public void Enable(int ienable)
        {
            if (ienable == 0)
            {
                if (wsc.Connected)
                {
                    wsc.DisconnectAsync(this);
                }
            }
            else
            {
                if (!wsc.Connected)
                {
                    wsc.ConnectAsync();
                }
            }
        }

        public void Initialize(string host,string password)
        {
            init();
            m_password = password;
            keepAliveTimer = new CTimer(OnTimer, Timeout.Infinite);
            wsc = new WebSocketClient();
            wsc.URL = "ws://" + host + ":56525/user";
            m_ErrURL = "http://" + host + "/csv/errors.txt";
            wsc.Host = host + ":56525";
            wsc.Origin = "http://".ToString() + host + ":56525";
            wsc.DisconnectCallBack += onDisconnect;
            wsc.ReceiveCallBack += onReceive;
            wsc.ConnectionCallBack += onConnect;
        }
    }
}
