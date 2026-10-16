using System.Text.Json;
using UnityEngine;

namespace Crisis.Runtime;

// Browser/native diagnostic adapter ONLY: invokes the unchanged original
// Hand.SetMode developer implementation. It does not implement arm animation,
// IKArmSequencer, fake CharacterBody callbacks, or render hand meshes.
public static class OriginalHandModes {
    public static string Run() {
        // Separate world and unchanged clock so source execution cannot reset
        // the live browser game's physics, input or time.
        var world = new RuntimeWorld(makeCurrent: false, resetClock: false);
        var handObject = new GameObject("Original source hand fixture", world);
        var hand = handObject.AddComponent<Hand>();
        hand.fist = new GameObject("Original fist visual", world);
        hand.trigger = new GameObject("Original trigger visual", world);
        hand.forwardAssist = new GameObject("Original forward-assist visual", world);
        var outputs = new List<object>();
        foreach (var mode in new[] { Hand.Mode.Fist, Hand.Mode.ForwardAssist, Hand.Mode.Trigger }) {
            // This is the actual recovered original developer method, not an
            // adapter pretending that a hand mode was changed.
            hand.SetMode(mode);
            outputs.Add(new {
                mode = (int)hand.CurrentMode,
                fist = hand.fist.activeSelf,
                trigger = hand.trigger.activeSelf,
                forwardAssist = hand.forwardAssist.activeSelf
            });
        }
        return JsonSerializer.Serialize(new {
            originalHandMethodExecuted = true,
            preservedGameWorldAndClock = true,
            states = outputs,
            ikControllerPlaybackImplemented = false,
            scope = "Unmodified original Hand.SetMode source called in isolated native C# game-object fixture; no live character/IK playback"
        });
    }
}
