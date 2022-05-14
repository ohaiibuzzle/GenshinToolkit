using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinToolkit.GameSettingsEditor
{
    internal class GenshinSettingsEnums
    {

        public enum GraphicsPreset
        {
            Lowest = 1,
            Low = 2,
            Medium = 3,
            High = 4,
            Custom = -1,            
        }

        public enum SettingMappings
        {
            Invalid,
            VSync,
            RenderResolution,
            ShadowQuality,
            VisualEffects,
            SFXQuality,
            EnvironmentDetail,
            FPS,
            AntiAliasing,
            VolumetricFog,
            Reflection,
            MotionBlur,
            Bloom,
            CrowdDensity,
            Invalid2,
            SubsurfaceScattering,
            CoOpDetail,
            AnisotropicFiltering,
        }

        public enum VSync
        {
            INVALID,
            Off,
            On,
        };

        public enum RenderResolution
        {
            INVALID,
            x0_6,
            x0_8,
            x1_0,
            x1_1,
            x1_2,
            x1_3,
            x1_4,
            x1_5
        }

        public enum ShadowQuality
        {
            Invalid,
            Lowest,
            Low,
            Medium,
            High
        }

        public enum VisualEffects
        {
            Invalid,
            Lowest,
            Low,
            Medium,
            High
        }
        public enum SFXQuality
        {
            Invalid,
            Lowest,
            Low,
            Medium,
            High
        }
        public enum EnvironmentDetail
        {
            Invalid,
            Lowest,
            Low,
            Medium,
            High,
            Highest
        }
        public enum FPS
        {
            Invalid,
            F30,
            F60
        }
        public enum AntiAliasing
        {
            INVALID,
            Off,
            TAA,
            SMAA,
        }
        public enum VolumetricFog
        {
            INVALID,
            Off,
            On,
        }
        public enum Reflection
        {
            INVALID,
            Off,
            On,
        }
        public enum MotionBlur
        {
            Invalid,
            Off,
            Low,
            High,
            Extreme
        }
        public enum Bloom
        {
            Invalid,
            Off,
            On,
        }
        public enum CrowdDensity
        {
            Invalid,
            Low,
            High,
        }

        public enum SubsurfaceScattering
        {
            Invalid,
            Off,
            Medium,
            High,
        }

        public enum CoOpDetail
        {
            Invalid,
            On,
            Partial,
            Off,
        }
        public enum AnisotropicFiltering
        {
            Invalid,
            x1,
            x2,
            x4,
            x8,
            x16,
        }
    }
}
