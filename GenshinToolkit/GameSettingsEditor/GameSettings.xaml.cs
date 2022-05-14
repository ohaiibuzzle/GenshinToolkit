using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Windows;
using GenshinToolkit.GameSettingsEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenshinToolkit
{
    /// <summary>
    /// Interaction logic for GameSettings.xaml
    /// </summary>
    public partial class GameSettings : Window
    {
        GenshinSettings.GenshinSettingsData gameSettings;
        string generalDataKey = "";
        string generalData = "";

        public GameSettings()
        {
            InitializeComponent();

            // Read the registry key
            RegistryKey targetKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\miHoYo\Genshin Impact");
            if (targetKey == null)
            {
                MessageBox.Show("The registry key could not be found.\n\nTry starting the game once", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }
            var values = targetKey.GetValueNames();

            // Find the subkey starting with "GENERAL_DATA"

            foreach (var value in values)
            {
                if (value.StartsWith("GENERAL_DATA"))
                {
                    generalDataKey = value;
                    break;
                }
            }
            generalData = System.Text.Encoding.UTF8.GetString(targetKey.GetValue(generalDataKey) as byte[]);

            if (generalDataKey == "")
            {
                MessageBox.Show("Could not find the key for the game settings.");
                this.Close();
                return;
            }
            LoadGraphicsSettings();
        }

        private void LoadGraphicsSettings()
        {
            
            gameSettings = JsonConvert.DeserializeObject<GenshinSettings.GenshinSettingsData>(generalData);

            if (gameSettings.isGraphicsAvailable)
            {
                PresetCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.GraphicsPreset)));
                VSyncCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.VSync)).Skip(1).ToArray());

                var resolutionSettings = Enum.GetNames(typeof(GenshinSettingsEnums.RenderResolution)).Skip(1).ToArray();
                for (int i = 0; i < resolutionSettings.Length; i++)
                {
                    resolutionSettings[i] = resolutionSettings[i].Replace("_", ".");
                }
                RRCb.ItemsSource = resolutionSettings;

                ShadowQCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.ShadowQuality)).Skip(1).ToArray());
                VFXCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.VisualEffects)).Skip(1).ToArray());
                SFXCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.SFXQuality)).Skip(1).ToArray());
                EnvDetailCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.EnvironmentDetail)).Skip(1).ToArray());

                var fpsSettings = (Enum.GetNames(typeof(GenshinSettingsEnums.FPS)).Skip(1).ToArray());
                for (int i = 0; i < fpsSettings.Length; i++)
                {
                    fpsSettings[i] = fpsSettings[i].Replace("F", "");
                }
                FPSCb.ItemsSource = fpsSettings;

                AACb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.AntiAliasing)).Skip(1).ToArray());
                VFogCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.VolumetricFog)).Skip(1).ToArray());
                ReflectionCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.Reflection)).Skip(1).ToArray());
                MotionBlurCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.MotionBlur)).Skip(1).ToArray());
                BloomCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.Bloom)).Skip(1).ToArray());
                CrowdCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.CrowdDensity)).Skip(1).ToArray());
                SSScaterCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.SubsurfaceScattering)).Skip(1).ToArray());
                TeamEffectsCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.CoOpDetail)).Skip(1).ToArray());
                AFCb.ItemsSource = (Enum.GetNames(typeof(GenshinSettingsEnums.AnisotropicFiltering)).Skip(1).ToArray());
            }

            // Set the values
            // If it's custom populate the other settings, if not, set the preset
            if (gameSettings.graphicsData.currentVolatielGrade != -1)
            {
                ApplyPreset(gameSettings.graphicsData.currentVolatielGrade-1);
                PresetCb.SelectedIndex = gameSettings.graphicsData.currentVolatielGrade - 1;
            }
            else
            {
                PresetCb.SelectedIndex = 4;
                VSyncCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.VSync).value;
                RRCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.RenderResolution).value;
                ShadowQCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.ShadowQuality).value;
                VFXCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.VisualEffects).value;
                SFXCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.SFXQuality).value;
                EnvDetailCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.EnvironmentDetail).value;
                FPSCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.FPS).value;
                AACb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.AntiAliasing).value;
                VFogCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.VolumetricFog).value;
                ReflectionCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.Reflection).value;
                MotionBlurCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.MotionBlur).value;
                BloomCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.Bloom).value;
                CrowdCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.CrowdDensity).value;
                SSScaterCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.SubsurfaceScattering).value;
                TeamEffectsCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.CoOpDetail).value;
                AFCb.SelectedIndex = (int)Array.Find<GenshinSettings.CustomVolatileGrade>(gameSettings.graphicsData.customVolatileGrades.ToArray(), x => x.key == (int)GenshinSettingsEnums.SettingMappings.AnisotropicFiltering).value;
            }
        }

        private void PresetCb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (PresetCb.SelectedIndex)
            {
                case 4: // Custom
                    break;
                default:
                    ApplyPreset(PresetCb.SelectedIndex);
                    break;
            }

            if (PresetCb.SelectedIndex != 4)
            {
                ToggleCustomSettings(false);
            }
            else
            {
                ToggleCustomSettings(true);
            }
        }

        private void ToggleCustomSettings(bool status)
        {
            VSyncCb.IsEnabled = status;
            RRCb.IsEnabled = status;
            ShadowQCb.IsEnabled = status;
            VFXCb.IsEnabled = status;
            SFXCb.IsEnabled = status;
            EnvDetailCb.IsEnabled = status;
            FPSCb.IsEnabled = status;
            AACb.IsEnabled = status;
            VFogCb.IsEnabled = status;
            ReflectionCb.IsEnabled = status;
            MotionBlurCb.IsEnabled = status;
            BloomCb.IsEnabled = status;
            CrowdCb.IsEnabled = status;
            SSScaterCb.IsEnabled = status;
            TeamEffectsCb.IsEnabled = status;
            AFCb.IsEnabled = status;
        }

        private void ApplyPreset(int preset)
        {
            switch (preset)
            {
                case 0:
                    VSyncCb.SelectedIndex = 1;
                    RRCb.SelectedIndex = 1;
                    ShadowQCb.SelectedIndex = 0;
                    VFXCb.SelectedIndex = 0;
                    SFXCb.SelectedIndex = 0;
                    EnvDetailCb.SelectedIndex = 0;
                    FPSCb.SelectedIndex = 0;
                    AACb.SelectedIndex = 0;
                    VFogCb.SelectedIndex = 0;
                    ReflectionCb.SelectedIndex = 0;
                    MotionBlurCb.SelectedIndex = 0;
                    BloomCb.SelectedIndex = 1;
                    CrowdCb.SelectedIndex = 1;
                    SSScaterCb.SelectedIndex = 0;
                    TeamEffectsCb.SelectedIndex = 2;
                    AFCb.SelectedIndex = 0;
                    break;
                case 1:
                    VSyncCb.SelectedIndex = 1;
                    RRCb.SelectedIndex = 2;
                    ShadowQCb.SelectedIndex = 1;
                    VFXCb.SelectedIndex = 1;
                    SFXCb.SelectedIndex = 1;
                    EnvDetailCb.SelectedIndex = 1;
                    FPSCb.SelectedIndex = 0;
                    AACb.SelectedIndex = 2;
                    VFogCb.SelectedIndex = 0;
                    ReflectionCb.SelectedIndex = 0;
                    MotionBlurCb.SelectedIndex = 0;
                    BloomCb.SelectedIndex = 1;
                    CrowdCb.SelectedIndex = 1;
                    SSScaterCb.SelectedIndex = 0;
                    TeamEffectsCb.SelectedIndex = 2;
                    AFCb.SelectedIndex = 0;
                    break;
                case 2:
                    VSyncCb.SelectedIndex = 1;
                    RRCb.SelectedIndex = 2;
                    ShadowQCb.SelectedIndex = 2;
                    VFXCb.SelectedIndex = 2;
                    SFXCb.SelectedIndex = 2;
                    EnvDetailCb.SelectedIndex = 2;
                    FPSCb.SelectedIndex = 1;
                    AACb.SelectedIndex = 2;
                    VFogCb.SelectedIndex = 1;
                    ReflectionCb.SelectedIndex = 0;
                    MotionBlurCb.SelectedIndex = 2;
                    BloomCb.SelectedIndex = 1;
                    CrowdCb.SelectedIndex = 1;
                    SSScaterCb.SelectedIndex = 1;
                    TeamEffectsCb.SelectedIndex = 0;
                    AFCb.SelectedIndex = 0;
                    break;
                case 3:
                    VSyncCb.SelectedIndex = 1;
                    RRCb.SelectedIndex = 3;
                    ShadowQCb.SelectedIndex = 3;
                    VFXCb.SelectedIndex = 3;
                    SFXCb.SelectedIndex = 3;
                    EnvDetailCb.SelectedIndex = 3;
                    FPSCb.SelectedIndex = 1;
                    AACb.SelectedIndex = 2;
                    VFogCb.SelectedIndex = 1;
                    ReflectionCb.SelectedIndex = 1;
                    MotionBlurCb.SelectedIndex = 3;
                    BloomCb.SelectedIndex = 1;
                    CrowdCb.SelectedIndex = 1;
                    SSScaterCb.SelectedIndex = 1;
                    TeamEffectsCb.SelectedIndex = 0;
                    AFCb.SelectedIndex = 0;
                    break;
            }
        }
        private void SaveSettings()
        {
            // If a preset is set, customVolatileGrades is an empty array
            if (PresetCb.SelectedIndex != 4)
            {
                // Set preset 
                gameSettings.graphicsData.currentVolatielGrade = (int)PresetCb.SelectedIndex + 1;
                gameSettings.graphicsData.customVolatileGrades = new List<GenshinSettings.CustomVolatileGrade>();
            }
            else
            {
                // Set custom volatile grades
                gameSettings.graphicsData.currentVolatielGrade = (int)GenshinSettingsEnums.GraphicsPreset.Custom;

                // Recreate the array
                gameSettings.graphicsData.customVolatileGrades = new List<GenshinSettings.CustomVolatileGrade>();

                // Populate the keys in order
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.VSync,
                    value = VSyncCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.RenderResolution,
                    value = RRCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.ShadowQuality,
                    value = ShadowQCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.VisualEffects,
                    value = VFXCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.SFXQuality,
                    value = SFXCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.EnvironmentDetail,
                    value = EnvDetailCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.FPS,
                    value = FPSCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.AntiAliasing,
                    value = AACb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.VolumetricFog,
                    value = VFogCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.Reflection,
                    value = ReflectionCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.MotionBlur,
                    value = MotionBlurCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.Bloom,
                    value = BloomCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.CrowdDensity,
                    value = CrowdCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.SubsurfaceScattering,
                    value = SSScaterCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.CoOpDetail,
                    value = TeamEffectsCb.SelectedIndex
                });
                gameSettings.graphicsData.customVolatileGrades.Add(new GenshinSettings.CustomVolatileGrade()
                {
                    key = (int)GenshinSettingsEnums.SettingMappings.AnisotropicFiltering,
                    value = AFCb.SelectedIndex
                });
            }
            // Reserializes
            var serialized = JsonConvert.SerializeObject(gameSettings);

            // Convert to a bytes array
            var bytes = Encoding.UTF8.GetBytes(serialized);

            // Open the registry key in write mode
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\miHoYo\Genshin Impact", true))
            {
                // Write the serialized settings to the registry
                key.SetValue(generalDataKey, bytes);
            }
        }

        private void ShadowQCb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // If Shadow Quality is set to Low or Lower, Volumetric Fog is forced to Off and disabled
            if (ShadowQCb.SelectedIndex == 0 || ShadowQCb.SelectedIndex == 1)
            {
                VFogCb.SelectedIndex = 0;
                VFogCb.IsEnabled = false;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Save the settings
            SaveSettings();
            MessageBox.Show("Settings saved successfully!", "GenshinToolkit", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Close
            this.Close();
        }
    }
}
