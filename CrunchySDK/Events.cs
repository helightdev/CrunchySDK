using System;
using Players.SteamVR;
using Procedural.Scripts;

namespace CrunchySDK
{

    public class Events
    {
        public static Events Instance { get; } = new Events();

        public Events()
        {
        }

        #region Events
        public event CrunchyEvent<PostProcessingStepEventArgs> PostProcessingStepEvent;
        public event CrunchyEvent<FinalizePostProcessingStepEventArgs> FinalizePostProcessingStepEvent;
        public event CrunchyEvent<PreProcessingEventArgs> PreProcessingEvent;
        public event CrunchyEvent<NoArgs> MissionStartedEvent;

        public event CrunchyEvent<PlayerSpawnEventArgs> PlayerSpawnEvent; 
        #endregion

        #region Invoke

        internal void InvokePlayerSpawnEvent(CrunchPlayer player)
        {
            var ev = new PlayerSpawnEventArgs
            {
                CrunchPlayer = player
            };
            PlayerSpawnEvent?.Invoke(ev);
        }
        
        internal void InvokeMissionStartedEvent()
        {
            MissionStartedEvent?.Invoke(NoArgs.Singleton);
        }
        
        internal void InvokePreProcessingEvent(MapBuilder mapBuilder, out bool allow)
        {
            allow = true;
            var ev = new PreProcessingEventArgs
            {
                Allow = allow,
                MapBuilder = mapBuilder
            };
            PreProcessingEvent?.Invoke(ev);
            allow = ev.Allow;
        }
        
        
        internal void InvokePostProcessingStepEvent(MapBuilder.ProcessingStep step, MapBuilder builder, out bool allow)
        {
            allow = true;
            var ev = new PostProcessingStepEventArgs
            {
                Allow = allow,
                ProcessingStep = step,
                StepType = PostProcessingStepEnumFactory.Get(step.step.name),
                MapBuilder = builder
            };
            PostProcessingStepEvent?.Invoke(ev);
            allow = ev.Allow;
        }
        
        internal void InvokeFinalizePostProcessingStepEvent(MapBuilder.ProcessingStep step, MapBuilder builder)
        {
            var ev = new FinalizePostProcessingStepEventArgs
            {
                ProcessingStep = step,
                StepType = PostProcessingStepEnumFactory.Get(step.step.name),
                MapBuilder = builder
            };
            FinalizePostProcessingStepEvent?.Invoke(ev);
        }
    }
    #endregion

    #region EventArgs
    public class PreProcessingEventArgs : IEventArgs, ICancellableEvent
    {
        public MapBuilder MapBuilder { get; internal set; }
        public bool Allow { get; set; }
    }
    
    public class PostProcessingStepEventArgs : IEventArgs, ICancellableEvent
    {
        public MapBuilder MapBuilder { get; internal set;  }
        public MapBuilder.ProcessingStep ProcessingStep { get; internal set;  }
        public PostProcessingStepEnum StepType { get; internal set;  }
        public bool Allow { get; set; }
    }
    
    public class FinalizePostProcessingStepEventArgs : IEventArgs
    {
        public MapBuilder MapBuilder { get; internal set;  }
        public MapBuilder.ProcessingStep ProcessingStep { get; internal set;  }
        public PostProcessingStepEnum StepType { get; internal set;  }
    }

    public class PlayerSpawnEventArgs : IEventArgs
    {
        public CrunchPlayer CrunchPlayer { get; set; }
    }
    #endregion


    public delegate void CrunchyEvent<TEvent>(TEvent ev) where TEvent : IEventArgs;

    public class NoArgs : IEventArgs
    {
        public static NoArgs Singleton = new NoArgs();
    }
    
    public interface IEventArgs
    {
        
    }

    public interface ICancellableEvent
    {
        bool Allow { get; set; }
    }
    
}