namespace Embervale.Corruption;

/// <summary>
/// Which of the game's two endings the player's corruption currently makes them eligible for
/// (Phase 23H — the both-endings dial). Corruption is the lever behind the LORE's final choice:
/// reject power for <see cref="Dawnfire"/> (the Age of Dawn) or embrace it for
/// <see cref="LordOfEmbers"/> (claim the Ash Throne). <see cref="Undecided"/> is the middle band
/// where the player has not yet committed either way. Phase 49 (Act IV + Endings) reads this to
/// gate the final choice and its epilogues.
///
/// This is a <em>derived, runtime-only</em> value computed from the saved corruption meter
/// (<see cref="CorruptionTiers.EligibilityOf"/>); it is never serialized on its own, so unlike the
/// persisted enums it carries no append-only ordinal contract.
/// </summary>
public enum EndingPath
{
    Undecided,
    Dawnfire,
    LordOfEmbers,
}
