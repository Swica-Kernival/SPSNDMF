using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using VRC.SDK3.Avatars.Components;

namespace VF.Plugin.Passes {
    /// <summary>
    /// VRCFury normalizes the base mask of its completed FX controller. MA appends
    /// that controller to the avatar's FX instead, so the destination's base mask
    /// can still suppress the generated marker Transform curves. Extend that
    /// virtualized mask only for generated objects actually animated by SPS.
    /// </summary>
    internal class SpsFxTransformMaskPass : Pass<SpsFxTransformMaskPass> {
        public override string DisplayName => "SPS Generated FX Transform Mask";

        protected override void Execute(BuildContext context) {
            var state = context.GetState<SpsContext>();
            if (state.GeneratedFxTransformTargets.Count == 0) return;

            var animators = context.Extension<AnimatorServicesContext>();
            if (!animators.ControllerContext.Controllers.TryGetValue(
                    VRCAvatarDescriptor.AnimLayerType.FX, out var controller)) return;
            var mask = controller.Layers.FirstOrDefault()?.AvatarMask;
            // Both a null mask and a mask without Transform entries allow all
            // Transforms. Adding entries to the latter would narrow its scope.
            if (mask == null || mask.Elements.Count == 0) return;

            var elements = mask.Elements;
            foreach (var target in state.GeneratedFxTransformTargets) {
                if (target == null || !target.IsChildOf(context.AvatarRootTransform)) continue;
                var path = animators.ObjectPathRemapper.GetVirtualPathForObject(target);
                elements = elements.SetItem(path, 1);
            }
            // NDMF owns a clone of this mask; original entries and humanoid
            // flags are retained, and source mask assets are not modified.
            mask.Elements = elements;
        }
    }
}
