using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Shaders
{
    // TODO -- This can be made into a ModSystem with simple OnModLoad and Unload hooks.
    // TODO: SOURCEGEN: Get rid of this loader and replace references with sourcegenned references

    [Autoload(Side = ModSide.Client)]
    public sealed class ShaderLoading : ModSystem
    {
        private const string ShaderPath = "Common/Graphics/Shaders/";
        internal const string CalamityShaderPrefix = "BigEvil:";

        #region Big E's Shaders
        internal static Asset<Effect> RadialBlur;

        internal static Asset<Effect> Dissolve;
        internal static Asset<Effect> PaletteSwap;

        internal static Asset<Effect> BrainOfCthulhuForcefield;
        #endregion

        public override void PostSetupContent()
        {
            AssetRepository bigEvilAss = BigEvilMod.Instance.Assets;

            // Shorthand to load shaders immediately.
            // Strings provided to LoadShader are the .xnb file paths.
            Asset<Effect> LoadShader(string path) => bigEvilAss.Request<Effect>($"{ShaderPath}{path}", AssetRequestMode.ImmediateLoad);

            #region Loading Big E's Shaders
            RadialBlur = LoadShader("RadialBlur");
            RegisterScreenShader(RadialBlur, "RadialBlurPass", "RadialBlurShader");

            Dissolve = LoadShader("Dissolve");
            RegisterMiscShader(Dissolve, "DissolvePass", "Dissolve");

            PaletteSwap = LoadShader("PaletteSwap");
            RegisterMiscShader(PaletteSwap, "PaletteSwapPass", "PaletteSwap");

            BrainOfCthulhuForcefield = LoadShader("BrainOfCthulhuForcefield");
            RegisterScreenShader(BrainOfCthulhuForcefield, "BoCShieldPass", "BrainOfCthulhuForcefield");
            #endregion
        }

        // Shorthand to register a loaded shader in Terraria's graphics engine
        // All shaders registered this way are accessible under GameShaders.Misc
        // They will use the prefix described above
        private static void RegisterMiscShader(Asset<Effect> shader, string passName, string registrationName)
        {
            MiscShaderData passParamRegistration = new(shader, passName);
            GameShaders.Misc[$"{CalamityShaderPrefix}{registrationName}"] = passParamRegistration;
        }

        private static void RegisterSceneFilter(ScreenShaderData passReg, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            string prefixedRegistrationName = $"{CalamityShaderPrefix}{registrationName}";
            Filters.Scene[prefixedRegistrationName] = new Filter(passReg, priority);
            Filters.Scene[prefixedRegistrationName].Load();
        }

        // Shorthand to register a loaded shader in Terraria's graphics engine
        // All shaders registered this way are accessible under Filters.Scene
        // They will use the prefix described above
        private static void RegisterScreenShader(Asset<Effect> shader, string passName, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            ScreenShaderData passParamRegistration = new(shader, passName);
            RegisterSceneFilter(passParamRegistration, registrationName, priority);
        }
    }
}