using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>Broadcasts the race state on every PC to things that can't reference the RaceManager (kart prefabs, persistent services).</summary>
    [CreateAssetMenu(menuName = "Eric Racer/Events/Race State Event Channel")]
    public class RaceStateEventChannel : EventChannel<RaceStateId> { }
}
