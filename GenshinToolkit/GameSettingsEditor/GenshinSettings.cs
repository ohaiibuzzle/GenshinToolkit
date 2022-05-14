using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenshinToolkit
{
    internal class GenshinSettings
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class CustomVolatileGrade
        {
            public int key { get; set; }
            public int value { get; set; }
        }

        public class GraphicsData
        {
            public int currentVolatielGrade { get; set; }
            public List<CustomVolatileGrade> customVolatileGrades { get; set; }
            public string volatileVersion { get; set; }
        }

        public class GenshinSettingsData
        {
            [JsonIgnore]
            public bool isGraphicsAvailable { get; set; }

            [JsonIgnore]
            public GraphicsData graphicsData { get; set; }

            public string deviceUUID { get; set; }
            public string userLocalDataVersionId { get; set; }
            public int deviceLanguageType { get; set; }
            public int deviceVoiceLanguageType { get; set; }
            public string selectedServerName { get; set; }
            public int localLevelIndex { get; set; }
            public string deviceID { get; set; }
            public string targetUID { get; set; }
            public string curAccountName { get; set; }
            public string uiSaveData { get; set; }
            public string inputData { get; set; }

            [JsonProperty(PropertyName = "graphicsData")]
            public string graphicsDataSerialized { get; set; }

            public string globalPerfData { get; set; }
            public int miniMapConfig { get; set; }
            public bool enableCameraSlope { get; set; }
            public bool enableCameraCombatLock { get; set; }
            public bool completionPkg { get; set; }
            public bool completionPlayGoPkg { get; set; }
            public bool onlyPlayWithPSPlayer { get; set; }
            public bool needPlayGoFullPkgPatch { get; set; }
            public bool resinNotification { get; set; }
            public bool exploreNotification { get; set; }
            public int volumeGlobal { get; set; }
            public int volumeSFX { get; set; }
            public int volumeMusic { get; set; }
            public int volumeVoice { get; set; }
            public int audioAPI { get; set; }
            public int audioDynamicRange { get; set; }
            public int audioOutput { get; set; }
            public bool _audioSuccessInit { get; set; }
            public bool enableAudioChangeAndroidMinimumBufferCapacity { get; set; }
            public int audioAndroidMiniumBufferCapacity { get; set; }
            public bool motionBlur { get; set; }
            public bool gyroAiming { get; set; }
            public bool firstHDRSetting { get; set; }
            public double maxLuminosity { get; set; }
            public double uiPaperWhite { get; set; }
            public double scenePaperWhite { get; set; }
            public double gammaValue { get; set; }
            public List<object> _overrideControllerMapKeyList { get; set; }
            public List<object> _overrideControllerMapValueList { get; set; }
            public int lastSeenPreDownloadTime { get; set; }
            public bool mtrCached { get; set; }
            public bool mtrIsOpen { get; set; }
            public int mtrMaxTTL { get; set; }
            public int mtrTimeOut { get; set; }
            public int mtrTraceCount { get; set; }
            public int mtrAbortTimeOutCount { get; set; }
            public int mtrAutoTraceInterval { get; set; }
            public int mtrTraceCDEachReason { get; set; }
            public List<object> _customDataKeyList { get; set; }
            public List<object> _customDataValueList { get; set; }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                try
                {
                    graphicsData = JsonConvert.DeserializeObject<GraphicsData>(graphicsDataSerialized.ToString());
                    // Subtract 1 on all the value to convert from 1-based to 0-based index
                    for (int i = 0; i < graphicsData.customVolatileGrades.Count; i++)
                    {
                        graphicsData.customVolatileGrades[i].value = graphicsData.customVolatileGrades[i].value - 1;
                    }
                    isGraphicsAvailable = true;

                }
                catch (Exception)
                {
                    isGraphicsAvailable = false;
                }
            }

            [OnSerializing]
            private void OnSerializing(StreamingContext context)
            {
                if (isGraphicsAvailable)
                {
                    // Convert back to 1-based index
                    for (int i = 0; i < graphicsData.customVolatileGrades.Count; i++)
                    {
                        graphicsData.customVolatileGrades[i].value = graphicsData.customVolatileGrades[i].value + 1;
                    }
                    graphicsDataSerialized = JsonConvert.SerializeObject(graphicsData);
                }
                
            }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }     
    }
}
