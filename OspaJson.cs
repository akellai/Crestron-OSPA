using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OspaSS
{
    public class OspaJson
    {
        public class AccessKey
        {
            public string accessKey { get; set; }
        }

        public class AccessAllowed
        {
            public bool accessAllowed { get; set; }
        }

        public class AccessKeyBack
        {
            public string accessKeyBack { get; set; }
        }

        public class wsMessage
        {
            public float? m_TempIst_1_2 { get; set; }          // 1
            public int? m_RedoxIst { get; set; }               // 2
            public double? m_pHIst { get; set; }               // 3
            public float? m_ChlorIst { get; set; }             // 4
            public float? m_TempSoll_1 { get; set; }           // 5
            public float? m_TempSoll_2 { get; set; }           // 6
            public float? m_RaumTempIst { get; set; }          // 7
            public float? m_RaumTempSoll { get; set; }         // 8
            public float? m_RaumFeuchteIst { get; set; }       // 9
            public float? m_RaumFeuchteSoll { get; set; }      // 10
            public bool? m_WasserwerteGueltig { get; set; }    // 11
            public bool? m_AttrBetrieb1 { get; set; }          // 12
            public bool? m_AttrBetrieb2 { get; set; }          // 13
            public bool? m_AttrBetrieb3 { get; set; }          // 14
            public bool? m_AttrBetrieb4 { get; set; }          // 15
            public bool? m_AttrBetrieb5 { get; set; }          // 16
            public bool? m_AttrBetrieb6 { get; set; }          // 17
            public bool? m_AttrBetrieb7 { get; set; }          // 18
            public bool? m_AttrBetrieb8 { get; set; }          // 19
            public bool? m_AttrBetrieb9 { get; set; }          // 20
            public bool? m_AttrBetrieb10 { get; set; }         // 21
            public int[] m_AttrFuStufen { get; set; }
            public bool? m_Uws1SbEin { get; set; }             // 22
            public bool? m_Uws1WpEin { get; set; }             // 23
            public bool? m_WpEin { get; set; }                 // 24
            public bool? m_SbEin { get; set; }                 // 25
            public bool? m_RinneEin { get; set; }              // 26
            public bool? m_BodenreinigerEin { get; set; }      // 27
            public bool? m_FilteranlageBetrieb { get; set; }   // 28
            public bool? m_StoerungWasserwerte { get; set; }   // 29
            public bool? m_Sammelstoerung { get; set; }        // 30
            public int? m_LichtSzene { get; set; }             // 31
            public int? m_FilterBetriebsart { get; set; }      // 32
            public bool? m_KlimaBetrieb { get; set; }          // 33
            public bool? m_KlimaBadebetrieb { get; set; }      // 34
            public bool? m_KlimaFrischluftbetrieb { get; set; }// 35
            public bool? m_AlarmeAnstehend { get; set; }       // 36
            public int[] m_FilterLaufzeiten { get; set; }
            public int? m_FilterpumpeVoruebergehendSchalten { get; set; }  // 37
            public int[] m_AppAttrLeistungsEinstellung { get; set; }
            public int[] m_AppPsTrainingsprogramm { get; set; }
            public int[] m_AppAbdeckung { get; set; }
            public int[] m_AppLichtSzene { get; set; }
            public bool? m_TempRegler1_Aktiv { get; set; }     // 38
            public int? m_RedoxSoll { get; set; }              // 39
            public double? m_pHSoll { get; set; }              // 40
            public float? m_ChlorSoll { get; set; }            // 41
            public bool? m_KlimaFrischluftschaltung { get; set; }          // 42
            public bool? m_AbschaltenWegenDurchflussmangel { get; set; }   // 43
            public bool? m_AlarmMaxFrischwasserNachspeisung { get; set; }  // 44
            public bool? m_AlarmLeckageErkannt { get; set; }               // 45
            public bool? m_ErrTestAbsperrhahn1 { get; set; }               // 46
        }
    }
}
