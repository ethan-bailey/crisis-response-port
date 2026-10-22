using System;

namespace UnityEngine.Rendering
{
    public enum ReflectionProbeTimeSlicingMode
    {
        NoTimeSlicing = 0,
        AllFacesAtOnce = 1,
        IndividualFaces = 2
    }
}

namespace UnityEngine
{
    public enum LightShadows
    {
        None = 0,
        Hard = 1,
        Soft = 2
    }

    public interface IReflectionProbeDriver
    {
        int Render(ReflectionProbe probe, Rendering.ReflectionProbeTimeSlicingMode timeSlicingMode);
        bool IsFinishedRendering(ReflectionProbe probe, int requestId);
    }

    public sealed class ReflectionProbe : Behaviour
    {
        public Rendering.ReflectionProbeTimeSlicingMode timeSlicingMode { get; set; }
        public IReflectionProbeDriver Driver { get; set; }

        private IReflectionProbeDriver RequiredDriver => Driver ??
            throw new NotSupportedException("Original reflection probe rendering driver is not bound");

        public int RenderProbe()
        {
            return RequiredDriver.Render(this, timeSlicingMode);
        }

        public bool IsFinishedRendering(int requestId)
        {
            return RequiredDriver.IsFinishedRendering(this, requestId);
        }
    }
}
