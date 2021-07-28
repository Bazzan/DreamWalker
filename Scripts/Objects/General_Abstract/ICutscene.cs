using System;
using UnityEngine;

namespace GP2_Team7
{
    /// <summary>
    /// Interface to define a class as eligible for
    /// cutscene procession. And yes, literally 
    /// anything be treated a cutscene theoretically,
    /// as long as this interface is in charge of it.
    /// (Wait, it's all cutscenes?)
    /// (...Always has been.)
    /// </summary>
    public interface ICutscene
    {
        /// <summary>
        /// The current time position of the cutscene.
        /// If the cutscene has no time dependency,
        /// set to return null.
        /// </summary>
        float? CutsceneTime { get; set; }

        /// <summary>
        /// The total length of the cutscene. If
        /// the cutscene has no time dependency,
        /// set to return null.
        /// </summary>
        float? CutsceneLength { get; }

        /// <summary>
        /// Calls when the player is performing inputs for
        /// skipping a cutscene. Returns whether the cutscene
        /// can currently be skipped or not.
        /// </summary>
        /// <param name="skipProgress">The skip meter. When above the threshold, the cutscene gets called to be stopped.</param>
        bool OnSkip(float skipProgress);

        bool IsInterruptible { get; }

        void OnCutsceneStart(float startPosition);

        void OnCutsceneUpdate();

        void StopCutscene();

        Action<Action[]> OnCutsceneEnd { get; set; }
    }
}
